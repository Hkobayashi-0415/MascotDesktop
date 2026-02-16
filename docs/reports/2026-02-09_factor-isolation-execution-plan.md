# Factor Isolation Execution Plan (One Factor x All Models)

- Date: 2026-02-09
- Goal: 1つの要因を固定して全MMDを巡回し、比較結果を残した上で次要因へ進む。

## Implemented Runtime Controls

### Model switching (Play mode)
- Source: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Source: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- Added:
  - model catalog discovery from `Unity_PJ/data/assets_user`
  - `Model: prev`
  - `Model: next`
  - `Model: reload`
  - `Model: rescan`
- Candidate policy:
  - 3D models (`.pmx`, `.pmd`, `.vrm`) are included entirely.
  - Images (`.png`, `.jpg`, `.jpeg`, `.bmp`) are selected only:
    - grouped by top-level character key (`characters/<name>`)
    - sorted by visual-priority filename rank (`face` > `body/skin` > `hair` > `main/diffuse/albedo/base` > `tex` > others)
    - capped to up to 4 images per group.

### Factor switching (Play mode)
- Source: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Source: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- Added presets:
  - `F0 Baseline`
  - `F1 Lower Key Light`
  - `F2 Lower Light + Albedo95`
  - `F3 Lower Light + Albedo88`
- Added HUD controls:
  - `Factor: prev`
  - `Factor: next`
- Behavior:
  - factor change applies lighting preset
  - current model is reloaded automatically
  - effects are non-cumulative (常に現在factorで再読込)

## Current Model Candidates

- Measured on 2026-02-09 (`Unity_PJ/data/assets_user/characters`):
  - PMX: 8
  - selected images: 32 (max 4 per character group)
  - total model candidates: 40
- `Model: rescan` updates candidate list only and does not switch current model.

### PMX Targets (8)
1. `characters/amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx`
2. `characters/amane_kanata_v1/mmd/amane_kanata.pmx`
3. `characters/azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx`
4. `characters/momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx`
5. `characters/momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx`
6. `characters/nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx`
7. `characters/nurse_taso_v1/mmd/NurseTaso.pmx`
8. `characters/SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx`

## Recommended Execution Order

For each factor (F0 -> F1 -> F2 -> F3):
1. Enter Play mode.
2. Set factor with `Factor: prev/next`.
3. For all models with `Model: prev/next`:
   - switch model
   - confirm `avatar.model.displayed`
   - capture screenshot
   - record washout / skin tone / texture contrast / edge quality
4. After all models are checked, move to next factor.

Notes:
- モデル切替時は同一factorのまま比較する。
- factor変更時のみ全モデルを再巡回する。

## Evaluation Sheet

| Factor | Model | White-out | Skin Tone | Contrast | Notes |
|---|---|---|---|---|---|
| F0 | amane_kanata_official_v1 |  |  |  |  |
| F0 | amane_kanata_v1 |  |  |  |  |
| F0 | azki_v1 |  |  |  |  |
| F0 | momone_nene_official_v1_BEA |  |  |  |  |
| F0 | momone_nene_official_v1_STD |  |  |  |  |
| F0 | nurse_taso_v1_src |  |  |  |  |
| F0 | nurse_taso_v1 |  |  |  |  |
| F0 | SakamataChloe_v1 |  |  |  |  |
| F1 | amane_kanata_official_v1 |  |  |  |  |
| F1 | amane_kanata_v1 |  |  |  |  |
| F1 | azki_v1 |  |  |  |  |
| F1 | momone_nene_official_v1_BEA |  |  |  |  |
| F1 | momone_nene_official_v1_STD |  |  |  |  |
| F1 | nurse_taso_v1_src |  |  |  |  |
| F1 | nurse_taso_v1 |  |  |  |  |
| F1 | SakamataChloe_v1 |  |  |  |  |
| F2 | amane_kanata_official_v1 |  |  |  |  |
| F2 | amane_kanata_v1 |  |  |  |  |
| F2 | azki_v1 |  |  |  |  |
| F2 | momone_nene_official_v1_BEA |  |  |  |  |
| F2 | momone_nene_official_v1_STD |  |  |  |  |
| F2 | nurse_taso_v1_src |  |  |  |  |
| F2 | nurse_taso_v1 |  |  |  |  |
| F2 | SakamataChloe_v1 |  |  |  |  |
| F3 | amane_kanata_official_v1 |  |  |  |  |
| F3 | amane_kanata_v1 |  |  |  |  |
| F3 | azki_v1 |  |  |  |  |
| F3 | momone_nene_official_v1_BEA |  |  |  |  |
| F3 | momone_nene_official_v1_STD |  |  |  |  |
| F3 | nurse_taso_v1_src |  |  |  |  |
| F3 | nurse_taso_v1 |  |  |  |  |
| F3 | SakamataChloe_v1 |  |  |  |  |

