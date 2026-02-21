# Unity Test Environment Recovery (U3-T1)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-19
- Scope: `tools/run_unity_tests.ps1` 実行時の環境差異復旧（特に `指定されたモジュールが見つかりません`）

## 根拠
- `tools/run_unity_tests.ps1:28`-`tools/run_unity_tests.ps1:46`
- `tools/run_unity_tests.ps1:49`-`tools/run_unity_tests.ps1:57`
- `docs/worklog/2026-02-19_test_run_debug.md`
- `docs/worklog/2026-02-19_unity_test_ops_standardization.md`

## 対象症状
- Unity起動前に PowerShell で以下が出る:
  - `Program 'Unity.exe' failed to run ... 指定されたモジュールが見つかりません`
- 実行時に `-testResults` / `-logFile` で指定された artifact が生成されない

## 標準復旧手順
1. 実行コマンドと出力先（timestamp）を記録する  
   例:
   ```powershell
   ./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"
   ```
   実行ログ先頭の `Args=` に出る `-testResults` / `-logFile` を控える。

2. artifact 生成有無を確認する
   ```powershell
   Test-Path "Unity_PJ/artifacts/test-results/editmode-YYYYMMDD_HHMMSS.xml"
   Test-Path "Unity_PJ/artifacts/test-results/editmode-YYYYMMDD_HHMMSS.log"
   ```
   起動前失敗の場合は `False/False` を記録する。

3. Unity実行パスを明示して再実行する（スクリプト仕様準拠）
   - 優先順:
     1. `-UnityPath` を明示
     2. `UNITY_EXE` を設定
     3. `UNITY_COM` を設定（`.com` で起動切替）
   例:
   ```powershell
   $env:UNITY_COM="C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"
   ./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"
   ```

4. 同一 `TestFilter` で1回だけ再試行し、結果を分類する
   - `Passed/Failed`（xml 生成あり）
   - `Blocked-Environment`（Unity起動前失敗、xml/log 未生成）

5. `Blocked-Environment` の場合はこれ以上テスト結果を推定しない  
   U3-T2 テンプレに `cause` と artifact 未生成を記録し、次の対応へ引き継ぐ。

## 判定ルール（運用）
- 失敗の一次判定は「終了コード」単独ではなく、以下の3点で行う:
  - Unity起動エラー文字列の有無
  - xml/log artifact の生成有無
  - xml 生成時の pass/fail

## 参照
- `docs/05-dev/unity-test-result-collection-template.md`
- `docs/NEXT_TASKS.md`
