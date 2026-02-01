# Cocoro Mascot Desktop (workspace)

> [!IMPORTANT]
> **このREADMEは legacy です。最新情報は [`workspace/README.md`](workspace/README.md) を参照してください。**
> 
> **ドキュメントは `workspace/docs/` が正式版です。**
> - Documentation Index: [`workspace/docs/00-overview/documentation-index.md`](workspace/docs/00-overview/documentation-index.md)
> - Documentation Rules: [`workspace/docs/00-overview/documentation-rules.md`](workspace/docs/00-overview/documentation-rules.md)

- This repository is rooted at `workspace/` only. `refs/` lives alongside but is not under git.
- Target: Windows-only, local desktop AI mascot (chat + audio + memory + notifications + media).
- Do not place files at `<NEW_ROOT>/` except `workspace/` and `refs/`.
- `refs/` contains read-only reference artifacts copied from the old root (CocoroAI builds, prototype). `refs/` is outside git; do not edit. Use `scripts/setup/make_refs_readonly.ps1` optionally to mark them +R.
- Prototype reference copies for schema live in `data/refs/prototype/`; authoritative DDL/templates live in `data/templates/db/`.
- Git root is `workspace/`. Keep secrets (API keys, SECRET_KEY, etc.) out of the repo; configs with secrets go to `secrets/` (gitignored) or env vars.
