# Worklog: MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts

- Date: 2026-02-16
- Task: NurseTaso の白/黒表示不良対策（モデル固有・最小差分で PMX 参照を復元）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx, MMD/ナースたそ/ナースたそ/NurseTaso.pmx, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, docs/worklog/2026-02-16_MascotDesktop_centerline_missing_reobservation.md, docs/worklog/2026-02-16_MascotDesktop_nursetaso_texture_reference_repair.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md
- Obsidian-Log: 未実施（本セッションはリポジトリ内修正と検証を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex, nurse_taso, pmx, texture, regression-guard]

## Summary
- 現行 `NurseTaso.pmx` は元配布版とハッシュ不一致で、差分は 7 材質の `texIdx` 変更のみだった。
- 変更されていた 7 材質（`舌`, `口`, `歯`, `まつげ`, `眉`, `脚`, `靴`）に face/body texture を強制割当していたため、モデル固有の白/黒アーティファクト要因になっている可能性が高いと判断。
- 影響を NurseTaso のみに限定するため、コード変更は行わず、`NurseTaso.pmx` を元配布版へ復元した。
- 既存固定済み状態（toon fallback / `_MascotEdgeContributionCap` 条件 / F0 Baseline）はコード無変更で維持。

## Investigation Evidence
1. ハッシュ比較
- Source (`MMD/ナースたそ/ナースたそ/NurseTaso.pmx`): `C59822440DCB7EB195F2F7ECD2677C288A7F6D8CD43D2D943BA0E7477038A881`
- Runtime before (`.../mmd_pkg/mmd/NurseTaso.pmx`): `6D3744564BC141201A2B38DD0301FD37168021F822689DCC3F402881B43B5118`

2. PMX 差分（before -> source）
- texture table は同一（7件）
- 材質差分は 7件（`texIdx` のみ）
  - `舌`: `1 -> -1`
  - `口`: `1 -> -1`
  - `歯`: `1 -> -1`
  - `まつげ`: `1 -> -1`
  - `眉`: `1 -> -1`
  - `脚`: `3 -> -1`
  - `靴`: `3 -> -1`

## Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- 変更前バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_20260216_214731`
- 反映:
  - `MMD/ナースたそ/ナースたそ/NurseTaso.pmx` を runtime 配置へコピー
- 結果:
  - After hash は Source hash と一致（`C598...A881`）

2. `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- 本記録を新規作成

## Commands
- 同定/前提確認
  - `Get-Content .git/HEAD`
  - `Get-Content .git/config`
  - `Get-Content AGENTS.md`
- スキル参照
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- 調査
  - `Get-FileHash`（NurseTaso の各 PMX）
  - PowerShell BinaryReader で PMX material diff 抽出
- 変更
  - `Copy-Item NurseTaso.pmx NurseTaso.pmx.bak_restore_source_20260216_214731 -Force`
  - `Copy-Item MMD/ナースたそ/ナースたそ/NurseTaso.pmx Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
- テスト
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. Static/Binary verification
- After hash = Source hash（一致）
- before/after PMX diff は `texIdx` 7件のみ
- `MaterialLoader.cs` / `TextureLoader.cs` は未変更（既存固定ロジック保持）

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_214858.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_214858.xml`: 未生成

## avatar.model.material_diagnostics (Before/After)
- Before（既存証跡）
  - `req-1094e2a70f3e4dff90a2593c97265f5e`: `toonMissingMats=0`, `toonMissingSpecMats=0`（中央線消失確認）
  - `req-e664f8957f63403e8d096391f43cde16`: `toonMissingMats=0`, `toonMissingSpecMats=0`（継続確認）
  - `req-01671cdf44e24f74abe5bb5d4bfea622`（Nurse関連既存記録）: `toonMissingMats=0`, `toonMissingSpecMats=0`
- After（本端末）
  - Unity 実行環境制約により新規採取不可（要ユーザー実機採取）

## Rationale (Key Points)
- 不具合は NurseTaso 固有であり、共通ローダー改変は他モデル影響リスクが高い。
- 実際のバイナリ差分が `NurseTaso.pmx` の 7材質 `texIdx` のみに限定されていたため、1要因最小差分として PMX 復元を選択。
- 既存固定済み対策を壊さないため、`MaterialLoader` / `SimpleModelBootstrap` の値・ロジックは一切変更しない方針を採用。

## Risk / Alternatives
- リスク
  - `texIdx=-1` が仕様意図でない場合、白表示が残る可能性がある。
- 代替案
  - PMX を部分編集して 7材質の割当先を再調整（今回は副作用最小化のため不採用）
  - コード側で NurseTaso 分岐を追加（保守性低下のため不採用）

## Rollback
1. 本変更の即時ロールバック
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_20260216_214731 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

## Next Actions
1. ユーザー実機で `NurseTaso.pmx` を reload し、白/黒表示箇所の解消を目視確認する。
2. 同条件で `avatar.model.material_diagnostics` を再採取し、Before/After を比較する。
   - 必須比較項目: `toonMissingMats`, `toonMissingSpecMats`, `samples`
3. 中央線/白飛びの回帰がないことを同時確認する（`req-1094...`, `req-e664...` 基準）。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Follow-up Update (2026-02-16 13:00 UTC) - User Runtime Verification
- User runtime result:
  1. 黒表示箇所は白表示へ変化（NurseTaso 固有不良が改善方向）。
  2. `avatar.model.material_diagnostics`:
     - request_id: `req-fa09daf52459470b928236c034deec6e`
     - `materials=15`, `transparentMats=7`, `transparentByTextureAlphaMats=7`
     - `toonMissingMats=0`, `toonMissingSpecMats=0`, `toonMissingResolveMats=0`, `toonMissingUnknownMats=0`
     - `sphereAddMissingMats=0`, `sphereMulMissingMats=0`
     - `brightDiffuseMats=1`
     - `samples` でも `mainTex=True` と `toonStatus=loaded_fallback_white/loaded` を確認。
  3. 再発なし（中央線/白飛びの再発報告なし）。

### Acceptance Check
- 欠け再発なし（toon missing 系カウンタ悪化なし）: Pass
- 中央線再発なし（目視）: Pass（ユーザー報告）
- ライト白飛び再発なし（目視）: Pass（ユーザー報告）
- `avatar.model.material_diagnostics` before/after 比較提示: Pass

### Before/After (Diagnostics)
- Before（既存証跡）
  - `req-01671cdf44e24f74abe5bb5d4bfea622`
  - `toonMissingMats=0`, `toonMissingSpecMats=0`, `brightDiffuseMats=1`
- After（ユーザー実機）
  - `req-fa09daf52459470b928236c034deec6e`
  - `toonMissingMats=0`, `toonMissingSpecMats=0`, `brightDiffuseMats=1`
- 判定
  - toon missing 系は悪化なし、主要カウンタは同等水準を維持。

### Status
- 本タスク受け入れ条件は満たしたため、現状態を暫定安定点として保持する。

## Follow-up Update (2026-02-16 22:33 UTC) - Leg Only Patch
- User request: 「まず足（脚）から」
- 方針: 1要因最小差分として、`脚` 材質のみ `texIdx` を更新（`靴`含む他材質は未変更）。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_only_20260216_223307`
- 変更内容:
  - material `脚`（index=13）
    - `texIdx: -1 -> 3`
    - `textures\body_texture_diffuse.png` を参照
- バイナリ書換:
  - `textureIndexSize=1` を確認の上、`texOffset=672170` の 1byte を `0xFF(-1)` から `0x03(3)` に変更

### Additional Verification
1. Static/Binary
- before/after 差分は 1件のみ:
  - `脚` の `texIdx` 変更だけ
- `靴` は `texIdx=-1` のまま（未変更）

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_223342.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_223342.xml`: 未生成

### Rollback (for this follow-up)
1. 脚のみパッチを戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_only_20260216_223307 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload し、脚の白欠損が実体テクスチャ表示へ改善したか確認
2. `avatar.model.material_diagnostics` を採取し、`samples` に脚材質の `mainTex=True` が反映されるか確認
3. 中央線/白飛びの再発有無を同時確認

## Follow-up Update (2026-02-16 22:49 UTC) - Leg Texture Reassignment 3 -> 0
- User runtime report (`req-4bb89fce3ead459688e95a9654efb218`):
  - 欠損白表示が黒表示化
  - `transparentMats=8` / `transparentByTextureAlphaMats=8`（前回 7 -> 8 に増加）
  - `toonMissing*=0` は維持
- 解釈:
  - `脚 texIdx=3(body_texture)` が `texture_alpha` 透明経路を1件増やし、見た目悪化に寄与している可能性。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_3_to_0_20260216_224848`
- 変更内容:
  - material `脚`（index=13）
    - `texIdx: 3 -> 0`
    - `textures\cloth_texture_diffuse.png` を参照
- 他材質（`靴`含む）は変更なし

### Additional Verification
1. Static/Binary
- before/after 差分は 1件のみ:
  - `脚` の `texIdx: 3 -> 0`
- ハッシュ:
  - after: `017F5EB73641600E2EFBBCBDBB184881C5357C07E6C48EEF91F695FAE3DFCF3C`
  - backup(3): `9D0A97D1403FA7117AA3762A6F688607D4D3EBF48501FE6EF3629F555A39A62F`
  - backup(-1): `C59822440DCB7EB195F2F7ECD2677C288A7F6D8CD43D2D943BA0E7477038A881`

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_224920.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_224920.xml`: 未生成

### Rollback (for this follow-up)
1. `3 -> 0` を戻して直前状態(3)へ
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_3_to_0_20260216_224848 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
2. さらに初期状態(-1)へ
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_only_20260216_223307 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload し、脚の白/黒欠損が改善したか確認
2. `avatar.model.material_diagnostics` を採取して、最低限以下を比較
   - `transparentMats`
   - `transparentByTextureAlphaMats`
   - `samples`（脚相当材質の `diffMax` / `mainTex` / `shader`）
3. 他モデルの中央線/欠損に回帰がないことを再確認

## Follow-up Update (2026-02-16 23:06 UTC) - Leg Rollback 0 -> -1
- User runtime report (`req-9a235a7e27d442beb45ee0c7efa604a9`):
  - 脚は依然として黒表示
  - `transparentMats=8` / `transparentByTextureAlphaMats=8` 維持
  - `toonMissing*=0` 維持
- 解釈:
  - `脚 texIdx=0` でも黒化が解消せず、脚材質への main texture 割当自体が不適合の可能性が高い。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_0_to_minus1_20260216_230619`
- 変更内容:
  - material `脚`（index=13）
    - `texIdx: 0 -> -1`
- 他材質（`靴`含む）は変更なし

### Additional Verification
1. Static/Binary
- before/after 差分は 1件のみ:
  - `脚` の `texIdx: 0 -> -1`
- ハッシュ:
  - after: `C59822440DCB7EB195F2F7ECD2677C288A7F6D8CD43D2D943BA0E7477038A881`
  - backup(0): `017F5EB73641600E2EFBBCBDBB184881C5357C07E6C48EEF91F695FAE3DFCF3C`

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_230652.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_230652.xml`: 未生成

### Rollback (for this follow-up)
1. `0 -> -1` を戻して直前状態(0)へ
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_0_to_minus1_20260216_230619 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload し、脚表示（黒/白）と全体の欠損状態を確認
2. `avatar.model.material_diagnostics` を再採取して、`transparentMats` / `transparentByTextureAlphaMats` の戻りを確認
3. 次段は `靴` 単独の1要因検証（`-1 -> 3`）へ進む

## Follow-up Update (2026-02-16 23:13 UTC) - Shoe Only Patch (-1 -> 3)
- User runtime report (`req-5d04c4338b464d07a78762b48c4bc39f`):
  - 白表示継続
  - `transparentMats=7` / `transparentByTextureAlphaMats=7`
  - `toonMissing*=0`
- 方針:
  - 次の1要因として `靴` 材質のみ main texture を割当し、見た目変化を分離観測する。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_minus1_to_3_20260216_231259`
- 変更内容:
  - material `靴`（index=14）
    - `texIdx: -1 -> 3`
    - `textures\body_texture_diffuse.png` を参照
- 他材質（`脚`含む）は変更なし（`脚=-1` 維持）

### Additional Verification
1. Static/Binary
- before/after 差分は 1件のみ:
  - `靴` の `texIdx: -1 -> 3`
- ハッシュ:
  - after: `2EF91E8866A2C36E9CF0EA44A61F6E95ECB035E43C34979D3268F6FA2C543203`
  - backup(-1): `C59822440DCB7EB195F2F7ECD2677C288A7F6D8CD43D2D943BA0E7477038A881`

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_231331.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_231331.xml`: 未生成

### Rollback (for this follow-up)
1. `靴 -1 -> 3` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_minus1_to_3_20260216_231259 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload し、白/黒欠損の変化を確認
2. `avatar.model.material_diagnostics` を採取して、`transparentMats` / `transparentByTextureAlphaMats` と `samples` を比較
3. 他モデル（中央線/欠損）に回帰がないことを確認

## Follow-up Update (2026-02-16 23:24 UTC) - Shoe Diffuse RGB Normalization
- User runtime report (`req-1daa42dbb0794281bc2e51dd84172d43`):
  - 足=白、靴=黒（靴は変化なし）
  - `transparentMats=8` / `transparentByTextureAlphaMats=8`
  - `toonMissing*=0`
- 解釈:
  - `靴` は tex割当だけでは黒化が解消していないため、材質色（Diffuse）寄与を単独検証する。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_diffuse_rgb_to1_20260216_232344`
- 変更内容:
  - material `靴`（index=14）の Diffuse RGB のみ
    - `0.376, 0.376, 0.376 -> 1.0, 1.0, 1.0`
  - alpha は維持（`1.0`）
  - `texIdx` は維持（`3`）
- 他材質（`脚`含む）は変更なし

### Additional Verification
1. Static/Binary
- before/after 差分は 1件のみ:
  - `靴` Diffuse RGB のみ変更
- 差分確認:
  - `TexIdxBefore=3`, `TexIdxAfter=3`
  - `DiffBefore=0.376,0.376,0.376,1`
  - `DiffAfter=1,1,1,1`
- ハッシュ:
  - after: `EDB58D859EDE1027B271D473E9102FE5050645E3AE4CBEDFF8C081B32E9617A6`
  - backup(before): `2EF91E8866A2C36E9CF0EA44A61F6E95ECB035E43C34979D3268F6FA2C543203`

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_232419.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_232419.xml`: 未生成

### Rollback (for this follow-up)
1. 靴Diffuse変更を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_diffuse_rgb_to1_20260216_232344 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload し、靴の黒表示が改善するか確認
2. `avatar.model.material_diagnostics` を採取し、`samples` の `diffMax` 変化を確認
3. 他モデル（中央線/欠損）に回帰がないことを確認

## Follow-up Update (2026-02-16 23:38 UTC) - Diagnostic Material Name Tagging (No Visual Change)
- User request:
  - 代替案として「靴材質名が実表示と不一致」切り分けを先に実装。
  - 表示挙動を変えず、診断ログだけ強化する。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 追加:
  - `MmdMaterialNameTag = "MASCOT_MMD_MATERIAL_NAME"`
  - `ResolveDiagnosticMaterialName(MmdMaterial)` helper
- 反映箇所:
  - `ConfigMaterial(...)` 内で
    - `material.SetOverrideTag(MmdMaterialNameTag, ResolveDiagnosticMaterialName(mmdMaterial));`
- 意図:
  - `SimpleModelBootstrap.GetMaterialDiagnosticName()` がタグ値を優先する既存実装に対し、PMX材質名を確実に供給する。

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `DiagnosticsHelpers_GetMaterialDiagnosticName_PrefersMmdMaterialTag`
- 追加 helper:
  - `InvokeGetMaterialDiagnosticName(Material material)`
- 意図:
  - 診断名が `material.name` ではなく `MASCOT_MMD_MATERIAL_NAME` を優先することを保証。

### Additional Verification
1. Static checks
- `MaterialLoader.cs`
  - `MmdMaterialNameTag` 定数追加を確認
  - `SetOverrideTag(MmdMaterialNameTag, ...)` 追加を確認
  - `ResolveDiagnosticMaterialName` 追加を確認
- `SimpleModelBootstrapTests.cs`
  - 新規テストと reflection helper 追加を確認

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_233813.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_233813.xml`: 未生成

### Rollback (for this follow-up)
1. `MaterialLoader.cs`
- `MmdMaterialNameTag` / `ResolveDiagnosticMaterialName` / `SetOverrideTag(MmdMaterialNameTag, ...)` を削除
2. `SimpleModelBootstrapTests.cs`
- `DiagnosticsHelpers_GetMaterialDiagnosticName_PrefersMmdMaterialTag` と `InvokeGetMaterialDiagnosticName` を削除

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` の `samples` を確認
   - `MMD/... (Instance)` ではなく、PMX材質名（例: `靴`, `脚`）が先頭に出るか
3. 出力された材質名を基に、黒表示部位に一致する材質のみを次の1要因変更対象にする

## Follow-up Update (2026-02-16 23:50 UTC) - Force Include Leg/Shoe In Diagnostics Samples
- User runtime report (`req-409ed31057344429abcc3dd934263809`):
  - 変化なし
  - `samples` は PMX材質名化できたが、`脚` / `靴` が出力されず黒部位と突合不能
- 対応方針:
  - 表示無変更で、`avatar.model.material_diagnostics` の `samples` 収集に優先出力ルールを追加。

### Additional Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 追加:
  - `PriorityMaterialDiagnosticNames = { "脚", "靴" }`
  - `ShouldAlwaysIncludeMaterialDiagnosticSample(string materialName)`
  - `IsPriorityMaterialDiagnosticSample(string sample)`
- 変更:
  - `materialDiagnosticSamples` 収集ロジックを拡張し、`脚`/`靴` は条件に関係なく候補化。
  - 上限到達時は、`脚`/`靴` サンプルを非優先サンプルと置換して保持。
- 意図:
  - `samples` に `脚`/`靴` が落ちないようにして、黒表示部位の実材質を確実に特定する。

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `DiagnosticsHelpers_ShouldAlwaysIncludeMaterialDiagnosticSample_TargetsLegAndShoe`
  - `DiagnosticsHelpers_IsPriorityMaterialDiagnosticSample_DetectsTargetSamples`
- 追加 helper:
  - `InvokeShouldAlwaysIncludeMaterialDiagnosticSample`
  - `InvokeIsPriorityMaterialDiagnosticSample`

### Additional Verification
1. Static checks
- `SimpleModelBootstrap.cs` に `PriorityMaterialDiagnosticNames` と2 helper の追加を確認
- `samples` 収集部で `forceIncludeMaterialSample` 分岐が追加されたことを確認
- `SimpleModelBootstrapTests.cs` に新規2テストと reflection helper を確認

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260216_235051.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260216_235051.xml`: 未生成

### Rollback (for this follow-up)
1. `SimpleModelBootstrap.cs`
- `PriorityMaterialDiagnosticNames` と `ShouldAlwaysInclude...` / `IsPriority...` を削除
- `materialDiagnosticSamples` 収集を変更前条件へ戻す
2. `SimpleModelBootstrapTests.cs`
- 追加した2テストと2 helper を削除

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` の `samples` に `脚` と `靴` が出るか確認
3. 出力された `脚`/`靴` の `shader`, `transparentReason`, `mainTex`, `diffMax` を共有

## Follow-up Update (2026-02-17 00:04 UTC) - Shoe Texture Rollback 3 -> -1
- User runtime report (`req-d59a660a68f24b28b1e6ddc40f8aa9cb`):
  - `脚` は `status=missing_spec, mainTex=False`
  - `靴` は `status=loaded, mainTex=True, shininess=20, diffMax=1`
  - 黒表示（靴）は変化なし、スカートが白/黒に揺れる
- 方針:
  - 1要因最小差分で `靴` のみ `texIdx` を `3 -> -1` に戻し、透明材質件数揺れへの影響を切り分け。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- バックアップ作成:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_3_to_minus1_20260217_000410`
- 変更内容（1要因のみ）:
  - material `靴`（index=14）
    - `texIdx: 3 -> -1`
    - `textureIndexSize=1` のため `texOffset=672268` の 1byte を `0x03 -> 0xFF` に変更
- 変更しないもの:
  - `脚` は `texIdx=-1` のまま
  - `靴` Diffuse RGB（`1,1,1`）は維持
  - 共通コード（MaterialLoader/SimpleModelBootstrap/lighting）は未変更

### Additional Verification
1. PMX static verification
- `脚`: `texIdx=-1`（offset=672170）
- `靴`: `texIdx=-1`（offset=672268）
- backup 比較:
  - `diff_count=1`
  - `first_diff=672268`
  - `byte_before_at_672268=3`
  - `byte_after_at_672268=255`
- hash:
  - current: `D5C8BDC9FEB48AE2FBAFBEB8278657B02F0B174A303095E3D1232F143B81BC8A`
  - backup: `EDB58D859EDE1027B271D473E9102FE5050645E3AE4CBEDFF8C081B32E9617A6`

2. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_000450.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_000450.xml`: 未生成

### Rollback (for this follow-up)
1. 今回の `靴 3 -> -1` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_shoe_3_to_minus1_20260217_000410 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `transparentMats`, `transparentByTextureAlphaMats`, `brightDiffuseMats`, `samples(脚/靴)`
3. 目視で以下を報告
   - `靴` の黒表示変化有無
   - `スカート` の白/黒揺れ変化有無
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 00:12 UTC) - Investigation Only (Leg/Skirt/Shoe State)
- User runtime report (`req-b7b3e41665e44f2098a1efcf70588f8b`):
  - `脚`: `status=missing_spec`, `mainTex=False`（白）
  - `靴`: `status=missing_spec`, `mainTex=False`（見た目許容）
  - `スカート`: `status=loaded`, `mainTex=True`（黒）

### Investigation Commands
1. PMX材質比較（runtime vs source）
2. 関連テクスチャハッシュ比較
3. PNGアルファ統計（`System.Drawing.Bitmap`）
4. `texIdx=6` 参照材質の特定

### Investigation Findings
1. Runtime PMX と Source PMX の差分は 1件のみ
- `靴` Diffuse: `0.376,0.376,0.376,1.000 -> 1.000,1.000,1.000,1.000`
- `脚` / `スカート` の材質パラメータは source と一致

2. スカート関連テクスチャの runtime/source ハッシュは一致
- `textures/skirt_texture_diffuse.png`: 一致
- `textures/cloth_texture_diffuse.png`: 一致
- `textures/body_texture_diffuse.png`: 一致

3. `skirt_texture_diffuse.png` のアルファ分布
- `size=1024x1024`
- `alpha=0` が `377,927 / 1,048,576`（`36.0419%`）
- `alpha<255` も同数（半透明画素は実質なし）
- つまり「不透明 or 完全透明」の二値マスク

4. `texIdx=6 (textures/skirt_texture_diffuse.png)` の参照先
- `スカート` 材質のみ（index=10）

### Current Interpretation
- スカート黒は PMX材質差分ではなく、`transparentReason=texture_alpha` 経路で `skirt_texture_diffuse.png` の alpha=0 領域が強く効いている可能性が高い。
- NurseTaso 固有フォルダ内テクスチャで完結しているため、モデル固有対策が可能。

### Next One-Factor Candidate (Not Applied Yet)
1. `textures/skirt_texture_diffuse.png` の alpha を 255 固定化（RGBは不変更）
- 影響範囲は `スカート` 材質のみ（texIdx参照確認済み）
- 共通ローダー/他モデルには影響しない
- ロールバックは `.bak_*` 復元で即時可能

## Follow-up Update (2026-02-17 00:17 UTC) - Skirt Texture Alpha 255 Fix (Model-Local)
- User approved one-factor model-local mitigation for `スカート黒`.
- Scope-limited change: `NurseTaso` 配下の `skirt_texture_diffuse.png` の alpha のみ変更（共通コード無変更）。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_alpha255_20260217_001654`
- Change:
  - 全ピクセルで alpha を `255` に固定
  - RGB は不変更

### Additional Verification
1. Before alpha stats
- `before_size=1024x1024`
- `before_alpha_zero=377927`
- `before_alpha_lt255=377927`
- `before_alpha_zero_ratio=36.0419%`

2. Pixel-level comparison (current vs backup)
- `pixel_total=1048576`
- `rgb_mismatch=0`（RGB不変）
- `alpha_mismatch=377927`（alpha変更画素のみ）
- `current_alpha_not255=0`（全画素 alpha=255）
- hash:
  - current: `DB8390724036D8F1A128E23E1401EB4A79BFEADFC1D580D1123590E2B1B47F9E`
  - backup: `4C4948EC7DF9D8A88BD5D1EECC02AB942EF9CD41FFABBD953842B91D51E16B02`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_001738.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_001738.xml`: 未生成

### Rollback (for this follow-up)
1. `skirt_texture_diffuse.png` のみ戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_alpha255_20260217_001654 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取
   - 必須比較: `transparentMats`, `transparentByTextureAlphaMats`, `samples(スカート/脚/靴)`
3. 目視確認
   - `スカート黒` が改善したか
   - `脚白` の変化有無
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 00:29 UTC) - Glove Black Isolation (texIdx 3 -> -1)
- User runtime report (`req-55f593b814bc49f28ed037cc29f9cf87`):
  - `スカートOK`, `脚NG(白)`, `靴OK`
  - 追加目視: `手袋NG(黒)`, `手首NG(白)`
- Investigation:
  - `グローブ` は `texIdx=3` (`textures/body_texture_diffuse.png`) を参照
  - 同一 `texIdx=3` は `身体` も共有
  - 共有テクスチャ alpha 改変は `身体` へ副作用が出るため不採用
- Action:
  - 1要因最小差分として、`グローブ` のみ `texIdx: 3 -> -1` を適用

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_3_to_minus1_20260217_002842`
- Change:
  - material `グローブ`（index=12）
  - `texIdx: 3 -> -1`
  - `textureIndexSize=1` のため `texOffset=672076` の 1byte を `0x03 -> 0xFF`
- Unchanged:
  - `身体 texIdx=3`
  - `脚 texIdx=-1`
  - `靴 texIdx=-1`

### Additional Verification
1. Binary delta
- `diff_count=1`
- `first_diff=672076`
- `byte_before_at_672076=3`
- `byte_after_at_672076=255`

2. Hash
- current: `496DE1DAFF80606DD01F7C5D7B2830CB5487828370F0BFF7D066D62304A8C5C7`
- backup: `D5C8BDC9FEB48AE2FBAFBEB8278657B02F0B174A303095E3D1232F143B81BC8A`

3. PMX reread
- `身体 texIdx=3`
- `グローブ texIdx=-1`
- `脚 texIdx=-1`
- `靴 texIdx=-1`

4. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_002920.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_002920.xml`: 未生成

### Rollback (for this follow-up)
1. `グローブ 3 -> -1` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_3_to_minus1_20260217_002842 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 必須比較: `transparentMats`, `transparentByTextureAlphaMats`, `samples(グローブ/脚/身体)`
3. 目視確認
   - 手袋黒が改善したか
   - 手首白の変化有無
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 00:42 UTC) - Leg Toon Reference Align (missing_spec white mitigation)
- User runtime report (`req-7a1d506dd3d14e5f944a8626afa901ca`):
  - 手首OK / 手袋は白化 / 足は白
  - diagnostics で `脚`: `status=missing_spec`, `mainTex=False`
- Approach:
  - `脚` は `mainTex=False` を維持したまま、toon 参照のみを `顔` と同一化して白化緩和を狙う。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_toon_minus1_to_2_20260217_004210`
- Material inspect (before):
  - `顔` (index=4): `toonFlag=0`, `toonTexIdx=2` (`textures\facetoon.bmp`)
  - `脚` (index=13): `toonFlag=0`, `toonTexIdx=-1` (`<none>`)
- One-factor change:
  - `脚` の `toonTexIdx: -1 -> 2`
  - offset `672174` を `0xFF -> 0x02` に変更
- Material inspect (after):
  - `顔`: `toonFlag=0`, `toonTexIdx=2`
  - `脚`: `toonFlag=0`, `toonTexIdx=2`

### Additional Verification
1. Binary delta
- `diff_count=1`
- `first_diff=672174`
- `byte_before_at_672174=255`
- `byte_after_at_672174=2`

2. Hash
- current: `88A001077309EA552EB4B742E11FCF1CB44FFD2B7F911EB8334D18FD088C841A`
- backup: `496DE1DAFF80606DD01F7C5D7B2830CB5487828370F0BFF7D066D62304A8C5C7`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_004248.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_004248.xml`: 未生成

### Rollback (for this follow-up)
1. `脚 toonTexIdx 2 -> -1` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_toon_minus1_to_2_20260217_004210 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 必須比較: `samples(脚/グローブ/靴)`, `toonStatus`, `transparentMats`
3. 目視確認
   - `脚白` の改善有無
   - `手袋白` の変化有無
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 00:57 UTC) - Leg Main Texture Candidate Switch (-1 -> 6)
- User runtime report (`req-5db2ca398cbd4551a0d4fc1dddc9a977`):
  - `脚` は `toonStatus=loaded` になっても見た目変化なし
  - `脚` は引き続き `status=missing_spec`, `mainTex=False`
- Interpretation:
  - `脚白` の主要因は toon ではなく `mainTex=False`。

### Investigation (texture candidates)
1. Texture table (PMX)
- `[0] textures\cloth_texture_diffuse.png`
- `[1] textures\face.png`
- `[2] textures\facetoon.bmp`
- `[3] textures\body_texture_diffuse.png`
- `[4] textures\epron_texture_diffuse.png`
- `[5] textures\hair_tex_diffuse.png`
- `[6] textures\skirt_texture_diffuse.png`

2. Alpha profile
- `cloth_texture_diffuse.png`: alpha0=47.0861%
- `face.png`: alpha0=0%
- `body_texture_diffuse.png`: alpha0=35.3402%
- `epron_texture_diffuse.png`: alpha0=57.4396%
- `hair_tex_diffuse.png`: alpha0=73.0094%
- `skirt_texture_diffuse.png`: alpha0=0%（直前にalpha255化済み）

3. Candidate selection reason
- `脚` へ割当可能で alpha 透過起因の黒化リスクが低い候補を優先し、`texIdx=6` を採用。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_tex_minus1_to_6_20260217_005751`
- One-factor change:
  - `脚` (index=13) `texIdx: -1 -> 6`
  - offset `672170`: `0xFF -> 0x06`
- Unchanged:
  - `グローブ texIdx=-1`
  - `靴 texIdx=-1`
  - `スカート texIdx=6`

### Additional Verification
1. Binary delta
- `diff_count=1`
- `first_diff=672170`
- `byte_before_at_672170=255`
- `byte_after_at_672170=6`

2. Hash
- current: `3E69FE75E63422831231EF9216AAC11AB04DBED8FB82CEC4F3751FE1224610FB`
- backup: `88A001077309EA552EB4B742E11FCF1CB44FFD2B7F911EB8334D18FD088C841A`

3. PMX reread
- `脚 texIdx=6 (textures\skirt_texture_diffuse.png)`

4. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_005832.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_005832.xml`: 未生成

### Rollback (for this follow-up)
1. `脚 texIdx 6 -> -1` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_leg_tex_minus1_to_6_20260217_005751 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 必須比較: `samples(脚/グローブ/靴)`, `mainTex`, `transparentReason`, `transparentMats`
3. 目視確認
   - `脚白` が改善したか
   - 手袋/靴/スカートの維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 01:17 UTC) - Glove Texture Candidate Switch (-1 -> 1)
- User runtime report (`req-af44a617ff28452cb1ce8d42bb1a4f9c`):
  - `脚` は `status=loaded`, `mainTex=True` まで改善したが表示は一部不正
  - `グローブ` は白のまま
- Action plan execution:
  - `グローブ` の `mainTex=False` 解消を優先し、1要因で `texIdx` を設定。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_minus1_to_1_20260217_011712`
- One-factor change:
  - material `グローブ`（index=12）
  - `texIdx: -1 -> 1` (`textures\face.png`)
  - offset `672076`: `0xFF -> 0x01`
- Unchanged:
  - `脚 texIdx=6`
  - `靴 texIdx=-1`
  - `身体 texIdx=3`

### Additional Verification
1. Binary delta
- `diff_count=1`
- `first_diff=672076`
- `byte_before_at_672076=255`
- `byte_after_at_672076=1`

2. Hash
- current: `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`
- backup: `3E69FE75E63422831231EF9216AAC11AB04DBED8FB82CEC4F3751FE1224610FB`

3. PMX reread
- `顔 texIdx=1 (textures\face.png)`
- `グローブ texIdx=1 (textures\face.png)`
- `身体 texIdx=3`
- `脚 texIdx=6`
- `靴 texIdx=-1`

4. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_011750.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_011750.xml`: 未生成

### Rollback (for this follow-up)
1. `グローブ texIdx 1 -> -1` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_minus1_to_1_20260217_011712 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 必須比較: `samples(グローブ/脚/靴)`, `mainTex`, `transparentReason`, `transparentMats`
3. 目視確認
   - `グローブ白` の改善有無
   - `脚` の白/灰混在の変化
   - `靴` / `スカート` の維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 10:15 UTC) - Material Hit Test (Glove Diffuse Red)
- User runtime report (`req-012c47ead2874337b9bfc18e3ba66652`):
  - 見た目変化なし
  - diagnostics上は `脚=loaded/mainTex=True` だが目視不整合
- Hypothesis:
  - 材質名と実表示部位がズレている可能性を先に切り分ける必要あり。
- Action (diagnostic-only):
  - `グローブ` 材質の Diffuse RGB を識別色（赤）へ変更してヒットテスト。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_rgb_hit_red_20260217_101436`
- One-factor diagnostic change:
  - material `グローブ` (index=12)
  - Diffuse RGB: `0.8,0.8,0.8 -> 1.0,0.0,0.0`
  - Diffuse A: `1.0` を維持
  - offset range starts at `672011` (R,G,B,A float block)
- Unchanged:
  - `脚` Diffuse / `靴` Diffuse
  - `texIdx` 系設定
  - 共通コード（MaterialLoader/SimpleModelBootstrap/lighting）

### Additional Verification
1. PMX readback
- `グローブ diffuse=1,0,0,1`
- `脚 diffuse=0.8,0.8,0.8,1`
- `靴 diffuse=1,1,1,1`

2. Binary delta (current vs backup)
- `diff_count=11`
- `first_diff=672011`
- `last_diff=672022`
- hash:
  - current: `98371FCAFAAF3109D41312272AFEC6684F2DBBD915F5B582AA5F333396EE361F`
  - backup: `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_101510.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_101510.xml`: 未生成

### Rollback (for this follow-up)
1. 診断色変更を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_rgb_hit_red_20260217_101436 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. 目視で確認
   - 「グローブ」表示部位が赤になるか
   - 赤にならない場合、どの部位が赤になったか
3. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `mainTex`, `status`, `transparentMats`

## Follow-up Update (2026-02-17 10:32 UTC) - Glove Diagnostic Revert + texIdx 1 -> 6
- User runtime report (`req-621e4ac2cd544ac192f5941241c992d2`):
  - `グローブ` が赤表示になることを確認（材質対応のズレなし）
- Decision:
  - 診断色を先に元へ戻し、その後 `グローブ` の main texture 候補を `texIdx=6` に変更。

### Step 1: Revert diagnostic red (cleanup)
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_restore_20260217_20260217_103145`
- Change:
  - `グローブ` Diffuse RGB `1,0,0 -> 0.8,0.8,0.8`（A=1維持）
- Verification:
  - `glove_diffuse=0.8,0.8,0.8,1`
  - `glove_texidx=1`
  - binary delta: `diff_count=11`, `first_diff=672011`, `last_diff=672022`
  - hash: current `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`

### Step 2: Glove tex candidate switch (one-factor)
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_1_to_6_20260217_103220`
- Change:
  - `グローブ` (index=12) `texIdx: 1 -> 6`
  - offset `672076`: `0x01 -> 0x06`
- Verification:
  - binary delta: `diff_count=1`, `first_diff=672076`
  - `byte_before_at_672076=1`, `byte_after_at_672076=6`
  - hash:
    - current `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`
    - backup `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`
  - PMX reread:
    - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=6 (textures\skirt_texture_diffuse.png)`
    - `脚 texIdx=6`
    - `靴 texIdx=-1`

### Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_103253.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_103253.xml`: 未生成

### Rollback (for this follow-up)
1. Step2のみ戻す（texIdx 6 -> 1）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_1_to_6_20260217_103220 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
2. Step1まで含めて戻す（診断赤状態へ）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_restore_20260217_20260217_103145 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `mainTex`, `transparentReason`, `transparentMats`, `brightDiffuseMats`
3. 目視確認
   - `グローブ白` が改善したか
   - `脚` の白/灰混在が悪化しないか
   - `靴` / `スカート` の維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 10:39 UTC) - Glove Diffuse White Lift (0.8 -> 1.0)
- User runtime report (`req-a9710a4b781b4ebf912dc17abae48642`):
  - `グローブ` は `脚` と同じグレー（同一見え）
- Interpretation:
  - 現在 `グローブ` と `脚` は同じ `texIdx=6` を参照し、Diffuseのみが差分ポイント。
  - 1要因で `グローブ` Diffuse を白へ上げて視覚分離を確認。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_080_to_100_20260217_103854`
- One-factor change:
  - material `グローブ` (index=12)
  - Diffuse `0.8,0.8,0.8,1 -> 1.0,1.0,1.0,1`
- Unchanged:
  - `グローブ texIdx=6`
  - `脚 texIdx=6`, `脚 diffuse=0.8,0.8,0.8,1`
  - `靴 texIdx=-1`

### Additional Verification
1. PMX readback
- `グローブ diffuse=1,1,1,1 texIdx=6`
- `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
- `靴 diffuse=1,1,1,1 texIdx=-1`

2. Binary delta (current vs backup)
- `diff_count=9`
- `first_diff=672011`
- `last_diff=672021`
- hash:
  - current: `2736114333A827549D8A79D25ED0E4B48AC4151923FCCBE55AE47C77F7C05033`
  - backup: `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_103927.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_103927.xml`: 未生成

### Rollback (for this follow-up)
1. `グローブ Diffuse 1.0 -> 0.8` を戻す
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_diffuse_080_to_100_20260217_103854 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `diffMax`, `mainTex`, `transparentMats`, `brightDiffuseMats`
3. 目視確認
   - `グローブ` が脚より白く見えるか
   - `脚` の白/灰混在が悪化しないか
   - `靴` / `スカート` の維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 10:54 UTC) - Two-Step Glove Isolation (Diffuse Revert + texIdx 6->0)
- User runtime report (`req-6a8558668d914079af8f19b0f5f3cdf8`):
  - `グローブ` 見た目は変化なし
  - diagnostics上は `グローブ loaded/mainTex=True`, `脚 loaded/mainTex=True`
- Interpretation:
  - `グローブ` と `脚` が同一 `texIdx=6` のため同色化が継続している可能性が高い。

### Step A (cleanup): Glove diffuse revert 1.0 -> 0.8
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepA_glove_diffuse_100_to_080_20260217_105302`
- Change:
  - `グローブ` Diffuse `1,1,1,1 -> 0.8,0.8,0.8,1`
  - offset starts at `672011`
- Verification:
  - `stepA_diff_count=9`, `stepA_first_diff=672011`, `stepA_last_diff=672021`
  - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=6`
  - `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
  - hash current `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`
- Step A test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Unity起動不可（`指定されたモジュールが見つかりません`）
  - `editmode-20260217_105333.log/xml`: 未生成

### Step B (one-factor): Glove texIdx 6 -> 0
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepB_glove_tex_6_to_0_20260217_105350`
- Change:
  - `グローブ` `texIdx: 6 -> 0`
  - offset `672076`: `0x06 -> 0x00`
- Verification:
  - `stepB_diff_count=1`, `stepB_first_diff=672076`
  - `stepB_byte_before_at_672076=6`, `stepB_byte_after_at_672076=0`
  - PMX reread:
    - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=0 (textures\cloth_texture_diffuse.png)`
    - `脚 diffuse=0.8,0.8,0.8,1 texIdx=6 (textures\skirt_texture_diffuse.png)`
    - `靴 texIdx=-1`
  - hash current `A8DC08802D4B49AB121CE55F1337EB80E06AA0E540DBBD4E789B284421384466`
- Step B test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Unity起動不可（`指定されたモジュールが見つかりません`）
  - `editmode-20260217_105420.log/xml`: 未生成

### Rollback (for this follow-up)
1. Step Bのみ戻す（`texIdx 0 -> 6`）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepB_glove_tex_6_to_0_20260217_105350 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
2. Step Aまで戻す（`Diffuse 0.8 -> 1.0` 状態へ）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepA_glove_diffuse_100_to_080_20260217_105302 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `mainTex`, `transparentReason`, `transparentMats`, `brightDiffuseMats`
3. 目視確認
   - `グローブ` が `脚` と分離して見えるか
   - `脚` の白/灰混在が悪化しないか
   - `靴` / `スカート` の維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 13:19 UTC) - Immediate Rollback for Glove Black (texIdx 0 -> 6)
- User runtime report (`req-6477d9bc674248e4b262c27ce0edd55c`):
  - `グローブ` が黒化
  - diagnostics: `グローブ transparentReason=texture_alpha`, `transparentMats=6`
- Decision:
  - 直前変更 `グローブ texIdx 6 -> 0` が黒化要因と判断し、1要因で即時ロールバック。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_0_to_6_rollback_20260217_131828`
- One-factor rollback:
  - `グローブ` `texIdx: 0 -> 6`
  - offset `672076`: `0x00 -> 0x06`
- Unchanged:
  - `グローブ diffuse=0.8,0.8,0.8,1`
  - `脚 texIdx=6`
  - `靴 texIdx=-1`

### Additional Verification
1. Binary delta (current vs rollback backup)
- `diff_count=1`
- `first_diff=672076`
- `byte_before_at_672076=0`
- `byte_after_at_672076=6`
- hash:
  - current: `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`
  - backup: `A8DC08802D4B49AB121CE55F1337EB80E06AA0E540DBBD4E789B284421384466`

2. PMX reread
- `グローブ diffuse=0.8,0.8,0.8,1 texIdx=6 (textures\skirt_texture_diffuse.png)`
- `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
- `靴 texIdx=-1`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_131902.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_131902.xml`: 未生成

### Rollback (for this follow-up)
1. 今回のロールバックを再取り消し（必要時のみ）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_0_to_6_rollback_20260217_131828 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `transparentReason`, `transparentMats`, `brightDiffuseMats`
3. 目視確認
   - `グローブ黒` が解消されたか
   - `脚` の状態維持
   - `靴` / `スカート` の維持
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 16:52 UTC) - StepA/StepB Execution (Glove Split + Shoe Hit Test)
- User runtime report (`req-ccdfbd23d925412d94fda6abdf988719`):
  - `グローブ` と `足の一部` がグレー同系色、`足の一部` は白も残存
- Goal:
  - 1) `グローブ` を `脚` から分離
  - 2) 白部が `靴` 寄与かを識別色で判定

### Step A: Glove texIdx 6 -> 1 (separate from leg)
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepA_glove_tex_6_to_1_20260217_165031`
- One-factor change:
  - `グローブ` `texIdx: 6 -> 1` (`textures\face.png`)
  - offset `672076`: `0x06 -> 0x01`
- Verification:
  - `stepA_diff_count=1`, `stepA_first_diff=672076`
  - PMX reread:
    - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=1`
    - `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
    - `靴 texIdx=-1`
  - hash current: `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Unity起動不可（`指定されたモジュールが見つかりません`）
  - `editmode-20260217_165105.log/xml`: 未生成

### Step B: Shoe diagnostic diffuse -> green (hit test)
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepB_shoe_diffuse_hit_green_20260217_165129`
- One-factor diagnostic change:
  - `靴` Diffuse `1,1,1,1 -> 0,1,0,1`（識別色）
  - offset starts at `672203`
- Verification:
  - `stepB_diff_count=4`, `stepB_first_diff=672205`, `stepB_last_diff=672214`
  - PMX reread:
    - `靴 diffuse=0,1,0,1 texIdx=-1`
    - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=1`
    - `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
  - hash current: `BC8304DE9EA111BDDDBB824BB97850D091D73922EBCCCFE83B9169744E3244AC`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Unity起動不可（`指定されたモジュールが見つかりません`）
  - `editmode-20260217_165205.log/xml`: 未生成

### Step C status
- Pending user runtime observation (判定後に `靴` Diffuse を元へ戻す)

### Rollback (for this follow-up)
1. Step B を戻す（靴緑 -> 元色）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepB_shoe_diffuse_hit_green_20260217_165129 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
2. Step A まで戻す（グローブ texIdx 1 -> 6）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepA_glove_tex_6_to_1_20260217_165031 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload（現在、`靴` が診断用の緑）
2. 目視で確認
   - 足の白い箇所が緑に連動するか（= 靴由来）
   - グローブの見え方（脚と分離したか）
3. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(靴/脚/グローブ)`, `mainTex`, `transparentReason`, `transparentMats`

## Follow-up Update (2026-02-17 17:33 UTC) - Step C Completed (Shoe Green Reverted)
- User runtime report (`req-05a2d728be6f4ea59259019831263207`):
  - `グローブ` 白
  - `靴` のみ緑
- Interpretation:
  - Step B 診断で「足白は靴由来ではない」ことを確認できたため、診断色を即時復帰。

### Step C: Shoe diffuse revert green -> white
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepC_shoe_diffuse_green_to_white_20260217_173322`
- One-factor change:
  - `靴` Diffuse `0,1,0,1 -> 1,1,1,1`
  - offset starts at `672203`

2. Verification
- `stepC_diff_count=4`
- `stepC_first_diff=672205`
- `stepC_last_diff=672214`
- hash:
  - current: `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`
  - backup: `BC8304DE9EA111BDDDBB824BB97850D091D73922EBCCCFE83B9169744E3244AC`
- PMX reread:
  - `グローブ diffuse=0.8,0.8,0.8,1 texIdx=1 (textures\face.png)`
  - `脚 diffuse=0.8,0.8,0.8,1 texIdx=6 (textures\skirt_texture_diffuse.png)`
  - `靴 diffuse=1,1,1,1 texIdx=-1`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_173352.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_173352.xml`: 未生成

### Current Confirmed State
- `グローブ`: `texIdx=1`, `diffuse=0.8,0.8,0.8,1`
- `脚`: `texIdx=6`, `diffuse=0.8,0.8,0.8,1`
- `靴`: `texIdx=-1`, `diffuse=1,1,1,1`

### Rollback (for this follow-up)
1. Step C を再度取り消す（緑へ戻す必要時のみ）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_stepC_shoe_diffuse_green_to_white_20260217_173322 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `mainTex`, `status`, `transparentMats`
3. 目視確認
   - `グローブ` が白で維持されるか
   - `足の一部白` がどの部位に残るか（靴ではないことを再確認）
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 17:49 UTC) - Glove White Fallback Rollback (texIdx 1 -> 6)
- User observation:
  - 「グローブの白は欠損の白」
- Decision:
  - 欠損白回避を最優先し、`グローブ` を `texIdx=1` から `texIdx=6` へ1要因ロールバック。

### Additional Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_1_to_6_for_whitefallback_20260217_174852`
- One-factor change:
  - `グローブ` `texIdx: 1 -> 6`
  - offset `672076`: `0x01 -> 0x06`

### Additional Verification
1. Binary delta
- `diff_count=1`
- `first_diff=672076`
- `byte_before_at_672076=1`
- `byte_after_at_672076=6`
- hash:
  - current: `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`
  - backup: `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`

2. PMX reread
- `グローブ diffuse=0.8,0.8,0.8,1 texIdx=6 (textures\skirt_texture_diffuse.png)`
- `脚 diffuse=0.8,0.8,0.8,1 texIdx=6`
- `靴 diffuse=1,1,1,1 texIdx=-1`

3. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_174925.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_174925.xml`: 未生成

### Rollback (for this follow-up)
1. 今回変更を戻す（`texIdx 6 -> 1`）
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_1_to_6_for_whitefallback_20260217_174852 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Next Action Request (Runtime)
1. 実機で `NurseTaso.pmx` を reload
2. `avatar.model.material_diagnostics` を採取して共有
   - 比較必須: `samples(グローブ/脚/靴)`, `mainTex`, `transparentReason`, `transparentMats`
3. 目視確認
   - `グローブ欠損白` が解消したか
   - `脚` の白/灰混在の残り方
   - 他モデルの中央線/欠け/白飛び再発有無

## Follow-up Update (2026-02-17 18:40 UTC) - Exhaustive Investigation Closure + Source Baseline Restore
- User concern:
  - 同じ操作の反復で進展が見えない
  - グローブ/脚の白・灰混在が継続
- Action:
  - NurseTaso ローカル資産の差分を網羅再調査し、source との差分を全量特定
  - 差分が確認された2ファイルを source 同一へ復元

### Exhaustive Investigation Matrix (All Possibilities)
1. Texture decode capability failure (format unsupported)
- Evidence:
  - 参照テクスチャは `.png` / `.bmp`（`facetoon.bmp`）
  - 他 16-bit PNG (`body/cloth/epron/hair`) は現行でも読み込みされ diagnostics 上 `status=loaded, mainTex=True` を継続
- Verdict:
  - 主因ではない

2. Texture path resolution ambiguity
- Evidence:
  - NurseTaso で参照する主要ファイル名は assets_user 内で重複がほぼなく、`TextureLoader.ResolveTexturePath` の fallback を要しない構成
- Verdict:
  - 主因ではない

3. PMX material mapping drift (material->texture/toon)
- Evidence (runtime vs source PMX):
  - `グローブ texIdx: 3 -> 6`
  - `脚 texIdx: -1 -> 6`
  - `脚 toonIdx: -1 -> 2`
- Verdict:
  - 主因候補（確定）

4. Texture content drift (skirt texture alpha flatten)
- Evidence:
  - `textures/skirt_texture_diffuse.png` hash mismatch（runtime only）
  - source: 16-bit RGBA, alpha0=36.042%
  - runtime(before restore): 8-bit RGBA, alpha0=0%
- Verdict:
  - 主因候補（確定）

5. Global lighting / fixed safeguards regression
- Evidence:
  - 本 follow-up で `MaterialLoader.cs` / `SimpleModelBootstrap.cs` は未編集
  - fixed済み項目（toon fallback / edge cap条件 / F0 Baseline）を変更していない
- Verdict:
  - 今回の不具合要因ではない

### Additional Changes (Model-local only)
1. Restore skirt texture to source
- File:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_restore_source_20260217_183325`
- Result:
  - runtime hash == source hash (`4C4948EC7DF9D8A88BD5D1EECC02AB942EF9CD41FFABBD953842B91D51E16B02`)

2. Restore PMX to source
- File:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_full_20260217_183335`
- Result:
  - runtime hash == source hash (`C59822440DCB7EB195F2F7ECD2677C288A7F6D8CD43D2D943BA0E7477038A881`)

### Post-change Verification
1. Runtime/source file diff (excluding backups)
- Result: none

2. PMX material mapping diff (runtime vs source)
- Result: none

3. skirt alpha distribution
- Result: `alpha0=36.042%`（source同等）

### Commands (This Follow-up)
- 同定/確認
  - `Get-Content .git/HEAD`
  - `Get-Content .git/config`
- 調査
  - `Get-FileHash`（runtime/source PMX・texture）
  - `& ./.tmp_pmx_dump.ps1 -pmxPath ... | ConvertFrom-Json`
  - PNG IHDR/alpha統計確認（PowerShell + `System.Drawing.Bitmap`）
  - byte-level diff確認（PMX runtime vs source）
- 変更
  - `Copy-Item <runtime skirt> <backup> -Force`
  - `Copy-Item <source skirt> <runtime skirt> -Force`
  - `Copy-Item <runtime pmx> <backup> -Force`
  - `Copy-Item <source pmx> <runtime pmx> -Force`
- テスト
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_183406.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_183406.xml`: 未生成

### Rollback (for this follow-up)
1. Restore PMX change only
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_full_20260217_183335 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

2. Restore skirt texture change only
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_restore_source_20260217_183325 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png -Force`

### Next Action Request (Runtime Mandatory)
1. NurseTaso を reload
2. `avatar.model.material_diagnostics` を採取（after）
- 比較対象 before: 最新 `req-ccd045ba18ee41b5a05b68c13d2a6427`
- 必須比較項目:
  - `transparentMats`
  - `transparentByTextureAlphaMats`
  - `toonMissingMats`, `toonMissingSpecMats`
  - `samples`（`脚`,`靴`,`グローブ` の `status/mainTex/toonStatus/transparentReason`）
3. 目視確認
- `脚` / `グローブ` の白灰混在の改善
- `靴` の表示安定
- 他モデルの中央線/欠け/白飛び再発なし

### Decision Rationale
- 現在の症状は共通ローダーではなく、NurseTasoローカル資産ドリフト（PMX 15byte差分 + skirt texture改変）に収束。
- これ以上の `texIdx` 試行を続けるより、source baseline に戻す方が再現性・ロールバック性・他モデル無影響の要件に合致。

### Record Check (This Follow-up)
- Report-Path exists: True (`docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`)
- Repo-Refs / Obsidian-Refs: 既存記載あり
- Obsidian-Log: 既存理由記載あり（未実施理由）
- Execution-Tool / Execution-Agent / Execution-Model: 既存記載あり
- Tags include agent/model/tool: 既存記載あり

## Follow-up Update (2026-02-17 19:10 UTC) - User Requested Immediate Return to Pre-Regression State
- User request:
  - 「まず、今の状態を再発前に戻してから調査してください」
- Action:
  - 直前の source 復元を取り消し、再発前状態（session直前状態）へ戻した。

### Changes (Rollback to Pre-Regression)
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Safety backup (current before rollback):
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_pre_restore_request_20260217_190755`
- Restored from:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_full_20260217_183335`
- Hash after rollback:
  - `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`

2. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png`
- Safety backup (current before rollback):
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_pre_restore_request_20260217_190755`
- Restored from:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_restore_source_20260217_183325`
- Hash after rollback:
  - `DB8390724036D8F1A128E23E1401EB4A79BFEADFC1D580D1123590E2B1B47F9E`

### State Confirmation
- PMX material snapshot after rollback:
  - `スカート texIdx=6`
  - `グローブ texIdx=6`
  - `脚 texIdx=6, toonIdx=2`
  - `靴 texIdx=-1`
- This matches the pre-regression session state used before source restore.

### Commands
- Backup and rollback
  - `Copy-Item ...NurseTaso.pmx ...NurseTaso.pmx.bak_pre_restore_request_20260217_190755 -Force`
  - `Copy-Item ...skirt_texture_diffuse.png ...skirt_texture_diffuse.png.bak_pre_restore_request_20260217_190755 -Force`
  - `Copy-Item ...NurseTaso.pmx.bak_restore_source_full_20260217_183335 ...NurseTaso.pmx -Force`
  - `Copy-Item ...skirt_texture_diffuse.png.bak_restore_source_20260217_183325 ...skirt_texture_diffuse.png -Force`
- Verification
  - `Get-FileHash ...NurseTaso.pmx`
  - `Get-FileHash ...skirt_texture_diffuse.png`
  - `& ./.tmp_pmx_dump.ps1 -pmxPath ... | ConvertFrom-Json`

### Tests
1. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_190814.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_190814.xml`: 未生成

### Rollback (for this follow-up)
1. Revert this rollback (return to source-restored state)
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_pre_restore_request_20260217_190755 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_pre_restore_request_20260217_190755 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png -Force`

### Next Step
- From this restored state, continue with read-only exhaustive investigation only (no edits) as requested.

## Follow-up Update (2026-02-17 19:40 UTC) - Read-only Exhaustive Investigation (No Edits)
- User runtime baseline:
  - request_id: `req-c4df733b5e544d4a8e5d55de118c2de5`
  - `transparentMats=5`, `transparentByTextureAlphaMats=5`
  - `脚: status=loaded, mainTex=True, toonStatus=loaded`
  - `靴: status=missing_spec, mainTex=False`
- User visual status:
  - 正常: `靴/スカート/手首/足(一部)`
  - 未解消: `グローブ=グレー`, `足(欠損)=ほぼ白`

### Investigation Scope
- No file edits (read-only only)
- Targeted materials only: `脚`, `グローブ`, `靴`, `スカート`

### Findings
1. Current PMX mapping state (hash=`53EA6B7B...`)
- `スカート texIdx=6`
- `グローブ texIdx=6`
- `脚 texIdx=6, toonIdx=2`
- `靴 texIdx=-1`

2. UV range validation (current PMX)
- `脚`: UV out-of-range `0/2898 (0%)`
- `グローブ`: UV out-of-range `0/2088 (0%)`
- `スカート`: UV out-of-range `0/2868 (0%)`
- 結論: Clamp/Repeat問題ではない

3. UV-sampled actual color on current mapping (`texIdx=6`, skirt texture)
- `脚` sampled avg: `rgb=162.9,166.0,169.8`（明るい）
- `グローブ` sampled avg: `rgb=27.2,27.3,27.5`（かなり暗い）
- 結論: 同じ `texIdx=6` でも UV 領域差で見えが分離している
  - 脚は白寄りに見える
  - グローブは灰/黒寄りに見える

4. Candidate texture simulation (UV固定で texIdx を差し替えた場合の期待値)
- `texIdx=1 (face.png)`
  - `脚`: `rgb=255.0,250.0,237.0 a=255`
  - `グローブ`: `rgb=255.0,249.3,236.0 a=255`
- `texIdx=0 (cloth)`
  - `脚`: `rgb=129.2,129.5,131.3 a=149.8`
  - `グローブ`: `rgb=126.1,128.5,136.7 a=156.7`
- `texIdx=3 (body)`
  - `脚`: `rgb=159.2,153.5,149.7 a=174.7`
  - `グローブ`: `rgb=103.8,101.6,101.1 a=148.6`
- `texIdx=6 (current)`
  - `脚`: `rgb=162.9,166.0,169.8 a=255`
  - `グローブ`: `rgb=27.2,27.3,27.5 a=255`

### Root Cause (for current unresolved parts)
- `脚` と `グローブ` が同じ `texIdx=6` を共有しており、
  UV領域が別のため sampled RGB が大きく乖離している。
- その結果、
  - `脚` は白寄り（足欠損部が白に見えやすい）
  - `グローブ` は灰/黒寄り
  が同時発生する。

### High-confidence Next Candidate (Not applied)
- One-factor candidate:
  - `グローブ texIdx: 6 -> 1` のみ
- Rationale:
  - `脚` を触らず、未解消の `グローブ` のみを分離できる
  - `face.png` は alpha=255 で透明化由来の黒化リスクが低い
  - 他正常部位（靴/スカート/手首）へ影響しない最小差分

### Commands (read-only)
- PMX backup matrix extraction
  - `& ./.tmp_pmx_dump.ps1 -pmxPath ... | ConvertFrom-Json`
- UV range extraction
  - `& ./.tmp_pmx_uvdump.ps1 -pmxPath ...`
- UV color sampling by texture candidate
  - `& ./.tmp_uv_texture_candidates.ps1 -pmxPath ... -texRoot ...`
  - `& ./.tmp_uv_color_sample.ps1 -pmxPath ... -texPathCurrent ... -texPathSource ...`

### Tests
- No code or asset edits in this follow-up
- Unity test not rerun (read-only investigation phase)

### Decision
- 現在は調査のみ完了。変更は未適用。
- 次に進める場合は `グローブ texIdx 6->1` の1要因だけを実施する。

## Follow-up Update (2026-02-17 20:12 UTC) - One-factor Apply: Glove texIdx 6 -> 1
- Approval basis:
  - read-only調査結果に基づく high-confidence candidate を1要因のみ適用
- Baseline diagnostics (before):
  - request_id: `req-c4df733b5e544d4a8e5d55de118c2de5`

### Change
1. File
- `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_6_to_1_candidate_20260217_201021`

2. One-factor edit
- material `グローブ` only
- `texIdx: 6 -> 1`
- byte offset: `672076` (`0x06 -> 0x01`)

### Verification
1. Binary delta vs backup
- `diff_count=1`
- `first_diff=672076`
- `last_diff=672076`

2. PMX readback (`.tmp_pmx_dump.ps1`)
- `スカート texIdx=6`
- `グローブ texIdx=1 (textures\face.png)`
- `脚 texIdx=6, toonIdx=2`
- `靴 texIdx=-1`

3. Hash
- before: `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`
- after:  `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`

### Tests
1. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_201049.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_201049.xml`: 未生成

### Rollback
1. Revert this one-factor change
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_glove_tex_6_to_1_candidate_20260217_201021 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

### Runtime Request (After)
1. NurseTaso reload
2. Capture diagnostics (after)
- compare against before `req-c4df733b5e544d4a8e5d55de118c2de5`
- required fields:
  - `transparentMats`, `transparentByTextureAlphaMats`
  - `toonMissingMats`, `toonMissingSpecMats`
  - `samples(脚/グローブ/靴/スカート)`
3. Visual check
- `グローブ`（灰/黒の改善有無）
- `足(欠損)` の白寄りの変化有無
- `靴/スカート/手首/足(正常部)` が維持されるか

## Follow-up Update (2026-02-17 20:25 UTC) - Emergency RCA for Other-model Face Centerline Regression
- Trigger:
  - User report: 他モデル（`amane_kanata.pmx`）で顔中心線再発
  - diagnostics: `req-efc60530b2314f75b352e393b7cc6146`

### Immediate Containment
1. Nurse-side latest test change rollback
- File:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Action:
  - `NurseTaso.pmx.bak_glove_tex_6_to_1_candidate_20260217_201021` から復元
- Result hash:
  - `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`

### RCA (Read-only)
1. Official model asset drift check
- `amane_kanata.pmx` hash comparison
  - runtime vs cleanup_archive originals: all identical
  - hash: `1E6F2DDC0D486FA8CDCD88F5714A188BBE1896F311D4A897669543D451B4FA6C`
- Conclusion:
  - asset drift is not the cause for this regression

2. MaterialLoader intent check
- `MaterialLoader.cs` confirms intended behavior:
  - `toonStatus == loaded_fallback_white` -> `_MascotEdgeContributionCap = 0f`
  - ref: `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:217-220`

3. Shader-side effective behavior check (root cause)
- Outline fragment code:
  - `half edgeContributionCap = _MascotEdgeContributionCap <= 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
  - ref: `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc:45`
- Impact:
  - when loader sets cap to `0`, shader forces it back to `1.0`
  - so fallback-white toon meshes still get full outline contribution
  - this can reproduce centerline on face materials even though diagnostics counters remain normal

4. Shader property presence check
- All relevant outline shaders declare `_MascotEdgeContributionCap`
  - e.g. `MeshPmdMaterialOutline-CullBack-NoCastShadow.shader:29`
- Conclusion:
  - issue is not missing property; it is fragment condition logic

### Diagnostic Correlation
- `req-efc605...` shows sampled face materials using outline shaders and `toonStatus=loaded_fallback_white`
- This matches the condition path where cap should be 0 but is effectively overridden to 1 by shader code

### Commands (RCA)
- `Get-Content .../MaterialLoader.cs`
- `Select-String` over shader sources for `_MascotEdgeContributionCap`
- line-number dump for:
  - `MeshPmdMaterialVertFrag.cginc`
  - `MaterialLoader.cs`
  - outline shader property block
- hash compare for `amane_kanata.pmx` with cleanup archive originals

### Tests
- No code edits in this RCA follow-up
- Unity editmode test not rerun in this read-only RCA step

### Decision
- Root cause confirmed at shader line-level.
- Next minimal fix candidate (not yet applied):
  - `MeshPmdMaterialVertFrag.cginc` line 45 conditional update so `cap=0` remains 0.

## Follow-up Update (2026-02-17 23:20 UTC) - Hotfix for Other-model Face Centerline Regression
- Trigger:
  - user report + diagnostics `req-efc60530b2314f75b352e393b7cc6146`
- RCA summary:
  - loader sets `_MascotEdgeContributionCap=0` on `toonStatus=loaded_fallback_white`
  - shader fragment path treated `<=0` as fallback-to-1, canceling intended cap

### Code Change (one-line, one-factor)
1. File
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`

2. Diff intent
- Keep backward-compat for negative sentinel only, while honoring valid `0` cap.

3. Actual diff
- before:
  - `half edgeContributionCap = _MascotEdgeContributionCap <= 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
- after:
  - `half edgeContributionCap = _MascotEdgeContributionCap < 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`

### Verification
1. Line verification
- `MeshPmdMaterialVertFrag.cginc:45` updated as above.

2. Nurse containment check (already done in prior RCA step)
- `NurseTaso.pmx` restored to pre-candidate hash:
  - `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`

### Commands
- Edit:
  - `apply_patch` on `MeshPmdMaterialVertFrag.cginc`
- Verify:
  - line dump around `MeshPmdMaterialVertFrag.cginc:45`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_231645.log`: 未生成
  - `Unity_PJ/artifacts/test-results/editmode-20260217_231645.xml`: 未生成

### Rollback
1. Revert one-line hotfix
- restore line in `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
  - from `< 0.0 ? 1.0 : ...`
  - back to `<= 0.0 ? 1.0 : ...`

### Runtime Validation Request
1. Reopen model with known regression (`amane_kanata.pmx`)
2. Capture diagnostics (after):
- compare to before `req-efc60530b2314f75b352e393b7cc6146`
3. Visual checks:
- face centerline disappearance
- no regression on NurseTaso unresolved areas (glove/leg)
- no white blowout increase

## Follow-up Update (2026-02-17 23:26 UTC) - Diagnostics Exhaustive Capture for Missing Main Texture
- Trigger:
  - unresolved NurseTaso white/gray artifacts and repeated inability to pinpoint exact missing material from `samples`
- Scope:
  - diagnostics-only change (no rendering/shader/PMX/light behavior change)

### Code Change (one-factor)
1. File
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

2. Diff intent
- Ensure materials with missing main texture are always included in `avatar.model.material_diagnostics.samples`.
- This removes blind spots when missing materials are opaque/non-priority and were previously excluded.

3. Actual change
- Added local flag:
  - `hasMissingMainTexture`
- Set `hasMissingMainTexture = true` inside existing branch:
  - `ShouldInspectMainTextureMaterial(...) && material.mainTexture == null`
- Added `hasMissingMainTexture` to sample include condition:
  - `if (forceIncludeMaterialSample || hasMissingMainTexture || ...)`

### Why this is safe
- No material/shader property values are changed.
- No PMX or texture references are edited.
- No light/render-factor tuning is changed.
- Impact is limited to diagnostics sample selection only.

### Commands
- Verify patch location:
  - `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs | Select-Object -Skip 1080 -First 210`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_232132.log`
  - `Unity_PJ/artifacts/test-results/editmode-20260217_232132.xml`

### Rollback
1. Revert this diagnostics-only change
- Remove `hasMissingMainTexture` variable and revert sample include condition in:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

### Runtime Request (after)
1. Reload NurseTaso and capture:
- `avatar.model.material_diagnostics`
- `avatar.model.missing_spec_materials`
- `avatar.model.missing_main_textures`
2. Compare against latest stable before:
- `req-c4df733b5e544d4a8e5d55de118c2de5`
3. Confirm whether the unresolved parts (`グローブ`, `足欠損部`) now appear explicitly in `samples` with status.

## Follow-up Update (2026-02-17 23:31 UTC) - Rollback to Pre-centerline-investigation State (User Requested)
- Trigger:
  - user requested immediate rollback to stage where centerline had not been visible.
- Approval:
  - `実行`

### Rollback Scope (one-factor by file)
1. Diagnostics-only capture rollback
- File:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Reverted:
  - removed `hasMissingMainTexture` local flag
  - removed `hasMissingMainTexture` from material sample include condition
- Effect:
  - restores pre-23:26 diagnostics sampling behavior (no rendering change intended)

2. Centerline hotfix rollback
- File:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- Reverted line:
  - from: `_MascotEdgeContributionCap < 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
  - to:   `_MascotEdgeContributionCap <= 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
- Effect:
  - restores pre-23:20 shader condition exactly.

### Verification
- `SimpleModelBootstrap.cs` no longer contains `hasMissingMainTexture`
- `MeshPmdMaterialVertFrag.cginc:45` equals `<= 0.0 ? 1.0 : ...`

### Commands
- `apply_patch` (2 files, rollback-only)
- `Select-String` for line verification
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_233120.log`
  - `Unity_PJ/artifacts/test-results/editmode-20260217_233120.xml`

### Rationale
- fastest path requested by user was rollback, not further forward changes.
- rollback was applied in smallest reversible units (file-by-file).

### Roll-forward (if needed)
1. Reapply centerline hotfix
- `MeshPmdMaterialVertFrag.cginc` line 45
  - `<= 0.0` -> `< 0.0`
2. Reapply diagnostics exhaustive capture
- restore `hasMissingMainTexture` flag and include condition in `SimpleModelBootstrap.cs`

## Follow-up Update (2026-02-17 23:38 UTC) - Restore Original Centerline Settings (User Requested)
- Trigger:
  - user requested immediate restoration to original settings and to stop root-cause exploration.
- Approval:
  - direct user instruction: `元の設定に復帰させてください`

### Change (single-factor)
1. File
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`

2. Restored line
- from:
  - `_MascotEdgeContributionCap <= 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
- to:
  - `_MascotEdgeContributionCap < 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`

### Intent
- Restore the original centerline suppression behavior where valid cap `0` remains effective.
- No other settings/files changed.

### Verification
- `MeshPmdMaterialVertFrag.cginc:45` confirmed as `< 0.0 ? 1.0 : ...`

### Tests
1. Unity EditMode test
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - 失敗（Unity起動不可）
- Error:
  - `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260217_233814.log`
  - `Unity_PJ/artifacts/test-results/editmode-20260217_233814.xml`

### Rollback
- revert line 45 back to `<= 0.0 ? 1.0 : ...` in same file.

## Follow-up Update (2026-02-18 00:09 UTC) - Final Runtime Verification (Official model)

- Trigger:
  - user requested rollback to the state referenced at `line 2002` (no extra processing).
- Approval:
  - `実行`

### Changes (Rollback-only, no extra edits)
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Restored from:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_restore_source_full_20260217_183335`
- Hash after restore:
  - `53EA6B7B8A71BA0A75F30FDC9CA027DD1720C3D32B030FACB6A5726279193959`

2. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png`
- Restored from:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png.bak_restore_source_20260217_183325`
- Hash after restore:
  - `DB8390724036D8F1A128E23E1401EB4A79BFEADFC1D580D1123590E2B1B47F9E`

3. Centerline-related files verification
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc:45`
  - `half edgeContributionCap = _MascotEdgeContributionCap < 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `hasMissingMainTexture` is not present.

### Commands
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `Get-Content .../2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md (line checks)`
- `Get-FileHash` for runtime/backup PMX + PNG
- `Copy-Item ...NurseTaso.pmx.bak_restore_source_full_20260217_183335 ...NurseTaso.pmx -Force`
- `Copy-Item ...skirt_texture_diffuse.png.bak_restore_source_20260217_183325 ...skirt_texture_diffuse.png -Force`
- `Select-String` for shader line / `hasMissingMainTexture`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - failed to launch Unity (`指定されたモジュールが見つかりません`)
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260218_030215.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260218_030215.xml` (missing)

### Decision Rationale
- User requested pure rollback without additional processing.
- The selected backup files are explicitly tied in this report to the pre-regression snapshot used around the `line 2002` checkpoint.
- Verification was kept to hash/line checks only to avoid introducing unrelated edits.

### Next Action
1. Runtime reload verification by user for:
- NurseTaso texture-missing symptoms
- face centerline absence on official model

### Rollback Policy
1. If this rollback must be undone:
- restore from current pre-change copies if available, or copy from the previous runtime backup set used before this operation.
2. For centerline settings:
- only line 45 in `MeshPmdMaterialVertFrag.cginc` should be toggled (`< 0.0` vs `<= 0.0`) if explicitly requested.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills: `none`
- Repo-Refs:
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/skirt_texture_diffuse.png`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0303.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, rollback, nurse_taso, centerline]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-18 03:14 JST) - Rollback to Pre-recurrence State (User Requested)
- Trigger:
  - user requested rollback from latest to just before centerline recurrence report.
- Basis:
  - recurrence report point: `2026-02-17 20:25 UTC` section.
  - target checkpoint: immediate previous update `2026-02-17 20:12 UTC`.
- Approval:
  - `実行`

### Changes (minimal rollback only)
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup before change:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_pre_recurrence_restore_20260218_031414`
- One-byte restore:
  - offset `672076`: `0x06 -> 0x01` (`グローブ texIdx 6 -> 1`)
- Hash after restore:
  - `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`

2. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- Backup before change:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc.bak_pre_recurrence_restore_20260218_031415`
- Restored line 45:
  - from: `_MascotEdgeContributionCap < 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`
  - to:   `_MascotEdgeContributionCap <= 0.0 ? 1.0 : saturate(_MascotEdgeContributionCap);`

3. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Verification only:
  - `hasMissingMainTexture` not present (kept as target checkpoint behavior).

### Verification
- `NurseTaso.pmx` hash = `C6F1ACE58BC7492FDE9972313572603D90BF2C1500D5B30D79F73F5290B4BDA0`
- `MeshPmdMaterialVertFrag.cginc:45` = `<= 0.0 ? 1.0 : ...`
- `SimpleModelBootstrap.cs` has no `hasMissingMainTexture`

### Commands
- `Get-FileHash .../NurseTaso.pmx`
- `Select-String .../MeshPmdMaterialVertFrag.cginc -Pattern edgeContributionCap`
- `Select-String .../SimpleModelBootstrap.cs -Pattern hasMissingMainTexture`
- `Copy-Item ...NurseTaso.pmx ...NurseTaso.pmx.bak_pre_recurrence_restore_20260218_031414 -Force`
- `Copy-Item ...MeshPmdMaterialVertFrag.cginc ...MeshPmdMaterialVertFrag.cginc.bak_pre_recurrence_restore_20260218_031415 -Force`
- byte write (`offset 672076`) for `NurseTaso.pmx`
- `apply_patch` on `MeshPmdMaterialVertFrag.cginc` (line 45 only)
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - failed to launch Unity (`指定されたモジュールが見つかりません`)
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260218_031441.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260218_031441.xml` (missing)

### Decision Rationale
- User intent was strict rollback to the latest state immediately before recurrence.
- This checkpoint requires only two concrete reversions (PMX one-byte + shader one-line).
- No extra tuning or diagnostic feature changes were introduced.

### Rollback Policy
1. Revert this operation
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_pre_recurrence_restore_20260218_031414 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc.bak_pre_recurrence_restore_20260218_031415 Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc -Force`

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills: `none`
- Repo-Refs:
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0315.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, rollback, pre-recurrence, centerline]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-18 04:02 JST) - NurseTaso Texture Load Path Audit (non-texIdx)
- Trigger:
  - user requested audit of texture load route excluding texIdx edits (`None` / dummy-like observations already confirmed by user).
- Approval:
  - `実行`

### Scope
1. Read-only investigation only
- No code/config change.
- Verify load route correctness from:
  - `SimpleModelBootstrap` -> `AssetPathResolver` -> `ReflectionModelLoaders` -> `MmdGameObject` -> `TextureLoader` -> `MaterialLoader`.

### Findings
1. Model path resolution route is valid
- `AssetPathResolver.ResolveRelative` resolves from canonical root first (`assets_user`) then `StreamingAssets`.
- Relevant code:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs:36`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs:75`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs:97`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:343`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:352`

2. PMX->TextureLoader route is valid for NurseTaso
- `MmdGameObject` uses PMX parent directory as `TextureLoader` base path.
- Relevant code:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:354`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:364`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:365`

3. PMX texture references are structurally clean
- `NurseTaso.pmx` texture table has 7 entries; all ASCII, no whitespace:
  - `textures\\cloth_texture_diffuse.png`
  - `textures\\face.png`
  - `textures\\facetoon.bmp`
  - `textures\\body_texture_diffuse.png`
  - `textures\\epron_texture_diffuse.png`
  - `textures\\hair_tex_diffuse.png`
  - `textures\\skirt_texture_diffuse.png`
- Relevant decode/assignment code:
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:384`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:392`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:516`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:519`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:521`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:524`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:535`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs:538`

4. All referenced texture paths resolve via model-relative candidate
- `TextureLoader.ResolveTexturePath` first successful strategy is `model_relative` for all 7 referenced textures.
- Files exist under:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/textures/*.png|*.bmp`
- Relevant resolution code:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:213`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:311`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:370`

5. Decode compatibility check passed (file-level)
- `System.Drawing` read succeeded for all referenced files (`png`/`bmp`) with valid dimensions.
- No decode-fail indicator observed in file-level check.
- Relevant decode path code:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:638`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:643`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:719`

6. `None` origin in current NurseTaso is `missing_spec`, not `missing_resolve`
- Current PMX-based expected counts (read-only reconstruction):
  - main: `loaded_candidate=9`, `missing_spec=6`, `missing_resolve=0`
  - toon: `loaded_candidate=2`, `missing_spec=13`, `missing_resolve=0`
- `MaterialLoader` behavior confirms this mapping:
  - requested path empty => `missing_spec`
  - requested path set but load null => `missing_resolve`
- Relevant code:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:292`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:307`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:108`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:136`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:232`

7. Historical evidence in-repo is consistent with this result
- Prior records repeatedly report Nurse系 as `resolve=0` with `spec>0`, i.e. path resolution failure is not dominant.
- References:
  - `docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md:28`
  - `docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md:32`
  - `docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md:44`
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md:559`
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md:1588`

### Investigation Conclusion
- In current repository state, NurseTaso texture loading route (path resolution + file presence + decode path selection) is valid.
- For `mainTexture=None` observations, dominant cause is `missing_spec` materials (texture unspecified in PMX), not non-texIdx path/resolve failure.
- Therefore, no additional non-texIdx loading defect was confirmed in this audit.

### Commands (executed)
- skill docs:
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- code route inspection:
  - `Get-Content` / `Select-String` on `TextureLoader.cs`, `MaterialLoader.cs`, `MmdGameObject.cs`, `PmxReader.cs`, `AssetPathResolver.cs`, `SimpleModelBootstrap.cs`, tests
- PMX data checks:
  - `./.tmp_pmx_dump.ps1 -pmxPath .../NurseTaso.pmx`
  - file/path existence and strategy reconstruction (`model_relative`)
- decode sanity:
  - `System.Drawing.Image` metadata reads for referenced textures
- test command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test
- Result:
  - failed to launch Unity (`指定されたモジュールが見つかりません`)
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260218_040253.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260218_040253.xml` (missing)

### Next Action
1. Runtime trace confirmation (if environment is available)
- run with `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1`
- capture `[TextureLoader] resolve ...` and `avatar.model.missing_main_textures` for current NurseTaso snapshot.

### Rollback Policy
1. This update is documentation-only.
- rollback is deleting this follow-up section if needed.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `bug-investigation`
  - `worklog-update`
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
  - `docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md`
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0402.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, investigation, nurse_taso, texture-loading, path-audit]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-18 04:15 JST) - Cross-model Review for Root-cause Validity
- Trigger:
  - user requested review across other models to validate whether NurseTaso-only handling is non-root.
- Approval:
  - `実行`

### Review Scope (read-only)
1. Code audit for model-specific branches
- Search in runtime loader/material path code for model-name conditions.

2. Data audit across all PMX in `assets_user`
- Target models (6):
  - `characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
  - `characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx`
  - `characters/azki/official_v1/mmd_pkg/mmd/AZKi_4th_src/AZKi_4th.pmx`
  - `characters/momosuzu_nene/bea_v1/mmd_pkg/mmd/PMX/momosuzu_nene_BEA.pmx`
  - `characters/momosuzu_nene/std_v1/mmd_pkg/mmd/PMX/momosuzu_nene_STD.pmx`
  - `characters/sakamata_chloe/official_v1/mmd_pkg/mmd/SakamataChloe_src/SakamataChloe.pmx`

### Findings (ordered by severity)
1. [High] Main-texture fallback policy is effectively model-dependent by material-name heuristic
- File:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:327`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:333`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:344`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:352`
- Detail:
  - `missing_spec` fallback to white is limited to names containing `"shadow"` and low alpha (`<=0.25`).
  - This means behavior differs by naming convention, not only by render semantics.
- Cross-model evidence:
  - `NurseTaso` `missing_spec` materials are non-shadow names (`舌`,`口`,`歯`,`まつげ`,`眉`,`靴`) -> no white fallback path.
  - `amane_kanata` and `momosuzu_nene` have `*_Shadow*` missing-spec materials -> fallback path can apply.
  - Therefore, a NurseTaso-only patch would hide a shared policy gap.

2. [Medium] Cross-model PMX audit shows resolution failures are not the dominant issue
- Dataset result (all 6 models):
  - `MainResolve=0` on all models (no unresolved main-texture paths by current resolver strategy audit).
  - `MainSpec>0` appears in multiple models:
    - NurseTaso: `MainSpec=6`
    - amane_kanata: `MainSpec=1`
    - momosuzu_nene_BEA/STD: `MainSpec=1`
    - AZKi/SakamataChloe: `MainSpec=0`
- Implication:
  - Path-resolution fixes alone are non-root for this class of symptoms.

3. [Medium] Material diagnostic names still rely on fallback `material.name` when tag is absent
- File:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:1402`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:1408`
- Detail:
  - `MASCOT_MMD_MATERIAL_NAME` tag is read, but when absent diagnostics fallback to Unity material name.
  - This weakens comparability across models/sessions and increases ambiguity in cross-model RCA.

4. [Low] Runtime code has no explicit per-model branch (except default config path)
- Evidence:
  - model-name search in runtime code found no model-conditional logic in loading/material pipeline.
  - only default config value references one model path:
    - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs:9`
- Implication:
  - current non-root behavior is caused by heuristic policy, not hardcoded model IDs.

### Cross-model PMX Summary
- `NurseTaso`: `Mats=15, MainLoaded=9, MainSpec=6, MainResolve=0, ToonLoaded=2, ToonSpec=13, ToonResolve=0`
- `amane_kanata`: `Mats=16, MainLoaded=15, MainSpec=1, MainResolve=0, ToonLoaded=0, ToonSpec=16, ToonResolve=0`
- `AZKi_4th`: `Mats=33, MainLoaded=33, MainSpec=0, MainResolve=0, ToonLoaded=0, ToonSpec=33, ToonResolve=0`
- `momosuzu_nene_BEA`: `Mats=19, MainLoaded=18, MainSpec=1, MainResolve=0, ToonLoaded=0, ToonSpec=19, ToonResolve=0`
- `momosuzu_nene_STD`: `Mats=18, MainLoaded=17, MainSpec=1, MainResolve=0, ToonLoaded=0, ToonSpec=18, ToonResolve=0`
- `SakamataChloe`: `Mats=36, MainLoaded=36, MainSpec=0, MainResolve=0, ToonLoaded=0, ToonSpec=36, ToonResolve=0`

### Review Conclusion
- User concern is valid: NurseTaso-only補完 is not root.
- Root issue is common fallback policy for `missing_spec` materials (name/alpha heuristic), which yields model-dependent outcomes.
- Recommended next phase should be common-policy redesign, not character-specific exceptions.

### Commands
- Skill references:
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
- Code scans:
  - `Select-String` over `Assets/**/*.cs|shader|cginc` for model-specific conditions
  - targeted `Get-Content` for `MaterialLoader.cs`, `SimpleModelBootstrap.cs`, tests
- Data scans:
  - enumerate PMX under `Unity_PJ/data/assets_user`
  - `./.tmp_pmx_dump.ps1` per PMX + resolve-audit scripts for spec/resolve counts

### Tests
1. Not executed (review-only, no code edits)
- existing known limitation remains: Unity runner unavailable in this environment.

### Next Action
1. Design common fallback policy for `missing_spec` without model-name dependence.
2. Add tests covering:
- non-shadow `missing_spec` material behavior
- cross-model regression guards (NurseTaso + amane_kanata + AZKi + nene + chloe)

### Rollback Policy
1. This update is documentation-only.
- rollback = remove this follow-up section.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `code-review`
  - `bug-investigation`
- Repo-Refs:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
  - `Unity_PJ/data/assets_user/characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx`
  - `Unity_PJ/data/assets_user/characters/azki/official_v1/mmd_pkg/mmd/AZKi_4th_src/AZKi_4th.pmx`
  - `Unity_PJ/data/assets_user/characters/momosuzu_nene/bea_v1/mmd_pkg/mmd/PMX/momosuzu_nene_BEA.pmx`
  - `Unity_PJ/data/assets_user/characters/momosuzu_nene/std_v1/mmd_pkg/mmd/PMX/momosuzu_nene_STD.pmx`
  - `Unity_PJ/data/assets_user/characters/sakamata_chloe/official_v1/mmd_pkg/mmd/SakamataChloe_src/SakamataChloe.pmx`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0415.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, review, cross-model, root-cause]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`


## Follow-up Update (2026-02-18 09:20 JST) - Non-dependent fallback implementation and impact check
- Trigger:
  - user requested implementation of a non-dependent design and impact confirmation.
- Approval:
  - `実行`

### Scope
1. Implement root-side policy change (no model/material-name dependence) for `missing_spec` main texture fallback.
2. Update EditMode tests to match uniform policy.
3. Re-run target tests and capture result.

### Changes Applied
1. Unified fallback condition in MaterialLoader
- File:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:327`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs:335`
- Details:
  - `ShouldUseWhiteFallbackMainTexture(...)` now returns `mmdMaterial != null`.
  - Removed dependency on material name (`shadow`) and alpha heuristic from fallback decision.
  - `ResolveWhiteFallbackMainTextureReason(...)` now returns `"uniform_missing_spec"` for non-null material and `"unknown"` for null.

2. Updated tests for common-policy behavior
- File:
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:452`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:466`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:480`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:494`
- Details:
  - Opaque/Transparent/Shadow `missing_spec` tests now expect fallback `True`.
  - Added null-material guard test expecting fallback `False`.

### Rationale
- Root issue identified in prior review was policy dependence on naming conventions, which creates cross-model inconsistency.
- This change enforces one shared rule for `missing_spec`, minimizing model-specific branching risk.

### Commands (executed)
- Repo identity (git unavailable -> fallback path):
  - `git rev-parse --show-toplevel` (failed: git not found)
  - `git remote -v` (failed: git not found)
  - `git status --short` (failed: git not found)
  - `Get-Content .git/HEAD`
  - `Get-Content .git/config`
- Change verification:
  - `Select-String -Path Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Pattern 'uniform_missing_spec|ShouldUseWhiteFallbackMainTexture|ResolveWhiteFallbackMainTextureReason' -Context 2,2`
  - `Select-String -Path Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Pattern 'MaterialLoaderWhiteFallback_UsesMissingSpecFallbackForOpaqueMaterial|MaterialLoaderWhiteFallback_UsesMissingSpecFallbackForTransparentMaterial|MaterialLoaderWhiteFallback_UsesMissingSpecFallbackForShadowMaterial|MaterialLoaderWhiteFallback_DoesNotUseFallbackWhenMaterialIsNull' -Context 0,4`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test (targeted)
- Result:
  - Unity executable launch failure (`指定されたモジュールが見つかりません`).
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260218_091926.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260218_091926.xml` (missing)

### Impact / Risk
1. Expected impact
- `missing_spec` materials across models now follow a uniform white fallback policy when material is present.

2. Remaining risk
- Without runtime execution, visual impact confirmation is pending.
- Environment issue blocks automated test pass/fail confirmation.

### Next Action
1. Restore Unity runtime dependency on this machine, then rerun EditMode tests.
2. Validate NurseTaso rendering and confirm centerline artifact behavior in runtime.

### Rollback Policy
1. Revert this logic only:
- `ShouldUseWhiteFallbackMainTexture(...)` in `MaterialLoader.cs` to previous heuristic.
- associated test expectations in `SimpleModelBootstrapTests.cs`.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `bug-investigation`
  - `worklog-update`
- Repo-Refs:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `tools/run_unity_tests.ps1`
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0920.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, implementation, non-dependent, fallback-policy, nurse_taso]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-18 09:47 JST) - Model hierarchy leak on model switch (nene/chloe)
- Trigger:
  - user reported that `model:next` increases `Model` objects under `Untitled` for `momosuzu_nene` (2 variants) and `SakamataChloe`.
- Approval:
  - `実行`

### Scope
1. Reproduce route in code and identify why old `Model` entries remain.
2. Apply minimal root-cause fix without changing model selection behavior.
3. Re-run available tests.

### Root Cause
1. Orphan `Model` object creation in LibMMD split-mesh path
- File:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:310`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:314`
- Detail:
  - `CreatePartObjects()` created `new GameObject("Model")` per load and used it as `SkinnedMeshRenderer.rootBone`.
  - The object was not parented to the PMX root, so it stayed in hierarchy as top-level orphan.
  - Repeated `model:next` therefore accumulated visible `Model` entries.

### Changes Applied
1. Reuse owned bone root for part meshes
- File:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:310`
- Diff intent:
  - if `_boneRootGameObject` is null, create one `Model` and parent it to current `transform`.
  - set part renderers' `rootBone` to `_boneRootGameObject.transform`.
  - stop creating unparented temporary `Model` root per reload.

### Commands (executed)
- Route inspection:
  - `Get-ChildItem ... | Select-String 'model:next|LoadModel|Destroy\(|RegisterActiveModelRoot|new GameObject\("Model"\)'`
  - `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` (target sections)
  - `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
  - `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs` (target sections)
- Model list sanity:
  - `Get-ChildItem Unity_PJ/data/assets_user -Recurse -Filter *.pmx | Where-Object { ... }`
- Test:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test (targeted)
- Result:
  - Unity executable launch failure (`指定されたモジュールが見つかりません`).
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260218_094704.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260218_094704.xml` (missing)

### Impact / Risk
1. Expected impact
- PMX split-mesh path no longer leaks top-level `Model` GameObjects on reload/switch.
- Model switching behavior and asset resolution flow are unchanged.

2. Remaining risk
- Runtime verification is pending due local Unity environment issue.

### Next Action
1. After Unity environment repair, run runtime check:
- initial state: no extra `Model`
- repeat `model:next` across all candidates
- verify hierarchy keeps one active model root only.

### Rollback Policy
1. Revert the `CreatePartObjects()` block in:
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `bug-investigation`
  - `worklog-update`
- Repo-Refs:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `tools/run_unity_tests.ps1`
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_0947.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, bugfix, hierarchy-leak, pmx, model-switch]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-18 14:05 JST) - User verification passed and task closed
- Trigger:
  - user reported "改善していました" and requested closure + record.
- Approval:
  - `記録しておいてください`

### Scope
1. Record closure status only.
2. No code/config changes.

### Result
1. Manual verification accepted
- User confirmed hierarchy leak symptom is improved (no ongoing Model accumulation concern).

2. Task status
- This issue is closed.

### Commands (executed)
- `Set-Content D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_1405.md`
- `Add-Content docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`

### Tests
1. User runtime verification
- Result: Passed (reported by user).

### Next Action
1. None (closed).

### Rollback Policy
1. Documentation-only update.
- rollback = remove this follow-up section.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `worklog-update`
- Repo-Refs:
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260218_1405.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, closure, verification]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-19 00:23 JST) - Texture issue resolved by model-side fix (user report)
- Trigger:
  - user reported texture issue is resolved by model modification and requested record save.
- Approval:
  - `記録として保存`

### Scope
1. Record closure status only.
2. No code/config change.

### Result
1. Resolution status
- Texture issue has been resolved by model-side correction (reported by user).

2. Task status
- Marked as resolved and recorded.

### Commands (executed)
- `Set-Content D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260219_0023.md`
- `Add-Content docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`

### Tests
1. Runtime verification
- Result: user-confirmed resolved.

### Next Action
1. None.

### Rollback Policy
1. Documentation-only update.
- rollback = remove this follow-up section.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `worklog-update`
- Repo-Refs:
  - `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260219_0023.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, closure, texture, model-fix]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`

## Follow-up Update (2026-02-19 00:41 JST) - Runtime code review + debugger run + refactor proposal
- Trigger:
  - user requested code review and debugger execution, then refactor proposal focused on stronger error handling and log collection.
- Approval:
  - `実行`

### Scope
1. Review runtime model-loading and diagnostics flow.
2. Execute available debugger-equivalent command (EditMode test runner).
3. Produce refactor proposal document.

### Review Targets
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
- `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
- `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`

### Main Findings (summary)
1. High: loader exceptions are flattened and lose stack/type context before structured logging.
2. High: request_id continuity is missing inside reflection loader layer.
3. High: PMX fallback reflection scan is too broad and side-effect prone.
4. Medium: RuntimeLog writes synchronously to disk on caller thread.
5. Medium: log rotation/retention/level gating are missing.
6. Medium: silent catch blocks hide diagnostic enumeration failures.
7. Medium: mixed RuntimeLog/Debug.Log usage reduces cross-layer traceability.
8. Low: test coverage for failure classification and log policy is insufficient.

### Deliverable
1. Refactor proposal document created:
- `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`

### Commands (executed)
- static review scans (`Get-ChildItem` + `Select-String`) over runtime/avatar/libmmd/test files
- targeted file reads (`Get-Content`) with line-number extraction
- debugger-equivalent test run:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Tests
1. Unity EditMode test run
- Result:
  - Unity executable launch failure (`指定されたモジュールが見つかりません`).
- Artifact target:
  - `Unity_PJ/artifacts/test-results/editmode-20260219_003936.log` (missing)
  - `Unity_PJ/artifacts/test-results/editmode-20260219_003936.xml` (missing)

### Next Action
1. Decide whether to start Phase 1 implementation from proposal (error DTO + request_id propagation + stage logs).

### Rollback Policy
1. Documentation-only change in this update.
- rollback = remove this follow-up section and proposal file if needed.

### Record Metadata
- Report-Path: `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md`
- Execution-Tool: `codex`
- Execution-Agent: `codex`
- Execution-Model: `gpt-5`
- Used-Skills:
  - `code-review`
  - `bug-investigation`
  - `worklog-update`
- Repo-Refs:
  - `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `tools/run_unity_tests.ps1`
- Obsidian-Refs:
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Obsidian-Log:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260219_0041.md`
- Tags: `[agent/codex, model/gpt-5, tool/codex, review, refactor, error-handling, logging]`

### Record Check
- Report-Path exists: `True`
- Repo-Refs present: `True`
- Obsidian-Refs present: `True`
- Obsidian-Log recorded: `True`
- Execution-Tool / Agent / Model recorded: `True`
- Tags recorded: `True`
