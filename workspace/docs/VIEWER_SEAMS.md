# VIEWER_SEAMS

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-01-01
- Scope: Diagnose and mitigate seam artifacts (face center lines, hair edges).

## Symptoms
- Visible face center line or texture seam in normal viewing sizes.
- Seams reduce when zoomed in but remain at normal distance.

## Viewer-side Diagnostics (Quick)
- `diag=solid`: renders a flat color. If the seam remains, shading/mesh (D) is likely.
- `diag=nearest`: nearest filter for seam-sensitive textures. If seam reduces, texture bleed (A) is likely.
- `diag=mipmap_on`: if seams change mainly when zoomed out, mipmaps (B) are likely.
- `diag=premul` / `alphaBleed`: if seams reduce with alpha settings, alpha bleed (C) is likely.

## Viewer-side Mitigations
- `seamFix.bias` / `repeatMin` for UV bleed control.
- `alphaTest` for translucent materials.
- `premultiplyAlpha` / `alphaBleed` for textures with alpha fringes.

These reduce the seam but do not always remove it.

## When Material-Side Fix Is Required
If seams remain across all presets, the source asset likely needs correction:
- Add padding/bleed around UV islands (2-8 px) in texture atlases.
- Fill transparent pixels with nearby opaque colors (alpha bleed).
- Adjust mesh smoothing groups / normals if shading seam persists.

## Related Docs
- `docs/MANIFEST.md`
- `docs/MOTIONS_DEMO.md`
- `docs/PATHS.md`
