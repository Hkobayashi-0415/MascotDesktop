import json
import logging
import os
import random
import sys
import threading
import tkinter as tk
import uuid
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
from urllib.parse import unquote, urlparse
import webbrowser
import time

# Workspace root (three levels up: apps/avatar/poc -> workspace)
WS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir, os.pardir))
sys.path.append(os.path.join(WS_ROOT, "apps"))

from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402

LOG_DIR = Path(WS_ROOT) / "logs" / "avatar"
CURRENT_MODEL_PATH = None
VIEWER_DIR = Path(WS_ROOT) / "viewer"
STATE = {
    "model_path": None,
    "current_motion_path": None,
    "current_slot": None,
    "motion": None,
    "last_error": None,
    "updated_at": None,
}
DEFAULT_MODEL = "data/assets_user/characters/kanata_official_v1/mmd/model.pmx"
# Browser auto-openはデフォルトOFF。必要なら環境変数 AVATAR_OPEN_BROWSER=1 をセット。
DEFAULT_OPEN_BROWSER = os.environ.get("AVATAR_OPEN_BROWSER", "0") == "1"
_UNSET = object()


def warn_if_non_ascii_path():
    paths = [os.getcwd(), WS_ROOT]
    if any(any(ord(ch) > 127 for ch in p) for p in paths):
        msg = "Non-ASCII path detected. Recommend using ASCII path like C:\\dev\\MascotDesktop\\workspace"
        logging.warning(msg)
        try:
            print(msg)
        except Exception:
            pass


def find_default_motion(model_path: str | None):
    """Try to find a default idle motion relative to the model."""
    if not model_path:
        return None
    base = Path(WS_ROOT) / model_path
    candidates = [base.parent / "idle.vmd"]
    for c in candidates:
        if c.exists():
            return str(c.relative_to(WS_ROOT)).replace("\\", "/")
    return None


def load_manifest(model_path: str | None):
    """manifest.json を model と同階層から読み込む（存在しない場合は None）。"""
    if not model_path:
        return None, None, "MANIFEST_NOT_FOUND"
    manifest_path = (Path(WS_ROOT) / model_path).parent / "manifest.json"
    if not manifest_path.exists():
        return None, manifest_path, "MANIFEST_NOT_FOUND"
    try:
        with open(manifest_path, "r", encoding="utf-8") as f:
            data = json.load(f)
        return data, manifest_path, None
    except Exception as e:
        get_logger("avatar.ipc").warning(
            "avatar.manifest.load_failed",
            extra=log_extra(
                "avatar.ipc",
                "avatar.manifest.load_failed",
                request_id=ensure_request_id(),
                error=str(e),
                manifest=str(manifest_path),
            ),
        )
        return None, manifest_path, "MANIFEST_NOT_FOUND"


def build_motion_dict(motion_path: str, slot: str | None = None, variant: dict | None = None):
    variant = variant or {}
    return {
        "motion_path": motion_path,
        "slot": slot,
        "loop": bool(variant.get("loop", True)),
        "time_scale": float(variant.get("time_scale", 1.0)),
        "root_lock": bool(variant.get("root_lock", False)),
        "crossfade_sec": float(variant.get("crossfade_sec", 0.35)),
        "physics": bool(variant.get("physics", False)),
    }


def resolve_motion_from_slot(slot: str, model_path: str):
    manifest, manifest_path, manifest_err = load_manifest(model_path)
    if manifest and "motions" in manifest and "slots" in manifest["motions"]:
        slot_info = manifest["motions"]["slots"].get(slot)
        if slot_info and slot_info.get("variants"):
            variants = slot_info["variants"]
            weights = [v.get("weight", 1) for v in variants]
            total = sum(weights)
            r = random.uniform(0, total)
            upto = 0
            chosen = variants[0]
            for v, w in zip(variants, weights):
                if upto + w >= r:
                    chosen = v
                    break
                upto += w
            path = chosen.get("path")
            if path:
                base_dir = manifest_path.parent if manifest_path else Path(WS_ROOT)
                resolved = (base_dir / path).resolve()
                if not resolved.exists():
                    return None, "MOTION_NOT_FOUND"
                try:
                    rel = resolved.relative_to(WS_ROOT)
                    rel_str = str(rel).replace("\\", "/")
                except Exception:
                    rel_str = str(resolved).replace("\\", "/")
                return build_motion_dict(rel_str, slot=slot, variant=chosen), None
        return None, "MOTION_NOT_FOUND"
    if slot == "idle":
        fallback = find_default_motion(model_path)
        if fallback:
            return build_motion_dict(fallback, slot="idle", variant={}), None
        return None, "MOTION_NOT_FOUND"
    if manifest_err:
        return None, "MANIFEST_NOT_FOUND"
    return None, "MOTION_NOT_FOUND"


def update_state(model_path=_UNSET, motion_path=_UNSET, motion=_UNSET, slot=_UNSET, error=_UNSET):
    global STATE, CURRENT_MODEL_PATH
    if model_path is not _UNSET:
        CURRENT_MODEL_PATH = model_path
        STATE["model_path"] = model_path
    if motion is not _UNSET:
        STATE["motion"] = motion
        STATE["current_motion_path"] = motion.get("motion_path") if motion else None
    elif motion_path is not _UNSET:
        STATE["current_motion_path"] = motion_path
    if slot is not _UNSET:
        STATE["current_slot"] = slot
    if error is not _UNSET:
        STATE["last_error"] = error
    STATE["updated_at"] = time.time()


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
        if parsed.path == "/viewer" or parsed.path == "/viewer/":
            index_file = VIEWER_DIR / "index.html"
            if not index_file.exists():
                self.send_error(500, "viewer files not found")
                return
            self.send_response(200)
            self.send_header("Content-Type", "text/html; charset=utf-8")
            self.end_headers()
            with open(index_file, "rb") as f:
                self.wfile.write(f.read())
            return
        if parsed.path == "/viewer/state":
            # ensure some baseline state
            if not STATE.get("model_path"):
                if CURRENT_MODEL_PATH:
                    update_state(model_path=CURRENT_MODEL_PATH, motion=STATE.get("motion"), error=None)
                elif os.path.exists(os.path.join(WS_ROOT, DEFAULT_MODEL)):
                    motion_dict, motion_err = resolve_motion_from_slot("idle", DEFAULT_MODEL)
                    update_state(
                        model_path=DEFAULT_MODEL,
                        motion=motion_dict,
                        slot=motion_dict.get("slot") if motion_dict else None,
                        error=None if motion_dict else motion_err,
                    )
            payload = success(
                {
                    "status": "ok",
                    "dto_version": "0.1.0",
                    "model_path": STATE.get("model_path"),
                    "current_motion_path": STATE.get("current_motion_path") or (STATE.get("motion") or {}).get("motion_path"),
                    "current_slot": STATE.get("current_slot") or (STATE.get("motion") or {}).get("slot"),
                    "motion": STATE.get("motion"),
                    "last_error": STATE.get("last_error"),
                    "updated_at": STATE.get("updated_at"),
                },
                req_id,
            )
            self._set_headers(200, request_id=req_id)
            self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))
            return
        if parsed.path.startswith("/viewer/"):
            rel = parsed.path[len("/viewer/") :]
            rel = "index.html" if rel == "" else rel
            target = (VIEWER_DIR / rel).resolve()
            if not str(target).startswith(str(VIEWER_DIR.resolve())):
                self.send_error(403, "forbidden")
                return
            if not target.exists() or not target.is_file():
                self.send_error(404, "not found")
                return
            ct = "application/octet-stream"
            lower = target.name.lower()
            if lower.endswith(".html"):
                ct = "text/html; charset=utf-8"
            elif lower.endswith(".js"):
                ct = "text/javascript; charset=utf-8"
            elif lower.endswith(".css"):
                ct = "text/css; charset=utf-8"
            self.send_response(200)
            self.send_header("Content-Type", ct)
            self.end_headers()
            with open(target, "rb") as f:
                self.wfile.write(f.read())
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
            slot = payload.get("slot")
            if not motion_path and not slot:
                self._set_headers(400, request_id=req_id)
                payload_err = error(req_id, "AVATAR.PLAY.MISSING_PARAM", "slot or motion_path is required", 400)
                self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                return
            if not STATE.get("model_path"):
                self._set_headers(400, request_id=req_id)
                payload_err = error(req_id, "AVATAR.PLAY.NO_MODEL", "model is not loaded", 400)
                self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                return

            motion = None
            if slot:
                motion, motion_err = resolve_motion_from_slot(slot, STATE.get("model_path"))
                if not motion:
                    code = motion_err or "MOTION_NOT_FOUND"
                    self._set_headers(404, request_id=req_id)
                    payload_err = error(
                        req_id,
                        code,
                        f"motion not found for slot '{slot}'",
                        404,
                    )
                    self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                    return
            if motion_path:
                motion = build_motion_dict(motion_path, slot=slot or (motion.get("slot") if motion else None))

            update_state(motion=motion, slot=motion.get("slot") if motion else slot, error=None)
            note = "motion set; viewer will pick up on next poll"
            self._set_headers(200, request_id=req_id)
            payload_ok = success(
                {
                    "status": "ok",
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "note": note,
                    "motion": motion,
                },
                req_id,
            )
            self.wfile.write(json.dumps(payload_ok, ensure_ascii=False).encode("utf-8"))
            get_logger("avatar.ipc").info(
                "avatar.play",
                extra=log_extra(
                    "avatar.ipc",
                    "avatar.play",
                    request_id=req_id,
                    status_code=200,
                    motion_path=motion.get("motion_path") if motion else motion_path,
                    slot=motion.get("slot") if motion else slot,
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
            abs_model = os.path.join(WS_ROOT, model_path)
            if not os.path.exists(abs_model):
                self._set_headers(404, request_id=req_id)
                payload_err = error(req_id, "MODEL_NOT_FOUND", "model file not found", 404)
                self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                update_state(error=payload_err["error_code"])
                get_logger("avatar.ipc").warning(
                    "avatar.load.model_missing",
                    extra=log_extra(
                        "avatar.ipc",
                        "avatar.load.model_missing",
                        request_id=req_id,
                        model_path=model_path,
                        error_code="MODEL_NOT_FOUND",
                    ),
                )
                return

            motion = None
            if payload.get("default_motion_path"):
                motion = build_motion_dict(payload["default_motion_path"], slot="idle")
            else:
                motion, motion_err = resolve_motion_from_slot("idle", model_path)
            if not motion:
                self._set_headers(404, request_id=req_id)
                payload_err = error(
                    req_id,
                    motion_err or "MOTION_NOT_FOUND",
                    "idle motion not found (manifest missing and idle.vmd absent)",
                    404,
                )
                self.wfile.write(json.dumps(payload_err, ensure_ascii=False).encode("utf-8"))
                update_state(model_path=model_path, motion=None, slot=None, error=payload_err["error_code"])
                get_logger("avatar.ipc").warning(
                    "avatar.load.motion_missing",
                    extra=log_extra(
                        "avatar.ipc",
                        "avatar.load.motion_missing",
                        request_id=req_id,
                        model_path=model_path,
                        error_code="MOTION_NOT_FOUND",
                    ),
                )
                return
            update_state(model_path=model_path, motion=motion, slot=motion.get("slot") if motion else None, error=None)
            self._set_headers(200, request_id=req_id)
            payload_ok = success(
                {
                    "status": "ok",
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "loaded_model": model_path,
                    "default_motion": motion.get("motion_path") if motion else None,
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
    if os.path.exists(os.path.join(WS_ROOT, DEFAULT_MODEL)):
        motion, motion_err = resolve_motion_from_slot("idle", DEFAULT_MODEL)
        update_state(
            model_path=DEFAULT_MODEL,
            motion=motion,
            slot=motion.get("slot") if motion else None,
            error=None if motion else motion_err,
        )
        get_logger("avatar.ipc").info(
            "avatar.load.default",
            extra=log_extra(
                "avatar.ipc",
                "avatar.load.default",
                request_id="auto-default",
                model_path=DEFAULT_MODEL,
                motion_path=motion.get("motion_path") if motion else None,
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
