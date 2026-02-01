import json
import logging
import os
from logging.handlers import RotatingFileHandler
from pathlib import Path
from typing import Dict

from .context import get_context

FEATURES = [
    "ipc",
    "config",
    "window",
    "chat",
    "audio",
    "avatar",
    "assets",
    "memory",
    "reminders",
    "health",
    "security",
]


class JsonFormatter(logging.Formatter):
    def format(self, record: logging.LogRecord) -> str:  # type: ignore[override]
        base: Dict[str, object] = {
            "timestamp": self.formatTime(record, "%Y-%m-%dT%H:%M:%S"),
            "level": record.levelname,
            "message": record.getMessage(),
        }
        ctx = get_context()
        for k, v in ctx.items():
            base[k] = v
        # extra fields on the record
        for key in ("component", "feature", "event", "reason", "status_code", "duration_ms"):
            if hasattr(record, key):
                base[key] = getattr(record, key)
        return json.dumps(base, ensure_ascii=False)


_bootstrapped = False
_component = None
_loggers: Dict[str, logging.Logger] = {}


def bootstrap_logging(component: str, log_dir: Path) -> None:
    global _bootstrapped, _component
    if _bootstrapped:
        return
    _bootstrapped = True
    _component = component

    os.makedirs(log_dir, exist_ok=True)
    formatter = JsonFormatter()

    # Root console
    root = logging.getLogger()
    root.setLevel(logging.INFO)
    root.handlers.clear()
    stream_h = logging.StreamHandler()
    stream_h.setFormatter(formatter)
    root.addHandler(stream_h)

    # Feature-specific + component aggregate
    for feature in FEATURES + [component]:
        logger = logging.getLogger(f"{component}.{feature}")
        logger.setLevel(logging.INFO)
        logger.propagate = False
        os.makedirs(log_dir / feature, exist_ok=True) if feature not in FEATURES else None
        path = log_dir / (f"{feature}.log" if feature in FEATURES else f"{component}.log")
        handler = RotatingFileHandler(path, maxBytes=5 * 1024 * 1024, backupCount=5, encoding="utf-8")
        handler.setFormatter(formatter)
        logger.addHandler(handler)
        _loggers[feature] = logger


def get_logger(feature: str) -> logging.Logger:
    if feature in _loggers:
        return _loggers[feature]
    # fallback: component aggregate
    if _component:
        return logging.getLogger(f"{_component}.{feature}")
    return logging.getLogger(feature)


def log_extra(feature: str, event: str, **fields) -> Dict[str, object]:
    ctx = get_context()
    return {
        "feature": feature,
        "event": event,
        **fields,
        **{k: v for k, v in ctx.items()},
    }
