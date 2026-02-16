# Worklog: pmx-whiteout-transparent-reason-diagnostics

- Date: 2026-02-11
- Task: PMX白飛び調査向け Transparent理由タグ診断の実装と比較条件固定ログの追加
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, Unity_PJ/docs/NEXT_TASKS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-whiteout-transparent-reason-diagnostics.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1833.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, diagnostics, transparent-reason]

## Summary
`transparentMats` の総数に加えて、Transparent化理由 (`diffuse_alpha` / `edge_alpha` / `texture_alpha`) を材質タグ経由で集計できるようにした。`avatar.model.material_diagnostics` に理由別カウントと `assetKind` / `renderFactor` を追加し、再現条件固定でモデル比較しやすいログ形式へ拡張した。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `MASCOT_TRANSPARENT_REASON` タグを追加。
  - Transparent判定理由を `opaque` / `diffuse_alpha` / `edge_alpha` / `texture_alpha`（複合は `+` 連結）で生成し、`SetOverrideTag` で材質に付与。
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - 材質ごとに Transparent理由タグを読み取り、以下を追加集計:
    - `transparentByDiffuseAlphaMats`
    - `transparentByEdgeAlphaMats`
    - `transparentByTextureAlphaMats`
    - `transparentByMultiReasonMats`
    - `transparentReasonUnknownMats`
  - `avatar.model.material_diagnostics` に `assetKind` / `renderFactor` を追加。
  - サンプル文字列に `transparentReason` を追加。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `BuildTransparentReasonTag` の期待値テスト追加。
  - `HasTransparentReason` の分解判定テスト追加。

## Commands
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak -Force`
- `Copy-Item Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs.bak -Force`
- `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak -Force`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-11_pmx-whiteout-transparent-reason-diagnostics.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1833.md`

## Tests
- EditMode targeted test (`SimpleModelBootstrapTests`) を2回試行。
- 結果: 実行失敗（Unity起動時に `指定されたモジュールが見つかりません`）。
- 判定根拠: PowerShell出力で `Unity.exe` / `Unity.com` のプロセス起動失敗を確認。今回実行分の `artifacts/test-results` には新規 `editmode-20260211_183248.*` / `editmode-20260211_183305.*` は生成されていない。

## Rationale (Key Points)
- 引継ぎの未確定点は「Transparent件数だけで説明できない」ことなので、次段判断のためには理由別内訳が必要。
- 既存の材質タグ仕組み（`MASCOT_MAIN_TEX_STATUS`）に揃えて透明理由もタグ化することで、最小差分で診断面を拡張できる。
- `assetKind` / `renderFactor` を同ログに含めることで、再現条件の混在をログ側で検出しやすくした。

## Next Actions
1. Unity実行環境（不足モジュール）を解消し、同じ `SimpleModelBootstrapTests` フィルタを再実行して成功ログを採取する。
2. `Candidate Mode: Model` + `F0 Baseline` 固定で8モデル再測定し、理由別カウントと白飛び有無の対応表を作成する。
3. `transparentByTextureAlphaMats` と白飛びの相関が高い場合は `SemiTransparentPixelRatioThreshold` のA/Bを実施する。
4. 相関が低い場合は Shader側（specular/toon合成）補正案の実験に進む。

## Rollback
1. `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
2. `Copy-Item Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs.bak Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -Force`
3. `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Force`
4. `Remove-Item docs/worklog/2026-02-11_pmx-whiteout-transparent-reason-diagnostics.md`
5. `Remove-Item D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1833.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
