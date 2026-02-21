# 作業ログ: U5-T2 Minimal Core Bridge 実装着手

- 日付: 2026-02-20
- タスク: Runtime HUD から health/chat/config の loopback bridge 導線を追加し、U5-T2 を実装着手状態へ更新
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs.meta`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/05-dev/unity-test-environment-recovery.md`
  - `docs/05-dev/unity-test-result-collection-template.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_u5_t2_minimal_core_bridge.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0941.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t2, minimal-core-bridge]

## 変更内容
1. `RuntimeDebugHud` に bridge 導線を追加。
   - `Chat: hello/happy` を bridge 経由送信 + local fallback に変更。
   - `Bridge: health` / `Bridge: config/get` / `Bridge: config/set` ボタンを追加。
   - 最新 bridge 結果（status/request_id）を HUD 表示へ追加。
2. `LoopbackHttpClientTests` を新規追加。
   - bridge disabled 時のエラーコード検証。
   - header `X-Request-Id` と body `request_id` 注入の検証。
   - request_id 自動生成時の相関検証。
3. `NEXT_TASKS` / `dev-status` を同期更新。
   - U5-T2 を `In Progress` へ変更。
   - 検証ブロッカー（Unity起動時モジュール不足）を追記。

## 実行コマンド
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`
- `apply_patch`（`RuntimeDebugHud.cs` 更新）
- `apply_patch`（`LoopbackHttpClientTests.cs` / `.meta` 追加）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Get-ChildItem Unity_PJ/artifacts/test-results | Sort-Object LastWriteTime -Descending | Select-Object -First 20 Name,Length,LastWriteTime`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260220_094003.{xml,log}`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260220_094011.{xml,log}`
- `apply_patch`（`docs/NEXT_TASKS.md` 更新）
- `apply_patch`（`docs/05-dev/dev-status.md` 更新）

## テスト結果
- 実行1: `LoopbackHttpClientTests`
  - 結果: 失敗（Unity起動前エラー）
  - エラー: `指定されたモジュールが見つかりません`（`tools/run_unity_tests.ps1:69` の Unity.exe 起動時）
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094003.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094003.log`: 未生成
- 実行2: `RuntimeErrorHandlingAndLoggingTests`
  - 結果: 失敗（Unity起動前エラー）
  - エラー: `指定されたモジュールが見つかりません`
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094011.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094011.log`: 未生成
- 実行3: `SimpleModelBootstrapTests`
  - 結果: 失敗（Unity起動前エラー）
  - エラー: `指定されたモジュールが見つかりません`
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094011.xml`: 未生成
    - `Unity_PJ/artifacts/test-results/editmode-20260220_094011.log`: 未生成

## テスト結果 (追記: 10:29-15:00)
- 実行1: `LoopbackHttpClientTests`
  - 結果: 失敗 (1 Passed, 2 Failed)
  - Log: `Unity_PJ/artifacts/test-results/editmode-20260220_102912.log`
  - 詳細:
    - PostJsonAsync_GeneratesRequestId_WhenMissing: Expected True, Was False
    - PostJsonAsync_InjectsRequestIdIntoHeaderAndBody_WhenMissingInBody: Expected True, Was False
- 実行2: `RuntimeErrorHandlingAndLoggingTests`
  - 結果: 成功 (7 Passed)
  - Log: `Unity_PJ/artifacts/test-results/editmode-20260220_104153.log`
- 実行3: `SimpleModelBootstrapTests`
  - 結果: 成功 (34 Passed)
  - Log: `Unity_PJ/artifacts/test-results/editmode-20260220_150020.log`

## 判断理由（要点）
- U5-T2 の完了条件は HUD からの loopback 実行可能化であり、まず導線と request_id 連携実装を優先した。
- Unity環境差異により実行検証が不可能なため、状態は `Done` ではなく `In Progress` を維持した。
- テストは要求コマンドを実行し、失敗内容と artifact 生成有無を明示して追跡可能性を確保した。

## 次アクション
- `docs/05-dev/unity-test-environment-recovery.md` に沿って Unity実行環境を復旧。
- U5-T2 の実機検証（HUD操作で `request_id` 相関確認）を完了。
- 復旧後に U5-T3 契約テストの追加・再実行へ進む。

## ロールバック方針
- 変更ファイルを変更前内容へ戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs.meta`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0941.md` に `Rolled back` または `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes

## Follow-up (2026-02-20 15:21)
- 変更内容:
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs` を修正。
  - `SetUp` で `LoopbackHttpClient.runtimeConfig` に `RuntimeConfig` を注入する初期化を追加。
- 修正理由:
  - `editmode-20260220_102912.xml` の失敗ログで、失敗2件とも `IPC.HTTP.CONFIG_MISSING` が出力されていた。
  - 原因は test fixture 側で `runtimeConfig` を明示注入していなかったこと。
- 再実行コマンド:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"`
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"`
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 再実行結果:
  - 3件ともテスト成功。
  - artifact:
    - `Unity_PJ/artifacts/test-results/editmode-20260220_154422.xml`: LoopbackHttpClientTests (Passed)
    - `Unity_PJ/artifacts/test-results/editmode-20260220_154445.xml`: RuntimeErrorHandlingAndLoggingTests (Passed)
    - `Unity_PJ/artifacts/test-results/editmode-20260220_154500.xml`: SimpleModelBootstrapTests (Passed)

## Follow-up (2026-02-20 15:46)
- 文書同期:
  - `docs/NEXT_TASKS.md`: `U5-T2` を `Done` へ更新。
  - `docs/05-dev/dev-status.md`: U5-T2 完了と最新実行結果を反映。
- 次アクション更新:
  - `U5-T3`（契約テスト自動化）へ移行。
