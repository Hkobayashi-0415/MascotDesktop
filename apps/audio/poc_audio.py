import argparse
import json
import sys


def handle_tts(req):
    req_id = req.get("request_id", "req-unknown")
    text = req.get("text") or req.get("tts_text") or ""
    return {
        "dto_version": req.get("dto_version", "0.1.0"),
        "request_id": req_id,
        "status": "ok",
        "played": True,
        "text_echo": text,
    }


def main():
    parser = argparse.ArgumentParser(description="Audio PoC mock")
    parser.add_argument("--input", "-i", help="Path to TTS request JSON. If omitted, read stdin.")
    args = parser.parse_args()
    if args.input:
        with open(args.input, "r", encoding="utf-8") as f:
            req = json.load(f)
    else:
        req = json.load(sys.stdin)
    resp = handle_tts(req)
    json.dump(resp, sys.stdout, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()
