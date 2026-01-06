# Motions Demo (Manifest + Slot)

This document shows how to use a manifest for slot-based motion playback
without committing any binary assets.

## Prerequisites
- Model and motions are placed under `data/assets_user` (git-ignored).
- Viewer running at `http://127.0.0.1:8770/viewer`.
- Avatar API running at `http://127.0.0.1:8770`.

## Working directory
Commands below assume you are at the workspace root (the Git root where
`apps/`, `data/`, `docs/` exist).

Example:
`Set-Location C:\dev\MascotDesktop\workspace`

## Placement rules
- Place the manifest next to the model file:
  `data/assets_user/characters/<slug>/mmd/manifest.json`
- Motion paths in the manifest are resolved **relative to the manifest**
  directory.

## Demo steps (PowerShell)
1) Copy the demo manifest template and edit paths:
   - Copy:
     `Copy-Item -Path .\data\templates\motions\manifest.demo.json -Destination .\data\assets_user\characters\<slug>\mmd\manifest.json`
   - Edit `manifest.json` and replace `<slug>` and the `path` values to match
     your local files (relative to the manifest directory).

2) Load model:
   `Invoke-RestMethod http://127.0.0.1:8770/avatar/load -Method Post -Body '{"dto_version":"0.1.0","request_id":"req-load-demo","model_path":"data/assets_user/characters/<slug>/mmd/model.pmx"}' -ContentType 'application/json'`

3) Play a slot:
   `Invoke-RestMethod http://127.0.0.1:8770/avatar/play -Method Post -Body '{"dto_version":"0.1.0","request_id":"req-play-idle","slot":"idle"}' -ContentType 'application/json'`

4) Confirm state:
   `Invoke-RestMethod http://127.0.0.1:8770/viewer/state -Method Get`

## Expected results
- `/viewer/state` returns `motion.motion_path` and `motion.slot`.
- Viewer overlay shows `Loaded: <model> + <motion>`.

## Fallback behavior (no manifest)
- If `manifest.json` is missing, the loader searches for `idle.vmd` in the
  model directory (`.../mmd/idle.vmd`).
- If `idle.vmd` is absent, the API returns `error_code: MOTION_NOT_FOUND`.

## Error codes
- `MANIFEST_NOT_FOUND`: manifest was not found or not readable.
- `MOTION_NOT_FOUND`: slot resolved to no motion and no fallback idle.
- `MODEL_NOT_FOUND`: model path does not exist or is blocked.

## Notes
- Do NOT commit `.pmx/.vmd/texture` assets. Keep them under `data/assets_user`.
