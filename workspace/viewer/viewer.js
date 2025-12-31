import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { MMDLoader } from 'three/addons/loaders/MMDLoader.js';
import { MMDAnimationHelper } from 'three/addons/animation/MMDAnimationHelper.js';

const statusEl = document.getElementById('status');
const reloadBtn = document.getElementById('reload');
const canvas = document.getElementById('canvas');

let renderer, scene, camera, controls;
let currentMesh = null;
let currentAction = null;
let mixer = null;
let currentMotionMeta = null;
let helper = null;
let currentRootBone = null;
let baseRootPosition = null;
let currentModelPath = null;
let alphaToCoverageSupported = false;

function setStatus(msg, isError = false) {
  statusEl.textContent = msg;
  statusEl.style.color = isError ? '#f66' : '#eee';
}

function initThree() {
  renderer = new THREE.WebGLRenderer({ antialias: true, canvas });
  renderer.setPixelRatio(window.devicePixelRatio);
  renderer.setSize(window.innerWidth, window.innerHeight);
  if ('outputColorSpace' in renderer) {
    renderer.outputColorSpace = THREE.SRGBColorSpace;
  } else if ('outputEncoding' in renderer) {
    renderer.outputEncoding = THREE.sRGBEncoding;
  }
  scene = new THREE.Scene();
  scene.background = new THREE.Color(0x111111);
  camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 1, 5000);
  camera.position.set(0, 10, 40);
  controls = new OrbitControls(camera, renderer.domElement);
  controls.enableDamping = true;
  controls.dampingFactor = 0.05;
  // Alpha-to-coverage (WebGL2 MSAA) for better edges where利用可能
  const gl = renderer.getContext();
  alphaToCoverageSupported = gl && typeof WebGL2RenderingContext !== 'undefined' && gl instanceof WebGL2RenderingContext;
  if (alphaToCoverageSupported && gl.SAMPLE_ALPHA_TO_COVERAGE) {
    gl.enable(gl.SAMPLE_ALPHA_TO_COVERAGE);
  }
  const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 1.0);
  hemi.position.set(0, 20, 0);
  scene.add(hemi);
  const dir = new THREE.DirectionalLight(0xffffff, 0.8);
  dir.position.set(20, 20, 10);
  scene.add(dir);

  window.addEventListener('resize', onWindowResize);
  animate();
}

function onWindowResize() {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
}

function disposeCurrent() {
  if (helper) {
    try {
      helper.remove(currentMesh);
    } catch (_) {
      // ignore
    }
    helper = null;
  }
  currentRootBone = null;
  baseRootPosition = null;
  currentModelPath = null;
  if (currentAction) {
    currentAction.stop();
    currentAction = null;
  }
  if (mixer) {
    mixer.stopAllAction();
    mixer = null;
  }
  if (currentMesh) {
    scene.remove(currentMesh);
    currentMesh.traverse((child) => {
      if (child.geometry) child.geometry.dispose();
      if (child.material) {
        if (Array.isArray(child.material)) {
          child.material.forEach((m) => m.dispose && m.dispose());
        } else if (child.material.dispose) {
          child.material.dispose();
        }
      }
    });
    currentMesh = null;
  }
}

async function fetchState() {
  const res = await fetch('/viewer/state');
  if (!res.ok) throw new Error(`state fetch failed: ${res.status}`);
  return res.json();
}

function applyRootLock() {
  if (currentMotionMeta?.root_lock && currentRootBone && baseRootPosition) {
    currentRootBone.position.copy(baseRootPosition);
  }
}

function applyActionCleanup(prevAction, newAction, fade, motionInfo) {
  if (!prevAction || !newAction || prevAction === newAction) {
    if (newAction && fade > 0) newAction.fadeIn(fade);
    return;
  }
  try {
    prevAction.crossFadeTo(newAction, fade, false);
    newAction.reset();
    newAction.play();
    if (motionInfo?.time_scale) newAction.setEffectiveTimeScale(motionInfo.time_scale);
    setTimeout(() => {
      try {
        prevAction.stop();
        mixer?.uncacheAction?.(prevAction);
        mixer?.uncacheClip?.(prevAction.getClip?.());
      } catch (_) {
        /* ignore */
      }
    }, Math.max(0, fade * 1000 + 50));
  } catch (_) {
    prevAction.stop();
  }
}

async function applyMotion(loader, motionInfo) {
  if (!motionInfo || !motionInfo.motion_path) {
    setStatus(`Loaded: ${currentModelPath} (no motion)`);
    currentMotionMeta = motionInfo;
    return true;
  }
  const motionPath = `/static/${motionInfo.motion_path}`;
  return new Promise((resolve) => {
    loader.loadAnimation(
      motionPath,
      currentMesh,
      (vmd) => {
        const prevAction = currentAction;
        if (!helper) {
          helper = new MMDAnimationHelper({ afterglow: 0.0, physics: motionInfo.physics === true });
        } else {
          // Avoid double-add of the same mesh when reloading motions
          try {
            helper.remove(currentMesh);
          } catch (e) {
            console.warn('helper.remove error (ignored):', e);
          }
        }
        helper.add(currentMesh, {
          animation: vmd,
          loop: motionInfo.loop !== false,
          timeScale: motionInfo.time_scale ?? 1,
          physics: motionInfo.physics === true,
        });
        const obj = helper.objects.get(currentMesh);
        mixer = obj?.mixer || mixer || null;
        const newAction = mixer ? mixer.clipAction(vmd) : null;
        if (newAction) {
          newAction.reset();
          newAction.play();
          if (motionInfo.time_scale) {
            newAction.setEffectiveTimeScale(motionInfo.time_scale);
          }
          const fade = motionInfo.crossfade_sec ?? 0.35;
          applyActionCleanup(prevAction, newAction, fade, motionInfo);
        }
        currentAction = newAction;
        currentMotionMeta = motionInfo;
        setStatus(`Loaded: ${currentModelPath} + ${motionInfo.motion_path}`);
        resolve(true);
      },
      undefined,
      (err) => {
        setStatus(`VMD load failed: ${err.message}`, true);
        resolve(false);
      }
    );
  });
}

async function loadModelAndMotion(state) {
  const { model_path, motion } = state;
  const motionInfo = motion || {
    motion_path: state.current_motion_path || null,
    loop: true,
    time_scale: 1.0,
  };
  currentMotionMeta = motionInfo;
  if (!model_path) throw new Error('model_path not set in state');
  setStatus(`Loading model: ${model_path} ...`);

  const baseDir = model_path.split('/').slice(0, -1).join('/');
  const cleanBase = baseDir.replace(/\\/g, '/');
  const resourceBase = `/static/${cleanBase}/`;
  const textureSibling = cleanBase.endsWith('/mmd')
    ? `/static/${cleanBase.replace(/\/mmd$/, '/texture')}/`
    : resourceBase;

  const manager = new THREE.LoadingManager();
  manager.setURLModifier((url) => {
    if (url.startsWith('blob:') || url.startsWith('data:')) return url;
    const clean = url.replace(/^\.?\//, '');

    // Case1: relative texture/* path from PMX
    if (cleanBase.endsWith('/mmd') && clean.startsWith('texture/')) {
      return `${textureSibling}${clean.slice('texture/'.length)}`;
    }

    // Case2: resourcePath prepended (/static/.../mmd/texture/*)
    const mmdTexturePrefix = `/static/${cleanBase}/texture/`;
    if (cleanBase.endsWith('/mmd') && url.startsWith(mmdTexturePrefix)) {
      const rest = url.slice(mmdTexturePrefix.length);
      return `${textureSibling}${rest}`;
    }

    if (!url.startsWith('/static/')) return `/static/${clean}`;
    return url;
  });

  const loader = new MMDLoader(manager);
  loader.setResourcePath(resourceBase);

  // If model is already loaded and unchanged, only swap motion
  if (currentMesh && currentModelPath === model_path) {
    return applyMotion(loader, motionInfo);
  }

  return new Promise((resolve, reject) => {
    loader.load(
      `/static/${model_path}`,
      (mesh) => {
        disposeCurrent();
        currentMesh = mesh;
        // Normalize transform to avoid drift/axisズレ
        currentMesh.position.set(0, 0, 0);
        currentMesh.rotation.set(0, 0, 0);
        currentMesh.scale.set(1, 1, 1);
        currentModelPath = model_path;
        scene.add(currentMesh);

        // Material quality tweaks (viewer-side only)
        const maxAniso = renderer.capabilities.getMaxAnisotropy
          ? renderer.capabilities.getMaxAnisotropy()
          : 1;
        const alphaTagRegex = /(hair|lash|透明|alpha)/i;
        const seamSensitiveRegex = /(face|eye|skin|口|目)/i;
        currentMesh.traverse((child) => {
          if (!child.isMesh) return;
          const materials = Array.isArray(child.material) ? child.material : [child.material];
          materials.forEach((mat) => {
            if (!mat) return;
            const maps = ['map', 'emissiveMap', 'alphaMap', 'specularMap', 'normalMap'];
            maps.forEach((k) => {
              const tex = mat[k];
              if (tex) {
                tex.anisotropy = Math.max(tex.anisotropy || 1, maxAniso);
                const seamSensitive = seamSensitiveRegex.test(mat.name || '');
                const alphaLike = alphaTagRegex.test(mat.name || '') || seamSensitive;
                if (alphaLike) {
                  tex.wrapS = THREE.ClampToEdgeWrapping;
                  tex.wrapT = THREE.ClampToEdgeWrapping;
                  tex.minFilter = THREE.LinearFilter; // avoid mip seam
                  tex.magFilter = THREE.LinearFilter;
                  tex.generateMipmaps = false;
                  // 軽微〜中程度のUVオフセットでセンター縫い目の色混ざりを緩和（シーム多発材のみ）
                  const seamBias = 0.0015;
                  tex.offset.x += seamBias;
                  tex.offset.y += seamBias;
                  tex.repeat.x = Math.max(0.995, tex.repeat.x || 1.0);
                  tex.repeat.y = Math.max(0.995, tex.repeat.y || 1.0);
                } else {
                  tex.minFilter = THREE.LinearMipmapLinearFilter;
                  tex.magFilter = THREE.LinearFilter;
                }
                if ('colorSpace' in tex) tex.colorSpace = THREE.SRGBColorSpace;
              }
            });
            if (mat.transparent || alphaTagRegex.test(mat.name || '') || seamSensitiveRegex.test(mat.name || '')) {
              mat.alphaTest = Math.max(mat.alphaTest || 0, 0.5);
              if (alphaToCoverageSupported) mat.alphaToCoverage = true;
            }
            mat.needsUpdate = true;
          });
        });

        // Center camera/controls to model bounds
        const box = new THREE.Box3().setFromObject(currentMesh);
        const center = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());
        const radius = Math.max(size.x, size.y, size.z) || 10;
        controls.target.copy(center);
        camera.position.set(center.x, center.y + radius * 0.2, center.z + radius * 2.0);
        controls.update();

        // root lock setup (simple)
        const rootNames = ['センター', 'センタ', 'Center', 'センター', 'センターボーン'];
        currentRootBone = currentMesh.skeleton?.bones?.find((b) => rootNames.includes(b.name)) || currentMesh.skeleton?.bones?.[0] || null;
        baseRootPosition = currentRootBone ? currentRootBone.position.clone() : null;

        applyMotion(loader, motionInfo).then((ok) => resolve(ok));
      },
      undefined,
      (err) => {
        setStatus(`Model load failed: ${err.message}`, true);
        reject(err);
      }
    );
  });
}

async function reloadFromState() {
  try {
    const state = await fetchState();
    await loadModelAndMotion(state);
  } catch (err) {
    console.error(err);
    setStatus(err.message, true);
  }
}

function animate() {
  requestAnimationFrame(animate);
  const delta = clock.getDelta();
  controls.update();
  if (mixer) mixer.update(delta);
  applyRootLock();
  renderer.render(scene, camera);
}

const clock = new THREE.Clock();

function main() {
  initThree();
  reloadBtn.addEventListener('click', reloadFromState);
  reloadFromState();
}

main();
