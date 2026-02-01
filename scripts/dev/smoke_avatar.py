#!/usr/bin/env python3
"""
smoke_avatar.py - Avatar API smoke test for slot-based motion switching.

Usage:
    python scripts/dev/smoke_avatar.py --model "data/assets_user/characters/miku/mmd/model.pmx" --slots idle,speaking --loops 10 --delay-sec 0.5

Options:
    --report-json <path>    Output JSON summary to file
    --random                Shuffle slot order each loop
    --jitter-sec <float>    Random jitter added to delay (0 to jitter-sec)

This script:
1. Loads the specified model via /avatar/load
2. Iterates through slots, calling /avatar/play for each
3. Repeats for N loops
4. Returns exit code 0 if all succeed, non-zero on failure
"""

import argparse
import json
import random as rand_module
import sys
import time
import urllib.request
import urllib.error
from datetime import datetime


DEFAULT_BASE_URL = "http://127.0.0.1:8770"


def log(msg: str, level: str = "INFO"):
    """Simple logger."""
    print(f"[{level}] {msg}", flush=True)


def call_api(base_url: str, path: str, method: str = "GET", body: dict | None = None) -> tuple[int, dict | None]:
    """Call Avatar API and return (status_code, response_json or None)."""
    url = f"{base_url}{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    headers = {"Content-Type": "application/json"} if body else {}
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=10) as resp:
            status = resp.status
            text = resp.read().decode("utf-8")
            return status, json.loads(text) if text else {}
    except urllib.error.HTTPError as e:
        text = e.read().decode("utf-8") if e.fp else ""
        try:
            return e.code, json.loads(text)
        except Exception:
            return e.code, {"raw": text}
    except Exception as ex:
        log(f"Request failed: {ex}", "ERROR")
        return 0, {"exception": str(ex)}


def check_health(base_url: str) -> bool:
    """Check /avatar/health returns 200."""
    status, _ = call_api(base_url, "/avatar/health")
    return status == 200


def load_model(base_url: str, model_path: str, request_id: str) -> tuple[bool, str, dict | None]:
    """Load model via /avatar/load. Returns (success, error_message, response)."""
    body = {
        "dto_version": "0.1.0",
        "request_id": request_id,
        "model_path": model_path,
    }
    status, resp = call_api(base_url, "/avatar/load", method="POST", body=body)
    if status == 200:
        return True, "", resp
    error_code = resp.get("error_code", "UNKNOWN") if resp else "NO_RESPONSE"
    return False, f"status={status} error_code={error_code}", resp


def play_slot(base_url: str, slot: str, request_id: str) -> tuple[bool, str, dict | None]:
    """Play slot via /avatar/play. Returns (success, error_message, response)."""
    body = {
        "dto_version": "0.1.0",
        "request_id": request_id,
        "slot": slot,
    }
    status, resp = call_api(base_url, "/avatar/play", method="POST", body=body)
    if status == 200:
        return True, "", resp
    error_code = resp.get("error_code", "UNKNOWN") if resp else "NO_RESPONSE"
    return False, f"status={status} error_code={error_code}", resp


def get_state(base_url: str) -> dict | None:
    """Get /viewer/state."""
    status, resp = call_api(base_url, "/viewer/state")
    if status == 200:
        return resp
    return None


def run_smoke(
    base_url: str,
    model_path: str,
    slots: list[str],
    loops: int,
    delay_sec: float,
    jitter_sec: float = 0.0,
    randomize: bool = False,
) -> dict:
    """Run smoke test. Returns result dict."""
    result = {
        "started_at": datetime.now().isoformat(),
        "model_path": model_path,
        "slots": slots,
        "loops": loops,
        "delay_sec": delay_sec,
        "jitter_sec": jitter_sec,
        "randomize": randomize,
        "total_plays": 0,
        "failures": 0,
        "failure_details": [],
        "success": False,
    }

    log(f"Starting smoke: model={model_path} slots={slots} loops={loops} delay={delay_sec}s jitter={jitter_sec}s random={randomize}")

    # Health check
    if not check_health(base_url):
        log("Health check failed. Is server running?", "ERROR")
        result["failure_details"].append({"reason": "health_check_failed"})
        return result
    log("Health check passed")

    # Load model
    ok, err, resp = load_model(base_url, model_path, "smoke-load-1")
    if not ok:
        log(f"Model load failed: {err}", "ERROR")
        result["failure_details"].append({"reason": "model_load_failed", "error": err, "response": resp})
        return result
    log(f"Model loaded: {model_path}")

    for loop_i in range(loops):
        loop_slots = slots.copy()
        if randomize:
            rand_module.shuffle(loop_slots)

        for slot in loop_slots:
            req_id = f"smoke-play-{loop_i+1}-{slot}"
            ok, err, resp = play_slot(base_url, slot, req_id)
            result["total_plays"] += 1
            if not ok:
                log(f"FAIL loop={loop_i+1} slot={slot} {err}", "ERROR")
                result["failures"] += 1
                result["failure_details"].append({
                    "loop": loop_i + 1,
                    "slot": slot,
                    "error": err,
                    "response": resp,
                })
            else:
                state = get_state(base_url)
                current_slot = state.get("motion", {}).get("slot") if state else None
                log(f"OK   loop={loop_i+1} slot={slot} -> state.slot={current_slot}")

            # Delay with jitter
            actual_delay = delay_sec
            if jitter_sec > 0:
                actual_delay += rand_module.uniform(0, jitter_sec)
            time.sleep(actual_delay)

    result["finished_at"] = datetime.now().isoformat()
    result["success"] = result["failures"] == 0
    log(f"Smoke complete: {result['total_plays']} plays, {result['failures']} failures")
    return result


def main():
    parser = argparse.ArgumentParser(description="Avatar API smoke test (slot switching)")
    parser.add_argument("--model", required=True, help="Model path (relative to workspace)")
    parser.add_argument("--slots", default="idle", help="Comma-separated slot names")
    parser.add_argument("--loops", type=int, default=10, help="Number of loops")
    parser.add_argument("--delay-sec", type=float, default=0.5, help="Delay between plays (seconds)")
    parser.add_argument("--jitter-sec", type=float, default=0.0, help="Random jitter added to delay (0 to jitter-sec)")
    parser.add_argument("--random", action="store_true", help="Shuffle slot order each loop")
    parser.add_argument("--report-json", type=str, default=None, help="Output JSON summary to file")
    parser.add_argument("--base-url", default=DEFAULT_BASE_URL, help="Base URL for Avatar API")
    args = parser.parse_args()

    slots = [s.strip() for s in args.slots.split(",") if s.strip()]
    if not slots:
        log("No slots specified", "ERROR")
        sys.exit(1)

    result = run_smoke(
        args.base_url,
        args.model,
        slots,
        args.loops,
        args.delay_sec,
        jitter_sec=args.jitter_sec,
        randomize=args.random,
    )

    # Write JSON report if requested
    if args.report_json:
        try:
            with open(args.report_json, "w", encoding="utf-8") as f:
                json.dump(result, f, ensure_ascii=False, indent=2)
            log(f"Report written to: {args.report_json}")
        except Exception as e:
            log(f"Failed to write report: {e}", "ERROR")

    sys.exit(0 if result["success"] else 1)


if __name__ == "__main__":
    main()
