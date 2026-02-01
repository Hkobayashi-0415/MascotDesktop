"""
Simple request context utilities for correlation across logs.

Stores request_id/session_id/character_id/component/feature in a thread-local
dict so logging filters can enrich records. Intended for local PoC (single
threaded HTTP server).
"""

import threading
import uuid
from typing import Any, Dict, Optional

_local = threading.local()


def _ctx() -> Dict[str, Any]:
    if not hasattr(_local, "data"):
        _local.data = {}
    return _local.data  # type: ignore[attr-defined]


def set_context(
    request_id: Optional[str] = None,
    session_id: Optional[str] = None,
    character_id: Optional[str] = None,
    component: Optional[str] = None,
    feature: Optional[str] = None,
):
    ctx = _ctx()
    if request_id is not None:
        ctx["request_id"] = request_id
    if session_id is not None:
        ctx["session_id"] = session_id
    if character_id is not None:
        ctx["character_id"] = character_id
    if component is not None:
        ctx["component"] = component
    if feature is not None:
        ctx["feature"] = feature


def clear_context():
    _ctx().clear()


def get_context() -> Dict[str, Any]:
    return _ctx().copy()


def ensure_request_id() -> str:
    ctx = _ctx()
    if "request_id" not in ctx:
        ctx["request_id"] = f"req-{uuid.uuid4()}"
    return ctx["request_id"]
