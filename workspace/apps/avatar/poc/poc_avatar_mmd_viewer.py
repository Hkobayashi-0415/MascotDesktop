import json
import logging
import os
import sys
import threading
import tkinter as tk
import uuid
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
from urllib.parse import unquote, urlparse
import webbrowser

# Workspace root (three levels up: apps/avatar/poc -> workspace)
WS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir, os.pardir))
sys.path.append(os.path.join(WS_ROOT, "apps"))

from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402

LOG_DIR = Path(WS_ROOT) / "logs" / "avatar"
CURRENT_MODEL_PATH = None
DEFAULT_MODEL = "data/assets_user/characters/kanata_official_v1/mmd/model.pmx"
# Browser auto-openはデフォルトOFF。必要なら環境変数 AVATAR_OPEN_BROWSER=1 をセット。
DEFAULT_OPEN_BROWSER = os.environ.get("AVATAR_OPEN_BROWSER", "0") == "1"

# Simple HTML viewer using three.js + MMDLoader (CDN)
VIEWER_HTML = """<!doctype html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>MMD Viewer PoC</title>
  <script type="importmap">
    {
      "imports": {
        "three": "https://unpkg.com/three@0.159.0/build/three.module.js",
        "three/addons/": "https://unpkg.com/three@0.159.0/examples/jsm/"
      }
    }
  </script>
  <style>
    body, html { margin: 0; padding: 0; overflow: hidden; background: #111; color: #eee; }
    #info { position: absolute; top: 8px; left: 8px; z-index: 10; font-family: Arial, sans-serif; background: rgba(0,0,0,0.5); padding: 6px 8px; border-radius: 4px; }
  </style>
</head>
<body>
  <div id="info">MMD Viewer PoC<br/>Loading model...</div>
  <script type="module">
    import * as THREE from 'three';
    import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
    import { MMDLoader } from 'three/addons/loaders/MMDLoader.js';

    const info = document.getElementById('info');
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x111111);
    const camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 1, 5000);
    camera.position.set(0, 10, 40);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.05;

    const hemi = new THREE.HemisphereLight(0xffffff, 0x444444, 1.0);
    hemi.position.set(0, 20, 0);
    scene.add(hemi);
    const dir = new THREE.DirectionalLight(0xffffff, 0.8);
    dir.position.set(20, 20, 10);
    scene.add(dir);

    let mesh = null;

    async function fetchModel() {
      const res = await fetch('/avatar/current_model');
      if (!res.ok) throw new Error('no model set');
      const data = await res.json();
      if (!data.model_url || !data.resource_base) throw new Error('invalid model info');
      return data;
    }

    async function loadModel() {
      try {
        info.textContent = 'Loading model...';
        const { model_url, resource_base } = await fetchModel();
        const loader = new MMDLoader();
        loader.setResourcePath(resource_base);
        loader.load(
          model_url,
          (obj) => {
            if (mesh) scene.remove(mesh);
            mesh = obj;
            mesh.position.y = -10;
            scene.add(mesh);
            info.textContent = 'Loaded: ' + model_url;
          },
          (xhr) => { info.textContent = 'Loading... ' + (xhr.total ? Math.floor((xhr.loaded / xhr.total) * 100) : '?') + '%'; },
          (err) => { info.textContent = 'Load error: ' + err; console.error(err); }
        );
      } catch (e) {
        info.textContent = 'No model loaded. POST /avatar/load first.';
        console.warn(e);
      }
    }

    window.addEventListener('resize', () => {
      camera.aspect = window.innerWidth / window.innerHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(window.innerWidth, window.innerHeight);
    });

    function animate() {
      requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    }
    animate();
    loadModel();
    setInterval(loadModel, 3000); // simple poll for new model
  </script>
</body>
</html>
"""


def warn_if_non_ascii_path():
    paths = [os.getcwd(), WS_ROOT]
    if any(any(ord(ch) > 127 for ch in p) for p in paths):
        msg = "Non-ASCII path detected. Recommend using ASCII path like C:\\dev\\MascotDesktop\\workspace"
        logging.warning(msg)
        try:
            print(msg)
        except Exception:
            pass


class ViewerUI:
    """Minimal placeholder viewer (no real MMD render yet)."""

    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Avatar Mode1 (MMD) Viewer - PoC")
        self.root.geometry("480x320+200+200")
        self.root.config(bg="#111")
        label = tk.Label(
            self.root,
            text="MMD Viewer PoC\n(placeholder)\nSend /avatar/load via HTTP to log requests.",
            fg="white",
            bg="#111",
            font=("Segoe UI", 12),
            justify="center",
        )
        label.pack(expand=True, fill="both")

        btn_frame = tk.Frame(self.root, bg="#111")
        btn_frame.pack(side="bottom", fill="x")
        tk.Button(btn_frame, text="Reload", command=self.reload_model, bg="#222", fg="#eee").pack(side="left", padx=4, pady=4)

    def reload_model(self):
        # trigger refresh on frontend by updating CURRENT_MODEL_PATH (no-op if same)
        global CURRENT_MODEL_PATH
        if CURRENT_MODEL_PATH:
            get_logger("avatar.ipc").info(
                "avatar.load.reload_request",
                extra=log_extra("avatar.ipc", "avatar.load.reload_request", request_id=ensure_request_id(), model=CURRENT_MODEL_PATH),
            )
        # nothing else needed; /avatar/current_model will be polled by the viewer

    def run(self):
        self.root.mainloop()


def success(body: dict, req_id: str):
    data = body.copy()
    data["ok"] = True
    data["request_id"] = req_id
    return data


def error(req_id: str, code: str, message: str, status: int = 400):
    return {
        "ok": False,
        "request_id": req_id,
        "error_code": code,
        "message": message,
        "status": status,
    }


class Handler(BaseHTTPRequestHandler):
    server_version = "AvatarMMDViewer/0.1"

    def _set_headers(self, code=200, request_id=None):
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        if request_id:
            self.send_header("X-Request-Id", request_id)
        self.end_headers()

    def log_message(self, fmt, *args):
        get_logger("avatar.ipc").info("%s - %s", self.address_string(), fmt % args)

    def do_GET(self):
        parsed = urlparse(self.path)
        req_id = self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
        clear_context()
        set_context(request_id=req_id, component="avatar", feature="avatar.health")
        if parsed.path == "/avatar/health":
            body = success({"status": "ok", "dto_version": "0.1.0", "service": "avatar-mmd"}, req_id)
            self._set_headers(200, request_id=req_id)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            get_logger("avatar.health").info(
                "avatar.health", extra=log_extra("avatar.health", "avatar.health", request_id=req_id, status_code=200)
            )
            return
        if parsed.path == "/viewer":
            # Serve inline HTML viewer
            self.send_response(200)
            self.send_header("Content-Type", "text/html; charset=utf-8")
            self.end_headers()
            self.wfile.write(VIEWER_HTML.encode("utf-8"))
            return
        if parsed.path == "/favicon.ico":
            self.send_response(204)
            self.end_headers()
            return
        if parsed.path == "/avatar/current_model":
            if CURRENT_MODEL_PATH:
                model_url = f"/static/{CURRENT_MODEL_PATH.replace('\\\\','/')}"
                base_dir = os.path.dirname(CURRENT_MODEL_PATH).replace("\\", "/")
                payload = {
                    "ok": True,
                    "model_url": model_url,
                    "resource_base": f"/static/{base_dir}/" if base_dir else "/static/",
                }
                self._set_headers(200, request_id=req_id)
                self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))
                return
            self._set_headers(404, request_id=req_id)
            self.wfile.write(json.dumps(error(req_id, "AVATAR.LOAD.NOT_SET", "no model loaded", 404)).encode("utf-8"))
            return
        if parsed.path.startswith("/static/"):
            rel = parsed.path[len("/static/") :]
            safe_rel = os.path.normpath(unquote(rel))
            abs_path = os.path.normpath(os.path.join(WS_ROOT, safe_rel))
            if not abs_path.startswith(WS_ROOT):
                self.send_error(403, "forbidden")
                return
            if not os.path.exists(abs_path) or not os.path.isfile(abs_path):
                self.send_error(404, "not found")
                return
            # naive content-type
            ct = "application/octet-stream"
            lower = abs_path.lower()
            if lower.endswith(".pmx"):
                ct = "application/octet-stream"
            elif lower.endswith(".png"):
                ct = "image/png"
            elif lower.endswith(".jpg") or lower.endswith(".jpeg"):
                ct = "image/jpeg"
            elif lower.endswith(".tga"):
                ct = "image/x-tga"
            elif lower.endswith(".bmp"):
                ct = "image/bmp"
            self.send_response(200)
            self.send_header("Content-Type", ct)
            self.end_headers()
            with open(abs_path, "rb") as f:
                self.wfile.write(f.read())
            return
        self._set_headers(404, request_id=req_id)
        payload = error(req_id, "AVATAR.IPC.NOT_FOUND", "not found", 404)
        self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))

    def do_POST(self):
        parsed = urlparse(self.path)
        length = int(self.headers.get("Content-Length", "0"))
        raw = self.rfile.read(length) if length > 0 else b"{}"
        try:
            payload = json.loads(raw.decode("utf-8"))
        except Exception:
            req_id = self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
            self._set_headers(400, request_id=req_id)
            payload_err = error(req_id, "AVATAR.IPC.BAD_REQUEST.INVALID_JSON", "invalid json", 400)
            self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
            return

        req_id = payload.get("request_id") or self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
        clear_context()
        set_context(request_id=req_id, component="avatar", feature="avatar.ipc")

        if parsed.path == "/avatar/play":
            motion_path = payload.get("motion_path")
            note = "motion playback not implemented (stub)"
            self._set_headers(200, request_id=req_id)
            payload_ok = success(
                {
                    "status": "ok",
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "request_id": req_id,
                    "note": note,
                    "motion_path": motion_path,
                },
                req_id,
            )
            self.wfile.write(json.dumps(payload_ok, ensure_ascii=False).encode("utf-8"))
            get_logger("avatar.ipc").info(
                "avatar.play.stub",
                extra=log_extra(
                    "avatar.ipc",
                    "avatar.play.stub",
                    request_id=req_id,
                    status_code=200,
                    motion_path=motion_path,
                    note=note,
                ),
            )
            return

        if parsed.path == "/avatar/load":
            model_path = payload.get("model_path")
            if not model_path:
                self._set_headers(400, request_id=req_id)
                payload_err = error(req_id, "AVATAR.LOAD.MISSING_MODEL", "model_path is required", 400)
                self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                get_logger("avatar.ipc").warning(
                    "avatar.load.validation_failed",
                    extra=log_extra(
                        "avatar.ipc",
                        "avatar.load.validation_failed",
                        request_id=req_id,
                        error_code="AVATAR.LOAD.MISSING_MODEL",
                    ),
                )
                return
            global CURRENT_MODEL_PATH
            CURRENT_MODEL_PATH = model_path
            self._set_headers(200, request_id=req_id)
            payload_ok = success(
                {
                    "status": "ok",
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "loaded_model": model_path,
                    "note": "Viewer will attempt to load model via /viewer page.",
                },
                req_id,
            )
            self.wfile.write(json.dumps(payload_ok, ensure_ascii=False).encode("utf-8"))
            get_logger("avatar.ipc").info(
                "avatar.load",
                extra=log_extra(
                    "avatar.ipc",
                    "avatar.load",
                    request_id=req_id,
                    status_code=200,
                    model_path=model_path,
                ),
            )
            return

        self._set_headers(404, request_id=req_id)
        payload_err = error(req_id, "AVATAR.IPC.NOT_FOUND", "not found", 404)
        self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
        get_logger("avatar.ipc").warning(
            "avatar.not_found",
            extra=log_extra(
                "avatar.ipc",
                "avatar.not_found",
                request_id=req_id,
                status_code=404,
                path=parsed.path,
                error_code="AVATAR.IPC.NOT_FOUND",
            ),
        )


def run_server(host="127.0.0.1", port=8770):
    bootstrap_logging("avatar", LOG_DIR)
    warn_if_non_ascii_path()
    server = HTTPServer((host, port), Handler)
    get_logger("avatar.health").info(
        "avatar.viewer.start",
        extra=log_extra("avatar.health", "avatar.viewer.start", request_id=ensure_request_id(), host=host, port=port),
    )
    threading.Thread(target=server.serve_forever, daemon=True).start()
    return server


def main():
    server = run_server()
    # auto-load default model if exists
    global CURRENT_MODEL_PATH
    if os.path.exists(os.path.join(WS_ROOT, DEFAULT_MODEL)):
        CURRENT_MODEL_PATH = DEFAULT_MODEL
        get_logger("avatar.ipc").info(
            "avatar.load.default",
            extra=log_extra(
                "avatar.ipc",
                "avatar.load.default",
                request_id="auto-default",
                model_path=DEFAULT_MODEL,
            ),
        )
    else:
        get_logger("avatar.ipc").warning(
            "avatar.load.default_missing",
            extra=log_extra(
                "avatar.ipc",
                "avatar.load.default_missing",
                request_id="auto-default",
                model_path=DEFAULT_MODEL,
            ),
        )
    if DEFAULT_OPEN_BROWSER:
        try:
            webbrowser.open("http://127.0.0.1:8770/viewer")
        except Exception:
            pass
    ui = ViewerUI()
    try:
        ui.run()
    finally:
        server.shutdown()
        server.server_close()
        get_logger("avatar.health").info(
            "avatar.viewer.stop", extra=log_extra("avatar.health", "avatar.viewer.stop", request_id=ensure_request_id())
        )


if __name__ == "__main__":
    main()
