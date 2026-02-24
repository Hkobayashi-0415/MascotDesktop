# Worklog: Code Review + Minimal Refactor Handover (2026-02-24)

- Date: 2026-02-24
- Task: 指定6ファイルのコードレビューと必要最小限リファクタリング
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `tools/run_unity_tests.ps1`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_2109.md`
- Report-Path: docs/worklog/2026-02-24_code_review_refactor_handover.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_2109.md
- Tags: [agent/codex, model/gpt-5, tool/codex, code-review, refactor, runtime-hud, windowing]

## Summary
- Runtime/Windowing/Docs の指定6ファイルをレビューし、挙動退行リスクの高い箇所のみ修正した。
- 仕様変更は未実施（Hide/Show は最小化/復帰仕様を維持）。
- EditMode テストは実行を試行したが、Unityプロセス起動前失敗で未完了。

## Findings (Severity Order)
1. High: Resident状態が実ウィンドウ状態と不整合になる可能性
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs:70`, `:126`, `:92`, `:138`
- 問題: `ShowWindow` 戻り値未評価のまま `IsHidden` を更新していたため、最小化/復帰失敗時に内部状態と実状態が乖離する。
- 対応: `ShowWindow` 失敗時は WARN + 早期 return とし、状態更新を抑止。

2. Medium: DragWindow の HWND 解決が他操作と不統一で失敗しやすい
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs:101-108`
- 問題: `DragWindow` が `GetActiveWindow` のみで、`SetTopmost` と異なり `GetForegroundWindow` フォールバックがなかった。
- 対応: `GetForegroundWindow` フォールバックを追加してハンドル解決の一貫性を確保。

3. Medium: Frameless適用ログが成功誤判定になる可能性
- File: `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs:214-242`
- 問題: style変更後の検証・frame更新結果チェックがなく、失敗時でも `window.frameless.applied` が出る可能性があった。
- 対応: style反映検証と `SetWindowPos` 結果チェックを追加し、失敗時は error_code 付き WARN へ分岐。

4. Medium: 候補rescanログが候補数不変でも継続出力される
- File: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:368-413`
- 問題: 自動rescan時に `ui.hud.model_candidates_rescanned` を毎回出力し、空候補継続時のログ可読性を下げる。
- 対応: 候補数変化時のみログ、手動 rescan は強制ログ。

5. Doc Alignment: Hide/Show の現仕様（最小化/復帰）が手順に明示されていない
- File: `docs/05-dev/QUICKSTART.md:33`, `docs/05-dev/unity-runtime-manual-check.md:41`
- 対応: `SW_MINIMIZE` / `SW_RESTORE` の現仕様を追記。

## Changes
### Change Unit 1: Windowing failure handling hardening
- Files:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- Diff Summary:
  - `ResidentController`: `ShowWindow` 失敗時に `WINDOW.RESIDENT.MINIMIZE_FAILED` / `WINDOW.RESIDENT.RESTORE_FAILED` を出して状態更新を中止。
  - `WindowController`: `DragWindow`/`SaveWindowState`/`TryRestoreWindowRect` で `GetForegroundWindow` フォールバック追加。
  - `WindowController`: Frameless適用後のstyle検証 + frame更新失敗検知（`WINDOW.FRAMELESS.STYLE_NOT_APPLIED` / `WINDOW.FRAMELESS.FRAME_UPDATE_FAILED`）。
- Test Result:
  - 実行コマンド: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeLogTests" -RequireArtifacts`
  - 結果: Failed（Unity.exe/Unity.com 起動前失敗: 指定されたモジュールが見つかりません）
  - 代替静的検証: 追加 error_code/分岐の存在確認、既存Win32分岐との整合確認を実施。

### Change Unit 2: HUD log-noise reduction
- Files:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- Diff Summary:
  - 自動rescanの `ui.hud.model_candidates_rescanned` を候補数差分時に限定。
  - ボタン由来の `Model: rescan(list)` は `forceLog: true` で操作ログを維持。
- Test Result:
  - Unityテストは上記と同様に起動前失敗。
  - 代替静的検証: `forceLog` 経路と差分判定条件の整合を確認。

### Change Unit 3: Runbook/doc alignment
- Files:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
- Diff Summary:
  - Hide/Show の現仕様を「最小化/復帰（`SW_MINIMIZE` / `SW_RESTORE`）」として明記。
- Test Result:
  - ドキュメント変更のため実行テスト対象外。
  - 代替静的検証: 実装 (`ResidentController`) と手順文言の一致を確認。

## Commands
```powershell
# Repo identification (git unavailable fallback)
Get-Content .git/HEAD
Get-Content .git/config
Get-ChildItem -Force -Name .git

# Target file review
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs
Get-Content docs/05-dev/QUICKSTART.md
Get-Content docs/05-dev/unity-runtime-manual-check.md

# Spot checks
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs -Pattern 'WINDOW.RESIDENT'
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs -Pattern 'GetForegroundWindow|WINDOW.FRAMELESS'
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs -Pattern 'forceLog|candidateCountsChanged'

# EditMode test attempt
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeLogTests" -RequireArtifacts
```

## Test Results
- EditMode test run: Failed
  - Date/Time: 2026-02-24 21:08
  - Reason: Unity.exe および Unity.com の起動前失敗（`指定されたモジュールが見つかりません`）
  - Artifact: 未生成（プロセス起動前失敗のため）
- Static verification: Passed
  - 条件分岐・error_code・ログ文言・ドキュメント仕様整合を確認。

## Decision Notes
- 仕様不変を優先し、Windowingの失敗時整合性のみを補強した。
- Unity実行不能時の品質担保として、ログ契約と分岐整合の静的根拠を優先した。

## Next Actions
1. ユーザー環境で `Toggle Topmost` / `Hide/Show` / `Drag Window` の実機確認を実施。
2. Unity起動可能環境で EditMode テストを再実行し、artifact を採取。
3. 必要なら Windowing 用の EditMode テスト（Win32分岐は抽象化して）を追加検討。

## Rollback Plan
- 差分を戻す対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
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
