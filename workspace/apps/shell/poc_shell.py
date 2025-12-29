import json
import logging
import os
import sys
import time
import tkinter as tk
import uuid
from pathlib import Path
from tkinter import ttk
from urllib import request
from urllib.error import HTTPError

sys.path.append(os.path.join(os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir)), "apps"))
from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402
from common.observability.sanitize import summarize_payload  # noqa: E402


def workspace_root() -> str:
    here = os.path.abspath(os.path.dirname(__file__))
    return os.path.abspath(os.path.join(here, os.pardir, os.pardir))


WS_ROOT = workspace_root()
DATA_DIR = os.path.join(WS_ROOT, "data")
USER_DIR = os.path.join(DATA_DIR, "user")
CONFIG_PATH = os.path.join(USER_DIR, "config.json")
CORE_URL = "http://127.0.0.1:8765"
LOG_DIR = Path(WS_ROOT) / "logs" / "shell"
SESSION_ID = f"s-{uuid.uuid4()}"
IMG_PATH = os.path.join(
    WS_ROOT, "data", "templates", "assets", "pngtuber_mode3", "states", "01_normal.gif"
)
PLACEHOLDER_SIZE = 320


def set_feature_ctx(feature: str):
    set_context(
        component="shell",
        feature=feature,
        session_id=SESSION_ID,
        request_id=ensure_request_id(),
    )


def ensure_dirs():
    os.makedirs(USER_DIR, exist_ok=True)
    os.makedirs(os.path.dirname(IMG_PATH), exist_ok=True)


def placeholder_image():
    """Create an in-memory solid placeholder."""
    img = tk.PhotoImage(width=PLACEHOLDER_SIZE, height=PLACEHOLDER_SIZE)
    img.put("#0da3ff", to=(0, 0, PLACEHOLDER_SIZE, PLACEHOLDER_SIZE))
    return img


def load_config():
    default = {"window_x": 100, "window_y": 100, "width": 320, "height": 320, "topmost": True}
    if not os.path.exists(CONFIG_PATH):
        return default
    try:
        with open(CONFIG_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)
        for k, v in default.items():
            data.setdefault(k, v)
        return data
    except Exception as e:
        logging.error("Failed to load config: %s", e)
        return default


def save_config(cfg):
    try:
        with open(CONFIG_PATH, "w", encoding="utf-8") as f:
            json.dump(cfg, f, ensure_ascii=False, indent=2)
        logging.info("Config saved: %s", CONFIG_PATH)
    except Exception as e:
        logging.error("Failed to save config: %s", e)


def save_window_config_remote(reason: str, entries: dict):
    """Update local cfg and send to core once."""
    req_id = f"req-{uuid.uuid4()}"
    set_context(request_id=req_id, session_id=SESSION_ID, component="shell", feature="config")
    payload = {
        "dto_version": "0.1.0",
        "request_id": req_id,
        "reason": reason,
        "entries": entries,
    }
    resp = http_post("/v1/config/set", payload, feature="config", event="config.set", request_id=req_id)
    clear_context()
    return resp


def http_post(path, payload, feature="ipc", event="http.post", request_id=None):
    req_id = request_id or f"req-{uuid.uuid4()}"
    set_context(request_id=req_id, session_id=SESSION_ID, component="shell", feature=feature)
    url = CORE_URL + path
    data = json.dumps(payload).encode("utf-8")
    req = request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json", "X-Request-Id": req_id},
    )
    try:
        with request.urlopen(req, timeout=5) as resp:
            raw = resp.read().decode("utf-8")
            try:
                res_body = json.loads(raw)
            except Exception:
                extra = log_extra(
                    feature,
                    f"{event}.bad_response",
                    request_id=req_id,
                    session_id=SESSION_ID,
                    status_code=resp.getcode(),
                    error_code="CORE.IPC.BAD_RESPONSE.NON_JSON",
                )
                get_logger(feature).warning(f"{event}.bad_response", extra=extra)
                return {
                    "ok": False,
                    "error_code": "CORE.IPC.BAD_RESPONSE.NON_JSON",
                    "message": "non-JSON response",
                    "request_id": req_id,
                }
            status = resp.getcode()
            extra = log_extra(
                feature,
                event,
                request_id=req_id,
                session_id=SESSION_ID,
                status_code=status,
                error_code=res_body.get("error_code"),
            )
            if res_body.get("error_code"):
                get_logger(feature).warning(event, extra=extra)
            else:
                get_logger(feature).info(event, extra=extra)
            return res_body
    except HTTPError as e:
        err_body = e.read().decode("utf-8") if hasattr(e, "read") else ""
        get_logger(feature).error(
            f"{event}.failed",
            extra=log_extra(
                feature,
                f"{event}.failed",
                request_id=req_id,
                status_code=getattr(e, "code", None),
                error_code="CORE.IPC.BAD_RESPONSE.HTTP_ERROR",
                error=str(e),
            ),
        )
        return {
            "ok": False,
            "error_code": "CORE.IPC.BAD_RESPONSE.HTTP_ERROR",
            "message": str(e),
            "request_id": req_id,
            "raw": err_body,
        }
    except Exception as e:
        get_logger(feature).error(
            f"{event}.failed",
            extra=log_extra(feature, f"{event}.failed", request_id=req_id, error=str(e)),
        )
        return {"ok": False, "error_code": "SHELL.IPC.HTTP_ERROR", "message": str(e), "request_id": req_id}
    finally:
        clear_context()


def http_get(path, feature="ipc", event="http.get"):
    req_id = f"req-{uuid.uuid4()}"
    set_context(request_id=req_id, session_id=SESSION_ID, component="shell", feature=feature)
    url = CORE_URL + path
    req = request.Request(url, headers={"X-Request-Id": req_id})
    try:
        with request.urlopen(req, timeout=5) as resp:
            raw = resp.read().decode("utf-8")
            try:
                res_body = json.loads(raw)
            except Exception:
                extra = log_extra(
                    feature,
                    f"{event}.bad_response",
                    request_id=req_id,
                    session_id=SESSION_ID,
                    status_code=resp.getcode(),
                    error_code="CORE.IPC.BAD_RESPONSE.NON_JSON",
                )
                get_logger(feature).warning(f"{event}.bad_response", extra=extra)
                return {
                    "ok": False,
                    "error_code": "CORE.IPC.BAD_RESPONSE.NON_JSON",
                    "message": "non-JSON response",
                    "request_id": req_id,
                }
            status = resp.getcode()
            extra = log_extra(
                feature,
                event,
                request_id=req_id,
                session_id=SESSION_ID,
                status_code=status,
                error_code=res_body.get("error_code"),
            )
            if res_body.get("error_code"):
                get_logger(feature).warning(event, extra=extra)
            else:
                get_logger(feature).info(event, extra=extra)
            return res_body
    except HTTPError as e:
        err_body = e.read().decode("utf-8") if hasattr(e, "read") else ""
        get_logger(feature).error(
            f"{event}.failed",
            extra=log_extra(
                feature,
                f"{event}.failed",
                request_id=req_id,
                status_code=getattr(e, "code", None),
                error_code="CORE.IPC.BAD_RESPONSE.HTTP_ERROR",
                error=str(e),
            ),
        )
        return {
            "ok": False,
            "error_code": "CORE.IPC.BAD_RESPONSE.HTTP_ERROR",
            "message": str(e),
            "request_id": req_id,
            "raw": err_body,
        }
    except Exception as e:
        get_logger(feature).error(
            f"{event}.failed",
            extra=log_extra(feature, f"{event}.failed", request_id=req_id, error=str(e)),
        )
        return {"ok": False, "error_code": "SHELL.IPC.HTTP_ERROR", "message": str(e), "request_id": req_id}
    finally:
        clear_context()


class ShellApp:
    def __init__(self):
        self.cfg = load_config()
        self.root = tk.Tk()
        self.root.title("Mascot PoC (Shell)")
        self.root.overrideredirect(True)
        self.root.attributes("-topmost", bool(self.cfg.get("topmost", True)))
        self.bg_color = "#222222"
        self.root.config(bg=self.bg_color)
        self._last_topmost_toggle_ts = 0.0

        # Size/position with offscreen guard
        screen_w = self.root.winfo_screenwidth()
        screen_h = self.root.winfo_screenheight()
        w = int(self.cfg.get("width", 320))
        h = int(self.cfg.get("height", 320))
        x = int(self.cfg.get("window_x", 100))
        y = int(self.cfg.get("window_y", 100))
        if x < 0 or x > screen_w - 50:
            x = 100
        if y < 0 or y > screen_h - 50:
            y = 100
        if w <= 0 or h <= 0:
            w, h = 320, 320
        self.root.geometry(f"{w}x{h}+{x}+{y}")

        self._drag_data = {"x": 0, "y": 0}

        container = ttk.Frame(self.root, padding=0, style="Container.TFrame")
        container.pack(fill="both", expand=True)

        self.image = None
        try:
            set_feature_ctx("assets")
            self.image = tk.PhotoImage(file=IMG_PATH)
            get_logger("assets").info(
                "image.load.ok",
                extra=log_extra("assets", "image.load.ok", request_id=ensure_request_id(), path=IMG_PATH),
            )
        except tk.TclError as e:
            set_feature_ctx("assets")
            get_logger("assets").warning(
                "image.load.failed",
                extra=log_extra("assets", "image.load.failed", request_id=ensure_request_id(), error=str(e)),
            )
            self.image = placeholder_image()

        self.label = tk.Label(
            container,
            image=self.image,
            bg=self.bg_color,
            bd=0,
            width=w,
            height=h,
            anchor="center",
        )
        self.label.pack(fill="both", expand=True)

        # Bind drag move
        self.label.bind("<ButtonPress-1>", self.start_move)
        self.label.bind("<B1-Motion>", self.do_move)
        self.label.bind("<ButtonRelease-1>", self.end_move)
        self.label.bind("<Button-1>", self.focus_window)  # ensure focus for key events
        # Toggle topmost with key 't' on key press (debounced); close with Esc
        self.root.bind_all("<KeyPress-t>", self.on_topmost_press)
        self.root.bind("<Escape>", self.close)

        self.root.protocol("WM_DELETE_WINDOW", self.close)

        # Health check + config sync
        self.health_ok = False
        try:
            health = http_get("/health", feature="health", event="health.check")
            self.health_ok = health.get("status") == "ok"
        except Exception as e:
            set_feature_ctx("health")
            get_logger("health").error(
                "health.check.failed",
                extra=log_extra("health", "health.check.failed", request_id=ensure_request_id(), error=str(e)),
            )

        try:
            cfg_resp = http_post(
                "/v1/config/get",
                {"dto_version": "0.1.0", "request_id": "cfg-get-1"},
                feature="config",
                event="config.get",
            )
            if cfg_resp.get("status") == "ok":
                self.cfg.update(cfg_resp.get("config", {}))
                w = int(self.cfg.get("width", 320))
                h = int(self.cfg.get("height", 320))
                x = int(self.cfg.get("window_x", 100))
                y = int(self.cfg.get("window_y", 100))
                screen_w = self.root.winfo_screenwidth()
                screen_h = self.root.winfo_screenheight()
                if x < 0 or x > screen_w - 50:
                    x = 100
                if y < 0 or y > screen_h - 50:
                    y = 100
                if w <= 0 or h <= 0:
                    w, h = 320, 320
                self.root.geometry(f"{w}x{h}+{x}+{y}")
                self.root.attributes("-topmost", bool(self.cfg.get("topmost", True)))
        except Exception as e:
            set_feature_ctx("config")
            get_logger("config").error(
                "config.get.failed",
                extra=log_extra("config", "config.get.failed", request_id=ensure_request_id(), error=str(e)),
            )

        # Final sanity to make the window obvious on screen.
        self.root.update_idletasks()
        self.root.deiconify()
        # ensure key events are captured
        try:
            self.root.focus_force()
            self.root.focus_set()
            self.label.focus_set()
        except Exception:
            pass
        set_feature_ctx("window")
        get_logger("window").info(
            "window.ready",
            extra=log_extra(
                "window",
                "window.ready",
                request_id=ensure_request_id(),
                x=self.root.winfo_x(),
                y=self.root.winfo_y(),
                w=self.root.winfo_width(),
                h=self.root.winfo_height(),
                topmost=bool(self.root.attributes("-topmost")),
            ),
        )

    def start_move(self, event):
        self._drag_data["x"] = event.x
        self._drag_data["y"] = event.y
        self._drag_data["dragging"] = True

    def do_move(self, event):
        dx = event.x - self._drag_data["x"]
        dy = event.y - self._drag_data["y"]
        x = int(self.root.winfo_x() + dx)
        y = int(self.root.winfo_y() + dy)
        self.root.geometry(f"+{x}+{y}")

    def end_move(self, event=None):
        if not self._drag_data.get("dragging"):
            return
        self._drag_data["dragging"] = False
        x = int(self.root.winfo_x())
        y = int(self.root.winfo_y())
        w = int(self.root.winfo_width())
        h = int(self.root.winfo_height())
        self.cfg["window_x"] = x
        self.cfg["window_y"] = y
        self.cfg["width"] = w
        self.cfg["height"] = h
        try:
            save_window_config_remote(
                "drag_end",
                {"window_x": x, "window_y": y, "width": w, "height": h},
            )
            set_feature_ctx("window")
            get_logger("window").info(
                "window.drag_end",
                extra=log_extra(
                    "window",
                    "window.drag_end",
                    request_id=ensure_request_id(),
                    x=x,
                    y=y,
                    w=w,
                    h=h,
                ),
            )
        except Exception as e:
            set_feature_ctx("window")
            get_logger("window").warning(
                "window.drag_end.failed",
                extra=log_extra("window", "window.drag_end.failed", request_id=ensure_request_id(), error=str(e)),
            )

    def toggle_topmost(self, event=None):
        now = time.monotonic()
        # strong debounce to avoid OSキーリピートによる多重トグル
        if now - getattr(self, "_last_topmost_toggle_ts", 0.0) < 0.8:
            return
        self._last_topmost_toggle_ts = now
        current = bool(self.root.attributes("-topmost"))
        new_state = not current
        self.root.attributes("-topmost", new_state)
        # lift to apply immediately and regain focus
        try:
            self.root.lift()
            self.root.update_idletasks()
            self.root.focus_force()
            self.root.focus_set()
            self.label.focus_set()
        except Exception:
            pass
        self.cfg["topmost"] = new_state
        set_feature_ctx("window")
        actual = bool(self.root.attributes("-topmost"))
        get_logger("window").info(
            "window.topmost_toggle",
            extra=log_extra(
                "window",
                "window.topmost_toggle",
                request_id=ensure_request_id(),
                topmost=new_state,
                topmost_applied=actual,
                reason="topmost_toggle",
            ),
        )
        try:
            save_window_config_remote(
                "topmost_toggle",
                {"topmost": new_state},
            )
        except Exception as e:
            set_feature_ctx("config")
            get_logger("config").warning(
                "config.set.failed",
                extra=log_extra("config", "config.set.failed", request_id=ensure_request_id(), error=str(e)),
            )

    def on_topmost_press(self, event=None):
        # single toggle per press; debounce handled in toggle_topmost
        self.toggle_topmost(event)

    def on_topmost_release(self, event=None):
        # kept for backward compatibility; no-op
        return

    def focus_window(self, event=None):
        try:
            self.root.focus_force()
        except Exception:
            pass

    def close(self, event=None):
        # Save position/size
        self.cfg["window_x"] = self.root.winfo_x()
        self.cfg["window_y"] = self.root.winfo_y()
        self.cfg["width"] = self.root.winfo_width()
        self.cfg["height"] = self.root.winfo_height()
        save_config(self.cfg)
        self.root.destroy()

    def run(self):
        set_feature_ctx("window")
        get_logger("window").info(
            "window.start",
            extra=log_extra(
                "window",
                "window.start",
                request_id=ensure_request_id(),
                topmost=self.cfg.get("topmost"),
            ),
        )
        self.root.mainloop()


def main():
    bootstrap_logging("shell", LOG_DIR)
    ensure_dirs()
    app = ShellApp()
    app.run()


if __name__ == "__main__":
    main()
