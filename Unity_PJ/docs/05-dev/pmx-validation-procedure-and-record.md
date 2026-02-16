# PMX Validation Procedure And Record Sheet

- Date: 2026-02-09
- Scope: `Unity_PJ/data/assets_user/characters` 配下のPMX 8件を対象に、表示可否と見た目品質を検証する。
- Related:
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
  - `docs/reports/2026-02-09_factor-isolation-execution-plan.md`
  - `docs/worklog/2026-02-09_pmx-texture-24bit-tga-fix_1704.md`

## Targets (PMX 8)

1. `amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx`
2. `amane_kanata_v1/mmd/amane_kanata.pmx`
3. `azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx`
4. `momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx`
5. `momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx`
6. `nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx`
7. `nurse_taso_v1/mmd/NurseTaso.pmx`
8. `SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx`

## Preconditions

1. Unity Play Mode に入り、`MascotDesktop Runtime HUD` が表示されていること。
2. `Model: rescan(list)` は候補再取得のみで、モデル切替はしないこと。
3. 判定ログを Console で確認できること。
4. 優先確認事項:
   - 天音かなたPMXは「正常表示と言い切れない」ため重点確認する。
5. テクスチャ解決の一時調査が必要な場合は、起動前に環境変数 `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1` を設定する。

## Procedure

1. 初期化
   - `Model: rescan` を1回実行する。
   - `Model Candidates` の値を記録する。
2. Factorごとの巡回
   - Factor順: `F0 -> F1 -> F2 -> F3`
   - `Factor: prev/next` で目的Factorへ合わせる。
   - `Model: next/prev` でPMX対象8件をすべて表示する。
3. 各モデルの判定（毎回）
    - `avatar.model.loader_selected` が `Pmx` であること。
    - `avatar.model.displayed` が `pmx model displayed` であること。
    - `avatar.model.material_diagnostics` と `avatar.model.remediation_hint` を採取し、少なくとも `transparentRatio` / `textureAlphaShare` / `edgeAlphaShare` / `highShininessRatio` / `brightDiffuseRatio` / `missingResolveTotal` / `hint` を記録する。
    - （必要時）`[TextureLoader]` ログと `Material` ログで解決先テクスチャと透明判定を確認する。
    - 見た目を確認し、白飛び/黒つぶれ/肌色/コントラスト/質感破綻を記録する。
    - 必要ならスクリーンショットを保存する。
4. 終了判定
   - 8件 x 4Factor の記録が埋まっていること。
   - 天音かなたの異常表示有無を別欄に要約すること。

## Record Sheet A: Run Log

| Run ID | Date | Tester | Build/Editor | Candidate Count | Notes |
|---|---|---|---|---|---|
| R20260213_F0F3_3models | 2026-02-13 | User | Unity 6000.3.7f1 (manual) | 8 | remediation_hint と Game画面スクリーンショットを同時採取 |
| R20260213_F0F3_3models_r2 | 2026-02-13 | User | Unity 6000.3.7f1 (manual) | 8 | F0/F3 を request_id 付きで再採取（3モデル x 2） |

## Record Sheet B: Per Model And Factor

| Factor | Model Path | loader_selected=Pmx | model.displayed=pmx | White-out | Black Crush | Skin Tone | Contrast | Texture/UV Issue | Screenshot Path | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| F0 | amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F0 | amane_kanata_v1/mmd/amane_kanata.pmx | Yes | Yes | Low | No | Slightly bright | Medium | Minor | chat-image:kanata-f0 | 明部がやや白寄りだが全体破綻なし |
| F0 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | Yes | Yes | No | No | Natural | Medium | No | chat-image:azki-f0 | 対照として安定 |
| F0 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | Yes | Yes | Low | No | Slightly bright | Medium | Minor | chat-image:nene-bea-f0 | 明るめだが形状は維持 |
| F0 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx |  |  |  |  |  |  |  |  |  |
| F0 | nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F0 | nurse_taso_v1/mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F0 | SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx |  |  |  |  |  |  |  |  |  |
| F1 | amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F1 | amane_kanata_v1/mmd/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F1 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx |  |  |  |  |  |  |  |  |  |
| F1 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx |  |  |  |  |  |  |  |  |  |
| F1 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx |  |  |  |  |  |  |  |  |  |
| F1 | nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F1 | nurse_taso_v1/mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F1 | SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx |  |  |  |  |  |  |  |  |  |
| F2 | amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F2 | amane_kanata_v1/mmd/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F2 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx |  |  |  |  |  |  |  |  |  |
| F2 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx |  |  |  |  |  |  |  |  |  |
| F2 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx |  |  |  |  |  |  |  |  |  |
| F2 | nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F2 | nurse_taso_v1/mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F2 | SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx |  |  |  |  |  |  |  |  |  |
| F3 | amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx |  |  |  |  |  |  |  |  |  |
| F3 | amane_kanata_v1/mmd/amane_kanata.pmx | Yes | Yes | No | No | Natural | Medium | Minor | chat-image:kanata-f3 | F0比で白さが抑制 |
| F3 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | Yes | Yes | No | No | Natural | Medium | No | chat-image:azki-f3 | F0と同等で安定 |
| F3 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | Yes | Yes | No | No | Natural | Medium | Minor | chat-image:nene-bea-f3 | F0比で白さが抑制 |
| F3 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx |  |  |  |  |  |  |  |  |  |
| F3 | nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F3 | nurse_taso_v1/mmd/NurseTaso.pmx |  |  |  |  |  |  |  |  |  |
| F3 | SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx |  |  |  |  |  |  |  |  |  |

## Record Sheet C: Kanata Focus

| Model Path | Symptom | Suspected Layer (Texture/Material/Lighting) | Repro Step | Temporary Workaround | Final Fix |
|---|---|---|---|---|---|
| amane_kanata_v1/mmd/amane_kanata.pmx |  |  |  |  |  |
| amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx |  |  |  |  |  |

## Record Sheet D: Diagnostic Snapshot (from `avatar.model.remediation_hint`)

| Factor | Model Path | hint | transparentRatio | textureAlphaShare | edgeAlphaShare | highShininessRatio | brightDiffuseRatio | missingResolveTotal | Notes |
|---|---|---|---|---|---|---|---|---|---|
| F0 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0.938 | 0 | edge_alpha優勢 |
| F0 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 1 | 0 | texture_alpha優勢 |
| F0 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0.947 | 0 | edge_alpha/高輝度寄与 |
| F3 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0 | 0 | F3でbrightDiffuse低下 |
| F3 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 0 | 0 | F3でbrightDiffuse低下 |
| F3 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0 | 0 | F3でbrightDiffuse低下 |

### Interim Decision (2026-02-12)

- Primary: `shader_lighting_candidate` を優先して補正検証を進める。
- Reason:
  - かなた/ねね_BEA は F0/F3 の両方で `shader_lighting_candidate`。
  - AZKi は F0/F3 の両方で `materialloader_threshold_candidate` だが、対照として大きな白飛びは報告されていない。
  - 3モデルすべてで F3 時 `brightDiffuseRatio=0` まで低下し、照明寄与の差が明確に観測できる。
- Secondary:
  - `materialloader_threshold_candidate` は AZKi を対照にして閾値A/Bで副次検証する。

## Record Sheet E: F2 Diagnostic Snapshot (from `avatar.model.remediation_hint`)

| Factor | Model Path | hint | transparentRatio | textureAlphaShare | edgeAlphaShare | highShininessRatio | brightDiffuseRatio | missingResolveTotal | Notes |
|---|---|---|---|---|---|---|---|---|---|
| F2 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0.938 | 0 | F0と同傾向 |
| F2 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 1 | 0 | F0と同傾向 |
| F2 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0.947 | 0 | F0と同傾向 |

## Record Sheet F: Re-run Snapshot 2026-02-13 (F0/F2)

| Factor | Model Path | hint | transparentRatio | textureAlphaShare | edgeAlphaShare | highShininessRatio | brightDiffuseRatio | missingResolveTotal | Notes |
|---|---|---|---|---|---|---|---|---|---|
| F0 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0.938 | 0 | D/Eと同値（再現） |
| F0 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 1 | 0 | D/Eと同値（再現） |
| F0 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0.947 | 0 | D/Eと同値（再現） |
| F2 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0.938 | 0 | Eと同値（再現） |
| F2 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 1 | 0 | Eと同値（再現） |
| F2 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0.947 | 0 | Eと同値（再現） |

## Record Sheet G: Re-run Snapshot 2026-02-13 (F3)

| Factor | Model Path | hint | transparentRatio | textureAlphaShare | edgeAlphaShare | highShininessRatio | brightDiffuseRatio | missingResolveTotal | Notes |
|---|---|---|---|---|---|---|---|---|---|
| F3 | amane_kanata_v1/mmd/amane_kanata.pmx | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0 | 0 | Dと同値（再現） |
| F3 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 0 | 0 | Dと同値（再現） |
| F3 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0 | 0 | Dと同値（再現） |

## Record Sheet H: Re-run Snapshot 2026-02-13 (F0/F3 with request_id)

| Factor | Model Path | request_id | hint | transparentRatio | textureAlphaShare | edgeAlphaShare | highShininessRatio | brightDiffuseRatio | missingResolveTotal | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|
| F0 | amane_kanata_v1/mmd/amane_kanata.pmx | req-e880bff90c024cc287ae76ffec9bffdc | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0.938 | 0 | chat-log(2026-02-13T17:02:29Z), chat-image:kanata-f0 |
| F3 | amane_kanata_v1/mmd/amane_kanata.pmx | req-1e81b032ed8842558d71f71b95dca97c | shader_lighting_candidate | 0.813 | 0.231 | 0.923 | 1 | 0 | 0 | chat-log(2026-02-13T17:03:45Z), chat-image:kanata-f3 |
| F0 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | req-e5cd228568ec442aadc7ebb606e3b0af | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 1 | 0 | chat-log(2026-02-13T17:04:39Z), chat-image:azki-f0 |
| F3 | azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx | req-c41b499b042943628ecdff194b169362 | materialloader_threshold_candidate | 0.485 | 0.875 | 0.125 | 0 | 0 | 0 | chat-log(2026-02-13T17:03:57Z), chat-image:azki-f3 |
| F0 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | req-d847dee92f164e9586c1b2ff0815115f | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0.947 | 0 | chat-log(2026-02-13T17:05:09Z), chat-image:nene-bea-f0 |
| F3 | momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx | req-a7b924008f4449208af9a65e6c0f401a | shader_lighting_candidate | 0.895 | 0.529 | 0.706 | 0.789 | 0 | 0 | chat-log(2026-02-13T17:05:51Z), chat-image:nene-bea-f3 |

### Decision Update (2026-02-13 Re-run)

- F3再採取（3モデル）でも `Record Sheet D` と同値で再現したため、暫定方針を維持する。
- F0/F3再採取（`Record Sheet H`）でも同値で再現し、指標の揺らぎは確認されなかった。
- Primary: `shader_lighting_candidate` 優先（kanata / nene_BEA）
- Secondary: `materialloader_threshold_candidate` は AZKi 対照で副次評価

### Final Assessment (Current)

- F0/F3 の目視比較では、kanata / nene_BEA の白さは F3 で低減し、AZKi は F0/F3 で安定。
- `remediation_hint` の再現性は F0/F2/F3 で維持され、分類の揺らぎは見られない。
- 現時点の優先対応:
  - 1) Shader側補正の継続（採用）
  - 2) 残課題（欠け/グレー）は texture/asset 側の個別確認で切り分け

### Implementation Update (2026-02-14)

- `SimpleModelBootstrap` のライト自動設定をデフォルト無効化した。
  - `SimpleModelConfig.autoConfigureSceneLight=false`（既定）
  - `SimpleModelConfig.createSceneLightIfMissing=false`（既定）
  - `autoConfigureSceneLight=false` 時は `SimpleModelLight` が存在して有効なら起動時に自動で無効化する。
- `TextureLoader` の解決ログを強化した。
  - fallback解決時: `[TextureLoader] resolve fallback: request=..., resolved=..., strategy=...`
  - recursive解決時: `[TextureLoader] resolve recursive: ...`
  - 解決失敗時: `[TextureLoader] resolve failed: request=..., base=..., textureDirExists=..., texturesDirExists=...`
- AZKi を対照として、kanata/nene_BEA の失敗ログと `strategy` 差分を比較して原因を特定する。

