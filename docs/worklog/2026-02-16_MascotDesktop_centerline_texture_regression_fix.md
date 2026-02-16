# Worklog: MascotDesktop_centerline_texture_regression_fix

- Date: 2026-02-16
- Task: 顔中央線継続 + テクスチャ欠損再発の回帰調査と最小差分修正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md, docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md, docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md, docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md, docs/worklog/2026-02-15_MascotDesktop_edge_alpha_transparency_fix_and_targa_warning.md, docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md
- Obsidian-Log: 未実施: このセッションはリポジトリ内の修正・検証を優先したため
- Tags: [agent/codex, model/gpt-5, tool/codex, regression-fix, centerline, texture]

## Summary
- 主要回帰点を2系統で特定した。
  - 中央線側: outline pass が `Lighting Off` のみで `ZWrite Off` / alpha blend 未指定のため、edge alpha・edge cap が不透明系で実質効かず線が強く出る。
  - テクスチャ欠損側: `.tga` 読込失敗時、`.png` 代替は `MASCOTDESKTOP_PMX_TGA_PNG_FALLBACK` 有効時にしか実行されず、通常設定では欠損に直結する。
- 上記を最小差分で修正し、実機再現ログ比較向けにテスト観点を追加した。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- `.tga` ロード分岐を修正。
  - env var 有効時は従来どおり PNG 優先。
  - その後の TGA デコード失敗時、sibling PNG が存在すれば自動フォールバックを実施。
  - 自動フォールバック時に warning を出力。
- private helper を追加。
  - `GetSiblingPngPath(string)`
  - `ShouldTrySiblingPngFallbackAfterTgaFailure(string, bool)`

2. outline shader 8ファイル
- 対象:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader`
- Outline Pass に以下を追加。
  - `ZWrite Off`
  - `Blend SrcAlpha OneMinusSrcAlpha`

3. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `TextureLoaderTgaFallback_TriesSiblingPngAfterTgaFailureWhenPngExists`
  - `TextureLoaderTgaFallback_DoesNotRetrySiblingPngWhenAlreadyTriedFirst`
- Reflection helper 追加:
  - `InvokeTextureLoaderShouldTrySiblingPngFallbackAfterTgaFailure`

## Commands
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `Get-Content AGENTS.md -Raw`
- `Get-Content docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md -Raw`
- `Get-Content docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md -Raw`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md -Raw`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md -Raw`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_edge_alpha_transparency_fix_and_targa_warning.md -Raw`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md -Raw`
- `Select-String` による関連箇所抽出（`MaterialLoader` / `TextureLoader` / `SimpleModelBootstrap` / shader）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. 静的確認
- Outline shader 8ファイルに `ZWrite Off` / `Blend SrcAlpha OneMinusSrcAlpha` が存在することを `Select-String` で確認。
- `TextureLoader.cs` の fallback helper と warning 文言の存在を確認。
- `SimpleModelBootstrapTests.cs` の追加テスト/Reflection helper の存在を確認。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- Log/XML: 未生成（`Unity_PJ/artifacts/test-results/editmode-20260216_010117.log/.xml` ともに未作成）

## Rationale (Key Points)
- 中央線は edge/outline 経路由来のため、material判定だけでなく pass 状態（ZWrite/Blend）を補正しないと再発しやすい。
- 欠損再発は TGA デコードに依存するため、TGA が null を返した場合だけ PNG を救済的に使うことで、副作用を限定して回帰を止める。
- env var の A/B 比較は維持しつつ、通常運用での欠損再発を抑える設計にした。

## Impact / Regression Risk
- 影響範囲:
  - PMX outline 表示（全モデル）
  - TGA テクスチャ読込（PNG sibling がある資産）
- 主なリスク:
  - outline が従来より薄く見える可能性
  - `tga -> png` 救済時、色味差が出る可能性
- 回帰監視ポイント:
  - `avatar.model.material_diagnostics` の `transparentByEdgeAlphaMats` / `samples`
  - `avatar.model.missing_main_textures`
  - `[TextureLoader] resolve failed` と `TGA decode failed; applied sibling PNG fallback` の件数

## Rollback
1. `TextureLoader.cs`
- `.tga` 分岐の「TGA失敗後 sibling PNG fallback」を削除し、旧分岐へ戻す。
- helper `GetSiblingPngPath` / `ShouldTrySiblingPngFallbackAfterTgaFailure` を削除する。

2. outline shader 8ファイル
- Outline Pass の `ZWrite Off` / `Blend SrcAlpha OneMinusSrcAlpha` 追加行を削除する。

3. `SimpleModelBootstrapTests.cs`
- 追加した TGA fallback テスト2件と reflection helper を削除する。

## Next Actions
1. ユーザー環境で `characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx` を再表示し、中央線の残存有無を確認する。
2. 同一条件で `avatar.model.material_diagnostics` / `avatar.model.missing_main_textures` / `[TextureLoader]` warning を採取し、修正前後を比較する。
3. まだ中央線が残る場合は、`samples=` の材質名を軸に `DrawEdge` 有効材質を抽出し、材質単位の edge cap を追加検討する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Follow-up Update (2026-02-16 01:16 UTC)
- User-provided runtime diagnostics showed:
  - `missing_main_textures` event absent
  - `toonMissingMats=16` / `toonMissingSpecMats=16`
  - `transparentByEdgeAlphaMats=12`
- Interpretation:
  - Current visible欠損は main texture 解決ではなく、toon texture `missing_spec` 未補完が主因候補。
  - `TGA decode failed; applied sibling PNG fallback` が出ないのは今回の再発が `.tga` decode failure 経路ではないことと整合。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- Toon `missing_spec` 時の white fallback を追加:
  - `toonTexture == null && ShouldUseWhiteFallbackToonTexture(toonTextureStatus)`
  - `toonTextureStatus = loaded_fallback_white`
  - `_ToonTex` に `Texture2D.whiteTexture` を設定
- helper 追加:
  - `ShouldUseWhiteFallbackToonTexture(string)`

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `MaterialLoaderToonFallback_UsesWhiteFallbackWhenMissingSpec`
  - `MaterialLoaderToonFallback_DoesNotUseWhiteFallbackWhenResolveMissing`
- reflection helper 追加:
  - `InvokeMaterialLoaderShouldUseWhiteFallbackToonTexture`

### Additional Tests
- 静的確認:
  - `ShouldUseWhiteFallbackToonTexture` 参照と fallback 代入行の存在確認済み。
- EditMode targeted run:
  - Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Result: Unity起動不可（`指定されたモジュールが見つかりません`）
  - Log/XML: `editmode-20260216_011602.*` は未生成

## Follow-up Update (2026-02-16 01:26 UTC)
- User runtime logs (req-274119ff4d2c43cb82fdc2a167655030) confirmed:
  - `toonMissingMats=0` / `toonMissingSpecMats=0`（toon fallback は有効化済み）
  - `missingResolveTotal=0` / `avatar.model.missing_main_textures` なし
  - `transparentMats=13`, `transparentByEdgeAlphaMats=12`, `edgeAlphaShare=0.923`
- 0215履歴参照結果:
  - `docs/worklog/2026-02-15_MascotDesktop_rollback_3files_pre_nurse_taso.md`
  - `docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md`
  - いずれも中心線の主因を `edge_alpha` と透明経路の相互作用と記録。

### Additional Changes
1. Transparent shader の深度書き込みを無効化
- 対象8ファイル:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader`
- 変更内容:
  - `ZWrite On` -> `ZWrite Off`
- 意図:
  - edge_alpha 主導で transparent 材質が多数化するケースで、自己遮蔽/奥行き競合による欠損・中心線を抑制。

### Additional Tests
- 静的確認:
  - 上記8ファイルの `ZWrite On` が 0 件、`ZWrite Off` が想定件数であることを確認。
- EditMode targeted run:
  - Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Result: Unity起動不可（`指定されたモジュールが見つかりません`）
  - Log/XML: `editmode-20260216_012658.*` は未生成

## Follow-up Update (2026-02-16 01:31 UTC) - Rollback By User Request
- User report: 状況が悪化（欠損増加）。
- Action: 直前に投入した `Transparent shader の ZWrite Off 化` をロールバック。

### Rolled Back Changes
- 対象8ファイルを、surface pass の `ZWrite On` に復元:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-Trans.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader`
- 補足: outline pass 側の `ZWrite Off` は維持（surface pass のみ戻し）。

### Verification
- 静的確認:
  - 非Outline-Trans 4ファイル: `zwriteOn>0, zwriteOff=0`
  - Outline-Trans 4ファイル: `zwriteOn>0` かつ `zwriteOff=1`（outline pass 分）
- EditMode targeted run:
  - Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
  - Result: Unity起動不可（`指定されたモジュールが見つかりません`）
  - Log/XML: `editmode-20260216_013153.*` は未生成

## Follow-up Update (2026-02-16 01:45 UTC) - Rollback to 2026-02-15 Resolved Point
- User request: `2026-02-15_MascotDesktop_character_variant_mmd_pkg_migration.md` / `2026-02-15_MascotDesktop_revert_opaque_missing_spec_white_fallback.md` 時点の解消状態へ戻す。
- Action: 2/16で追加した回帰要因候補を段階ロールバックし、2/15系の実装へ整合させた。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `MaterialLoader.cs.bak_current` を再適用（`bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha` の2/15系判定）。
- `optional_shadow_spec_missing` のみ白fallback理由に残す構成を復元。

2. outline shader 8ファイル
- 対象:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader`
- 変更:
  - Outline Pass 追加分の `ZWrite Off` / `Blend SrcAlpha OneMinusSrcAlpha` を削除。

3. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- `.tga` 経路の「TGA失敗後 sibling PNG fallback」追加分を削除。
- helper `GetSiblingPngPath` / `ShouldTrySiblingPngFallbackAfterTgaFailure` を削除。
- Safe Mode対策の `Texture ret = null;` は維持（`CS0165` 再発防止）。

4. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `SimpleModelBootstrapTests.cs.bak_current` を再適用し、`TextureLoader`/`MaterialLoader` のロールバック後実装と整合化。

### Additional Commands
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
- `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak_current Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Force`
- `Select-String` による静的検証（outline pass設定 / `TextureLoader` helper残存 / `MaterialLoader` 判定式）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. 静的確認
- `MaterialLoader.cs` と `MaterialLoader.cs.bak_current` の SHA256 一致: `True`
- `MeshPmdMaterialOutline*.shader` から `ZWrite Off` は 0 件
- `TextureLoader.cs` から `ShouldTrySiblingPngFallbackAfterTgaFailure` / `GetSiblingPngPath` は 0 件
- `TextureLoader.cs` の `Texture ret = null;` は存在
- `SimpleModelBootstrapTests.cs` から 2/16追加テストシンボルは 0 件

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`
- Log/XML: `editmode-20260216_014521.log/.xml` は未生成

### Rollback (for this follow-up)
1. `MaterialLoader.cs` を本Follow-up直前の版へ戻す（必要なら作業前バックアップを再適用）。
2. outline shader 8ファイルに以下を再追加:
   - `ZWrite Off`
   - `Blend SrcAlpha OneMinusSrcAlpha`
3. `TextureLoader.cs` に `ShouldTrySiblingPngFallbackAfterTgaFailure` / `GetSiblingPngPath` と TGA失敗後fallback分岐を再追加。
4. `SimpleModelBootstrapTests.cs` に 2/16追加テスト（TGA fallback/Toon fallback/diagnostics helper）を再適用。

## Follow-up Update (2026-02-16 09:05 UTC) - Centerline Only Minimal Re-apply
- User runtime report:
  - texture は回復
  - 中央線は継続
  - `transparentMats=4`, `transparentByEdgeAlphaMats=3`, `toonMissingMats=16`
- Interpretation:
  - 2/15で中央線解消時に記録されていた「`edge_alpha` を透明トリガーに含める」条件に戻っていない状態と一致。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `ResolveTransparentReason` を最小1箇所修正:
  - before: `isTransparent = byDiffuseAlpha || byTextureAlpha`
  - after:  `isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha`
- それ以外（`TextureLoader`/outline shader/fallbackロジック）は変更なし。

### Additional Commands
- `Select-String` で `ResolveTransparentReason` 判定式を確認
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. 静的確認
- `MaterialLoader.cs` 内で `isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha` を確認。
- `bySurfaceTransparency` 判定は除去済み。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`
- Log/XML: `editmode-20260216_090500.log/.xml` は未生成

### Rollback (for this follow-up)
1. `MaterialLoader.cs` の `ResolveTransparentReason` を以下へ戻す:
   - `isTransparent = byDiffuseAlpha || byTextureAlpha`

## Follow-up Update (2026-02-16 09:32 UTC) - Line Ending Warning + Texture Recurrence Guard
- User runtime report:
  - Unity warning: outline shader 8ファイルで改行混在（LF/CRLF）
  - diagnostics: `transparentMats=13`, `transparentByEdgeAlphaMats=12`, `toonMissingMats=16`, `toonMissingSpecMats=16`
  - 観測: 中央線継続 + テクスチャ欠け再発

### Additional Changes
1. line endings normalization (warning fix)
- 対象8ファイルを CRLF に再正規化:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader`

2. toon missing fallback re-apply (minimal)
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `toonTexture == null && toonTextureStatus == missing_spec` のとき:
    - `toonTexture = Texture2D.whiteTexture`
    - `toonTextureStatus = loaded_fallback_white`
  - 目的:
    - `toonMissingSpec` 再発での見た目欠けを抑制
    - `SimpleModelBootstrap` の shadow policy (`loaded_fallback_white` 判定) を再度有効化

### Additional Commands
- CRLF 正規化（PowerShell一括置換）:
  - ``$text = $text -replace "`r?`n","`r`n"``
- 改行混在確認:
  - ``[regex]::Matches($raw, "(?<!`r)`n").Count``
- `MaterialLoader.cs` の fallback 追加確認:
  - `Select-String ... "MainTextureStatusMissingSpec.Equals(toonTextureStatus)|Texture2D.whiteTexture|MainTextureStatusLoadedFallbackWhite"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. 静的確認
- 8 shader の `lf_only=0` を確認（改行混在警告の直接原因を除去）。
- `MaterialLoader.cs` に toon fallback 3行が存在することを確認。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`
- Log/XML: `editmode-20260216_093235.log/.xml` は未生成

### Rollback (for this follow-up)
1. 8 shader の改行を変更前に戻す（必要ならファイル単位で復元）。
2. `MaterialLoader.cs` の toon fallback 追加ブロック（`missing_spec -> loaded_fallback_white`）を削除。

## Follow-up Update (2026-02-16 10:25 UTC) - Explicit Restore to Checkpoint at line 210
- User direction: `docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md:210` のチェックポイントへ戻す。
- Action:
  - `MaterialLoader.cs` を `MaterialLoader.cs.bak_current` へ再復元（セッション変更を打ち切り）。
  - 実行前バックアップを `MaterialLoader.cs.bak_session_20260216_102444` として保存。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Copy-Item ...MaterialLoader.cs.bak_current ...MaterialLoader.cs -Force`
- 復元により以下を除去:
  - `isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha`（再追加分）
  - toon `missing_spec` -> `loaded_fallback_white` の再追加分
- 復元後は checkpoint どおり:
  - `var bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha`

### Additional Commands
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_session_20260216_102444 -Force`
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
- `Select-String` で判定式・fallback有無を検証
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. Static checks
- `MaterialLoader.cs` と `MaterialLoader.cs.bak_current` のハッシュ一致: `after_same=True`
- `MaterialLoader.cs` 検出:
  - `var bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha` あり
  - `isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha` なし
  - toon fallback 再追加ブロックなし
- `TextureLoader.cs` は checkpoint どおり（`Texture ret = null` あり、sibling fallback helper なし）
- outline shader 8ファイルの Outline Pass で `ZWrite Off` / `Blend SrcAlpha OneMinusSrcAlpha` 追加なし（`outline_extra=False`）

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`
- Log/XML: `editmode-20260216_102523.log/.xml` は未生成

### Rollback (for this follow-up)
1. `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_session_20260216_102444 Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
