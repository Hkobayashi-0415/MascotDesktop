# Render Investigation: Not Brightness-Only

- Date: 2026-02-09
- Scope: PMX表示の白っぽさ・質感低下の原因切り分け

## Conclusion
The issue is **not only brightness**. At least three structural contributors exist in shader/material pipeline.

## Findings

### High: Early saturation in lighting equation flattens tone
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:54`
- Current code saturates base color before texture/toon modulation.
- With bright diffuse+ambient inputs, highlight/headroom is lost early and details wash out.

### High: Sphere-map vector likely invalid due to normal source in `surf`
- File: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc:81`
- `o.Normal` is used to compute sphere UV before setting a custom normal; this can become unstable/constant.
- Result: additive sphere contribution can become visually incorrect, not just "too bright".

### Medium: Transparent shader selection is too aggressive
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:32`
- File: `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:172`
- Any pixel alpha `< 0.99` marks whole material as transparent.
- Anti-aliased texture edges can push opaque-looking materials into transparent pipeline, changing final look significantly.

### Medium: Project color space is Gamma
- File: `Unity_PJ/project/ProjectSettings/ProjectSettings.asset:50`
- `m_ActiveColorSpace: 0` (Gamma).
- Combined with this custom toon shader, Gamma can amplify washout-like perception.

## Recommended Next Fix Order
1. Remove early base saturation and move clamp to final stage only.
2. Rework sphere UV from valid normal/view data (not current `o.Normal` path).
3. Relax transparent classification threshold/strategy.
4. Evaluate Linear color space in a controlled branch.
