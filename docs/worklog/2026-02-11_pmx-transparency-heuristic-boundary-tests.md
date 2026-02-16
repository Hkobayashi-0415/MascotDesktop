# Worklog: pmx-transparency-heuristic-boundary-tests

- Date: 2026-02-11
- Task: 透明判定ヒューリスティックの境界値テスト追加（引き継ぎ作業）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, docs/worklog/2026-02-11_pmx-whiteout-context-handoff.md, docs/worklog/2026-02-11_pmx-whiteout-transparency-heuristic-fix.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-transparency-heuristic-boundary-tests.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1650.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, transparency, tests, boundary]

## Summary
引き継ぎ内容に基づき、`MaterialLoader` の現行しきい値（strong count / semi ratio）に合わせて EditMode テストを2件追加した。Unity テスト実行は試行したが、`Unity.exe` 起動時のモジュール不足で実行不能だった。

## Changes
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - 追加: `MaterialLoaderTransparencyHeuristic_UsesSemiTransparentRatioThreshold`
  - 追加: `MaterialLoaderTransparencyHeuristic_UsesStrongTransparentPixelCountThreshold`
  - 既存テスト `MaterialLoaderTransparencyHeuristic_IgnoresSparseAlphaNoise` は維持

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`（該当行確認）
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`（該当行確認）
- `rg -n "MaterialLoaderTransparencyHeuristic|IsTextureTransparentByPixels|StrongTransparentAlphaThreshold|SemiTransparentAlphaThreshold|StrongTransparentPixelCountThreshold|SemiTransparentPixelRatioThreshold" ...`
- `apply_patch`（`Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`）
- `& .\tools\run_unity_tests.ps1 -TestPlatform EditMode -TestFilter MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests -Quit`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_164949.xml`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_164949.log`

## Tests
- Target: `MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests`
- Result: **Failed to execute**（Unity 起動失敗）
  - Error: `Program 'Unity.exe' failed to run ... 指定されたモジュールが見つかりません。`
  - XML/log output: `False / False`
- 追加テストの期待値:
  - semi-transparent 比率 9% は `False`、10% は `True`
  - strong-transparent 画素数 31/1000 は `False`、32/1000 は `True`

## Rationale (Key Points)
- 直近の `MaterialLoader` は旧しきい値ではなく、`0.20/0.60`, `32`, `10%` の判定へ更新済み。
- 既存テストは「疎なノイズ」と「明確な半透明」の2ケースのみで、境界値の回帰を防ぐには不足していた。
- 境界値を明示テスト化し、次セッションで閾値再調整が必要になっても破壊的変更を検出しやすくした。

## Rollback
1. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` から追加した2テストを削除する。
2. 本記録 `docs/worklog/2026-02-11_pmx-transparency-heuristic-boundary-tests.md` を削除する。
3. Obsidian 記録 `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1650.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unity 実機環境で `SimpleModelBootstrapTests` を再実行し、新規2テストの pass を確認する。
2. AZKi / Nurse の実表示比較（Before/After）を取得し、白飛び改善を確認する。
3. 改善不十分な場合は `MaterialLoader` しきい値の再調整案を作成し、同時に境界値テストを更新する。
