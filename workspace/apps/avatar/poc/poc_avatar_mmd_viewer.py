import json
import logging
import os
import sys
import threading
import tkinter as tk
import uuid
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
from urllib.parse import urlparse

# Workspace root (three levels up: apps/avatar/poc -> workspace)
WS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir, os.pardir))
sys.path.append(os.path.join(WS_ROOT, "apps"))

from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402

LOG_DIR = Path(WS_ROOT) / "logs" / "avatar"


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
            # Placeholder: no actual render yet
            self._set_headers(200, request_id=req_id)
            payload_ok = success(
                {
                    "status": "ok",
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "loaded_model": model_path,
                    "note": "MMD render not implemented; this is a placeholder viewer.",
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
