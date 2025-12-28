import json
import logging
import os
from http.server import BaseHTTPRequestHandler, HTTPServer
from urllib.parse import urlparse

WS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir))
DATA_DIR = os.path.join(WS_ROOT, "data")
USER_CONFIG = os.path.join(DATA_DIR, "config", "user", "config.json")

DEFAULT_CONFIG = {
    "window_x": 100,
    "window_y": 100,
    "width": 320,
    "height": 320,
    "topmost": True,
}


def ensure_dirs():
    os.makedirs(os.path.dirname(USER_CONFIG), exist_ok=True)


def load_config():
    ensure_dirs()
    if not os.path.exists(USER_CONFIG):
        return DEFAULT_CONFIG.copy()
    try:
        with open(USER_CONFIG, "r", encoding="utf-8") as f:
            data = json.load(f)
        merged = DEFAULT_CONFIG.copy()
        merged.update(data)
        return merged
    except Exception as e:
        logging.error("Failed to load config: %s", e)
        return DEFAULT_CONFIG.copy()


def save_config(cfg):
    ensure_dirs()
    try:
        with open(USER_CONFIG, "w", encoding="utf-8") as f:
            json.dump(cfg, f, ensure_ascii=False, indent=2)
        return True
    except Exception as e:
        logging.error("Failed to save config: %s", e)
        return False


class Handler(BaseHTTPRequestHandler):
    server_version = "CorePoCHTTP/0.1"

    def _set_headers(self, code=200):
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.end_headers()

    def log_message(self, fmt, *args):
        logging.info("%s - %s", self.address_string(), fmt % args)

    def do_GET(self):
        parsed = urlparse(self.path)
        if parsed.path == "/health":
            body = {
                "dto_version": "0.1.0",
                "status": "ok",
                "service": "core",
            }
            self._set_headers(200)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            return
        self._set_headers(404)
        self.wfile.write(json.dumps({"error": "not found"}).encode("utf-8"))

    def do_POST(self):
        parsed = urlparse(self.path)
        length = int(self.headers.get("Content-Length", "0"))
        raw = self.rfile.read(length) if length > 0 else b"{}"
        try:
            payload = json.loads(raw.decode("utf-8"))
        except Exception as e:
            self._set_headers(400)
            self.wfile.write(json.dumps({"error": f"bad json: {e}"}).encode("utf-8"))
            return

        if parsed.path == "/v1/chat/send":
            req_id = payload.get("request_id", "req-unknown")
            char_id = payload.get("character_id", "default")
            body = {
                "dto_version": payload.get("dto_version", "0.1.0"),
                "request_id": req_id,
                "status": "ok",
                "message": {
                    "role": "assistant",
                    "content": f"こんにちは！(mock echo) {payload.get('message', {}).get('content','')}",
                    "audio": {
                        "used_clip": False,
                        "clip_category": None,
                        "tts_model": "mock",
                        "audio_url": None,
                    },
                    "avatar_state": "02_smile",
                },
                "usage": {"prompt_tokens": 0, "completion_tokens": 0},
            }
            logging.info("chat.send req_id=%s character=%s", req_id, char_id)
            self._set_headers(200)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            return

        if parsed.path == "/v1/config/get":
            cfg = load_config()
            body = {
                "dto_version": payload.get("dto_version", "0.1.0"),
                "request_id": payload.get("request_id", "req-unknown"),
                "status": "ok",
                "config": cfg,
            }
            logging.info("config.get")
            self._set_headers(200)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            return

        if parsed.path == "/v1/config/set":
            cfg = load_config()
            updates = payload.get("entries", {})
            cfg.update(updates)
            ok = save_config(cfg)
            status = "ok" if ok else "error"
            body = {
                "dto_version": payload.get("dto_version", "0.1.0"),
                "request_id": payload.get("request_id", "req-unknown"),
                "status": status,
                "config": cfg,
            }
            logging.info("config.set entries=%s", list(updates.keys()))
            self._set_headers(200 if ok else 500)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            return

        self._set_headers(404)
        self.wfile.write(json.dumps({"error": "not found"}).encode("utf-8"))


def run(host="127.0.0.1", port=8765):
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
    )
    ensure_dirs()
    server = HTTPServer((host, port), Handler)
    logging.info("Core PoC HTTP listening on %s:%d", host, port)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        logging.info("Shutting down server")
    finally:
        server.server_close()


if __name__ == "__main__":
    run()
