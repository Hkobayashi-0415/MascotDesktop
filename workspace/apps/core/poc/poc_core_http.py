import json
import logging
import os
import sys
import time
import uuid
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
from urllib.parse import urlparse

WS_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir, os.pardir))
sys.path.append(os.path.join(WS_ROOT, "apps"))

from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402
from common.observability.sanitize import summarize_payload  # noqa: E402

DATA_DIR = os.path.join(WS_ROOT, "data")
USER_CONFIG = os.path.join(DATA_DIR, "config", "user", "config.json")

DEFAULT_CONFIG = {
    "window_x": 100,
    "window_y": 100,
    "width": 320,
    "height": 320,
    "pinned": True,
}

# simple dedup cache: request_id -> timestamp
RECENT_REQUESTS: dict[str, float] = {}
DEDUP_TTL_SECONDS = 10


def ensure_dirs():
    os.makedirs(os.path.dirname(USER_CONFIG), exist_ok=True)
    # Non-ASCII path guard (warn only)
    cwd = os.getcwd()
    if any(ord(ch) > 127 for ch in cwd):
        logging.warning(
            "Non-ASCII path detected (%s). Recommend using ASCII path like C:\\dev\\MascotDesktop\\workspace",
            cwd,
        )


def load_config():
    ensure_dirs()
    if not os.path.exists(USER_CONFIG):
        return DEFAULT_CONFIG.copy()
    try:
        with open(USER_CONFIG, "r", encoding="utf-8") as f:
            data = json.load(f)
        merged = DEFAULT_CONFIG.copy()
        merged.update(data)
        # backward compatibility: if pinned missing but topmost exists, map it
        if "pinned" not in merged and "topmost" in merged:
            merged["pinned"] = bool(merged.get("topmost"))
        return merged
    except Exception as e:
        get_logger("config").exception(
            "config.load.failed",
            extra=log_extra("config", "config.load.failed", request_id=ensure_request_id(), error=str(e)),
        )
        return DEFAULT_CONFIG.copy()


def save_config(cfg):
    ensure_dirs()
    try:
        with open(USER_CONFIG, "w", encoding="utf-8") as f:
            json.dump(cfg, f, ensure_ascii=False, indent=2)
        return True
    except Exception as e:
        get_logger("config").exception(
            "config.save.failed",
            extra=log_extra("config", "config.save.failed", request_id=ensure_request_id(), error=str(e)),
        )
        return False


def error_payload(request_id: str, error_code: str, message: str, retryable: bool, status: int):
    return {
        "ok": False,
        "request_id": request_id,
        "error_code": error_code,
        "retryable": retryable,
        "message": message,
        "status": status,
    }


def success_envelope(body: dict, request_id: str):
    data = body.copy()
    data["ok"] = True
    data["request_id"] = request_id
    return data


def map_exception_to_error(ex: Exception):
    return "CORE.GENERAL.UNHANDLED", False, "unexpected error"


class Handler(BaseHTTPRequestHandler):
    server_version = "CorePoCHTTP/0.1"

    def _set_headers(self, code=200, request_id=None):
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        if request_id:
            self.send_header("X-Request-Id", request_id)
        self.end_headers()

    def log_message(self, fmt, *args):
        get_logger("health").info("%s - %s", self.address_string(), fmt % args)

    def do_GET(self):
        start = time.perf_counter()
        parsed = urlparse(self.path)
        clear_context()
        req_id = self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
        set_context(request_id=req_id, component="core", feature="health")
        if parsed.path == "/health":
            body = {
                "dto_version": "0.1.0",
                "status": "ok",
                "service": "core",
            }
            self._set_headers(200, request_id=req_id)
            self.wfile.write(json.dumps(success_envelope(body, req_id), ensure_ascii=False).encode("utf-8"))
            duration_ms = int((time.perf_counter() - start) * 1000)
            get_logger("health").info(
                "health.ok",
                extra=log_extra(
                    "health",
                    "health.ok",
                    request_id=req_id,
                    duration_ms=duration_ms,
                    status_code=200,
                ),
            )
            return

        self._set_headers(404, request_id=req_id)
        payload = error_payload(req_id, "CORE.IPC.NOT_FOUND", "not found", False, 404)
        self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))
        duration_ms = int((time.perf_counter() - start) * 1000)
        get_logger("health").warning(
            "health.not_found",
            extra=log_extra(
                "health",
                "health.not_found",
                request_id=req_id,
                duration_ms=duration_ms,
                status_code=404,
                error_code="CORE.IPC.NOT_FOUND",
            ),
        )

    def do_POST(self):
        start = time.perf_counter()
        parsed = urlparse(self.path)
        length = int(self.headers.get("Content-Length", "0"))
        raw = self.rfile.read(length) if length > 0 else b"{}"

        # Parse JSON
        try:
            payload = json.loads(raw.decode("utf-8"))
        except Exception:
            req_id = self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
            self._set_headers(400, request_id=req_id)
            body = error_payload(req_id, "CORE.IPC.BAD_REQUEST.INVALID_JSON", "invalid json", False, 400)
            self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
            return

        req_id = payload.get("request_id") or self.headers.get("X-Request-Id") or f"req-{uuid.uuid4()}"
        clear_context()
        set_context(request_id=req_id, component="core", feature="ipc")

        try:
            # chat.send
            if parsed.path == "/v1/chat/send":
                set_context(feature="chat")
                char_id = payload.get("character_id", "default")
                message = payload.get("message", {})
                if not isinstance(message, dict) or "content" not in message:
                    code = "CORE.CHAT.VALIDATION.MISSING_MESSAGE"
                    body = error_payload(req_id, code, "message.content is required", False, 400)
                    self._set_headers(400, request_id=req_id)
                    self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                    duration_ms = int((time.perf_counter() - start) * 1000)
                    get_logger("chat").warning(
                        "chat.validation_failed",
                        extra=log_extra(
                            "chat",
                            "chat.validation_failed",
                            request_id=req_id,
                            character_id=char_id,
                            status_code=400,
                            error_code=code,
                            duration_ms=duration_ms,
                        ),
                    )
                    return
                body = success_envelope(
                    {
                        "dto_version": payload.get("dto_version", "0.1.0"),
                        "status": "ok",
                        "message": {
                            "role": "assistant",
                            "content": f"(mock echo) {message.get('content','')}",
                            "audio": {
                                "used_clip": False,
                                "clip_category": None,
                                "tts_model": "mock",
                                "audio_url": None,
                            },
                            "avatar_state": "02_smile",
                        },
                        "usage": {"prompt_tokens": 0, "completion_tokens": 0},
                    },
                    req_id,
                )
                self._set_headers(200, request_id=req_id)
                self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                duration_ms = int((time.perf_counter() - start) * 1000)
                get_logger("chat").info(
                    "chat.send",
                    extra=log_extra(
                        "chat",
                        "chat.send",
                        request_id=req_id,
                        session_id=payload.get("session_id"),
                        character_id=char_id,
                        duration_ms=duration_ms,
                        status_code=200,
                        error_code=None,
                    ),
                )
                return

            # config.get
            if parsed.path == "/v1/config/get":
                set_context(feature="config")
                cfg = load_config()
                body = success_envelope(
                    {
                        "dto_version": payload.get("dto_version", "0.1.0"),
                        "status": "ok",
                        "config": cfg,
                    },
                    req_id,
                )
                self._set_headers(200, request_id=req_id)
                self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                duration_ms = int((time.perf_counter() - start) * 1000)
                get_logger("config").info(
                    "config.get",
                    extra=log_extra(
                        "config",
                        "config.get",
                        request_id=req_id,
                        duration_ms=duration_ms,
                        status_code=200,
                        error_code=None,
                    ),
                )
                return

            # config.set
            if parsed.path == "/v1/config/set":
                set_context(feature="config")
                # dedup by request_id within TTL
                now_ts = time.time()
                # purge expired
                expired = [k for k, v in RECENT_REQUESTS.items() if now_ts - v > DEDUP_TTL_SECONDS]
                for k in expired:
                    RECENT_REQUESTS.pop(k, None)
                if req_id in RECENT_REQUESTS:
                    cfg = load_config()
                    body = success_envelope(
                        {
                            "dto_version": payload.get("dto_version", "0.1.0"),
                            "status": "ok",
                            "config": cfg,
                            "reason": payload.get("reason"),
                            "dedup": True,
                        },
                        req_id,
                    )
                    self._set_headers(200, request_id=req_id)
                    self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                    duration_ms = int((time.perf_counter() - start) * 1000)
                    get_logger("config").info(
                        "config.set.dedup",
                        extra=log_extra(
                            "config",
                            "config.set.dedup",
                            request_id=req_id,
                            duration_ms=duration_ms,
                            status_code=200,
                            reason=payload.get("reason"),
                        ),
                    )
                    return

                cfg = load_config()
                updates = payload.get("entries")
                if not isinstance(updates, dict) or not updates:
                    code = "CORE.CONFIG.VALIDATION.MISSING_FIELD"
                    body = error_payload(req_id, code, "entries must be a non-empty object", False, 400)
                    self._set_headers(400, request_id=req_id)
                    self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                    duration_ms = int((time.perf_counter() - start) * 1000)
                    get_logger("config").warning(
                        "config.set.validation_failed",
                        extra=log_extra(
                            "config",
                            "config.set.validation_failed",
                            request_id=req_id,
                            duration_ms=duration_ms,
                            status_code=400,
                            error_code=code,
                            reason=payload.get("reason"),
                        ),
                    )
                    return
                RECENT_REQUESTS[req_id] = now_ts
                cfg.update(updates)
                ok = save_config(cfg)
                status_code = 200 if ok else 500
                error_code = None if ok else "CORE.CONFIG.IO.WRITE_FAILED"
                body = {
                    "dto_version": payload.get("dto_version", "0.1.0"),
                    "status": "ok" if ok else "error",
                    "config": cfg,
                    "reason": payload.get("reason"),
                }
                if ok:
                    body = success_envelope(body, req_id)
                else:
                    err = error_payload(req_id, error_code, "failed to save config", True, status_code)
                    body.update(err)
                    body["status"] = "error"
                self._set_headers(status_code, request_id=req_id)
                self.wfile.write(json.dumps(body, ensure_ascii=False).encode("utf-8"))
                duration_ms = int((time.perf_counter() - start) * 1000)
                get_logger("config").info(
                    "config.set",
                    extra=log_extra(
                        "config",
                        "config.set",
                        request_id=req_id,
                        duration_ms=duration_ms,
                        status_code=status_code,
                        error_code=error_code,
                        reason=payload.get("reason"),
                        changed_keys=summarize_payload(updates if isinstance(updates, dict) else {}).get("keys"),
                    ),
                )
                return

            # not found
            self._set_headers(404, request_id=req_id)
            payload = error_payload(req_id, "CORE.IPC.NOT_FOUND", "not found", False, 404)
            self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))
            duration_ms = int((time.perf_counter() - start) * 1000)
            get_logger("ipc").warning(
                "http.not_found",
                extra=log_extra(
                    "ipc",
                    "http.not_found",
                    request_id=req_id,
                    duration_ms=duration_ms,
                    status_code=404,
                    path=parsed.path,
                    error_code="CORE.IPC.NOT_FOUND",
                ),
            )
        except Exception as ex:
            code, retryable, msg = map_exception_to_error(ex)
            self._set_headers(500, request_id=req_id)
            payload = error_payload(req_id, code, msg, retryable, 500)
            self.wfile.write(json.dumps(payload, ensure_ascii=False).encode("utf-8"))
            duration_ms = int((time.perf_counter() - start) * 1000)
            get_logger("ipc").exception(
                "http.unhandled",
                extra=log_extra(
                    "ipc",
                    "http.unhandled",
                    request_id=req_id,
                    duration_ms=duration_ms,
                    status_code=500,
                    error_code=code,
                ),
            )


def run(host="127.0.0.1", port=8765):
    logs_dir = os.path.join(WS_ROOT, "logs", "core")
    bootstrap_logging("core", Path(logs_dir))
    ensure_dirs()
    server = HTTPServer((host, port), Handler)
    get_logger("health").info(
        "core.start", extra=log_extra("health", "core.start", request_id=ensure_request_id())
    )
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        get_logger("health").info(
            "core.stop", extra=log_extra("health", "core.stop", request_id=ensure_request_id())
        )
    finally:
        server.server_close()


if __name__ == "__main__":
    run()
