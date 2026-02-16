# PMX Rendering Comprehensive Review (2026-02-09)

## Scope
- Target: PMX model rendering quality issue (pixelated/washed appearance) in `MascotDesktop` runtime.
- Evidence source: repository code + provided runtime logs/screenshot + Unity official docs.

## Findings (ordered by severity)

### 1) [High] Global texture downscale to 1024 is hardcoded and always applied
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:52` (`DefaultMaxTextureSize = 1024`)
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:365` (`new TextureLoader(relativePath, DefaultMaxTextureSize)`)
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:272`-`285` (`RescaleLargeTexture`)
- Why this matters:
  - All runtime-loaded textures over 1024 are forcibly reduced, degrading texture detail.
  - The resize target uses per-axis `Math.Min(...)`, which can break aspect ratio on non-square textures.

### 2) [High] Shader still contains explicit debug overrides that change final look
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:55` (`_AmbColor * 0.5`)
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:58`-`59` (toon multiplication disabled)
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:89`-`91` (sphere map contribution disabled)
- Why this matters:
  - These changes can make materials flatter/incorrect even if texture loading is correct.

### 3) [Medium] TGA loading path contains debug-first PNG override
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:130`-`136`
- Why this matters:
  - If a same-name `.png` exists, loader prefers PNG over TGA silently.
  - This can produce inconsistent visuals across environments and hide TGA-path regressions.

### 4) [Medium] Large-volume unguarded debug logs in hot path
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:46,105,134,150,154,263`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs:1032,1035,1046,1056`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:52,56`
- Why this matters:
  - Model load emits extremely large logs, increasing CPU/IO overhead and making incident triage harder.

### 5) [Medium] Transparency check scans full texture pixels repeatedly per material
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:29`-`30`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:174`-`179`
- Why this matters:
  - `GetPixels()` + full scan on large textures is expensive and repeated for shared textures.
  - Startup/model-switch latency can increase with model complexity.

## Confirmed / Ruled-out possibilities

### A) 24-bit TGA BGR->RGB path is present and exercised
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs:1050`-`1059`
  - Runtime log includes `pixelDepth=24` and `TGA 24-bit first color` output.
- Interpretation:
  - 24-bit conversion path is no longer the primary failure point.

### B) QualitySettings global mip limit is unlikely primary cause in this run
- Repository evidence:
  - `Unity_PJ/project/ProjectSettings/QualitySettings.asset:7` (`m_CurrentQuality: 5`)
  - `Unity_PJ/project/ProjectSettings/QualitySettings.asset:287` (`globalTextureMipmapLimit: 0` for active quality profile)
- Unity doc evidence:
  - `QualitySettings.globalTextureMipmapLimit` notes non-persistent runtime textures are not affected.
  - Source: https://docs.unity3d.com/cn/2023.2/ScriptReference/QualitySettings-globalTextureMipmapLimit.html
- Inference:
  - Hardcoded 1024 downscale and shader debug overrides are stronger causes than quality mip limit.

## External references (official)
- Unity `QualitySettings.globalTextureMipmapLimit`:
  - https://docs.unity3d.com/cn/2023.2/ScriptReference/QualitySettings-globalTextureMipmapLimit.html
- Unity `Screen.SetResolution`:
  - https://docs.unity3d.com/cn/2021.2/ScriptReference/Screen.SetResolution.html
- Unity texture import filter mode reference (default bilinear for imported textures):
  - https://docs.unity3d.com/es/2021.1/Manual/class-TextureImporter.html

## Suggested remediation order
1. Remove or gate debug shader modifications (restore toon/sphere path).
2. Make max texture size configurable; set default from 1024 to 2048/4096 or disable cap.
3. Fix rescale to preserve aspect ratio.
4. Remove debug PNG-first override or make it opt-in via debug flag.
5. Replace repeated full `GetPixels()` alpha scans with cached/material metadata strategy.

## Testability note
- Unity CLI execution failed in this environment (`Unity.exe` start failure: missing module), so runtime validation here is static review + provided logs only.
