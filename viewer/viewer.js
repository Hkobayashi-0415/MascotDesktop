import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { MMDLoader } from 'three/addons/loaders/MMDLoader.js';
import { MMDAnimationHelper } from 'three/addons/animation/MMDAnimationHelper.js';
import { TGALoader } from 'three/addons/loaders/TGALoader.js';

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
let diagOverlay = null;
let diagPreset = 'base';

const baseConfig = {
  diag: 'base',
  seamFix: {
    enabled: true,
    bias: 0.0015,
    repeatMin: 0.995,
    alphaTest: 0.5,
    useAlphaToCoverage: true,
    anisotropy: 'max', // number or 'max'
    magFilterMode: 'linear', // linear | nearest
    premultiplyAlpha: true,
    alphaBleed: true,
    alphaBleedRadius: 4,
    alphaBleedPasses: 2,
    allowMipMap: false,
  },
};

let config = JSON.parse(JSON.stringify(baseConfig));

function parseQueryOverrides() {
  const params = new URLSearchParams(window.location.search);
  if (params.has('diag')) config.diag = params.get('diag');
  const sf = config.seamFix;
  if (params.has('seamFix')) sf.enabled = params.get('seamFix') === 'true';
  if (params.has('bias')) sf.bias = parseFloat(params.get('bias'));
  if (params.has('repeatMin')) sf.repeatMin = parseFloat(params.get('repeatMin'));
  if (params.has('alphaTest')) sf.alphaTest = parseFloat(params.get('alphaTest'));
  if (params.has('mag')) sf.magFilterMode = params.get('mag');
  if (params.has('premul')) sf.premultiplyAlpha = params.get('premul') === 'true';
  if (params.has('alphableed')) sf.alphaBleed = params.get('alphableed') === 'true';
  if (params.has('alphableedRadius')) sf.alphaBleedRadius = parseInt(params.get('alphableedRadius'), 10);
  if (params.has('alphableedpasses')) sf.alphaBleedPasses = parseInt(params.get('alphableedpasses'), 10);
  if (params.has('alphableedPasses')) sf.alphaBleedPasses = parseInt(params.get('alphableedPasses'), 10);
  if (params.has('allowMipMap')) sf.allowMipMap = params.get('allowMipMap') === 'true';
  if (params.has('aniso')) {
    const a = params.get('aniso');
    sf.anisotropy = a === 'max' ? 'max' : parseInt(a, 10);
  }
}

function updateDiagOverlay() {
  if (!diagOverlay) {
    diagOverlay = document.createElement('div');
    diagOverlay.style.position = 'absolute';
    diagOverlay.style.right = '8px';
    diagOverlay.style.top = '32px';
    diagOverlay.style.padding = '6px 8px';
    diagOverlay.style.background = 'rgba(0,0,0,0.5)';
    diagOverlay.style.color = '#eee';
    diagOverlay.style.fontSize = '12px';
    diagOverlay.style.zIndex = '10';
    diagOverlay.style.display = 'none'; // Hidden by default
    diagOverlay.id = 'diag-overlay';
    document.body.appendChild(diagOverlay);
  }
  const sf = config.seamFix;
  const a2c = sf.useAlphaToCoverage !== false && alphaToCoverageSupported;
  diagOverlay.textContent = `diag=${diagPreset} | bias=${sf.bias} repeatMin=${sf.repeatMin} alphaTest=${sf.alphaTest} mag=${sf.magFilterMode} premul=${sf.premultiplyAlpha} alphaBleed=${sf.alphaBleed} p=${sf.alphaBleedPasses} a2c=${a2c} mip=${sf.allowMipMap} aniso=${sf.anisotropy}`;

  // Only show if in debug mode
  diagOverlay.style.display = document.body.classList.contains('debug-mode') ? 'block' : 'none';
}

function setDiagPreset(name) {
  diagPreset = name;
  config = JSON.parse(JSON.stringify(baseConfig));
  parseQueryOverrides();
  switch (name) {
    case 'raw': // disable viewer-side texture tweaks (baseline for comparison)
      config.seamFix.enabled = false;
      config.seamFix.premultiplyAlpha = false;
      config.seamFix.alphaBleed = false;
      config.seamFix.allowMipMap = false;
      break;
    case 'solid':
      config.seamFix.enabled = false;
      break;
    case 'nearest':
      config.seamFix.magFilterMode = 'nearest';
      config.seamFix.premultiplyAlpha = false;
      break;
    case 'mipmap_on':
      config.seamFix.magFilterMode = 'linear';
      config.seamFix.premultiplyAlpha = false;
      config.seamFix.allowMipMap = true;
      break;
    case 'premul':
      config.seamFix.premultiplyAlpha = true;
      break;
    default:
      break;
  }
  updateDiagOverlay();
  reloadFromState();
}

function applyAlphaBleed(texture, opts = {}) {
  if (!texture || !texture.image || texture._alphaBleedApplied) return texture;
  const { radius = 2, premultiplyAlpha = false, passes = 1 } = opts;
  const img = texture.image;
  const hasSize = img && typeof img.width === 'number' && typeof img.height === 'number' && img.width > 0 && img.height > 0;
  const isDrawable =
    (typeof HTMLImageElement !== 'undefined' && img instanceof HTMLImageElement) ||
    (typeof HTMLCanvasElement !== 'undefined' && img instanceof HTMLCanvasElement) ||
    (typeof ImageBitmap !== 'undefined' && img instanceof ImageBitmap) ||
    (typeof OffscreenCanvas !== 'undefined' && img instanceof OffscreenCanvas);
  // Guard: DataTexture 等 drawImage 非対応の画像はスキップ
  if (!hasSize || !isDrawable) {
    console.warn('alphaBleed: skip (non-drawable image)', img);
    return texture;
  }
  const canvas = document.createElement('canvas');
  canvas.width = img.width;
  canvas.height = img.height;
  const ctx = canvas.getContext('2d');
  try {
    ctx.drawImage(img, 0, 0);
  } catch (e) {
    console.warn('alphaBleed: drawImage failed, skip', e);
    return texture;
  }
  const data = ctx.getImageData(0, 0, canvas.width, canvas.height);
  const { width, height } = canvas;
  const idx = (x, y) => (y * width + x) * 4;
  const threshold = 8; // alpha <= 8 considered transparent
  const doPass = () => {
    const src = new Uint8ClampedArray(data.data);
    let changed = false;
    for (let y = 0; y < height; y++) {
      for (let x = 0; x < width; x++) {
        const i = idx(x, y);
        const a = src[i + 3];
        if (a > threshold) continue;
        let count = 0;
        let rr = 0;
        let gg = 0;
        let bb = 0;
        for (let ry = -radius; ry <= radius; ry++) {
          for (let rx = -radius; rx <= radius; rx++) {
            const nx = x + rx;
            const ny = y + ry;
            if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
            const ni = idx(nx, ny);
            const na = src[ni + 3];
            if (na > threshold) {
              rr += src[ni];
              gg += src[ni + 1];
              bb += src[ni + 2];
              count++;
            }
          }
        }
        if (count > 0) {
          data.data[i] = Math.round(rr / count);
          data.data[i + 1] = Math.round(gg / count);
          data.data[i + 2] = Math.round(bb / count);
          changed = true;
        }
      }
    }
    return changed;
  };

  const maxPasses = Math.max(1, passes | 0);
  for (let p = 0; p < maxPasses; p++) {
    const changed = doPass();
    if (!changed) break;
  }

  ctx.putImageData(data, 0, 0);
  texture.image = canvas;
  if ('premultiplyAlpha' in texture) texture.premultiplyAlpha = premultiplyAlpha;
  texture.needsUpdate = true;
  texture._alphaBleedApplied = true;
  return texture;
}

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

  const manager = new THREE.LoadingManager();

  // Register TGALoader for .tga files
  manager.addHandler(/\.tga$/i, new TGALoader(manager));

  // Cache buster timestamp
  const cacheBuster = `?t=${Date.now()}`;

  manager.setURLModifier((url) => {
    if (url.startsWith('blob:') || url.startsWith('data:')) return url;
    const clean = url.replace(/^\.?\//, '');

    // Simple path resolution: prepend /static/ if not already
    let finalUrl = url;
    if (!url.startsWith('/static/')) {
      finalUrl = `/static/${cleanBase}/${clean}`;
    }

    // Add cache buster for image files
    if (/\.(png|jpg|jpeg|tga|bmp)$/i.test(finalUrl)) {
      finalUrl += cacheBuster;
    }

    return finalUrl;
  });

  const loader = new MMDLoader(manager);

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

        // Reset all morph targets (表情) to 0
        if (currentMesh.morphTargetInfluences) {
          for (let i = 0; i < currentMesh.morphTargetInfluences.length; i++) {
            currentMesh.morphTargetInfluences[i] = 0;
          }
        }
        currentMesh.traverse((child) => {
          if (child.morphTargetInfluences) {
            for (let i = 0; i < child.morphTargetInfluences.length; i++) {
              child.morphTargetInfluences[i] = 0;
            }
          }
        });

        // Material quality tweaks (viewer-side only)
        const maxAnisoSupported = renderer.capabilities.getMaxAnisotropy
          ? renderer.capabilities.getMaxAnisotropy()
          : 1;
        const alphaTagRegex = /(hair|lash|透明|alpha)/i;
        const seamSensitiveRegex = /(face|eye|skin|口|目)/i;
        const sf = { ...config.seamFix, ...(state.config?.seamFix || {}) };
        const useNearest = sf.magFilterMode === 'nearest' || diagPreset === 'nearest';
        const allowMip = sf.allowMipMap || diagPreset === 'mipmap_on';
        const premul = sf.premultiplyAlpha || diagPreset === 'premul';
        const alphaBleedEnabled = !!sf.alphaBleed;
        const targetAniso = sf.anisotropy ?? 0;
        config.seamFix = sf;
        updateDiagOverlay();

        currentMesh.traverse((child) => {
          if (!child.isMesh) return;
          if (diagPreset === 'solid') {
            child.material = new THREE.MeshBasicMaterial({
              color: 0xdddddd,
              side: THREE.DoubleSide,
            });
            return;
          }
          const materials = Array.isArray(child.material) ? child.material : [child.material];
          materials.forEach((mat) => {
            if (!mat) return;
            const maps = ['map', 'emissiveMap', 'alphaMap', 'specularMap', 'normalMap', 'bumpMap'];
            maps.forEach((k) => {
              const tex = mat[k];
              if (tex) {
                const seamSensitive = seamSensitiveRegex.test(mat.name || '');
                const alphaLike = alphaTagRegex.test(mat.name || '') || seamSensitive;
                if (alphaBleedEnabled && seamSensitive) {
                  applyAlphaBleed(tex, {
                    radius: sf.alphaBleedRadius || 3,
                    premultiplyAlpha: premul,
                    passes: sf.alphaBleedPasses || 1,
                  });
                }
                if ('colorSpace' in tex) tex.colorSpace = THREE.SRGBColorSpace;
                tex.wrapS = THREE.ClampToEdgeWrapping;
                tex.wrapT = THREE.ClampToEdgeWrapping;
                if (useNearest) {
                  tex.minFilter = allowMip ? THREE.NearestMipmapNearestFilter : THREE.NearestFilter;
                  tex.magFilter = THREE.NearestFilter;
                } else {
                  tex.minFilter = allowMip ? THREE.LinearMipmapLinearFilter : THREE.LinearFilter;
                  tex.magFilter = THREE.LinearFilter;
                }
                tex.generateMipmaps = allowMip;
                if (premul && 'premultiplyAlpha' in tex) tex.premultiplyAlpha = true;
                if (targetAniso > 0) {
                  tex.anisotropy = Math.min(maxAnisoSupported, targetAniso);
                }
                if (sf.enabled && alphaLike) {
                  const bias = sf.bias ?? 0.0015;
                  // Use absolute offset, not cumulative (+=) to avoid drift on reload
                  if (!tex._seamBiasApplied) {
                    tex.offset.x = bias;
                    tex.offset.y = bias;
                    tex._seamBiasApplied = true;
                  }
                  const repMin = sf.repeatMin ?? 0.995;
                  tex.repeat.x = Math.max(repMin, tex.repeat.x || 1.0);
                  tex.repeat.y = Math.max(repMin, tex.repeat.y || 1.0);
                }
                tex.needsUpdate = true;
              }
            });
            const alphaLike =
              mat.transparent || alphaTagRegex.test(mat.name || '') || seamSensitiveRegex.test(mat.name || '');
            if (alphaLike) {
              mat.alphaTest = Math.max(mat.alphaTest || 0, sf.alphaTest ?? 0.5);
              if (alphaToCoverageSupported && sf.useAlphaToCoverage !== false) {
                mat.alphaToCoverage = true;
              }
            }
            if (targetAniso > 0) {
              mat.needsUpdate = true;
            }
          });
        });// Center camera/controls to model bounds
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

// LocalStorage keys
const LS_KEY_DIAG = 'mmdviewer_diag';
const LS_KEY_SEAMFIX = 'mmdviewer_seamfix';

function loadFromLocalStorage() {
  try {
    const savedDiag = localStorage.getItem(LS_KEY_DIAG);
    if (savedDiag) config.diag = savedDiag;
    const savedSeamFix = localStorage.getItem(LS_KEY_SEAMFIX);
    if (savedSeamFix) {
      const parsed = JSON.parse(savedSeamFix);
      Object.assign(config.seamFix, parsed);
    }
  } catch (e) {
    console.warn('Failed to load from localStorage:', e);
  }
}

function saveToLocalStorage() {
  try {
    localStorage.setItem(LS_KEY_DIAG, diagPreset);
    localStorage.setItem(LS_KEY_SEAMFIX, JSON.stringify({
      bias: config.seamFix.bias,
      alphaTest: config.seamFix.alphaTest,
      premultiplyAlpha: config.seamFix.premultiplyAlpha,
      anisotropy: config.seamFix.anisotropy,
    }));
  } catch (e) {
    console.warn('Failed to save to localStorage:', e);
  }
}

function updateSeamFixUI() {
  const biasEl = document.getElementById('sf-bias');
  const alphaTestEl = document.getElementById('sf-alphatest');
  const premulEl = document.getElementById('sf-premul');
  const anisoEl = document.getElementById('sf-aniso');
  if (biasEl) biasEl.value = config.seamFix.bias;
  if (alphaTestEl) alphaTestEl.value = config.seamFix.alphaTest;
  if (premulEl) premulEl.checked = config.seamFix.premultiplyAlpha;
  if (anisoEl) anisoEl.value = config.seamFix.anisotropy === 'max' ? 16 : config.seamFix.anisotropy;
}

function applySeamFixFromUI() {
  const biasEl = document.getElementById('sf-bias');
  const alphaTestEl = document.getElementById('sf-alphatest');
  const premulEl = document.getElementById('sf-premul');
  const anisoEl = document.getElementById('sf-aniso');
  if (biasEl) config.seamFix.bias = parseFloat(biasEl.value) || 0.0015;
  if (alphaTestEl) config.seamFix.alphaTest = parseFloat(alphaTestEl.value) || 0.5;
  if (premulEl) config.seamFix.premultiplyAlpha = premulEl.checked;
  if (anisoEl) config.seamFix.anisotropy = parseInt(anisoEl.value, 10) || 16;
  saveToLocalStorage();
  updateDiagOverlay();
  reloadFromState();
}

function updateDiagButtonHighlight() {
  const diagButtons = document.querySelectorAll('#diag-controls button');
  diagButtons.forEach((btn) => {
    const target = btn.dataset.diag || btn.textContent.toLowerCase();
    if (target === diagPreset) {
      btn.classList.add('active');
    } else {
      btn.classList.remove('active');
    }
  });
}

function main() {
  loadFromLocalStorage();
  parseQueryOverrides();
  initThree();

  // Set initial diag preset from localStorage or query
  const initialDiag = config.diag || 'base';
  setDiagPreset(initialDiag);
  updateDiagButtonHighlight();
  updateSeamFixUI();
  updateDiagOverlay();

  // Diag preset buttons
  const diagButtons = document.querySelectorAll('#diag-controls button');
  diagButtons.forEach((btn) => {
    btn.addEventListener('click', () => {
      const target = btn.dataset.diag || btn.textContent.toLowerCase();
      setDiagPreset(target);
      saveToLocalStorage();
      updateDiagButtonHighlight();
    });
  });

  // SeamFix Apply button
  const sfApply = document.getElementById('sf-apply');
  if (sfApply) {
    sfApply.addEventListener('click', applySeamFixFromUI);
  }

  // Keyboard shortcuts
  window.addEventListener('keydown', (e) => {
    if (e.key === '1') { setDiagPreset('base'); updateDiagButtonHighlight(); saveToLocalStorage(); }
    if (e.key === '2') { setDiagPreset('solid'); updateDiagButtonHighlight(); saveToLocalStorage(); }
    if (e.key === '3') { setDiagPreset('nearest'); updateDiagButtonHighlight(); saveToLocalStorage(); }
    if (e.key === '4') { setDiagPreset('mipmap_on'); updateDiagButtonHighlight(); saveToLocalStorage(); }
    if (e.key === '5') { setDiagPreset('premul'); updateDiagButtonHighlight(); saveToLocalStorage(); }
    // Ctrl+D to toggle debug mode
    if (e.key === 'd' && e.ctrlKey) {
      e.preventDefault();
      document.body.classList.toggle('debug-mode');
      updateDiagOverlay();
    }
  });

  reloadBtn.addEventListener('click', reloadFromState);

  // T2: Fetch available slots and update motion dropdown
  async function updateMotionDropdown() {
    const motionSelect = document.getElementById('motion-select');
    if (!motionSelect) return;

    try {
      const res = await fetch('/avatar/slots');
      const data = await res.json();

      if (!data.ok || !data.slots) {
        console.warn('Failed to fetch slots:', data);
        return;
      }

      // Clear existing options
      motionSelect.innerHTML = '';

      if (data.slots.length === 0) {
        const opt = document.createElement('option');
        opt.value = '';
        opt.textContent = '(no motions)';
        motionSelect.appendChild(opt);
        motionSelect.disabled = true;
        return;
      }

      // Add options from API
      for (const slot of data.slots) {
        const opt = document.createElement('option');
        opt.value = slot.file;
        opt.textContent = slot.label || slot.file;
        opt.dataset.slot = slot.slot;
        motionSelect.appendChild(opt);
      }

      motionSelect.disabled = data.slots.length <= 1;
    } catch (e) {
      console.error('Failed to update motion dropdown:', e);
    }
  }

  // Character load button
  const charSelect = document.getElementById('char-select');
  const charLoadBtn = document.getElementById('char-load');
  if (charSelect && charLoadBtn) {
    charLoadBtn.addEventListener('click', async () => {
      const slug = charSelect.value;
      setStatus(`Loading ${slug}...`);
      try {
        const res = await fetch('/avatar/load', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ model_path: `data/assets_user/characters/${slug}/mmd/model.pmx` }),
        });
        const data = await res.json();
        if (data.error) {
          setStatus(`Error: ${data.error}`, true);
        } else {
          setStatus(`Loaded: ${slug}`);
          // T2: Update motion dropdown after loading new character
          await updateMotionDropdown();
          reloadFromState();
        }
      } catch (e) {
        setStatus(`Load failed: ${e.message}`, true);
      }
    });
  }

  // Motion select + apply
  const motionSelect = document.getElementById('motion-select');
  const motionApply = document.getElementById('motion-apply');
  if (motionSelect && motionApply) {
    motionApply.addEventListener('click', async () => {
      const vmdFile = motionSelect.value;
      setStatus(`Applying ${vmdFile}...`);
      try {
        // Extract slug from currently loaded model path instead of UI dropdown
        // This ensures motion is applied to the actual loaded model, not UI selection
        let slug = null;
        if (currentModelPath) {
          // Extract slug from path like "data/assets_user/characters/<slug>/mmd/model.pmx"
          const match = currentModelPath.match(/characters\/([^\/]+)\/mmd\//);
          slug = match ? match[1] : null;
        }
        if (!slug) {
          // Fallback to UI selection if model path not available
          const charSelect = document.getElementById('char-select');
          slug = charSelect ? charSelect.value : 'amane_kanata_v1';
        }

        // Update manifest to use selected motion
        const res = await fetch('/avatar/set_motion', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            slug,
            motion_file: vmdFile
          }),
        });
        const data = await res.json();
        if (data.error) {
          setStatus(`Error: ${data.error}`, true);
        } else {
          setStatus(`Motion: ${vmdFile}`);
          // Reload to apply new motion
          setTimeout(() => reloadFromState(), 300);
        }
      } catch (e) {
        setStatus(`Apply failed: ${e.message}`, true);
      }
    });
  }

  // T2: Initial dropdown update and state reload
  updateMotionDropdown();
  reloadFromState();
}

main();

