# QUICKSTART

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-01-01
- Scope: Minimal local startup (bootstrap -> run).

## Steps

1) Move to workspace:

```powershell
Set-Location C:\dev\MascotDesktop\workspace
```

2) Bootstrap (venv + deps):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/setup/bootstrap.ps1
```

3) Run (core + avatar + viewer):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/run.ps1
```

## Expected
- Core health: `http://127.0.0.1:8765/health`
- Avatar health: `http://127.0.0.1:8770/avatar/health`
- Viewer opens: `http://127.0.0.1:8770/viewer`

## Notes
- Non-ASCII paths produce a WARN but continue. See `docs/PATHS.md` for ASCII migration.
- Assets are not committed. Place PMX/VMD/texture under `data/assets_user/` (gitignored).
- For slot-based motion demo, see `docs/MOTIONS_DEMO.md`.
