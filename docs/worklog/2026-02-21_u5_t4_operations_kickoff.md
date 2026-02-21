# Worklog: U5-T4 段階統合運用ドキュメント着手と状態同期

- Date: 2026-02-21
- Task: U5-T4 の運用ドキュメント整備と `NEXT_TASKS` / `dev-status` 同期、EditMode テスト再実行結果の記録
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `docs/05-dev/u5-llm-tts-stt-operations.md`
  - `docs/05-dev/u5-core-integration-plan.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `tools/run_unity_tests.ps1`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-21_u5_t4_operations_kickoff.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_1239.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t4, docs-sync, unity-test]

## Summary
U5-T4 の運用導線として `docs/05-dev/u5-llm-tts-stt-operations.md` を基準文書に位置づけ、`NEXT_TASKS` と `dev-status` を In Progress 状態に同期した。
検証として EditMode 3スイートを再実行したが、Unity 起動時に `指定されたモジュールが見つかりません` が再発し、artifact は未生成だった。

## Changes
1. `docs/05-dev/u5-core-integration-plan.md`
- Last Updated を 2026-02-21 へ更新。
- U5-T4 運用導線セクションを追加し、`u5-llm-tts-stt-operations.md` を正本参照に設定。

2. `docs/NEXT_TASKS.md`
- 改訂履歴 `R12` を追加（U5-T4 実行開始）。
- U5 チェックリストの運用手順参照先を `docs/05-dev/u5-llm-tts-stt-operations.md` に更新。
- U5タスク表の `U5-T4` を `Open -> In Progress` に更新。
- 参照に U5-T4 関連ドキュメントと本 worklog を追加。

3. `docs/05-dev/dev-status.md`
- 日付を 2026-02-21 へ更新。
- `U5-T3` 完了・`U5-T4` 実行中を反映。
- U5 基準ドキュメントに `u5-llm-tts-stt-operations.md` を追加。
- 2026-02-21 12:39 の再実行失敗（3スイート起動前失敗、artifact未生成）をリスクへ追記。

## Commands
```powershell
Get-Content .git/HEAD
Get-Content .git/config
Get-Content docs/NEXT_TASKS.md
Get-Content docs/05-dev/dev-status.md
Get-Content docs/05-dev/u5-core-integration-plan.md
Get-Content docs/05-dev/u5-llm-tts-stt-operations.md
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"; Start-Sleep -Seconds 2; ./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"; Start-Sleep -Seconds 2; ./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123901.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123901.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123903.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123903.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123905.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_123905.log
```

## Tests
| suite | command timestamp (expected artifact stem) | pass_fail | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|
| LoopbackHttpClientTests | 2026-02-21 12:39 (`editmode-20260221_123901`) | Failed (起動前) | Unity.exe 起動時 `指定されたモジュールが見つかりません` | missing | missing |
| RuntimeErrorHandlingAndLoggingTests | 2026-02-21 12:39 (`editmode-20260221_123903`) | Failed (起動前) | Unity.exe 起動時 `指定されたモジュールが見つかりません` | missing | missing |
| SimpleModelBootstrapTests | 2026-02-21 12:39 (`editmode-20260221_123905`) | Failed (起動前) | Unity.exe 起動時 `指定されたモジュールが見つかりません` | missing | missing |

補足:
- 先行実行の `editmode-20260221_123836` / `123843` も同様に未生成。
- `run_unity_tests.ps1` の PowerShell プロセス戻り値は 0 でも、Unity 起動失敗が標準エラーに出力されるため、artifact 有無と併用判定を採用。

## Rationale (Key Points)
- U5-T4 は実装前に運用ルールを固定しないとフェーズ進行時の rollback 条件がぶれるため、先に運用基準文書を正本化した。
- `NEXT_TASKS` / `dev-status` / 運用文書を同時同期することで、タスク状態と運用手順の不整合を防止した。
- テストは環境起因で失敗しても実行事実・原因・artifact 有無を残すことで、次回復旧判断に必要な証跡を確保した。

## Rollback
- Obsidianログは削除しない。
- ロールバック時は以下を戻す:
  - `docs/05-dev/u5-core-integration-plan.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/worklog/2026-02-21_u5_t4_operations_kickoff.md`
- ロールバック理由（何を・なぜ戻したか）を本ファイルへ追記する。
- Obsidianログ `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_1239.md` に `Rolled back` / `Superseded` 注記を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. `docs/05-dev/unity-test-environment-recovery.md` に従って Unity 実行環境のモジュール不足を復旧する。
2. 3スイートを再実行して artifact を再取得し、U5-T4 Phase A の判定ログへ転記する。
3. U5-T4 Phase A（LLM）実装差分に着手し、`request_id/error_code/retryable` の運用記録を採取する。
