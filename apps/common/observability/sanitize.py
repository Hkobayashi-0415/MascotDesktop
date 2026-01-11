"""
Utilities to keep sensitive data out of logs.
"""

from typing import Any, Dict


def summarize_payload(payload: Dict[str, Any]) -> Dict[str, Any]:
    """Return safe metadata (keys + size), not full payload."""
    try:
        keys = list(payload.keys())
    except Exception:
        keys = []
    return {
        "keys": keys,
        "size_bytes": len(str(payload).encode("utf-8")),
    }


def summarize_message(message: Dict[str, Any]) -> Dict[str, Any]:
    """For chat messages: keep role + content length only."""
    role = message.get("role")
    content = message.get("content", "") if isinstance(message, dict) else ""
    return {"role": role, "content_length": len(str(content))}
