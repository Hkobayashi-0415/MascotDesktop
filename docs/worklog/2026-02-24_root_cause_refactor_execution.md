# Worklog: Root-Cause Refactor Execution (2026-02-24)

- Date: 2026-02-24
- Task: 軽量化と保守性向上を目的とした根本治療リファクタ実行
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AssetCatalogService.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowNativeGateway.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/AssetCatalogServiceTests.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/WindowNativeGatewayTests.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `tools/run_unity_tests.ps1`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_2129.md`
- Report-Path: docs/worklog/2026-02-24_root_cause_refactor_execution.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_2129.md
- Tags: [agent/codex, model/gpt-5, tool/codex, root-cause, refactor, performance, logging, windowing]

## Summary
- 候補探索を `AssetCatalogService` へ分離し、キャッシュ + 空探索時指数バックオフで高頻度I/Oを抑制した。
- Windowネイティブ呼び出しを `WindowNativeGateway` へ集約し、Controller側の責務を簡素化した。
- WindowRect保存/復元失敗ログを追加し、失敗時の追跡性を強化した。
- 新規EditModeテストを追加したが、Unity起動前失敗のため実行検証は未完了。

## Findings (Severity Order)
1. High: 候補探索が高頻度で全列挙される負荷リスク
- File: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:350`, `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:203`
- Issue: 空候補時に短周期で全ファイル列挙が反復。
- Fix: `AssetCatalogService` で cache TTL + empty-scan backoff を導入。

2. Medium: Windowネイティブ呼び出しが分散しエラーハンドリングが不均一
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`, `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- Issue: HWND取得/Win32呼び出しの重複と失敗観測の漏れ。
- Fix: `WindowNativeGateway` へ統合し、Controllerでerror_code付きログを統一。

3. Medium: ルート列挙失敗時に探索全体へ影響
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:244`
- Issue: `EnumerateFiles` の例外で探索の安定性が低い。
- Fix: root単位の try/catch + `avatar.model.candidates.root_scan_failed` ログ追加。

## Changes
### Change Unit 1: Candidate discovery root-cause treatment
- Files:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AssetCatalogService.cs` (new)
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/AssetCatalogServiceTests.cs` (new)
- Diff Summary:
  - `AssetCatalogService` を追加し、`GetOrRefresh` で cache hit / throttle / force refresh を管理。
  - `SimpleModelBootstrap.DiscoverModelCandidates/DiscoverImageCandidates` を service経由へ変更。
  - `BuildRelativeAssetPathsFromRoots` に requestId引数（任意）と root単位例外ログ追加。
  - HUDの `Model: rescan(list)` を `forceRefresh: true` へ変更（強制再探索）。
  - `AssetCatalogServiceTests` で cache/backoff/force refresh/計算ロジックを検証。
- Test Result:
  - 実行コマンド: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.AssetCatalogServiceTests" -RequireArtifacts`
  - 結果: Failed（Unity.exe/Unity.com 起動前失敗: 指定されたモジュールが見つかりません）
  - 代替静的検証: 追加API参照整合、呼び出し経路、ログイベント名を確認。

### Change Unit 2: Windowing gateway consolidation
- Files:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowNativeGateway.cs` (new)
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/WindowNativeGatewayTests.cs` (new)
- Diff Summary:
  - Win32呼び出し（HWND取得、topmost、minimize/restore、frameless、rect、drag）を `WindowNativeGateway` に集約。
  - `ResidentController` / `WindowController` は gateway 呼び出し + ログ責務に簡素化。
  - `WindowController` に `window.rect.save_failed` / `window.rect.restore_failed` 失敗ログを追加。
  - `WindowNativeGatewayTests` で non-native fallback を固定値で検証。
- Test Result:
  - Unity起動前失敗により EditMode 未実行。
  - 代替静的検証: 旧P/Invokeの重複除去、controller→gateway依存の置換完了を確認。

### Change Unit 3: Runbook alignment for forced refresh semantics
- Files:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
- Diff Summary:
  - 候補探索は cache+backoff で自動負荷抑制されること、`Model: rescan(list)` が強制再探索であることを追記。
- Test Result:
  - ドキュメント変更のため実行テスト対象外。
  - 静的検証: 実装（HUD + service）との整合を確認。

## Commands
```powershell
# Skill/docs read
Get-Content D:\dev\00_repository_templates\ai_playbook\skills\phase-planning\SKILL.md

# Code inspection
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs

# Static spot checks
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Avatar/*.cs -Pattern 'GetOrRefresh|root_scan_failed|forceRefresh'
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Windowing/*.cs -Pattern 'WindowNativeGateway|window.rect.save_failed|window.rect.restore_failed'

# EditMode test attempt
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.AssetCatalogServiceTests" -RequireArtifacts
```

## Test Results
- EditMode test run: Failed
  - Date/Time: 2026-02-24 21:28
  - Reason: Unity.exe / Unity.com 起動前失敗（`指定されたモジュールが見つかりません`）
  - Artifact: 未生成
- Static verification: Passed
  - 参照整合（controller->gateway, bootstrap->service, HUD->force refresh）
  - ログ契約（error_code/event_name）
  - ドキュメント整合

## Decision Notes
- 高頻度探索問題はUI周期調整ではなく、探索層のキャッシュ/バックオフ制御で根治した。
- Windowingは機能別修正より先にネイティブ境界を一本化し、今後の修正コストを下げた。

## Next Actions
1. Unity起動可能環境で新規2テスト + 既存回帰（SimpleModelBootstrap/RuntimeLog）を実行。
2. Standaloneで `Topmost/Drag/Hide/Show` 実機挙動ログを再採取。
3. 必要に応じて `ModelDisplayService` 分離（Bootstrapの更なる縮小）を次フェーズで実施。

## Rollback Plan
- 差分を戻す対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AssetCatalogService.cs` (+ meta)
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowNativeGateway.cs` (+ meta)
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/AssetCatalogServiceTests.cs` (+ meta)
  - `Unity_PJ/project/Assets/Tests/EditMode/WindowNativeGatewayTests.cs` (+ meta)
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
- 手順:
  - 変更箇所を逆パッチで戻す。
  - 本worklogにロールバック理由を追記。
  - Obsidianログは削除せず `Rolled back` / `Superseded` を追記。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Used-Skills recorded: Yes
- Tags include agent/model/tool: Yes
