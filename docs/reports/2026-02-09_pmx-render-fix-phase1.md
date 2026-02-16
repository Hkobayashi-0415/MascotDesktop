# PMX Render Fix Phase 1 (2026-02-09)

## Goal
Address the highest-confidence causes of PMX visual degradation identified in the comprehensive review.

## Implemented fixes

1. Increased runtime texture cap from `1024` to `4096`.
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:52`
- Rationale: prevent unnecessary downscale of 2K textures.

2. Fixed resize math to preserve aspect ratio when downscaling is needed.
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:298`
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:303`
- Rationale: avoid independent width/height clamping distortion.

3. Disabled debug-first `.tga -> .png` override by default.
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:15`
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:135`
- Rationale: restore deterministic TGA path and avoid hidden substitutions.

4. Restored shader contributions previously disabled for debugging.
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:54`
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:57`
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:87`
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:88`
- Rationale: recover intended toon + sphere-map appearance.

5. Reduced verbose logging in hot paths.
- Files:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs`
- Rationale: avoid large per-texture/per-material debug spam under normal runs.

6. Added transparency-check cache per texture instance.
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:24`
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:172`
- Rationale: avoid repeated full-pixel scans for shared textures.

## Validation
- Static checks: completed (line-level verification for all modified paths).
- Runtime/EditMode tests: blocked by environment.
  - Unity CLI execution fails to start in this shell (`Unity.exe` module missing error).

## Risk notes
- Raising cap to 4096 may increase GPU/CPU memory usage for very large models.
- Visual output will intentionally change because toon/sphere contributions are now active again.

## Recommended next checks (when Unity runtime is available)
1. Compare before/after screenshots of the same PMX and camera settings.
2. Confirm texture dimensions after load in play mode.
3. Measure startup/load time for one large PMX set.
4. If memory pressure appears, tune cap via configurable setting in runtime config.
