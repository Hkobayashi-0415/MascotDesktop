# Unity Test Result Collection Template (U3-T2)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-19
- Scope: Unity EditMode/PlayMode テスト結果の収集フォーマット統一（artifact path / pass-fail / cause）

## 根拠
- `tools/run_unity_tests.ps1:49`-`tools/run_unity_tests.ps1:57`
- `tools/run_unity_tests.ps1:62`-`tools/run_unity_tests.ps1:64`
- `docs/worklog/2026-02-19_test_run_debug.md`
- `docs/worklog/2026-02-19_unity_test_ops_standardization.md`

## 記録単位
- 1回の `run_unity_tests.ps1` 実行につき1レコードを作成する。
- 同じ `TestFilter` を再試行した場合も別レコードとして残す。

## 標準フォーマット（再利用テンプレ）

```markdown
| run_at | test_platform | test_filter | command | pass_fail | cause | artifact_xml | artifact_log | notes |
|---|---|---|---|---|---|---|---|---|
| 2026-02-19 23:30:56 | EditMode | MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests | `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"` | Blocked-Environment | Unity起動失敗: 指定されたモジュールが見つかりません | `Unity_PJ/artifacts/test-results/editmode-20260219_233056.xml (not generated)` | `Unity_PJ/artifacts/test-results/editmode-20260219_233056.log (not generated)` | script出力に ResourceUnavailable |
```

## `pass_fail` の許容値
- `Passed`
- `Failed`
- `Blocked-Environment`

## `cause` 記載ルール
- `Passed`: `all tests passed` など簡潔に記載
- `Failed`: 失敗テスト名または主要失敗原因を記載
- `Blocked-Environment`: 起動前エラー（例: `指定されたモジュールが見つかりません`）を記載

## artifact path 記載ルール
- 生成済み: 実パスをそのまま記載
- 未生成: 実パス + `(not generated)` を付記

## 収集手順
1. 実行ログの `Args=` から `-testResults` / `-logFile` を抽出する。
2. `pass_fail` と `cause` を判定する。
3. artifact 生成有無を `Test-Path` で確認して記録する。
4. worklog の「テスト結果」セクションへこの表を貼り付ける。

## 参照
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/worklog/2026-02-19_test_run_debug.md`
