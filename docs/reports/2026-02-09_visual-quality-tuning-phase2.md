# Visual Quality Tuning Phase 2 (2026-02-09)

## Why
PMX was loading successfully, but displayed avatar appeared small and visually coarse in Game view.

## Changes
1. Explicit texture sampling defaults for runtime-loaded textures.
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- Added `ConfigureTextureSampling(Texture2D)` and applied it after load/rescale.
- Settings:
  - `filterMode = Trilinear`
  - `anisoLevel = 8`

2. Increased camera magnification for better on-screen detail.
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- In auto-created camera:
  - `fieldOfView = 30f` (from Unity default perspective ~60f)
  - `allowMSAA = true`

## Expected effect
- Model occupies larger area on screen (less pixelated look from low on-screen pixel count).
- Texture minification quality should improve via trilinear + anisotropic filtering.

## What to verify in Unity
1. Play Mode screenshot comparison before/after (same window size).
2. Confirm model appears larger in frame and face/hair details are less blocky.
3. If still too small, tune either:
   - camera FOV (e.g. 25-35), or
   - `NormalizeLoadedModelTransform` target depth/height.
