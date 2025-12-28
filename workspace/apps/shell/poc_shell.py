import json
import logging
import os
import sys
import tkinter as tk
from tkinter import ttk
from urllib import request


def workspace_root() -> str:
    here = os.path.abspath(os.path.dirname(__file__))
    return os.path.abspath(os.path.join(here, os.pardir, os.pardir))


WS_ROOT = workspace_root()
DATA_DIR = os.path.join(WS_ROOT, "data")
USER_DIR = os.path.join(DATA_DIR, "user")
CONFIG_PATH = os.path.join(USER_DIR, "config.json")
CORE_URL = "http://127.0.0.1:8765"
IMG_PATH = os.path.join(
    WS_ROOT, "data", "templates", "assets", "pngtuber_mode3", "states", "01_normal.gif"
)
PLACEHOLDER_SIZE = 320


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


def http_post(path, payload):
    url = CORE_URL + path
    data = json.dumps(payload).encode("utf-8")
    req = request.Request(url, data=data, headers={"Content-Type": "application/json"})
    with request.urlopen(req, timeout=5) as resp:
        return json.loads(resp.read().decode("utf-8"))


def http_get(path):
    url = CORE_URL + path
    with request.urlopen(url, timeout=5) as resp:
        return json.loads(resp.read().decode("utf-8"))


class ShellApp:
    def __init__(self):
        self.cfg = load_config()
        self.root = tk.Tk()
        self.root.title("Mascot PoC (Shell)")
        self.root.overrideredirect(True)
        self.root.attributes("-topmost", bool(self.cfg.get("topmost", True)))
        self.bg_color = "#222222"
        self.root.config(bg=self.bg_color)

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
            self.image = tk.PhotoImage(file=IMG_PATH)
            logging.info("Loaded image from %s", IMG_PATH)
        except tk.TclError as e:
            logging.warning("Image load failed (%s); using placeholder instead.", e)
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
        # Toggle topmost with key 't'; close with Esc
        self.root.bind("<KeyPress-t>", self.toggle_topmost)
        self.root.bind("<Escape>", self.close)

        self.root.protocol("WM_DELETE_WINDOW", self.close)

        # Health check + config sync
        self.health_ok = False
        try:
            health = http_get("/health")
            self.health_ok = health.get("status") == "ok"
            logging.info("Health: %s", health)
        except Exception as e:
            logging.error("Health check failed: %s", e)

        try:
            cfg_resp = http_post(
                "/v1/config/get",
                {"dto_version": "0.1.0", "request_id": "cfg-get-1"},
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
            logging.error("Config get failed: %s", e)

        # Final sanity to make the window obvious on screen.
        self.root.update_idletasks()
        self.root.deiconify()
        logging.info(
            "Window ready at %sx%s +%s+%s (topmost=%s)",
            self.root.winfo_width(),
            self.root.winfo_height(),
            self.root.winfo_x(),
            self.root.winfo_y(),
            bool(self.root.attributes("-topmost")),
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
        payload = {
            "dto_version": "0.1.0",
            "request_id": "cfg-set-pos",
            "entries": {"window_x": x, "window_y": y, "width": w, "height": h},
        }
        try:
            http_post("/v1/config/set", payload)
            logging.info("Drag end -> config.set once at (%s,%s) size=%sx%s", x, y, w, h)
        except Exception as e:
            logging.warning("Config set failed (release): %s", e)

    def toggle_topmost(self, event=None):
        current = bool(self.root.attributes("-topmost"))
        new_state = not current
        self.root.attributes("-topmost", new_state)
        self.cfg["topmost"] = new_state
        logging.info("Topmost toggled: %s", new_state)
        try:
            http_post(
                "/v1/config/set",
                {
                    "dto_version": "0.1.0",
                    "request_id": "cfg-set-topmost",
                    "entries": {"topmost": new_state},
                },
            )
        except Exception as e:
            logging.warning("Config set failed (topmost): %s", e)

    def close(self, event=None):
        # Save position/size
        self.cfg["window_x"] = self.root.winfo_x()
        self.cfg["window_y"] = self.root.winfo_y()
        self.cfg["width"] = self.root.winfo_width()
        self.cfg["height"] = self.root.winfo_height()
        save_config(self.cfg)
        self.root.destroy()

    def run(self):
        logging.info("Starting Shell PoC. Topmost=%s", self.cfg.get("topmost"))
        self.root.mainloop()


def main():
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[logging.StreamHandler(sys.stdout)],
    )
    ensure_dirs()
    app = ShellApp()
    app.run()


if __name__ == "__main__":
    main()
