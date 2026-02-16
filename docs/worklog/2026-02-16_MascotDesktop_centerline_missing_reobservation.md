# Worklog: MascotDesktop_centerline_missing_reobservation

- Date: 2026-02-16
- Task: 顔中央線 / 欠損再発の再観測再開（checkpoint固定のまま再採取試行と3要因切り分け）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_handoff.md, docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md, docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md, docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md, docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md, docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md, docs/worklog/2026-02-15_MascotDesktop_revert_opaque_missing_spec_white_fallback.md, docs/worklog/2026-02-15_MascotDesktop_character_variant_mmd_pkg_migration.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-NoCastShadow.shader, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-Trans-CullBack-NoCastShadow.shader, tools/run_unity.ps1, tools/run_unity_tests.ps1, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_centerline_missing_reobservation.md
- Obsidian-Log: 未実施: 今回は再観測試行と静的切り分けのみ（外部ノートへの転記は次回runtime再採取完了後に実施）
- Tags: [agent/codex, model/gpt-5, tool/codex, centerline, missing-texture, reobservation, static-isolation]

## Summary
- checkpoint基準 (`docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md:210`) の静的一致を再確認した。
- `avatar.model.material_diagnostics` 再採取を試行したが、Unity実行環境制約（モジュール不足）により再採取不能だった。
- 代替として `MaterialLoader` / outline shader / shadow policy を静的に切り分け、再発要因候補を優先度付きで特定した。

## Current Fixed Checkpoint Re-Verification
1. `MaterialLoader.cs` 同一性
- SHA256: `MaterialLoader.cs == MaterialLoader.cs.bak_current` は `True`。

2. outline 8 shader 改行状態
- 8ファイルすべて `mixed_lf=False`（CRLF混在なし）。

## Re-observation Attempt (No State Increase)
1. runtime採取試行
- `./tools/run_unity.ps1 -ExecuteMethod MascotDesktop.Editor.Automation.Ping ...`
- `./tools/run_unity.ps1 -UnityPath ...Unity.com -ExecuteMethod MascotDesktop.Editor.Automation.Ping ...`

2. 結果
- 2回とも `Program 'Unity.exe/Unity.com' failed to run ... 指定されたモジュールが見つかりません`。
- `Unity_PJ/artifacts/runtime-check/ping-*.log` は未生成（`Test-Path=False`）。
- 本端末では `avatar.model.material_diagnostics` の新規再採取は不可。

3. 代替探索
- `Unity_PJ/artifacts` 内の `*.log/*.txt` から `avatar.model.material_diagnostics` を検索したがヒット0件。

## Isolation Findings (MaterialLoader -> Outline -> Shadow Policy)
1. MaterialLoader
- `bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha`（`edge_alpha` をsurface透明トリガーから除外）を確認。
- `byDiffuseAlpha || byEdgeAlpha || byTextureAlpha` は存在しない。
- `toon missing_spec -> loaded_fallback_white` の再追加ロジックは存在しない。
- `Texture2D.whiteTexture` は main texture fallback 1箇所のみ（shadow限定）で、toon fallback には使われていない。

2. outline shader
- 対象8ファイル合計で `ZWrite Off` は 0 件。
- `Blend SrcAlpha OneMinusSrcAlpha` は 6 件あるが、Outline Passではなく Trans系 Surface Pass側（checkpoint整合）。
- Outline Passは `Lighting Off` のみで、2/16一時対策で入れた Outline Pass用 `ZWrite Off/Blend` は未適用状態。

3. shadow policy
- `SimpleModelBootstrap.cs` に `ApplyModelShadowPolicy` / `avatar.model.shadow_policy_applied` は 0 件。
- `MainTextureStatusLoadedFallbackWhite` 参照も 0 件。
- `docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md` に記録された policy ログ経路は、現行コード上では確認できない。

## Reference Comparison (Existing Runtime Logs in Worklogs)
- 既知ログA（line+欠損継続）
  - `transparentMats=13`, `transparentByEdgeAlphaMats=12`, `toonMissingMats=0`, `toonStatus=loaded_fallback_white`
  - source: `docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_handoff.md`
- 既知ログB（欠損再発）
  - `transparentMats=13`, `transparentByEdgeAlphaMats=12`, `toonMissingMats=16`, `toonMissingSpecMats=16`
  - source: `docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md`
- 新規採取は本端末で未達のため、A/Bのどちらに寄っているかの再判定は未実施。

## Changes
1. 変更ファイル
- `docs/worklog/2026-02-16_MascotDesktop_centerline_missing_reobservation.md`（本ログのみ追加）

2. コード変更
- なし（状態増加禁止を優先し、調査のみ実施）。

## Commands (Executed)
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `Get-Content AGENTS.md`
- `Get-Content docs/worklog/...`（指定9ファイル）
- `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
- `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- `Get-FileHash Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Algorithm SHA256`
- `Get-FileHash Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current -Algorithm SHA256`
- outline 8 shader の改行混在チェック（`(?<!\r)\n`）
- `./tools/run_unity.ps1 -ExecuteMethod MascotDesktop.Editor.Automation.Ping ...`
- `./tools/run_unity.ps1 -UnityPath ...Unity.com -ExecuteMethod MascotDesktop.Editor.Automation.Ping ...`
- `Select-String` による `MaterialLoader` / `SimpleModelBootstrap` / `MeshPmdMaterialOutline*.shader` 根拠抽出
- `Select-String` による `Unity_PJ/artifacts` 内 diagnostics 検索

## Tests
1. Runtime check (batch)
- Result: 失敗
- Error: `指定されたモジュールが見つかりません`（Unity.exe/Unity.com 起動不可）
- Log artifact: 未生成

2. 静的検証
- `MaterialLoader.cs == MaterialLoader.cs.bak_current`: `True`
- outline 8 shader mixed LF: 全件 `False`
- `ApplyModelShadowPolicy` / `avatar.model.shadow_policy_applied` 検索: 0件

## Rationale (Key Points)
- 指示どおり checkpoint固定を維持し、状態を増やさず再観測を最優先した。
- 再観測不能時は、次の変更判断に使える最小根拠を得るため、3要因を静的に分解して「存在/非存在」を確定した。
- runtime証拠がない段階での修正適用は、再発要因の混同リスクが高いため見送った。

## Rollback
1. この記録のみを取り消す場合
- `Remove-Item docs/worklog/2026-02-16_MascotDesktop_centerline_missing_reobservation.md`

2. コード状態
- コード変更なしのため追加ロールバック不要。

## Next Actions
1. ユーザー実機で `avatar.model.material_diagnostics` を再採取し、最低限以下を共有する。
- `transparentMats`
- `transparentByEdgeAlphaMats`
- `toonMissingMats`
- `toonMissingSpecMats`
- `samples`

2. 新規runtimeログ受領後、1要因ずつ最小差分で再開する。
- Step A: MaterialLoader（toon missing fallback有無）
- Step B: outline pass（ZWrite/Blend）
- Step C: shadow policy（実装有無/条件一致）

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Follow-up Update (2026-02-16 13:23 UTC) - Step A MaterialLoader Only
- User runtime diagnostics (request_id=`req-20930ff32234470b8ea99ba47cd27c59`) showed:
  - `materials=16`
  - `transparentMats=4`
  - `transparentByEdgeAlphaMats=3`
  - `toonMissingMats=16`
  - `toonMissingSpecMats=16`
  - `toonMissingResolveMats=0`
- User observation: 中央線は健在。
- Interpretation:
  - 欠損再発は `toon missing_spec` が支配的。
  - `transparentMats=4` まで下がっても中央線が残るため、中央線主因は `toon missing` 単独では説明し切れない。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- Toonの `missing_spec` を white fallback で救済する最小差分を追加:
  - `if (toonTexture == null && ShouldUseWhiteFallbackToonTexture(toonTextureStatus))`
  - `toonTexture = Texture2D.whiteTexture`
  - `toonTextureStatus = loaded_fallback_white`
- helper追加:
  - `ShouldUseWhiteFallbackToonTexture(string toonTextureStatus)`
- 既存checkpoint条件は維持:
  - `bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha`（edge_alphaをsurface透明トリガーに戻さない）

### Additional Commands
- `Select-String MaterialLoader.cs "ShouldUseWhiteFallbackToonTexture|toonTextureStatus = MainTextureStatusLoadedFallbackWhite|bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. Static checks
- `ShouldUseWhiteFallbackToonTexture` が存在。
- `toonTextureStatus = MainTextureStatusLoadedFallbackWhite` が存在。
- `bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha` を維持。
- outline 8 shader の `ZWrite Off` は 0 件を維持。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. `MaterialLoader.cs` を checkpointへ戻す:
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`

## Follow-up Update (2026-02-16 13:52 UTC) - Step B Outline Pass Only
- User runtime diagnostics (request_id=`req-7ddcd0e91ba64627a8a46cfeed2294c7`) showed:
  - `transparentMats=4`
  - `transparentByEdgeAlphaMats=3`
  - `toonMissingMats=0`
  - `toonMissingSpecMats=0`
  - `toonStatus=loaded_fallback_white`
- User observation: 線は健在。
- Interpretation:
  - Step A（toon fallback）で欠損側は抑制できた。
  - ただし中央線は残存し、欠損要因と中央線要因は分離して扱う必要がある。

### Additional Changes
1. outline pass の最小差分（2ファイルのみ）
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader`
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`
- Outline Pass に追加:
  - `ZWrite Off`
  - `Blend SrcAlpha OneMinusSrcAlpha`
- 目的:
  - Outlineジオメトリの深度競合を弱め、中央線（継ぎ目強調）の発生を抑制する。

2. 非対象（維持）
- `MaterialLoader.cs` の Step A 追加（toon fallback）は維持。
- `bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha` は維持。
- 他6本の outline shader は未変更（要因を拡げないため）。

### Additional Commands
- `Select-String` による outline 2ファイルの `ZWrite Off` / `Blend` 追加確認
- 改行混在確認（`(?<!\r)\n`）
- `Select-String MaterialLoader.cs ...` で Step A 維持確認
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. Static checks
- `MeshPmdMaterialOutline-CullBack.shader` Outline Pass: `ZWrite Off` / `Blend SrcAlpha OneMinusSrcAlpha` あり。
- `MeshPmdMaterialOutline-CullBack-NoCastShadow.shader` Outline Pass: 同上。
- 2ファイルの改行混在: `lf_only=0`。
- `MaterialLoader.cs` Step A ロジック維持を確認。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. outline 2ファイルから以下2行を削除:
- `ZWrite Off`
- `Blend SrcAlpha OneMinusSrcAlpha`
2. 必要なら Step A を含め checkpoint完全復元:
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`

## Follow-up Update (2026-02-16 14:11 UTC) - Rollback Ineffective Step B + Line Ending Warning Fix
- User report:
  - Warning: `MeshPmdMaterialOutline-CullBack.shader` / `MeshPmdMaterialOutline-CullBack-NoCastShadow.shader` で改行混在警告。
  - Runtime diagnostics (request_id=`req-e1b5cbc238a245d0ac6dd554db1bfe42`):
    - `transparentMats=4`
    - `transparentByEdgeAlphaMats=3`
    - `toonMissingMats=0`
    - `toonMissingSpecMats=0`
  - User observation: 線は健在。
- Decision:
  - AGENTS方針どおり、効果が確認できない Step B は戻す。
  - 変更を重ねず、次検証前に警告ゼロ状態へ整理する。

### Additional Changes
1. Rollback: outline 2ファイル（Step B撤回）
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader`
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`
- Outline Pass から削除:
  - `ZWrite Off`
  - `Blend SrcAlpha OneMinusSrcAlpha`

2. Line ending normalization
- 同2ファイルを CRLF に正規化（`lf_only=0` へ復帰）。

3. 非対象（維持）
- Step A (`MaterialLoader` toon fallback) は維持:
  - `ShouldUseWhiteFallbackToonTexture`
  - `toonTextureStatus = loaded_fallback_white`

### Additional Commands
- `Select-String` で outline 2ファイルの `ZWrite Off` / `Blend` 消去確認
- 改行混在確認（`(?<!\r)\n`）
- `Select-String` で `MaterialLoader.cs` Step A 維持確認
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. Static checks
- `MeshPmdMaterialOutline-CullBack.shader`: lines=87, `lf_only=0`, literal `` `r`n ``=0
- `MeshPmdMaterialOutline-CullBack-NoCastShadow.shader`: lines=73, `lf_only=0`, literal `` `r`n ``=0
- Outline Passは `Lighting Off` のみ（Step B追加行なし）
- `MaterialLoader.cs` で Step A ロジック維持を確認

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. このfollow-upのみ戻す場合
- 2 shader の行末を変更前状態へ戻す（必要ならファイル単位で復元）
2. Step A 含め checkpoint完全復元が必要な場合
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`

## Follow-up Update (2026-02-16 14:18 UTC) - Step C Shadow Policy Isolation
- User report:
  - 警告は解除。
  - 中央線は健在。
- User runtime diagnostics (request_id=`req-ae0c499af2d14aa69306e8ed631195b6`):
  - `transparentMats=4`
  - `transparentByEdgeAlphaMats=3`
  - `toonMissingMats=0`
  - `toonMissingSpecMats=0`
  - samples の `toonStatus=loaded_fallback_white` が継続
- Interpretation:
  - 欠損は Step A で抑制済み。
  - Step B は効果なしで撤回済み。
  - 次要因として shadow 経路を単独で有効化し、線への寄与を切り分ける。

### Additional Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 追加定数:
  - `MainTextureStatusLoadedFallbackWhite = "loaded_fallback_white"`
- PMX表示フローに追加:
  - `ApplyModelShadowPolicy(result.Root, requestId, absolutePath, sourceTier)`
- 追加メソッド:
  - `ApplyModelShadowPolicy(...)`
    - `ToonTextureStatusTag == loaded_fallback_white` 材質が1つ以上ある場合、モデル全Rendererで
      - `shadowCastingMode = Off`
      - `receiveShadows = false`
    - ログ出力:
      - `avatar.model.shadow_policy_applied`
  - `CountMaterialsWithTextureStatus(...)`

2. 非対象（維持）
- Step A (`MaterialLoader` toon fallback) は維持。
- Step B（outline pass追加）は撤回済みのまま維持。

### Additional Commands
- `Select-String SimpleModelBootstrap.cs "MainTextureStatusLoadedFallbackWhite|ApplyModelShadowPolicy\(|avatar.model.shadow_policy_applied|CountMaterialsWithTextureStatus\("`
- `Get-Content SimpleModelBootstrap.cs`（`TryDisplayPmx` 呼び出し位置確認）
- `Select-String MaterialLoader.cs ...`（Step A 維持確認）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

### Additional Verification
1. Static checks
- `SimpleModelBootstrap.cs` に以下を確認:
  - `ApplyModelShadowPolicy` 呼び出し（PMX表示フロー）
  - `avatar.model.shadow_policy_applied` ログ
  - `CountMaterialsWithTextureStatus` 実装
- `MaterialLoader.cs` で Step A 維持（`ShouldUseWhiteFallbackToonTexture`, `bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha`）。

2. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. `SimpleModelBootstrap.cs` から以下を削除:
- `MainTextureStatusLoadedFallbackWhite` 追加定数
- `ApplyModelShadowPolicy(...)` 呼び出し
- `ApplyModelShadowPolicy(...)` / `CountMaterialsWithTextureStatus(...)` メソッド
2. もしくは checkpointへ戻す場合:
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
- `SimpleModelBootstrap.cs` を本 follow-up 直前版へ復元

## Follow-up Update (2026-02-16 05:43 UTC) - Rollback Step C + Step D Outline Disable Isolation
- User runtime observation before this step:
  - 中央線は健在。
  - `avatar.model.shadow_policy_applied` は発火済み（`fallbackToonMats=16`）だが線は残存。
- Decision:
  - 効果不明/無効だった Step C（shadow policy）は AGENTS 方針どおりロールバック。
  - 次の 1 要因として outline 系寄与を確定するため、`_OutlineWidth` を 0 固定する切り分けを追加。

### Additional Changes
1. Rollback (ineffective change)
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 削除:
  - `MainTextureStatusLoadedFallbackWhite` 定数
  - `ApplyModelShadowPolicy(...)` 呼び出し（`TryDisplayPmx`）
  - `ApplyModelShadowPolicy(...)` / `CountMaterialsWithTextureStatus(...)`
  - `using UnityEngine.Rendering`

2. Isolation Step D (outline off)
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 変更:
  - `material.SetFloat("_OutlineWidth", mmdMaterial.EdgeSize);`
  - -> `material.SetFloat("_OutlineWidth", 0f);`
- Purpose:
  - 中央線が outline 幾何由来かを単独要因で判定する。

### Additional Commands
- `Select-String SimpleModelBootstrap.cs "ApplyModelShadowPolicy|shadow_policy_applied|MainTextureStatusLoadedFallbackWhite|using UnityEngine.Rendering"`
- `Get-Content MaterialLoader.cs`（`_OutlineWidth` 変更確認）
- `./tools/run_unity_tests.ps1`

### Additional Verification
1. Static checks
- `SimpleModelBootstrap.cs` で shadow policy 参照は 0 件。
- `MaterialLoader.cs` で `_OutlineWidth` は `0f` に変更済み。
- Step A（toon fallback）のロジックは維持。

2. Unity test script
- Command: `./tools/run_unity_tests.ps1`
- Observed:
  - `UnityPath=C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe`
  - 起動時に `指定されたモジュールが見つかりません`（`Unity.exe` 起動失敗）
- Result:
  - 本環境では自動テスト継続不可（runtime判定は実機ログ依存）。

### Rollback (for this follow-up)
1. Step Dのみ戻す
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `material.SetFloat("_OutlineWidth", 0f);` を `mmdMaterial.EdgeSize` に戻す。

2. 追加入力全体を戻す
- Step D を戻したうえで、必要なら本 follow-up 追記前の `SimpleModelBootstrap.cs`/`MaterialLoader.cs` バージョンへ復元。

### Next Action Request (Runtime)
1. この状態で実機再観測を 1 回実施
- 画面観測: 中央線の有無
- ログ: `avatar.model.material_diagnostics`（`samples` 含む）
2. 判定
- 線が消える: outline 経路が主因（outline shader/edge設定を次に最小化）
- 線が残る: outline 非主因（main pass/texture seam/mesh normal 側へ切り替え）

## Follow-up Update (2026-02-16 05:58 UTC) - Compile Error Hotfix (Safe Mode)
- User report:
  - Unity startup showed Safe Mode prompt: project contains compilation errors.

### Root Cause
- Source: `C:\Users\sugar\AppData\Local\Unity\Editor\Editor.log`
- Error:
  - `Assets\Scripts\Runtime\Avatar\SimpleModelBootstrap.cs(1806,42): error CS0103: The name 'AmbientMode' does not exist in the current context`
- Cause:
  - Step C rollback時に `using UnityEngine.Rendering;` を削除したが、同ファイルには `RenderSettings.ambientMode = AmbientMode.Flat;` が残っていた。

### Additional Changes
1. Compile hotfix (minimal)
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 追加:
  - `using UnityEngine.Rendering;`

### Additional Commands
- `Select-String C:\Users\sugar\AppData\Local\Unity\Editor\Editor.log "error CS|SimpleModelBootstrap.cs"`
- `Get-Content SimpleModelBootstrap.cs`（先頭usingと `AmbientMode` 使用行の確認）

### Additional Verification
1. Static checks
- `SimpleModelBootstrap.cs` 冒頭に `using UnityEngine.Rendering;` が存在。
- `AmbientMode.Flat` 使用行（`SimpleModelBootstrap.cs:1807`）との整合を確認。

2. Runtime/compile execution
- この端末では Unity / Unity付属dotnet起動が `指定されたモジュールが見つかりません` で失敗するため、実機Unityでの再コンパイル確認が必要。

### Rollback (for this follow-up)
1. 本hotfixのみ戻す場合
- `SimpleModelBootstrap.cs` から `using UnityEngine.Rendering;` を削除。

2. 注意
- 現在 `AmbientMode.Flat` が存在するため、hotfix削除時は再び `CS0103` が発生する。

## Follow-up Update (2026-02-16 06:24 UTC) - Step E Main Pass Specular Isolation
- User runtime diagnostics (request_id=`req-9a872acd775d4f4d80f6cb1f519cc14a`) showed:
  - `transparentMats=4`
  - `transparentByEdgeAlphaMats=3`
  - `toonMissingMats=0`
  - `toonMissingSpecMats=0`
- User observation: 中央線は健在。
- Interpretation:
  - Step D（`_OutlineWidth=0`）でも線が残るため、outline経路は主因から外す。
  - 次要因として main pass の specular 寄与を単独で抑制する。

### Additional Changes
1. Step D rollback
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 変更:
  - `material.SetFloat("_OutlineWidth", 0f);`
  - -> `material.SetFloat("_OutlineWidth", mmdMaterial.EdgeSize);`

2. Step E (one-factor)
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 追加:
  - `if (material.HasProperty("_MascotSpecularContributionCap")) { material.SetFloat("_MascotSpecularContributionCap", 0f); }`
- Purpose:
  - main pass の specular 成分をゼロ化し、中央線への寄与を切り分ける。

### Additional Commands
- `Select-String MaterialLoader.cs "_MascotSpecularContributionCap|SetFloat(\"_OutlineWidth\"|ShouldUseWhiteFallbackToonTexture"`
- `Get-Content MaterialLoader.cs`（差分行確認）
- `./tools/run_unity_tests.ps1`

### Additional Verification
1. Static checks
- `MaterialLoader.cs` に `_MascotSpecularContributionCap=0f` 設定を確認。
- `_OutlineWidth` は `mmdMaterial.EdgeSize` へ復帰を確認。
- Step A (`ShouldUseWhiteFallbackToonTexture`) 維持を確認。
- `SimpleModelBootstrap.cs` の `using UnityEngine.Rendering;` 維持を確認（前回CS0103再発防止）。

2. EditMode run
- Command: `./tools/run_unity_tests.ps1`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. Step Eのみ戻す
- `MaterialLoader.cs` の `if (material.HasProperty("_MascotSpecularContributionCap")) ...` ブロックを削除。

2. Step Dまで含めて戻す
- `_OutlineWidth` を `0f` に戻す。

### Next Action Request (Runtime)
1. この状態で実機再観測を1回実施
- 画面観測: 中央線の有無
- ログ: `avatar.model.material_diagnostics`（`samples` 含む）
2. 判定
- 線が消える: specular寄与が主因（cap値/閾値の最小調整へ）
- 線が残る: specular非主因（次は edge cap or normal/seam 側へ）

## Follow-up Update (2026-02-16 07:09 UTC) - Step F Edge Contribution Isolation
- User runtime diagnostics (request_id=`req-1ccc82ec94bd4511802541c5f9654ded`) showed:
  - `transparentMats=4`
  - `transparentByEdgeAlphaMats=3`
  - `toonMissingMats=0`
  - `toonMissingSpecMats=0`
- User observation: 中央線は健在。
- Interpretation:
  - Step E（specular cap 0）でも線が残るため、specular寄与は主因から外す。
  - 次要因として edge contribution を単独でゼロ化して切り分ける。

### Additional Changes
1. Step E rollback
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 削除:
  - `if (material.HasProperty("_MascotSpecularContributionCap")) { material.SetFloat("_MascotSpecularContributionCap", 0f); }`

2. Step F (one-factor)
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 追加:
  - `if (material.HasProperty("_MascotEdgeContributionCap")) { material.SetFloat("_MascotEdgeContributionCap", 0f); }`
- 維持:
  - `_OutlineWidth = mmdMaterial.EdgeSize`
  - Step A toon fallback ロジック

### Additional Commands
- `Select-String MaterialLoader.cs "_MascotSpecularContributionCap|_MascotEdgeContributionCap|SetFloat(\"_OutlineWidth\"|ShouldUseWhiteFallbackToonTexture"`
- `Get-Content MaterialLoader.cs`（差分行確認）
- `./tools/run_unity_tests.ps1`

### Additional Verification
1. Static checks
- `MaterialLoader.cs` で `_MascotSpecularContributionCap` 設定は 0 件。
- `_MascotEdgeContributionCap` を `0f` へ設定する1ブロックを確認。
- `_OutlineWidth` は `mmdMaterial.EdgeSize` のまま。

2. EditMode run
- Command: `./tools/run_unity_tests.ps1`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. Step Fのみ戻す
- `MaterialLoader.cs` の `if (material.HasProperty("_MascotEdgeContributionCap")) ...` を削除。

2. Step E比較に戻す
- 上記削除後、必要に応じて `_MascotSpecularContributionCap=0f` ブロックを再挿入。

### Next Action Request (Runtime)
1. この状態で実機再観測を1回実施
- 画面観測: 中央線の有無
- ログ: `avatar.model.material_diagnostics`（`samples` 含む）
2. 判定
- 線が消える: edge contribution 主因
- 線が残る: edge/specular/outline 以外（mesh seam or texture uv/normal 寄与）へ切替

## Follow-up Update (2026-02-16 09:06 UTC) - Option 2 Finalization (Conditional Edge Cap)
- User runtime result (request_id=`req-1094e2a70f3e4dff90a2593c97265f5e`):
  - 中央線は消失。
  - `toonMissingMats=0`, `toonMissingSpecMats=0` を維持。
- User decision:
  - Option 2 を採用（`loaded_fallback_white` 条件時のみ edge cap=0）。

### Additional Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 変更前（Step F暫定）:
  - 無条件で `_MascotEdgeContributionCap = 0f`
- 変更後（Option 2）:
  - toon解決後に条件適用:
    - `edgeContributionCap = (toonTextureStatus == loaded_fallback_white) ? 0f : 1f`
    - `material.SetFloat("_MascotEdgeContributionCap", edgeContributionCap)`
- 目的:
  - 中央線抑制を fallback toon 条件時に限定し、通常モデルへの副作用を抑える。

### Additional Commands
- `Get-Content MaterialLoader.cs`（変更ブロック確認）
- `Select-String MaterialLoader.cs "_MascotEdgeContributionCap|MainTextureStatusLoadedFallbackWhite.Equals(toonTextureStatus)|SetOverrideTag(ToonTextureStatusTag"`
- `./tools/run_unity_tests.ps1`

### Additional Verification
1. Static checks
- 無条件 `_MascotEdgeContributionCap=0f` は削除済み。
- toon status 確定後に `0f/1f` を設定する条件分岐を確認。
- Step A toon fallback は維持。

2. EditMode run
- Command: `./tools/run_unity_tests.ps1`
- Result: 実行失敗（Unity起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`

### Rollback (for this follow-up)
1. Option 2 を戻して暫定Step Fへ
- 条件分岐を削除し、無条件 `_MascotEdgeContributionCap=0f` を復元。
2. Option 2 を完全撤回
- `_MascotEdgeContributionCap` への設定ブロックを全削除し、shader既定値に任せる。

### Next Action Request (Runtime)
1. Option 2 状態で再観測を1回実施
- 画面観測: 中央線の有無
- ログ: `avatar.model.material_diagnostics`（`samples`）
2. 期待判定
- 線なし維持: Option 2 を恒久採用候補
- 再発: 条件拡張（toonStatus + shader/transparentReason）へ

## Follow-up Update (2026-02-16 10:50 UTC) - Option 2 Runtime Confirmation
- User runtime confirmation:
  - 問題なし（中央線再発なし）。
- Runtime diagnostics:
  - request_id=`req-e664f8957f63403e8d096391f43cde16`
  - `transparentMats=4`
  - `toonMissingMats=0`
  - `toonMissingSpecMats=0`
  - samples: `toonStatus=loaded_fallback_white` を維持。

### Conclusion
- Option 2（`loaded_fallback_white` 条件時のみ `_MascotEdgeContributionCap=0`）は、
  - 中央線抑制
  - 欠損再発抑制（Step A）
  の両立を確認。
- 本件はこの条件付き対策を恒久候補として採用可能。

### Additional Verification
1. Runtime (user environment)
- Result: Pass
- Evidence: `req-e664f8957f63403e8d096391f43cde16`

### Rollback (current stable point)
1. Option 2を戻す場合
- `MaterialLoader.cs` の `_MascotEdgeContributionCap` 条件設定ブロックを削除。
2. 中央線再発時の即時回避
- 無条件 `_MascotEdgeContributionCap=0f`（Step F暫定）へ戻す。

### Status
- Phase status: done (investigation + mitigation validated)
- Remaining optional work: commit / release note / regression test case 追加
