# Worklog: pmx-whiteout-transparency-threshold-integer-fix

- Date: 2026-02-12
- Task: 透明判定境界テスト失敗に対する整数閾値比較への修正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-12_pmx-whiteout-unitycom-external-test-ingest.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/artifacts/test-results/editmode-20260211_200355.xml
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-transparency-threshold-integer-fix.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0112.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, transparency, threshold, test-fix]

## Summary
`MaterialLoader.IsTextureTransparentByPixels` の比率比較を整数閾値比較に置き換え、境界値（10/100, 32/1000）で `Expected: True` が安定する実装に修正した。補助テストとして必要ピクセル数算出の境界テストを追加した。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `RatioThresholdEpsilon` を追加。
  - `CalculateRequiredTransparentPixels(totalPixels, ratioThreshold)` を追加。
  - 透明判定を以下に変更:
    - `strongTransparentPixelCount >= requiredStrongTransparentPixels`
    - `semiTransparentPixelCount >= requiredSemiTransparentPixels`
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `MaterialLoaderTransparencyHeuristic_CalculatesRequiredPixelsAtBoundary` を追加。
  - `InvokeMaterialLoaderRequiredTransparentPixels` 反射ヘルパーを追加。

## Commands
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak3 -Force`
- `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak3 -Force`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-transparency-threshold-integer-fix.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0112.md`

## Tests
- ローカル実行（Codex環境）:
  - コマンド実行は開始されるが、`Unity.com` 起動時に `指定されたモジュールが見つかりません` で停止。
  - `Unity_PJ/artifacts/test-results/editmode-20260212_011236.log/.xml` は未生成。
- 判定:
  - ローカル pass/fail 未確定（ユーザー環境での再実行が必要）。

## Rationale (Key Points)
- 問題の失敗2件は境界値のみで、比率浮動小数の比較が不安定要因になりうる。
- しきい値仕様（32/1000, 10/100）を保ちつつ、判定を整数化するのが最小差分かつ根本的。
- テスト期待値を書き換えるより、実装の境界判定を安定化する方が診断信頼性を維持できる。

## Next Actions
1. ユーザー環境で `Unity.com` から `SimpleModelBootstrapTests` を再実行し、失敗2件の解消を確認する。
2. 失敗が残る場合、`MaterialLoader.cs` の Alpha閾値（0.20/0.60）自体の期待仕様を再確認する。
3. テスト成功後、PMX白飛び本線の比較（AZKi/かなた/ねね）に復帰する。

## Rollback
1. `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak3 Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
2. `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak3 Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Force`
3. `Remove-Item docs/worklog/2026-02-12_pmx-whiteout-transparency-threshold-integer-fix.md`
4. `Remove-Item D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0112.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
