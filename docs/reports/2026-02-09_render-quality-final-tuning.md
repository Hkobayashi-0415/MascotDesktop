# Report: Render Quality Final Tuning (PMX roughness)

- Date: 2026-02-09
- Scope: Runtime rendering quality stabilization for PMX display (no asset/model replacement)

## Context

The PMX model load path is now successful (`avatar.model.displayed`), but final visual output still appears rough/pixelated in runtime screenshots.

## Root Cause Hypothesis

- Rendering quality depended on runtime defaults and scene camera state.
- `SimpleModelBootstrap` only configured camera values when `Camera.main` was missing, so existing scene camera quality was not normalized.
- Runtime quality knobs (`mipmap limit`, `AA`, `anisotropic`) were not explicitly enforced at startup.
- Model framing left relatively low on-screen pixel coverage for facial detail.

## Changes Applied

### 1) Runtime quality enforcement
File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

- Added `ApplyRuntimeRenderQuality()` and invoked it in startup.
- Forced high-quality runtime settings:
  - `QualitySettings.masterTextureLimit = 0`
  - `QualitySettings.maximumLODLevel = 0`
  - `QualitySettings.lodBias >= 2`
  - `QualitySettings.anisotropicFiltering = ForceEnable`
  - `QualitySettings.antiAliasing >= 4`
  - `QualitySettings.streamingMipmapsActive = false`
- Added runtime trace event: `avatar.render.quality.applied`

### 2) Camera configuration normalization
File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

- Camera is now resolved as:
  1. `Camera.main`
  2. any existing `Camera`
  3. create `SimpleModelCamera` if none
- Added `ConfigureCamera()` to apply consistent camera settings:
  - solid background
  - FOV `24`
  - `allowMSAA = true`, `allowHDR = true`
  - `allowDynamicResolution = false`
  - near/far clip tuning

### 3) Model framing adjustment
File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

- Increased model target height from `1.8` to `2.4`.
- Adjusted target Z from `3.0` to `2.5`.
- Purpose: increase on-screen effective detail (less apparent roughness at same output resolution).

## What Was NOT Changed

- No default MMD path/model switch in this patch.
- No texture file replacement.
- No shader pipeline swap.

## Validation Plan (Unity side)

1. Start Play mode and confirm log event `avatar.render.quality.applied` appears once on startup.
2. Verify PMX still reaches `avatar.model.displayed`.
3. Compare before/after for:
   - face detail visibility
   - edge jaggies
   - overall sharpness while idle/happy/sleepy motions
4. If still rough, capture:
   - Game view resolution value
   - Quality level name
   - new startup logs

## Rollback

- Revert only:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

## Follow-up (2026-02-09 11:18 UTC): model clipping fix

Symptom after phase: model was vertically clipped (upper body/head out of frame).

Root cause:
- Model placement used ground alignment (`-bounds.min.y`), while camera stayed at world Y=0.
- With increased model size, top reached outside camera vertical frustum.

Patch:
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Changed vertical placement from ground-based to center-based alignment.
  - before: `y = -bounds.min.y`
  - after: `y = -bounds.center.y`

Expected result:
- Full body remains inside frame, making quality comparison possible.

## Follow-up (2026-02-09 11:28 UTC): slight overexposure fix

Symptom:
- Model looked slightly blown out (whitish highlights).

Patch:
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Lighting/camera tuning for stable LDR look:
  - `camera.allowHDR = false`
  - key directional light intensity `1.0 -> 0.72`
  - ambient lighting fixed to Flat mode with low ambient color
  - existing scene light is also normalized (not only newly-created light)

Expected result:
- Reduced highlight clipping / over-brightness while keeping detail readability.

## Follow-up (2026-02-09 11:46 UTC): shader-side overexposure suppression

Symptom:
- Slight washout remained after camera/light tuning.

Root cause inference:
- In PMD shader, additive sphere map and high ambient contribution could push material color beyond intended range before final output.

Patch:
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- Reduced ambient term influence in lighting:
  - `(_AmbColor * 0.35) + (_Color * _LightColor0)`
- Reduced additive sphere contribution:
  - `o.Custom += sphereAdd * 0.35`
- Added custom color clamp after sphere composition:
  - `o.Custom = saturate(o.Custom)`

Expected result:
- Less blown highlights, better preservation of texture contrast.

## Follow-up (2026-02-09 11:51 UTC): additional brightness trim

Symptom:
- Image still slightly over-bright after previous fix.

Patch:
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - key light: `0.72 -> 0.58`
  - ambient light: `(0.16,0.16,0.18) -> (0.10,0.10,0.12)`
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
  - ambient factor: `0.35 -> 0.25`
  - sphereAdd factor: `0.35 -> 0.20`
  - final rgb multiplier: `* 0.90`

Expected result:
- Slightly darker, less washed output while keeping texture detail.
