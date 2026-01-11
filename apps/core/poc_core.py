import argparse
import json
import sys


def handle_chat(req):
    req_id = req.get("request_id", "req-unknown")
    return {
        "dto_version": req.get("dto_version", "0.1.0"),
        "request_id": req_id,
        "status": "ok",
        "message": {
            "role": "assistant",
            "content": "おはようございます！(mock)",
            "audio": {
                "used_clip": False,
                "clip_category": None,
                "tts_model": "mock",
                "audio_url": None,
            },
            "avatar_state": "02_smile",
        },
        "usage": {"prompt_tokens": 0, "completion_tokens": 0},
    }


def main():
    parser = argparse.ArgumentParser(description="Core PoC mock")
    parser.add_argument("--input", "-i", help="Path to ChatSendRequest JSON. If omitted, read stdin.")
    args = parser.parse_args()

    if args.input:
        with open(args.input, "r", encoding="utf-8") as f:
            req = json.load(f)
    else:
        req = json.load(sys.stdin)

    resp = handle_chat(req)
    json.dump(resp, sys.stdout, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()
