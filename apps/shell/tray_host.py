"""
MascotDesktop Tray Host

トレイ常駐でアバターウインドウを制御。
- トレイアイコン + メニュー
- WebViewでViewer埋め込み（枠なし・透過）
- 単一インスタンス制御
- Exit時の確実な子プロセス終了
"""

import atexit
import logging
import os
import signal
import subprocess
import sys
import threading
import time
from pathlib import Path

# Single instance lock
LOCK_FILE = None
LOCK_FD = None

# Workspace root
WS_ROOT = Path(__file__).resolve().parent.parent.parent
LOG_DIR = WS_ROOT / "logs"
ICON_PATH = WS_ROOT / "data" / "templates" / "assets" / "placeholders" / "pin_icon_24.png"

# State
avatar_process = None
webview_window = None
webview_started = threading.Event()


def setup_logging():
    """Setup launcher logging."""
    LOG_DIR.mkdir(parents=True, exist_ok=True)
    log_file = LOG_DIR / "tray_host.log"
    
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding="utf-8"),
            logging.StreamHandler() if os.environ.get("MASCOT_DEBUG") else logging.NullHandler(),
        ],
    )
    return logging.getLogger("tray_host")


def acquire_single_instance_lock():
    """Acquire single instance lock using a lock file."""
    global LOCK_FILE, LOCK_FD
    import msvcrt
    
    lock_path = LOG_DIR / "tray_host.lock"
    LOG_DIR.mkdir(parents=True, exist_ok=True)
    
    try:
        LOCK_FD = open(lock_path, "w")
        msvcrt.locking(LOCK_FD.fileno(), msvcrt.LK_NBLCK, 1)
        LOCK_FILE = lock_path
        return True
    except (IOError, OSError):
        return False


def release_single_instance_lock():
    """Release single instance lock."""
    global LOCK_FD, LOCK_FILE
    if LOCK_FD:
        try:
            import msvcrt
            msvcrt.locking(LOCK_FD.fileno(), msvcrt.LK_UNLCK, 1)
            LOCK_FD.close()
        except Exception:
            pass
        LOCK_FD = None
    if LOCK_FILE and LOCK_FILE.exists():
        try:
            LOCK_FILE.unlink()
        except Exception:
            pass
        LOCK_FILE = None


def start_avatar_server(logger):
    """Start Avatar HTTP server as subprocess."""
    global avatar_process
    
    avatar_script = WS_ROOT / "apps" / "avatar" / "poc" / "poc_avatar_mmd_viewer.py"
    
    if not avatar_script.exists():
        logger.error(f"Avatar script not found: {avatar_script}")
        return None
    
    python_exe = sys.executable
    cmd = [python_exe, str(avatar_script), "--no-browser", "--headless"]
    
    logger.info(f"Starting Avatar server: {' '.join(cmd)}")
    
    # Start without console window
    startupinfo = None
    if os.name == "nt":
        startupinfo = subprocess.STARTUPINFO()
        startupinfo.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startupinfo.wShowWindow = subprocess.SW_HIDE
    
    log_file = LOG_DIR / "avatar_tray.log"
    with open(log_file, "a", encoding="utf-8") as log_out:
        avatar_process = subprocess.Popen(
            cmd,
            cwd=str(WS_ROOT),
            stdout=log_out,
            stderr=subprocess.STDOUT,
            startupinfo=startupinfo,
            creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0,
        )
    
    logger.info(f"Avatar server PID: {avatar_process.pid}")
    return avatar_process


def stop_avatar_server(logger):
    """Stop Avatar server process."""
    global avatar_process
    if avatar_process is None:
        return
    
    if avatar_process.poll() is not None:
        logger.info(f"Avatar server already stopped (exit code: {avatar_process.returncode})")
        avatar_process = None
        return
    
    logger.info(f"Stopping Avatar server (PID {avatar_process.pid})...")
    
    try:
        if os.name == "nt":
            avatar_process.send_signal(signal.CTRL_BREAK_EVENT)
        else:
            avatar_process.terminate()
        
        try:
            avatar_process.wait(timeout=5)
            logger.info("Avatar server stopped gracefully")
        except subprocess.TimeoutExpired:
            logger.warning("Avatar server did not stop, killing...")
            avatar_process.kill()
            avatar_process.wait()
            logger.info("Avatar server killed")
    except Exception as e:
        logger.error(f"Error stopping Avatar server: {e}")
    
    avatar_process = None


def create_tray_icon(logger):
    """Create system tray icon with menu."""
    import pystray
    from PIL import Image, ImageDraw
    
    # Create icon programmatically (more reliable than loading file)
    icon_image = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(icon_image)
    # Draw a simple mascot-like circle
    draw.ellipse([8, 8, 56, 56], fill=(100, 150, 255, 255))
    draw.ellipse([20, 20, 32, 32], fill=(255, 255, 255, 255))  # left eye
    draw.ellipse([36, 20, 48, 32], fill=(255, 255, 255, 255))  # right eye
    draw.arc([20, 35, 44, 50], start=0, end=180, fill=(255, 255, 255, 255), width=2)  # smile
    
    def on_show(icon, item):
        logger.info("Menu: Show Avatar")
        show_viewer_window()
    
    def on_hide(icon, item):
        logger.info("Menu: Hide Avatar")
        hide_viewer_window()
    
    def on_reload(icon, item):
        logger.info("Menu: Reload")
        # TODO: Implement reload
        pass
    
    def on_open_logs(icon, item):
        logger.info("Menu: Open Logs")
        if os.name == "nt":
            os.startfile(str(LOG_DIR))
    
    def on_exit(icon, item):
        logger.info("Menu: Exit")
        icon.stop()
    
    menu = pystray.Menu(
        pystray.MenuItem("Show Avatar", on_show),
        pystray.MenuItem("Hide Avatar", on_hide),
        pystray.Menu.SEPARATOR,
        pystray.MenuItem("Reload", on_reload),
        pystray.MenuItem("Open Logs", on_open_logs),
        pystray.Menu.SEPARATOR,
        pystray.MenuItem("Exit", on_exit),
    )
    
    icon = pystray.Icon("MascotDesktop", icon_image, "MascotDesktop", menu)
    return icon


def show_viewer_window():
    """Show the viewer window."""
    global webview_window
    if webview_window:
        webview_window.show()


def hide_viewer_window():
    """Hide the viewer window."""
    global webview_window
    if webview_window:
        webview_window.hide()


def create_viewer_window(logger):
    """Create WebView window for viewer."""
    global webview_window
    import webview
    
    viewer_url = "http://127.0.0.1:8770/viewer"
    
    logger.info(f"Creating WebView window: {viewer_url}")
    
    webview_window = webview.create_window(
        "MascotDesktop",
        viewer_url,
        width=480,
        height=640,
        resizable=True,
        frameless=True,
        easy_drag=True,
        on_top=True,
        transparent=False,  # True requires special CSS
    )
    
    return webview_window


def wait_for_server(url, timeout=30):
    """Wait for HTTP server to be ready."""
    import urllib.request
    import urllib.error
    
    start = time.time()
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(url, timeout=2) as resp:
                if resp.status == 200:
                    return True
        except (urllib.error.URLError, OSError):
            pass
        time.sleep(0.5)
    return False


def run_webview(logger):
    """Run WebView in main thread (required by pywebview)."""
    import webview
    
    # Wait for Avatar server
    logger.info("Waiting for Avatar server...")
    if not wait_for_server("http://127.0.0.1:8770/avatar/health"):
        logger.error("Avatar server did not start in time")
        return
    
    logger.info("Avatar server ready")
    
    # Create window
    create_viewer_window(logger)
    webview_started.set()
    
    # Start WebView (blocks until all windows closed)
    webview.start()


def main():
    logger = setup_logging()
    logger.info("=== MascotDesktop Tray Host Starting ===")
    logger.info(f"Workspace: {WS_ROOT}")
    
    # Single instance check
    if not acquire_single_instance_lock():
        logger.error("Another instance is already running")
        print("MascotDesktop is already running.")
        return 1
    
    atexit.register(release_single_instance_lock)
    
    # Warn on non-ASCII path
    if any(ord(ch) > 127 for ch in str(WS_ROOT)):
        logger.warning("Non-ASCII path detected. Recommend using ASCII path.")
    
    # Start Avatar server
    start_avatar_server(logger)
    
    if avatar_process is None:
        logger.error("Failed to start Avatar server")
        return 1
    
    # Register cleanup
    def cleanup():
        logger.info("Cleanup...")
        stop_avatar_server(logger)
        release_single_instance_lock()
    
    atexit.register(cleanup)
    
    # Start tray icon in background thread
    tray_icon = create_tray_icon(logger)
    tray_thread = threading.Thread(target=tray_icon.run, daemon=True)
    tray_thread.start()
    
    logger.info("Tray icon started")
    
    # Run WebView in main thread
    try:
        run_webview(logger)
    except KeyboardInterrupt:
        logger.info("Interrupted")
    finally:
        tray_icon.stop()
        cleanup()
    
    logger.info("=== MascotDesktop Tray Host Stopped ===")
    return 0


if __name__ == "__main__":
    sys.exit(main())
