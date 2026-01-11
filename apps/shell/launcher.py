"""
MascotDesktop Unified Launcher

将来Core+Avatarを統合起動するためのランチャー構造。
現時点ではAvatarのみ起動。
"""

import argparse
import logging
import os
import signal
import subprocess
import sys
import time
from pathlib import Path

# Workspace root
WS_ROOT = Path(__file__).resolve().parent.parent.parent
LOG_DIR = WS_ROOT / "logs"


def setup_logging():
    """Setup launcher logging."""
    LOG_DIR.mkdir(parents=True, exist_ok=True)
    log_file = LOG_DIR / "launcher.log"
    
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.FileHandler(log_file, encoding="utf-8"),
            logging.StreamHandler(),
        ],
    )
    return logging.getLogger("launcher")


def start_avatar(args, logger):
    """Start Avatar server as subprocess."""
    avatar_script = WS_ROOT / "apps" / "avatar" / "poc" / "poc_avatar_mmd_viewer.py"
    
    if not avatar_script.exists():
        logger.error(f"Avatar script not found: {avatar_script}")
        return None
    
    # Build command
    python_exe = sys.executable
    cmd = [python_exe, str(avatar_script)]
    
    if args.model:
        cmd.extend(["--model", args.model])
    elif args.slug:
        cmd.extend(["--slug", args.slug])
    
    if args.no_browser:
        cmd.append("--no-browser")
    
    if args.port:
        cmd.extend(["--port", str(args.port)])
    
    logger.info(f"Starting Avatar: {' '.join(cmd)}")
    
    # Start process
    log_file = LOG_DIR / "avatar_launcher.log"
    with open(log_file, "a", encoding="utf-8") as log_out:
        proc = subprocess.Popen(
            cmd,
            cwd=str(WS_ROOT),
            stdout=log_out,
            stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0,
        )
    
    logger.info(f"Avatar PID: {proc.pid}")
    return proc


def stop_process(proc, logger, name="process"):
    """Stop a subprocess gracefully."""
    if proc is None:
        return
    
    if proc.poll() is not None:
        logger.info(f"{name} already stopped (exit code: {proc.returncode})")
        return
    
    logger.info(f"Stopping {name} (PID {proc.pid})...")
    
    try:
        if os.name == "nt":
            # Windows: send CTRL_BREAK_EVENT
            proc.send_signal(signal.CTRL_BREAK_EVENT)
        else:
            proc.terminate()
        
        # Wait up to 5 seconds
        try:
            proc.wait(timeout=5)
            logger.info(f"{name} stopped gracefully")
        except subprocess.TimeoutExpired:
            logger.warning(f"{name} did not stop, killing...")
            proc.kill()
            proc.wait()
            logger.info(f"{name} killed")
    except Exception as e:
        logger.error(f"Error stopping {name}: {e}")


def main():
    parser = argparse.ArgumentParser(
        description="MascotDesktop Unified Launcher",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python launcher.py                       # Start with default model
  python launcher.py --slug kanata_official_v1
  python launcher.py --model data/assets_user/characters/my_char/mmd/model.pmx
  python launcher.py --no-browser          # Don't open browser
""",
    )
    parser.add_argument("--model", type=str, default=None,
                        help="Path to PMX model file (relative to workspace root)")
    parser.add_argument("--slug", type=str, default=None,
                        help="Character slug to load")
    parser.add_argument("--no-browser", action="store_true",
                        help="Do not open browser on startup")
    parser.add_argument("--port", type=int, default=8770,
                        help="Avatar server port (default: 8770)")
    args = parser.parse_args()

    logger = setup_logging()
    logger.info("=== MascotDesktop Launcher Starting ===")
    logger.info(f"Workspace: {WS_ROOT}")
    
    # Warn on non-ASCII path
    if any(ord(ch) > 127 for ch in str(WS_ROOT)):
        logger.warning(f"Non-ASCII path detected. Recommend using ASCII path.")
    
    # Start Avatar
    avatar_proc = start_avatar(args, logger)
    
    if avatar_proc is None:
        logger.error("Failed to start Avatar")
        return 1
    
    # Wait for user interrupt
    logger.info("")
    logger.info("=== Launcher Running ===")
    logger.info("Press Ctrl+C to stop")
    logger.info("")
    
    try:
        # Wait for Avatar process to exit or Ctrl+C
        while True:
            if avatar_proc.poll() is not None:
                logger.info(f"Avatar exited (code: {avatar_proc.returncode})")
                break
            time.sleep(0.5)
    except KeyboardInterrupt:
        logger.info("")
        logger.info("Interrupt received, stopping...")
    finally:
        stop_process(avatar_proc, logger, "Avatar")
        logger.info("=== Launcher Stopped ===")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
