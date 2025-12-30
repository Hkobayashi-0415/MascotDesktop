import ctypes
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

sys.path.append(os.path.join(os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, os.pardir, os.pardir)), "apps"))
from common.observability.context import clear_context, ensure_request_id, set_context  # noqa: E402
from common.observability.logging import bootstrap_logging, get_logger, log_extra  # noqa: E402
from common.observability.sanitize import summarize_payload  # noqa: E402


def workspace_root() -> str:
    here = os.path.abspath(os.path.dirname(__file__))
    return os.path.abspath(os.path.join(here, os.pardir, os.pardir, os.pardir))


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
PLACEHOLDER_IMG = os.path.join(
    WS_ROOT, "data", "templates", "assets", "placeholders", "avatar_placeholder_256.png"
)
IMG_CANDIDATES = [
    os.path.join(
        WS_ROOT, "data", "assets_user", "characters", "demo", "pngtuber_mode3", "states", "01_normal.png"
    ),
    os.path.join(
        WS_ROOT, "data", "templates", "assets", "pngtuber_mode3", "states", "01_normal.png"
    ),
    IMG_PATH,  # legacy gif
    PLACEHOLDER_IMG,
]


def get_virtual_screen():
    """Return (left, top, right, bottom) of virtual desktop. Fallback to primary screen if unavailable."""
    try:
        user32 = ctypes.windll.user32
        left = user32.GetSystemMetrics(76)  # SM_XVIRTUALSCREEN
        top = user32.GetSystemMetrics(77)  # SM_YVIRTUALSCREEN
        width = user32.GetSystemMetrics(78)  # SM_CXVIRTUALSCREEN
        height = user32.GetSystemMetrics(79)  # SM_CYVIRTUALSCREEN
        if width and height:
            return (left, top, left + width, top + height)
    except Exception:
        pass
    # fallback: primary screen via Tk after init
    return None


def clamp_position(x, y, w, h, bounds):
    """Clamp window top-left so that the window stays within virtual screen."""
    left, top, right, bottom = bounds
    new_x = min(max(x, left), max(left, right - w))
    new_y = min(max(y, top), max(top, bottom - h))
    return int(new_x), int(new_y)


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


def warn_if_non_ascii_path():
    paths = [os.getcwd(), WS_ROOT]
    if any(any(ord(ch) > 127 for ch in p) for p in paths):
        msg = "Non-ASCII path detected. Recommend copying repo to ASCII path (e.g., C:\\dev\\MascotDesktop\\workspace)."
        logging.warning(msg)
        try:
            print(msg)
        except Exception:
            pass


def placeholder_image():
    """Create an in-memory solid placeholder."""
    img = tk.PhotoImage(width=PLACEHOLDER_SIZE, height=PLACEHOLDER_SIZE)
    img.put("#0da3ff", to=(0, 0, PLACEHOLDER_SIZE, PLACEHOLDER_SIZE))
    return img


def load_config():
    default = {
        "window_x": 100,
        "window_y": 100,
        "width": 320,
        "height": 320,
        "pinned": True,
        "image_fit_mode": "contain",
    }
    if not os.path.exists(CONFIG_PATH):
        return default
    try:
        with open(CONFIG_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)
        # backward compat: if pinned missing but topmost exists, map it once
        if "pinned" not in data and "topmost" in data:
            data["pinned"] = bool(data.get("topmost"))
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


def save_window_config_remote(reason: str, entries: dict, request_id: str | None = None):
    """Update local cfg and send to core once."""
    req_id = request_id or f"req-{uuid.uuid4()}"
    set_context(request_id=req_id, session_id=SESSION_ID, component="shell", feature="config")
    payload = {
        "dto_version": "0.1.0",
        "request_id": req_id,
        "reason": reason,
        "entries": entries,
    }
    resp = http_post(
        "/v1/config/set",
        payload,
        feature="config",
        event="ipc.config_set_sent",
        request_id=req_id,
    )
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
        self.root.attributes("-topmost", bool(self.cfg.get("pinned", True)))
        self.bg_color = "#222222"
        self.root.config(bg=self.bg_color)
        self._last_topmost_toggle_ts = 0.0
        self._last_pinned_request_ts = 0.0
        self._pinned_op_inflight = False

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
        self.virtual_bounds = get_virtual_screen()

        self._drag_data = {"x": 0, "y": 0}

        container = ttk.Frame(self.root, padding=0, style="Container.TFrame")
        container.pack(fill="both", expand=True)

        # Canvas for image rendering
        self.canvas = tk.Canvas(container, highlightthickness=0, bd=0, bg=self.bg_color)
        self.canvas.pack(fill="both", expand=True)
        self.canvas.bind("<Configure>", self.on_canvas_configure)

        self.orig_image = None
        self._resize_after_id = None
        self._last_applied_size = None  # (canvas_w, canvas_h, target_w, target_h, mode)
        self._overlay_items = set()
        self._overlay_hit = False
        load_err = None
        for cand in IMG_CANDIDATES:
            try:
                if not os.path.exists(cand):
                    continue
                set_feature_ctx("assets")
                img = tk.PhotoImage(file=cand)
                self.orig_image = img
                get_logger("assets").info(
                    "assets.image_loaded",
                    extra=log_extra(
                        "assets",
                        "assets.image_loaded",
                        request_id=ensure_request_id(),
                        path=cand,
                        w=img.width(),
                        h=img.height(),
                    ),
                )
                break
            except Exception as e:
                load_err = e
                continue
        if self.orig_image is None:
            self.orig_image = placeholder_image()
            set_feature_ctx("assets")
            get_logger("assets").warning(
                "assets.placeholder_used",
                extra=log_extra(
                    "assets",
                    "assets.placeholder_used",
                    request_id=ensure_request_id(),
                    path="generated_placeholder",
                    error=str(load_err) if load_err else None,
                ),
            )
        else:
            try:
                if self.orig_image.width() < 32 or self.orig_image.height() < 32:
                    self.orig_image = placeholder_image()
                    set_feature_ctx("assets")
                    get_logger("assets").info(
                        "assets.placeholder_used",
                        extra=log_extra(
                            "assets",
                            "assets.placeholder_used",
                            request_id=ensure_request_id(),
                            path="generated_placeholder",
                            reason="too_small",
                        ),
                    )
            except Exception:
                pass

        self.image = self.orig_image
        self.image_item = self.canvas.create_image(
            0, 0, anchor="center", image=self.image, tags=("img", "avatar_image")
        )

        # Topmost UI (context menu + canvas overlay buttons)
        self.menu = tk.Menu(self.root, tearoff=0)
        self.menu.add_command(label="Pinned (Always on top)", command=lambda: self.set_pinned(True, "menu_pinned"))
        self.menu.add_command(label="Normal", command=lambda: self.set_pinned(False, "menu_normal"))
        self.menu.add_separator()
        self.menu.add_command(label="Fit: Contain", command=lambda: self.set_fit_mode("contain", "menu"))
        self.menu.add_command(label="Fit: Cover", command=lambda: self.set_fit_mode("cover", "menu"))
        self.menu.add_separator()
        self.menu.add_command(label="Quit", command=lambda: self.quit_app("menu"))
        self.canvas.bind("<Button-3>", self.show_context_menu)
        self.draw_overlay_controls()

        # Overlay controls (PIN/FIT/X)
        self.draw_overlay_controls()

        # Bind drag move
        self.canvas.bind("<ButtonPress-1>", self.start_move)
        self.canvas.bind("<B1-Motion>", self.do_move)
        self.canvas.bind("<ButtonRelease-1>", self.end_move)
        self.canvas.bind("<Button-1>", self.focus_window)  # ensure focus for key events
        # Topmost toggling is UI-driven (button / right-click menu); keyboardトグルは無効化
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
                pinned_val = bool(self.cfg.get("pinned", self.cfg.get("topmost", True)))
                self.root.attributes("-topmost", pinned_val)
        except Exception as e:
            set_feature_ctx("config")
            get_logger("config").error(
                "config.get.failed",
                extra=log_extra("config", "config.get.failed", request_id=ensure_request_id(), error=str(e)),
            )

        # Final sanity to make the window obvious on screen.
        self.root.update_idletasks()
        self.apply_fit(force=True)
        self.root.deiconify()
        # ensure key events are captured
        try:
            self.root.focus_force()
            self.root.focus_set()
            self.canvas.focus_set()
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
        # Ignore drag start on overlay buttons
        hits = self.canvas.find_overlapping(event.x, event.y, event.x, event.y)
        for h in hits:
            if "ui_overlay" in self.canvas.gettags(h):
                self._overlay_hit = True
                return
        self._drag_data["x"] = event.x
        self._drag_data["y"] = event.y
        self._drag_data["dragging"] = True

    def do_move(self, event):
        if self._overlay_hit:
            return
        dx = event.x - self._drag_data["x"]
        dy = event.y - self._drag_data["y"]
        x = int(self.root.winfo_x() + dx)
        y = int(self.root.winfo_y() + dy)
        self.root.geometry(f"+{x}+{y}")

    def end_move(self, event=None):
        self._overlay_hit = False
        if not self._drag_data.get("dragging"):
            return
        self._drag_data["dragging"] = False
        x = int(self.root.winfo_x())
        y = int(self.root.winfo_y())
        w = int(self.root.winfo_width())
        h = int(self.root.winfo_height())
        if self.virtual_bounds:
            clamped_x, clamped_y = clamp_position(x, y, w, h, self.virtual_bounds)
            if (clamped_x, clamped_y) != (x, y):
                set_feature_ctx("window")
                get_logger("window").info(
                    "window.clamp",
                    extra=log_extra(
                        "window",
                        "window.clamp",
                        request_id=ensure_request_id(),
                        from_x=x,
                        from_y=y,
                        to_x=clamped_x,
                        to_y=clamped_y,
                        w=w,
                        h=h,
                    ),
                )
                x, y = clamped_x, clamped_y
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

    def set_pinned(self, enabled: bool, reason: str):
        """Idempotent setter; logs/IPC only when state changes."""
        if self._pinned_op_inflight:
            return
        self._pinned_op_inflight = True
        ui_req_id = f"req-{uuid.uuid4()}"
        try:
            # Always log the UI intent
            set_feature_ctx("window")
            get_logger("window").info(
                "window.pinned_ui_action",
                extra=log_extra(
                    "window",
                    "window.pinned_ui_action",
                    request_id=ui_req_id,
                    desired=enabled,
                    reason=reason,
                    current=bool(self.root.attributes("-topmost")),
                ),
            )

            current = bool(self.root.attributes("-topmost"))
            if enabled == current:
                return

            self.root.attributes("-topmost", enabled)
            try:
                self.root.lift()
                self.root.update_idletasks()
            except Exception:
                pass
            self.cfg["pinned"] = enabled
            req_id = f"req-{uuid.uuid4()}"
            set_feature_ctx("window")
            get_logger("window").info(
                "window.pinned_applied",
                extra=log_extra(
                    "window",
                    "window.pinned_applied",
                    request_id=req_id,
                    topmost=enabled,
                    topmost_applied=bool(self.root.attributes("-topmost")),
                    reason=reason,
                ),
            )
            save_window_config_remote(
                "pinned_toggle",
                {
                    "pinned": enabled,
                    "window_x": self.root.winfo_x(),
                    "window_y": self.root.winfo_y(),
                    "width": self.root.winfo_width(),
                    "height": self.root.winfo_height(),
                },
                request_id=req_id,
            )
            set_feature_ctx("ipc")
            get_logger("ipc").info(
                "ipc.config_set_sent",
                extra=log_extra(
                    "ipc",
                    "ipc.config_set_sent",
                    request_id=req_id,
                    reason="pinned_toggle",
                    topmost=enabled,
                ),
            )
        except Exception as e:
            set_feature_ctx("config")
            get_logger("config").warning(
                "config.set.failed",
                extra=log_extra("config", "config.set.failed", request_id=ui_req_id, error=str(e)),
            )
        finally:
            self._pinned_op_inflight = False

    def on_canvas_configure(self, event):
        # Throttle resize to avoid high cost during drag/resize
        if self._resize_after_id:
            self.root.after_cancel(self._resize_after_id)
        self._resize_after_id = self.root.after(120, self.apply_fit)

    def apply_fit(self, force: bool = False):
        """Resize image according to fit mode (contain/cover) and center it."""
        try:
            canvas_w = max(1, int(self.canvas.winfo_width()))
            canvas_h = max(1, int(self.canvas.winfo_height()))
            img_w = self.orig_image.width()
            img_h = self.orig_image.height()
            mode = self.cfg.get("image_fit_mode", "contain")
            if canvas_w <= 1 or canvas_h <= 1 or img_w <= 0 or img_h <= 0:
                return

            scale_x = canvas_w / img_w
            scale_y = canvas_h / img_h
            if mode == "cover":
                scale = max(scale_x, scale_y)
            else:
                scale = min(scale_x, scale_y)
            if scale == 0:
                return

            target_w = max(1, int(img_w * scale))
            target_h = max(1, int(img_h * scale))

            if not force and self._last_applied_size == (canvas_w, canvas_h, target_w, target_h, mode):
                return

            # Integer scale approximation with zoom/subsample
            scaled = self.orig_image
            if scale >= 1:
                zoom = max(1, int(round(scale)))
                scaled = scaled.zoom(zoom, zoom)
                # if overshoot, subsample back down
                if scaled.width() != target_w or scaled.height() != target_h:
                    down_x = max(1, int(round(scaled.width() / target_w)))
                    down_y = max(1, int(round(scaled.height() / target_h)))
                    scaled = scaled.subsample(down_x, down_y)
            else:
                sub = max(1, int(round(1 / scale)))
                scaled = scaled.subsample(sub, sub)
                # small adjust upward if still too small
                if scaled.width() < target_w or scaled.height() < target_h:
                    up_x = max(1, int(round(target_w / max(1, scaled.width()))))
                    up_y = max(1, int(round(target_h / max(1, scaled.height()))))
                    scaled = scaled.zoom(up_x, up_y)

            self.image = scaled
            self.canvas.delete("img")
            self.image_item = self.canvas.create_image(
                canvas_w // 2, canvas_h // 2, anchor="center", image=self.image, tags=("img", "avatar_image")
            )
            self._last_applied_size = (canvas_w, canvas_h, self.image.width(), self.image.height(), mode)
            self.canvas.tag_raise("ui_overlay")
            self.draw_overlay_controls()

            set_feature_ctx("assets")
            get_logger("assets").info(
                "assets.image_resized",
                extra=log_extra(
                    "assets",
                    "assets.image_resized",
                    request_id=ensure_request_id(),
                    mode=mode,
                    from_w=img_w,
                    from_h=img_h,
                    to_w=self.image.width(),
                    to_h=self.image.height(),
                    canvas_w=canvas_w,
                    canvas_h=canvas_h,
                ),
            )
            self._resize_after_id = None
        except Exception as e:
            set_feature_ctx("assets")
            get_logger("assets").warning(
                "assets.image_resized.failed",
                extra=log_extra("assets", "assets.image_resized.failed", request_id=ensure_request_id(), error=str(e)),
            )

    # --- Overlay controls ---
    def draw_overlay_controls(self):
        """Draw small clickable overlay buttons on the canvas."""
        try:
            for item in list(self._overlay_items):
                self.canvas.delete(item)
            self._overlay_items.clear()
        except Exception:
            pass
        pad = 4
        btn_w = 34
        btn_h = 20
        spacing = 2
        canvas_w = max(1, self.canvas.winfo_width())
        x = canvas_w - pad
        y = pad

        def add_btn(label, action_tag, cb):
            nonlocal x
            x0 = x - btn_w
            y0 = y
            x1 = x
            y1 = y + btn_h
            rect = self.canvas.create_rectangle(
                x0, y0, x1, y1, fill="#444", outline="#777", tags=("ui_overlay", action_tag)
            )
            text = self.canvas.create_text(
                x0 + btn_w / 2, y0 + btn_h / 2, text=label, fill="#fff", tags=("ui_overlay", action_tag)
            )
            self.canvas.tag_bind(rect, "<Button-1>", cb)
            self.canvas.tag_bind(text, "<Button-1>", cb)
            self._overlay_items.update([rect, text])
            x = x0 - spacing

        add_btn("X", "overlay_close", lambda e: self.on_overlay_click("quit"))
        fit_label = "FIT:C" if self.cfg.get("image_fit_mode", "contain") == "contain" else "FIT:K"
        add_btn(fit_label, "overlay_fit", lambda e: self.on_overlay_click("fit"))
        pin_label = "PIN" if bool(self.root.attributes("-topmost")) else "UNP"
        add_btn(pin_label, "overlay_pin", lambda e: self.on_overlay_click("pin"))
        self.canvas.tag_raise("ui_overlay")

    def on_overlay_click(self, action: str):
        """Handle overlay buttons; prevent drag start when overlay is clicked."""
        self._overlay_hit = True
        set_feature_ctx("window")
        get_logger("window").info(
            "window.ui_overlay_click",
            extra=log_extra("window", "window.ui_overlay_click", request_id=ensure_request_id(), action=action),
        )
        if action == "pin":
            self.set_pinned(not bool(self.root.attributes("-topmost")), "overlay")
        elif action == "fit":
            next_mode = "cover" if self.cfg.get("image_fit_mode", "contain") == "contain" else "contain"
            self.set_fit_mode(next_mode, "overlay")
        elif action == "quit":
            set_feature_ctx("window")
            get_logger("window").info(
                "app.quit_requested",
                extra=log_extra("window", "app.quit_requested", request_id=ensure_request_id(), source="overlay"),
            )
            self.close()
        self.root.after(50, self._reset_overlay_hit)

    def _reset_overlay_hit(self):
        self._overlay_hit = False

    def set_fit_mode(self, mode: str, source: str):
        mode = "cover" if mode == "cover" else "contain"
        current = self.cfg.get("image_fit_mode", "contain")
        if mode == current:
            return
        old = current
        self.cfg["image_fit_mode"] = mode
        set_feature_ctx("assets")
        get_logger("assets").info(
            "assets.fit_mode_changed",
            extra=log_extra(
                "assets",
                "assets.fit_mode_changed",
                request_id=ensure_request_id(),
                source=source,
                from_mode=old,
                to_mode=mode,
            ),
        )
        save_window_config_remote(
            "fit_mode_change",
            {"image_fit_mode": mode, "pinned": bool(self.root.attributes("-topmost"))},
        )
        self.draw_overlay_controls()
        self.apply_fit(force=True)

    def show_context_menu(self, event):
        try:
            self.menu.tk_popup(event.x_root, event.y_root)
        finally:
            self.menu.grab_release()

    def focus_window(self, event=None):
        try:
            self.root.focus_force()
        except Exception:
            pass

    def quit_app(self, source: str):
        set_feature_ctx("window")
        get_logger("window").info(
            "app.quit_requested",
            extra=log_extra("window", "app.quit_requested", request_id=ensure_request_id(), source=source),
        )
        self.close()

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
    warn_if_non_ascii_path()
    ensure_dirs()
    app = ShellApp()
    app.run()


if __name__ == "__main__":
    main()
