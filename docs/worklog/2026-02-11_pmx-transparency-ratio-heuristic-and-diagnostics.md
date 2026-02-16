# Worklog: pmx-transparency-ratio-heuristic-and-diagnostics

- Date: 2026-02-11
- Task: 透明判定の強透明条件を比率化し、material diagnostics に transparentMats を追加
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, docs/worklog/2026-02-11_pmx-whiteout-context-handoff.md, docs/worklog/2026-02-11_pmx-transparency-heuristic-boundary-tests.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-transparency-ratio-heuristic-and-diagnostics.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1709.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, transparency, diagnostics, whiteout]

## Summary
`MaterialLoader` の strong-transparent 判定を「絶対画素数」から「比率」へ変更し、大きいテクスチャで少数透明画素に引きずられる誤判定を抑制した。加えて `avatar.model.material_diagnostics` に `transparentMats` を追加し、白飛び再現時の透明シェーダ過多を定量観測できるようにした。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `StrongTransparentPixelCountThreshold` を廃止し、`StrongTransparentPixelRatioThreshold = 0.032f` を導入。
  - `IsTextureTransparentByPixels()` で `strongTransparentPixelCount` を比率化して判定。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - 既存テスト名更新: `MaterialLoaderTransparencyHeuristic_UsesStrongTransparentRatioThreshold`
  - 追加: `MaterialLoaderTransparencyHeuristic_DoesNotTriggerOnLargeTextureSparseStrongAlpha`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `avatar.model.material_diagnostics` に `transparentMats` を追加。
  - `MMD/Transparent/` シェーダ材質数をカウントするロジックを追加。

## Commands
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`（診断出力部確認）
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`（透明判定部確認）
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`（テスト確認）
- `apply_patch`（MaterialLoader.cs / SimpleModelBootstrapTests.cs / SimpleModelBootstrap.cs）
- `rg -n "StrongTransparentPixelRatioThreshold|DoesNotTriggerOnLargeTextureSparseStrongAlpha|transparentMats=|MMD/Transparent/" ...`
- `& .\tools\run_unity_tests.ps1 -TestPlatform EditMode -TestFilter MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests -Quit`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_170851.xml`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_170851.log`

## Tests
- Target: `MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests`
- Result: **Failed to execute**（Unity起動失敗）
  - Error: `Program 'Unity.exe' failed to run ... 指定されたモジュールが見つかりません。`
  - XML/log output: `False / False`
- Added coverage:
  - strong判定の比率閾値境界（31/1000=False, 32/1000=True）
  - 大サイズテクスチャ疎透明（40/10000=False）

## Rationale (Key Points)
- 直近観測では `MMD/Transparent/...` 材質が多く、白飛びモデルで過剰Transparent化の疑いが継続していた。
- 強透明を絶対個数で判定すると、解像度増加に対して誤判定しやすいため比率化が必要だった。
- `transparentMats` をログに追加し、今後の比較（AZKi/Nene/Nurse/Chloe）を定量的に判断できる状態にした。

## Rollback
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` の `transparentMats` 追加を戻す。
2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` の追加/改名テストを戻す。
3. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs` の strong 比率化を戻す。
4. 本記録 `docs/worklog/2026-02-11_pmx-transparency-ratio-heuristic-and-diagnostics.md` を削除。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unity 実機で 8モデルを再走査し、`transparentMats` と見た目白飛びの相関を取得する。
2. AZKi/Nene/Nurse の `transparentMats` が高止まりする場合は、semi判定閾値の再調整候補（例: 0.10 -> 0.12）をA/Bで検証する。
3. 依然白飛びが残る場合は、toon/sphere 欠損時のシェーダ係数抑制を検討する。
