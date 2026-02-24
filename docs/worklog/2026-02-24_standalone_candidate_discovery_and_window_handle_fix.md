# Worklog: Standalone Candidate Discovery + Window Handle Fallback (2026-02-24)

- Date: 2026-02-24
- Task: Standaloneで `Model Candidates: 0` / 操作不達に見える問題の原因調査と修正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-24_standalone_candidate_discovery_and_window_handle_fix.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_1902.md
- Tags: [agent/codex, model/gpt-5, tool/codex, runtime-hud, standalone, bug-investigation, code-review]

## Summary
- 根本原因は、候補探索が `Unity_PJ/data/assets_user` 単一固定だった点で、Standalone のビルド配置によって `Model Candidates: 0` になっていた。
- 候補探索を `assets_user + StreamingAssets` へ拡張し、`data/assets_user` を親ディレクトリ探索で自動検出するよう修正した。
- Topmost / Hide / Show の不達リスク低減として、Windowsハンドル取得に `GetForegroundWindow()` フォールバックを追加した。

## Findings (bug-investigation)
1. High: 候補探索が canonical root 固定で、Standalone配置に依存して空配列になる
- Evidence: `SimpleModelBootstrap.DiscoverAllRelativeAssetPaths()` が `Path.Combine(unityPjRoot, "data", "assets_user")` のみ走査していた。
- Impact: HUD の `Model Candidates` / `Image Candidates` が 0 になり、`Model: next/prev` がスキップされる。

2. Med: Window 操作で `GetActiveWindow()` 失敗時の救済がなく、操作が無反応に見える
- Evidence: `WindowController.SetTopmost` / `ResidentController.HideToResident` / `RestoreFromResident` で active handle のみ使用。
- Impact: Topmost / Hide / Show が不達のままになるケースがある。

## Code Review Notes
- `BuildRelativeAssetPathsFromRoots` を追加し、重複排除・正規化を明示化した。
- 新規テストでルート統合時の重複排除と欠損ルート無視をカバーした。
- 既存仕様（relative path優先、legacy path禁止）は維持。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `DiscoverAllRelativeAssetPaths()` を修正。
  - canonical root + `Application.streamingAssetsPath` を統合探索。
  - 診断ログ `avatar.model.candidates.discovered` を追加。
- `BuildRelativeAssetPathsFromRoots(IEnumerable<string>)` を追加。
- `ResolveProjectRoots()` に `TryFindUnityPjRoot()` を導入し、親ディレクトリ探索で `data/assets_user` を検出。

2. `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- `SetTopmost()` で `GetActiveWindow()` 失敗時に `GetForegroundWindow()` へフォールバック。

3. `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
- `HideToResident()` / `RestoreFromResident()` で `GetForegroundWindow()` フォールバックを追加。

4. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `BuildRelativeAssetPathsFromRoots_MergesDistinctRelativePathsAcrossRoots`
- `BuildRelativeAssetPathsFromRoots_ReturnsEmpty_WhenRootsAreMissing`

5. Documentation
- `docs/05-dev/QUICKSTART.md` に Standalone の assets root 前提と troubleshooting を追記。
- `docs/05-dev/unity-runtime-manual-check.md` に Standalone 配置条件と候補探索ログ確認を追記。
- `Unity_PJ/docs/05-dev/phase3-parity-verification.md` に候補探索ログ観点を追記。

## Commands
```powershell
# Investigation
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs
Get-ChildItem Unity_PJ/data/assets_user -Recurse
Get-ChildItem Unity_PJ/project/Assets/StreamingAssets -Recurse

# Verification (static)
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -Pattern "avatar.model.candidates.discovered|BuildRelativeAssetPathsFromRoots|TryFindUnityPjRoot"
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs -Pattern "GetForegroundWindow"
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs -Pattern "GetForegroundWindow"
Select-String -Path Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Pattern "BuildRelativeAssetPathsFromRoots_"

# Root detection sanity check (PowerShell simulation)
$start='D:/dev/MascotDesktop/Unity_PJ/project/platform_dev'; $current=Get-Item $start; while($current){ $probe=Join-Path $current.FullName 'data/assets_user'; if(Test-Path $probe){ $current.FullName; break }; $current=$current.Parent }
```

## Test Results
- 実施有無: 部分実施（静的検証のみ）
- 実施内容:
  - 変更箇所の存在確認 (`Select-String`)。
  - ルート探索想定の sanity check（PowerShell で親探索が `Unity_PJ` を検出）。
- 未実施:
  - Unity EditMode test 実行（この環境で Unity プロセス起動不可のため）。
- ユーザー環境での実行依頼:
  - 4スイート再実行（既存手順）。
  - Windows Standalone 起動後、`avatar.model.candidates.discovered` / `avatar.model.displayed` / `window.*` ログ確認。

## Decision Rationale
- 既存症状（candidates=0）は候補探索範囲の不足で説明可能かつ再発しやすい。
- ルート自動検出 + 複数ルート探索は挙動変更を最小にしつつ再現条件を狭められる。
- Windowハンドルのフォールバック追加は副作用が小さく、操作不達の回避に有効。

## Next Actions
1. Windows Standalone を再実行し、HUD candidates が 0 でないことを確認。
2. `State/Motion/Topmost/Hide/Show` を再検証し、ログ（request_id付き）を採取。
3. 4スイートを再実行し、最新XMLの `total/passed/failed/result` を記録。

## Rollback Plan
- 変更戻し対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
- 手順:
  - 該当差分を逆適用。
  - `docs/worklog/` に rollback 理由を追記。
  - Obsidianログは削除せず `Rolled back` / `Superseded` を追記。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Used-Skills recorded: Yes
- Tags include agent/model/tool: Yes
