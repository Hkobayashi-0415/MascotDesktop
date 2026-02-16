# Bullet Plugin Duplicate / DllNotFound Fix (2026-02-09)

## Symptom
- Unity warning:
  - `Multiple plugins with the same name 'libbulletc' ... Native/x64/libbulletc.dll and Native/UWP.disabled/x64/libbulletc.dll`
- Runtime failure:
  - `DllNotFoundException: libbulletc`
  - PMX load failed and fallback primitive was shown.

## Root cause
- `libbulletc.dll.meta` files under BulletUnity Native folders had only GUID lines (no explicit `PluginImporter` platform settings).
- Unity treated both desktop and `UWP.disabled` variants as Editor-compatible candidates.

## Fixes applied

1. Added explicit `PluginImporter` settings to Bullet native DLL metas.
- Enabled only desktop x64 plugin for Editor and Win64 runtime.
- Disabled Editor compatibility for UWP-disabled copies.
- Files:
  - `Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x64/libbulletc.dll.meta`
  - `Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x86/libbulletc.dll.meta`
  - `Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x64/libbulletc.dll.meta`
  - `Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x86/libbulletc.dll.meta`
  - `Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/ARM/libbulletc.dll.meta`

2. Added runtime fallback when Bullet native init fails.
- `MmdGameObject` now catches `DllNotFoundException` (and init exceptions), disables Bullet physics, and continues model init.
- Also guarded `_physicsReactor.Reset()` calls to avoid null dereference.
- File:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`

## Expected behavior after refresh
- Duplicate plugin warning should disappear (or at least no longer reference both Editor-compatible x64 variants).
- If native load still fails, PMX should no longer hard-fail solely due Bullet init; model should continue with `PhysicsMode=None`.

## Verification checklist for Unity side
1. Reimport affected plugin assets (`BulletUnity/Native/**/libbulletc.dll`).
2. Enter Play Mode and check Console:
   - No duplicate `libbulletc` warning.
   - No `DllNotFoundException: libbulletc` during PMX load.
3. Confirm PMX displays (not fallback capsule).
