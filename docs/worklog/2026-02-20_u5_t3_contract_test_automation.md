# 作業ログ: U5-T3 契約テスト自動化の実装着手

- 日付: 2026-02-20
- タスク: `request_id` / `error_code` / `retryable` 契約テストを EditMode 自動化へ拡張
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/05-dev/unity-test-environment-recovery.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_u5_t3_contract_test_automation.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_1557.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t3, contract-test]

## 変更内容
1. `LoopbackHttpClient` のレスポンス解釈を契約対応へ拡張。
   - `LoopbackHttpResult` に `ResponseRequestId` / `Retryable` を追加。
   - response body の `status/request_id/error_code/message/retryable` を解析。
   - response `request_id` 不一致時に `IPC.HTTP.REQUEST_ID_MISMATCH` を返す。
   - non-success/payload-error 時は body の `error_code/message/retryable` を優先して返す。
2. `LoopbackHttpClientTests` に契約テストを追加。
   - non-success 応答で `error_code/retryable/message` を取得できること。
   - response `request_id` 不一致を検出できること。
3. 進捗文書を更新。
   - `NEXT_TASKS`: U5-T3 を `In Progress` へ変更。
   - `dev-status`: U5-T3 実装着手と本日の検証状況を追記。

## 実行コマンド
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- `Get-Content docs/02-architecture/interfaces/ipc-contract.md`
- `apply_patch`（`LoopbackHttpClient.cs` 更新）
- `apply_patch`（`LoopbackHttpClientTests.cs` 更新）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260220_155717.{xml,log}`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260220_155723.{xml,log}`

## テスト結果
- 実行1: `LoopbackHttpClientTests`
  - 結果: 失敗（Unity起動前エラー）
  - エラー: `指定されたモジュールが見つかりません`（`tools/run_unity_tests.ps1:69`）
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155717.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155717.log`: 未生成
- 実行2: `RuntimeErrorHandlingAndLoggingTests`
  - 結果: 失敗（Unity起動前エラー）
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155723.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155723.log`: 未生成
- 実行3: `SimpleModelBootstrapTests`
  - 結果: 失敗（Unity起動前エラー）
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155723.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_155723.log`: 未生成

## 判断理由（要点）
- `ipc-contract` で要求される `error_code/retryable/request_id` を実際に result とテストで検証できる形にすることを優先。
- 実行環境エラーで検証が完了していないため、`U5-T3` は `Done` にせず `In Progress` とした。

## 次アクション
- `docs/05-dev/unity-test-environment-recovery.md` に沿って Unity 実行環境を復旧。
- 同一3コマンドを再実行し、artifact 生成と契約テスト通過を確認。
- 通過確認後に `U5-T3` を `Done` へ更新。

> **[2026-02-21 再実行済み]** `docs/worklog/2026-02-21_u5_t3_contract_test_rerun.md` を参照。
> - RuntimeErrorHandlingAndLoggingTests: 7/7 Pass ✅
> - SimpleModelBootstrapTests: 34/34 Pass ✅
> - LoopbackHttpClientTests: 3/5 Pass（2件 `LogAssert.Expect` 欠如で失敗）❌

## ロールバック方針
- 変更ファイルを変更前内容へ戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_1557.md` に `Rolled back` または `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
