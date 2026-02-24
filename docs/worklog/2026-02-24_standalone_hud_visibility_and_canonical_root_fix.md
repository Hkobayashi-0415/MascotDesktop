# Worklog: Standalone HUD Visibility and Canonical Root Fix (2026-02-24)

- Date: 2026-02-24
- Task: Standalone HUD表示切れと `Model Candidates: 0` 継続の是正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-24_standalone_hud_visibility_and_canonical_root_fix.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_1932.md
- Tags: [agent/codex, model/gpt-5, tool/codex, runtime-hud, standalone, model-discovery, bug-investigation, code-review]

## Summary
- HUD を固定高さからスクロール可能UIへ変更し、下段ボタン表示切れを解消した。
- 自動候補rescanを `Update()` 駆動の0.5秒間隔へ変更し、空候補時のログ連打を抑制した。
- canonical assets root を複数候補探索に拡張し、`Unity_PJ/data/assets_user` を優先解決できるようにした。

## Findings
1. High: HUD 固定高さ (`420x700`) で下段操作が切れる
- Evidence: ユーザー提供スクリーンショットで `Motion: wave` 以降の操作が表示されない。
- Impact: `Toggle Topmost` / `Hide/Show` などの検証不能。

2. High: Standalone で asset 解決が `ASSET.PATH.NOT_FOUND`
- Evidence: `runtime-20260224-00.jsonl` に `avatar.model.resolve_failed` (`characters/.../amane_kanata.pmx`)。
- Impact: モデル表示失敗、マゼンタフォールバック継続、`Model Candidates: 0`。

3. Medium: 空候補時に `ui.hud.model_candidates_rescanned` が高頻度連続
- Evidence: `runtime-20260224-02.jsonl` line 1478+ で連続出力。
- Impact: ログ可読性低下と調査コスト増。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- HUDを可変サイズ+スクロール (`GUILayout.BeginScrollView`) に変更。
- `Update()` で `EnsureAutoCandidateRescan()` を実行し、0.5秒間隔でのみ自動rescan。
- `EnsureModelCandidates()` から自動rescan呼び出しを除外し、表示同期専用へ整理。

2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `ResolveCanonicalAssetsRoots()` を追加し、`projectRoot` と `current directory` の祖先から canonical root を複数探索。
- `ResolveCanonicalAssetsRootForRelativePath()` を追加し、`modelRelativePath` が実在する root を優先選択。
- 候補探索ログ `avatar.model.candidates.discovered` に `canonical_roots` を追加。
- 新規ログ `avatar.paths.assets_roots_checked` を追加（selected canonical / streaming を記録）。

3. `docs/05-dev/QUICKSTART.md`
- HUDスクロール確認手順を追記。
- `avatar.paths.assets_roots_checked` の確認観点を追記。

4. `docs/05-dev/unity-runtime-manual-check.md`
- HUDスクロール確認手順と `avatar.paths.assets_roots_checked` の合格観点を追記。

## Commands
```powershell
# Logs and code inspection
Get-Content C:/Users/sugar/AppData/LocalLow/DefaultCompany/project/logs/runtime-20260224-00.jsonl -TotalCount 180
Get-Content C:/Users/sugar/AppData/LocalLow/DefaultCompany/project/logs/runtime-20260224-02.jsonl -Tail 220
Select-String -Path C:/Users/sugar/AppData/LocalLow/DefaultCompany/project/logs/runtime-20260224-*.jsonl -Pattern "avatar.model.resolve_failed|ui.hud.model_candidates_rescanned"

# Static checks
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs -Pattern "BeginScrollView|EnsureAutoCandidateRescan"
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -Pattern "ResolveCanonicalAssetsRoots|avatar.paths.assets_roots_checked"

# Unity tests (attempted)
./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests" -RequireArtifacts
```

## Test Results
- 実施有無: 部分実施
- 実施内容: 変更点の静的検証（該当メソッド/ログ/ドキュメント追記）
- Unityテスト: 実行失敗
  - 原因: この実行環境では `Unity.com` 起動時に「指定されたモジュールが見つかりません」が発生
  - 影響: EditModeテスト未実行

## Decision Notes
- HUDの可用性を優先してスクロール方式へ変更（画面サイズ依存の切れを回避）。
- canonical root は単一路ではなく複数候補探索へ変更し、Standalone配置差異に耐性を持たせた。
- 自動rescanはOnGUI依存をやめ、`Update` で一定間隔化してログ観測性を改善した。

## Next Actions
1. ユーザー環境で Standalone を再ビルドして実行。
2. HUD下段ボタンがスクロールで操作可能かを確認。
3. `avatar.paths.assets_roots_checked` / `avatar.model.candidates.discovered` / `avatar.model.displayed` を再採取。

## Rollback Plan
- 差分を戻す対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
- 手順:
  - 変更箇所を逆パッチ適用。
  - 本worklogにロールバック理由（何を・なぜ）を追記。
  - Obsidianログは削除せず `Rolled back` / `Superseded` を追記。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Used-Skills recorded: Yes
- Tags include agent/model/tool: Yes
