# Cocoro Mascot Desktop (workspace)

- Git root is `workspace/` only. `refs/` lives alongside but is not under git.
- Target: Windows-only, local desktop AI mascot (chat + audio + memory + notifications + media).
- Do not place files at `<NEW_ROOT>/` except `workspace/` and `refs/`.
- `refs/` contains read-only reference artifacts copied from the old root (CocoroAI builds, prototype). `refs/` is outside git; do not edit. Use `scripts/setup/make_refs_readonly.ps1` optionally to mark them +R.
- Prototype reference copies for schema live in `data/refs/prototype/`; authoritative DDL/templates live in `data/templates/db/`.
- Keep secrets (API keys, SECRET_KEY, etc.) out of the repo; configs with secrets go to `secrets/` (gitignored) or env vars.

## Quick start (developer)

1) `powershell -ExecutionPolicy Bypass -File scripts/setup/bootstrap.ps1`
2) `powershell -ExecutionPolicy Bypass -File scripts/run.ps1 [-Mode split|single]`
   - Default `Mode` is `split` (core/avatarを別コンソールで起動)。`single`なら1コンソールでまとめて起動。
   - `/health` と `/avatar/health` OK まで待機後、既定ブラウザで `http://127.0.0.1:8770/viewer` を開きます。
   - 非ASCIIパスは WARN のみで動作継続しますが、ASCIIパス (例: `C:\\dev\\MascotDesktop\\workspace`) を推奨。

## Assets and motions

- No PMX/VMD/texture/audio assets are committed. Place your own under `data/assets_user/` (gitignored).
- Motion manifest template: `data/templates/motions/manifest.sample.json`. If manifest is absent, `idle.vmd` under the model directory is searched; otherwise `/avatar/load` returns `MOTION_NOT_FOUND`.

## Known visual limitation

- Face seams may persist depending on source textures/UVs. Viewer-side diagnostics are available; see `docs/VIEWER_SEAMS.md`. Material-side fixes (padding/bleed) are likely required for full removal.
- More details: `docs/05-dev/QUICKSTART.md`, `docs/NEXT_TASKS.md`, `docs/VIEWER_SEAMS.md`.
