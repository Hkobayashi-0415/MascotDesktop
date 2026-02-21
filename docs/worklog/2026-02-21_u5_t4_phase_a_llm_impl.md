# Worklog: U5-T4 Phase A（LLM統合）実装完了

- Date: 2026-02-21
- Task: U5-T4 Phase A（LLM統合）: chat経路の外部Core優先化、失敗時fallback維持、request_id/error_code/retryable可観測性確保
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: gemini-2.5-pro
- Used-Skills: worklog-update
- Repo-Refs:
  - `docs/05-dev/u5-core-integration-plan.md`
  - `docs/05-dev/u5-llm-tts-stt-operations.md`
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/05-dev/unity-test-environment-recovery.md`
  - `docs/05-dev/unity-test-result-collection-template.md`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`
  - `docs/worklog/2026-02-21_u5_t4_operations_kickoff.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-21_u5_t4_phase_a_llm_impl.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseA_log_260221_1507.md
- Tags: [agent/antigravity, model/gemini-2.5-pro, tool/antigravity, u5, u5-t4, phase-a, llm-integration, unity-test]

## Summary
U5-T4 Phase A（LLM統合）を完了した。
Unity実行環境エラー（`指定されたモジュールが見つかりません`）を `UNITY_COM` 環境変数で回避し、事前確認3スイートを全通過した後、`CoreOrchestrator.SendChatWithBridgeResult` を新規追加・`RuntimeDebugHud` を修正・`CoreOrchestratorLlmIntegrationTests` を新規作成して 3/3 Passed を確認した。
既存テストの回帰（LoopbackHttpClientTests 5/5）も確認済み。

## Changes

### 1. `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`（行64-110付近）
- `SendChatWithBridgeResult(text, requestId, bridgeSucceeded, bridgeErrorCode, bridgeRetryable)` を新規追加。
- bridge成功時: `core.chat.bridge_success` をINFOログに記録し、テキスト解析でstate遷移。
- bridge失敗時: `core.chat.bridge_fallback` をWARNログに記録（errorCode/retryable含む）、ローカルルールでfallback遷移。
- 既存 `SendChat` は変更なし（既存テスト保護）。

### 2. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`（行467-500付近）
- `SendChatWithBridgeAsync` を修正。
  - bridge結果を `SendChatWithBridgeResult` に渡す構造に変更。
  - `IPC.HTTP.DISABLED` / `IPC.HTTP.CLIENT_MISSING` の場合は従来の `SendChat` fallback を維持。
  - bridge試行済みの場合（成功・失敗問わず）は `SendChatWithBridgeResult` で処理。

### 3. `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`（新規作成）
- 3テストケースを実装:
  1. `SendChatWithBridgeResult_BridgeSuccess_AppliesStateDerivedFromText` — bridge成功時のstate遷移確認
  2. `SendChatWithBridgeResult_BridgeFailed_FallsBackToLocalRule` — bridge失敗時のfallback確認
  3. `SendChatWithBridgeResult_RecordsRequestIdErrorCodeRetryable_ViaRuntimeLog` — ログ記録確認

### 4. `docs/NEXT_TASKS.md`
- R13 追加（Phase A完了）。
- U5チェックリストに Phase A 完了エントリを追加。
- U5タスク表の U5-T4 状態を `In Progress (Phase A Done)` に更新。
- 参照にPhase A実装ファイルとworklogを追加。

### 5. `docs/05-dev/dev-status.md`
- Phase A完了状態を反映。
- テスト実績（5件のartifact）を追記。
- U5-T4 Phase A 実装差分セクションを追加。
- UNITY_COM緩和策を現状NGリスクセクションに追記。
- 次アクションをPhase B（TTS）に更新。

## Commands

```powershell
# Unity環境復旧試行
$env:UNITY_COM = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"

# 事前確認テスト実行
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
# → editmode-20260221_150919.xml: 5/5 Passed

./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"
# → editmode-20260221_151713.xml: 7/7 Passed

./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"
# → editmode-20260221_152801.xml: 34/34 Passed

# Phase A 新規テスト実行
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests"
# → editmode-20260221_153948.xml: 3/3 Passed

# 回帰確認
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
# → editmode-20260221_154251.xml: 5/5 Passed
```

## Tests

| run_at | test_platform | test_filter | pass_fail | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|---|
| 2026-02-21 15:09 | EditMode | LoopbackHttpClientTests | Passed | 5/5 all passed (環境復旧: UNITY_COM使用) | `Unity_PJ/artifacts/test-results/editmode-20260221_150919.xml` | `editmode-20260221_150919.log` |
| 2026-02-21 15:17 | EditMode | RuntimeErrorHandlingAndLoggingTests | Passed | 7/7 all passed | `Unity_PJ/artifacts/test-results/editmode-20260221_151713.xml` | `editmode-20260221_151713.log` |
| 2026-02-21 15:28 | EditMode | SimpleModelBootstrapTests | Passed | 34/34 all passed | `Unity_PJ/artifacts/test-results/editmode-20260221_152801.xml` | `editmode-20260221_152801.log` |
| 2026-02-21 15:39 | EditMode | CoreOrchestratorLlmIntegrationTests | Passed | 3/3 all passed (Phase A 新規テスト) | `Unity_PJ/artifacts/test-results/editmode-20260221_153948.xml` | `editmode-20260221_153948.log` |
| 2026-02-21 15:42 | EditMode | LoopbackHttpClientTests | Passed | 5/5 all passed (Phase A 回帰確認) | `Unity_PJ/artifacts/test-results/editmode-20260221_154251.xml` | `editmode-20260221_154251.log` |

## Rationale (Key Points)

### 重大リスク
- bridge処理と既存ローカルルールが二重実行され、state遷移が重複するリスク。

### 対策
- 既存 `SendChat` を変更せず、新規 `SendChatWithBridgeResult` に処理を集約。
- HUD の `SendChatWithBridgeAsync` を修正し、bridge unavailable時のみ `SendChat` fallback、bridge試行済み（成功/失敗問わず）は `SendChatWithBridgeResult` へ一本化。

### 差分意図
- `CoreOrchestrator` は「bridge結果を知る」APIを持つが、bridge自体には直接依存しない（テスタビリティ維持）。
- `SendChatWithBridgeResult` はEditModeテストで直接呼び出し可能（MonoBehaviourなし・非同期なし）。
- bridge成功/失敗いずれも同一ログ縦断を保証（`request_id` が全パスで一致）。

## Rollback
- Obsidianログは削除しない。
- ロールバック時は以下を戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`（`SendChatWithBridgeResult` 削除）
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`（旧 `SendChatWithBridgeAsync` に戻す）
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`（削除）
  - `docs/NEXT_TASKS.md`、`docs/05-dev/dev-status.md`（Phase A完了記録を削除）
- Obsidianログ `D:/Obsidian/Programming/MascotDesktop_phaseA_log_260221_1507.md` に `Rolled back` 注記を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. U5-T4 Phase B（TTS統合）: `docs/05-dev/u5-llm-tts-stt-operations.md` Phase B定義に従い、応答テキスト→TTS再生要求の経路を接続する。
2. TTS失敗時の `retryable=true/false` 区別を実装し、EditModeテストを追加する。
3. Phase B完了後、Phase C（STT）へ進む。
