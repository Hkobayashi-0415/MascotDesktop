# Worklog: U5-T3 契約テスト再実行（Unity 環境復旧後）

- Date: 2026-02-21
- Task: `2026-02-20_u5_t3_contract_test_automation.md` の「次アクション」として同一 3 コマンドを再実行し、artifact 生成と契約テスト通過を確認
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: gemini-2.5-pro
- Used-Skills: worklog-update
- Repo-Refs:
  - `docs/worklog/2026-02-20_u5_t3_contract_test_automation.md`
  - `docs/05-dev/unity-test-environment-recovery.md`
  - `tools/run_unity_tests.ps1`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_1557.md`（前回ログ参照）
- Report-Path: docs/worklog/2026-02-21_u5_t3_contract_test_rerun.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_0056.md
- Tags: [agent/antigravity, model/gemini-2.5-pro, tool/antigravity, u5, u5-t3, contract-test]

## Summary
Unity 実行環境が復旧済みであることを確認し、U5-T3 の 3 テストを再実行した。
`RuntimeErrorHandlingAndLoggingTests`（7/7 Pass）と `SimpleModelBootstrapTests`（34/34 Pass）は完全通過。
`LoopbackHttpClientTests` は 5件中 3件通過、2件失敗（`LogAssert.Expect` 未指定による `UnhandledLogMessage`）。

## 実行コマンド
```powershell
# 環境確認
Test-Path "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"  # True
Test-Path "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"  # True

# テスト実行（Unity.exe が存在したため UNITY_COM 設定不要）
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"
```

## テスト結果

### 実行1: `LoopbackHttpClientTests`
- artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260221_010746.xml`: **生成済み** (14887 bytes)
  - `Unity_PJ/artifacts/test-results/editmode-20260221_010746.log`: 生成済み (121043 bytes)
- 結果: **Failed** — total=5, **passed=3, failed=2**, skipped=0

| テスト名 | 結果 |
|---|---|
| `PostJsonAsync_DisabledBridge_ReturnsDisabledError` | ✅ Passed |
| `PostJsonAsync_GeneratesRequestId_WhenMissing` | ✅ Passed |
| `PostJsonAsync_InjectsRequestIdIntoHeaderAndBody_WhenMissingInBody` | ✅ Passed |
| `PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody` | ❌ Failed |
| `PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError` | ❌ Failed |

**失敗原因（共通）**: `RuntimeLog.Error()` が Error レベルの Unity ログを出力しているが、テスト内で `LogAssert.Expect` が未指定のため `UnhandledLogMessage` として失敗。
- `PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody`: `ipc.http.response_failed` (error_code=CORE.TIMEOUT)
- `PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError`: `ipc.http.request_id_mismatch` (error_code=IPC.HTTP.REQUEST_ID_MISMATCH)

### 実行2: `RuntimeErrorHandlingAndLoggingTests`
- artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260221_011114.xml`: **生成済み** (51229 bytes)
  - `Unity_PJ/artifacts/test-results/editmode-20260221_011114.log`: 生成済み
- 結果: ✅ **Passed** — total=7, passed=7, failed=0, skipped=0

| テスト名 | 結果 |
|---|---|
| `LoaderFailureClassification_ImageDecodeFail_ReturnsExpectedDetail` | ✅ Passed |
| `LoaderFailureClassification_ImageFileNotFound_ReturnsExpectedDetail` | ✅ Passed |
| `LoaderFailureClassification_InvokeFail_ReturnsExpectedDetail` | ✅ Passed |
| `LoaderFailureClassification_NullRoot_ReturnsExpectedDetail` | ✅ Passed |
| `RequestIdPropagation_UsesSingleRequestIdAcrossLoaderStageLogs` | ✅ Passed |
| `RuntimeLogPolicy_MinLevelAndFilters_AreApplied` | ✅ Passed |
| `RuntimeLogPolicy_RotationAndRetention_AreApplied` | ✅ Passed |

### 実行3: `SimpleModelBootstrapTests`
- artifact:
  - `Unity_PJ/artifacts/test-results/editmode-20260221_011141.xml`: **生成済み** (28833 bytes)
  - `Unity_PJ/artifacts/test-results/editmode-20260221_011141.log`: 生成済み
- 結果: ✅ **Passed** — total=34, passed=34, failed=0, skipped=0

## 判断理由（要点）
- `Unity.exe` が存在したため環境変数 `UNITY_EXE`/`UNITY_COM` の設定は不要だった（前回の「モジュールが見つかりません」エラーが解消済み）。
- 2件の失敗は `LogAssert.Expect` の未指定に起因。実装側の `RuntimeLog.Error()` 呼び出しは正しいため、テストコード側の修正が必要。
- `RuntimeErrorHandlingAndLoggingTests`・`SimpleModelBootstrapTests` は全件通過のため U5-T3 関連外テストの品質は維持されている。

## 次アクション
- `LoopbackHttpClientTests` の失敗 2 件を修正する:
  - `PostJsonAsync_NonSuccess_UsesErrorCodeAndRetryableFromResponseBody` に `LogAssert.Expect(LogType.Error, ...)` を追加
  - `PostJsonAsync_ResponseRequestIdMismatch_ReturnsMismatchError` に同様の `LogAssert.Expect` を追加
- 修正後、`LoopbackHttpClientTests` を再実行し全件通過を確認する。
- 全件通過後に `U5-T3` を `Done` へ更新する。

## ロールバック方針
- このログは記録のみで変更ファイルなし。ロールバック対象外。
- テスト修正時は `LoopbackHttpClientTests.cs` のみ変更。修正前差分は `git diff` で確認する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
