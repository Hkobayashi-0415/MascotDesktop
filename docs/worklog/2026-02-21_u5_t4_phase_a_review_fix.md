# Worklog: U5-T4 Phase A レビュー修正（4項目）

- Date: 2026-02-21
- Task: U5-T4 Phase A コードレビュー指摘4項目の修正
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: gemini-2.5-pro
- Used-Skills: worklog-update
- Repo-Refs:
  - `tools/run_unity_tests.ps1`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`
  - `docs/worklog/2026-02-21_u5_t4_phase_a_llm_impl.md`
- Obsidian-Refs: n/a（本worklogは差分補完のみ: Obsidianログは前回作成済み）
- Report-Path: docs/worklog/2026-02-21_u5_t4_phase_a_review_fix.md
- Obsidian-Log: 前回ログ（MascotDesktop_phaseA_log_260221_1507.md）に注記として追記を推奨
- Tags: [agent/antigravity, model/gemini-2.5-pro, tool/antigravity, u5, u5-t4, phase-a, review-fix, unity-test]

## Summary
レビュー指摘4項目（High×1・Medium×2・Low×1）を全件修正した。
`run_unity_tests.ps1` にUnity.com自動フォールバック追加、`CoreOrchestrator.cs` のpathフィールド誤用修正、`RuntimeDebugHud.cs` のSendChatWithBridgeResult統一、テスト2件追加（AvatarStateChanged rid伝搬）。
CoreOrchestratorLlmIntegrationTests 5/5 Passed・LoopbackHttpClientTests 5/5 Passed（回帰）を確認。

## Changes

### 1. `tools/run_unity_tests.ps1`（行67-104）[High]
- 実行ブロックを `try/catch` で囲み、Unity.exe 起動失敗時に Unity.com へ自動フォールバック。
- 両方失敗時は `exit 1`（非0終了）で確実に失敗を報告する。
- 受入確認: Unity.exe 失敗 → Unity.com 試行 → 失敗時は非0終了。

### 2. `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`（行83-89）[Medium]
- `bridge_success` ログの `path` 引数に `message`（ユーザーテキスト）を渡していた誤用を修正。
- `path` に `/v1/chat/send`（エンドポイント）を設定し、`message` に `text_length` メタ情報のみ記録する。
- ユーザー生文がログに露出しなくなった。

### 3. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`（行477-505）[Medium]
- `isBridgeUnavailable` 分岐（`SendChat` 直行）を除去。
- `bridgeResult == null` を `IPC.HUD.BRIDGE_RESULT_NULL` として `SendChatWithBridgeResult` に統一。
- 全ケースで `core.chat.bridge_fallback` イベントが記録される観測軸の一元化が完成。

### 4. `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`（行115-181追加）[Low]
- `AvatarStateChanged` イベントのridが入力requestIdと一致することを bridge成功・失敗の両ケースで検証するテスト2件を追加。
- 全5件 Passed を確認。

## Tests

| run_at | filter | pass_fail | artifact_xml |
|---|---|---|---|
| 2026-02-21 16:18 | CoreOrchestratorLlmIntegrationTests | Passed 5/5 | `editmode-20260221_161858.xml` |
| 2026-02-21 16:36 | LoopbackHttpClientTests（回帰） | Passed 5/5 | `editmode-20260221_163613.xml` |

## Rationale

### 重大リスク
- High: Unity.exe 存在確認だけでは起動失敗を捕捉できず、UNITY_COM 設定が無効になる。
- Medium: bridge null/DISABLED 時の `SendChat` 直行で `core.chat.bridge_fallback` が記録されない。
- Medium: `path` フィールドにユーザー入力テキストが入り、情報保護要件に抵触しうる。

### 対策
- 起動失敗を `catch` で捕捉して Unity.com へ自動リトライ、両方失敗で非0終了。
- `bridgeResult == null` を新エラーコード `IPC.HUD.BRIDGE_RESULT_NULL` で `SendChatWithBridgeResult` に統一。
- `path` を `/v1/chat/send`（エンドポイント）に固定し、`message` は `text_length` のみ露出。

## Rollback
- 各ファイルを git diff で確認し、Phase A 初期コミット時点に戻す。
- テスト2件を削除して合計3件に戻す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs: n/a（前回ログ注記推奨）
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. U5-T4 Phase B（TTS統合）へ進む。
