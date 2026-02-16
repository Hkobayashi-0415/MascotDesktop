# Asset Layout for Unity_PJ

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Canonical local asset layout for MMD and motion resources.

## Canonical Asset Root
- `Unity_PJ/data/assets_user/characters/<slug>/mmd/model.pmx`
- `Unity_PJ/data/assets_user/characters/<slug>/mmd/manifest.json`
- `Unity_PJ/data/assets_user/characters/<slug>/motions/*.vmd`
- `Unity_PJ/data/assets_user/characters/<slug>/textures/*`

## Transition Compatibility
- Temporary source path allowed: `Old_PJ/workspace/data/assets_user/`
- Compatibility is for migration only.
- New assets should be placed under `Unity_PJ/data/assets_user/`.

## Policy
- Binary assets are local-only and gitignored.
- Runtime path resolution uses project-relative paths.
- ASCII-safe paths are strongly recommended.

## Cutover Rule
- After cutover, runtime must not access `Old_PJ/workspace/data/assets_user/`.

