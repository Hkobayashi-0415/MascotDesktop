# Worklog: U5-T3 LoopbackHttpClientTests LogAssert 修正・完了

- Date: 2026-02-21
- Task: `LoopbackHttpClientTests` 失敗 2件（`LogAssert.Expect` 未指定）を修正し、U5-T3 を Done へ更新
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: gemini-2.5-pro
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `Unity_PJ/artifacts/test-results/editmode-20260221_013844.xml`
  - `docs/worklog/2026-02-21_u5_t3_contract_test_rerun.md`
  - `docs/NEXT_TASKS.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_0056.md`（同日ログ参照）
- Report-Path: docs/worklog/2026-02-21_u5_t3_logassert_fix.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_0131.md
- Tags: [agent/antigravity, model/gemini-2.5-pro, tool/antigravity, u5, u5-t3, contract-test, logassert]

## Summary
`LoopbackHttpClientTests` の失敗 2件は `LogAssert.Expect` の未指定が原因だった。
`await` 直前に `UnityEngine.TestTools.LogAssert.Expect(LogType.Error, ...)` を各テストへ追加し、再実行で 5/5 Pass を確認。U5-T3 を Done へ更新。

## 変更内容

### `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- `PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody` に追加:
  ```csharp
  UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*CORE\\.TIMEOUT.*"));
  ```
- `PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError` に追加:
  ```csharp
  UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*IPC\\.HTTP\\.REQUEST_ID_MISMATCH.*"));
  ```

### `docs/NEXT_TASKS.md`
- 改訂履歴 R11 を追加
- U5 本体の「契約テスト整備」チェックボックスを `[x]` に変更
- U5-T3 状態を `In Progress` → `Done` に更新
- 参照セクションに本ワークログと前回ログを追記

## 実行コマンド
```powershell
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
```

## テスト結果
- artifact: `Unity_PJ/artifacts/test-results/editmode-20260221_013844.xml` (9756 bytes)
- 結果: ✅ **Passed** — total=5, passed=5, failed=0, skipped=0

| テスト名 | 結果 |
|---|---|
| `PostJsonAsync_DisabledBridge_ReturnsDisabledError` | ✅ Passed |
| `PostJsonAsync_GeneratesRequestId_WhenMissing` | ✅ Passed |
| `PostJsonAsync_InjectsRequestIdIntoHeaderAndBody_WhenMissingInBody` | ✅ Passed |
| `PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody` | ✅ Passed |
| `PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError` | ✅ Passed |

## 判断理由（要点）
- `RuntimeErrorHandlingAndLoggingTests.cs` の既存パターン（`.*error_code.*` 正規表現）に合わせた。
- `await` 直前に `LogAssert.Expect` を配置するのが NUnit + Unity TestTools の標準パターン。
- 実装コード側 (`LoopbackHttpClient.cs`) の変更は不要（正しく動作していた）。

## 次アクション
- U5-T4（LLM/TTS/STT 段階統合）が次のオープンタスク。

## ロールバック方針
- `LoopbackHttpClientTests.cs` の `LogAssert.Expect` 2行を削除すれば元の状態に戻る。
- `NEXT_TASKS.md` の U5-T3 状態を `In Progress` へ戻すことで合わせて整合が取れる。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
