# U8 Operations Automation

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-25
- Scope: U7完了後の運用自動化（runtime監視 / 文書同期チェック / 鮮度チェック / Scheduler診断）

## 目的
- `ui.hud.bootstrap_missing` の連続出力を自動検知し、手動監視依存を下げる。
- Unity導線文書更新時の legacy文書同期漏れ（`PACKAGING` / `RESIDENT_MODE`）を自動検知する。
- 日次チェック未実行（run summary が stale）を自動検知する。

## Check 1: bootstrap_missing 連続監視

### 実行コマンド
```powershell
./tools/check_runtime_bootstrap_missing.ps1
```

### 判定基準（デフォルト）
- 対象ログ: `runtime-*.jsonl`
- 監視イベント: `ui.hud.bootstrap_missing`
- Fail条件: 連続回数 `>= 3`
- 連続判定の時間窓: `MaxGapSeconds = 5`
- リセットイベント: `ui.hud.bootstrap_recovered`, `avatar.model.displayed`

### 判定の意図
- `RuntimeDebugHud` の欠損ログは2秒間隔で継続出力されるため、3連続（約4秒超継続）を異常閾値とする。
- 単発または短時間の欠損ログは起動直後ノイズの可能性があるため即Failにしない。

### 主なオプション
- `-LogDir <path>`: 監視対象ディレクトリを指定
- `-ThresholdConsecutive <int>`: Fail閾値を変更
- `-MaxGapSeconds <double>`: 連続とみなす最大間隔を変更
- `-ArtifactPath <path>`: 判定サマリーJSONを保存
- `-AllowNoLogs`: ログ未検出をPass扱い
- `-FailOnParseError`: JSON parse失敗をFail扱い

## Check 2: Unity導線と legacy文書の同期チェック

### 実行コマンド
```powershell
./tools/check_unity_legacy_docs_sync.ps1
```

### 判定基準（デフォルト）
- Unity導線文書:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
- legacy文書:
  - `docs/PACKAGING.md`
  - `docs/RESIDENT_MODE.md`
- Fail条件:
  - Unity導線2文書の `Last Updated` が不一致
  - legacy文書の `Last Updated` が Unity導線の最新日より古い（`MaxLagDays=0`）
  - legacy文書に Unity導線への参照リンクが不足

### 同日同期ルール
- 標準は `MaxLagDays=0`（Unity導線更新日に対して legacy が遅れていないこと）。
- 厳密に同日一致を強制する場合は `-RequireSameDay` を指定する。

### 主なオプション
- `-MaxLagDays <int>`: 許容遅延日数
- `-RequireSameDay`: legacy文書の同日一致を強制
- `-ArtifactPath <path>`: 判定サマリーJSONを保存

## 再実行可能な運用例
```powershell
./tools/check_runtime_bootstrap_missing.ps1 `
  -LogDir "C:\Users\<user>\AppData\LocalLow\DefaultCompany\project\logs" `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_monitor_summary.json"

./tools/check_unity_legacy_docs_sync.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_summary.json"
```

## 一括実行（ラッパー）
```powershell
./tools/run_u8_ops_checks.ps1 `
  -LogDir "C:\Users\<user>\AppData\LocalLow\DefaultCompany\project\logs" `
  -LogPattern "runtime-*.jsonl" `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"
```

- 実行時に以下3つのartifactを出力する:
  - `u8_runtime_monitor_summary_<profile>_<timestamp>.json`
  - `u8_docs_sync_summary_<profile>_<timestamp>.json`
  - `u8_ops_checks_run_<profile>_<timestamp>.json`

### 推奨プロファイル
```powershell
# 日次監視（推奨）: ログ未生成日は許容
./tools/run_u8_ops_checks.ps1 -Profile Daily

# 受入/リリース判定（推奨）: 同日同期を厳格化
./tools/run_u8_ops_checks.ps1 -Profile Gate
```

- `Daily`:
  - `AllowNoLogs=True`
  - `RequireSameDay=False`
  - `MaxLagDays=0`
- `Gate`:
  - `AllowNoLogs=False`
  - `RequireSameDay=True`
  - `MaxLagDays=0`
- `run_u8_ops_checks.ps1` は **Pass/Failを問わず** `u8_ops_checks_run_<profile>_<timestamp>.json` を生成する。
  - Fail時は `runtime_check_error` / `docs_sync_check_error` に例外メッセージが記録されるため、一次切り分けを artifact だけで再現できる。

## 定期実行（Task Scheduler）

### Scheduler 事前診断（1コマンド手順）

実登録前に事前診断を行い、`can_register` が `true` であることを確認してから登録すること。

```powershell
# Step 1: 事前診断（schtasks 可否・競合・ExecutionPolicy を確認）
./tools/diagnose_u8_scheduler.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/scheduler_diag_<timestamp>.json"

# Step 2: can_register=true を確認してから登録
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -Force
```

診断スクリプト終了コード:
- `0`: `can_register=true`（登録可能）
- `1`: `can_register=false`（blockers に理由が列挙される）

主なオプション:
- `-ExecutableProbeRetries <int>`: `schtasks.exe /?` の再試行回数（既定: `2`）
- `-ExecutableProbeRetryIntervalMs <int>`: 再試行間隔ms（既定: `250`）
- 診断artifactの `diagnostics.executable_probe_*` で、断続失敗の試行回数とエラー履歴を確認できる

### 登録・削除コマンド
```powershell
# 登録（毎日 09:00 実行）
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -Force

# 事前確認のみ（実登録なし）
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -DryRun

# 削除
./tools/unregister_u8_ops_checks_task.ps1
```
- 登録タスク名（既定）: `MascotDesktop_U8_DailyOpsChecks`
- 実行内容: `run_u8_ops_checks.ps1 -Profile Daily`

## Check 3: run summary 鮮度チェック

### 目的
最新の `u8_ops_checks_run_*.json` が一定時間内に生成されていない場合（日次チェック未実行）を Fail として検知する。

### 実行コマンド
```powershell
./tools/check_u8_ops_freshness.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_freshness_check_<timestamp>.json"
```

### 判定基準
| パラメータ | 既定値 | 意味 |
|---|---|---|
| `ThresholdHours` | 25.0 | 経過がこれ以上 → Fail (Stale) |
| `WarnHours` | 22.0 | 経過がこれ以上 → Pass with Warn |
| `Profile` | Any | 対象プロファイル（Any=全て対象、最新を `run_at_utc` で選択） |

- **選定ルール（`Profile=Any`）**: ファイル名タイムスタンプではなく `run_at_utc` フィールドが最新のものを対象とする。
- **`run_at_utc` 欠損/不正**: そのファイルをスキップし警告を出す。有効ファイル 0 件 → `Fail(InvalidArtifact)`.
- **`profile` 欠損（旧artifact互換）**: スクリプトは継続し、`latest_profile` は `filename` から推定（不可なら `Unknown`）する。
- **対象ファイル 0 件**: `Fail(NoArtifact)`.
- 終了コード: 0 = Pass または Warn, 1 = Fail.

### しきい値の根拠
- 日次監視（09:00）想定: 25 時間 = 1時間の猶予付き1日相当、22 時間 = 早期警告。

### 主なオプション
- `-ArtifactDir <path>`: 検索ディレクトリ（省略時: `Unity_PJ/artifacts/manual-check`）
- `-Profile <Custom|Daily|Gate|Any>`: 対象プロファイルを絞り込む
- `-ThresholdHours <double>`: Fail 閾値を変更
- `-WarnHours <double>`: Warn 閾値を変更
- `-ArtifactPath <path>`: 診断結果 JSON を保存

## Fail 記録テンプレート

U8 運用チェックが Fail した場合の記録は以下のテンプレートを使用する:

```
docs/worklog/_templates/u8_ops_fail_template.md
```

使い方:
1. テンプレートをコピーして `docs/worklog/<YYYY-MM-DD>_u8_ops_fail_<title>.md` として保存する。
2. `<...>` プレースホルダを実際の値に置き換える。
3. AGENTS.md §6 の必須項目（Repo-Refs / Obsidian-Log / Execution fields / Tags）を埋める。

## 失敗時一次対応
- `runtime monitor` が Fail:
  - artifact の `max_consecutive` / `max_sequence` を確認し、対象ログ区間を抽出する。
  - `ui.hud.bootstrap_missing` が閾値超過している場合、Player/Play Mode を再起動し、`avatar.model.displayed` 出力を確認する。
  - 再実行しても継続Failなら、`docs/worklog` に `max_sequence` と対処結果を記録する。
- `docs sync` が Fail:
  - `violations` を確認し、`QUICKSTART` / `unity-runtime-manual-check` / `PACKAGING` / `RESIDENT_MODE` の `Last Updated` と参照リンクを揃える。
  - 同日同期運用を強制する場合は `-RequireSameDay` の Fail を解消してから完了扱いにする。
- `freshness check` が Fail (Stale):
  - `./tools/run_u8_ops_checks.ps1 -Profile Daily` を実行して最新 run summary を生成する。
  - `Fail(NoArtifact)` の場合は ArtifactDir を確認し、初回実行コマンドを実行する。
- どちらかFail時は、`NEXT_TASKS` / `dev-status` の完了状態を更新せず、復旧後に同期する。
- 記録は `docs/worklog/_templates/u8_ops_fail_template.md` をコピーして使用する。

## 実行証跡（2026-02-25）
- 実行コマンド:
```powershell
./tools/run_u8_ops_checks.ps1 `
  -LogDir "C:\Users\sugar\AppData\LocalLow\DefaultCompany\project\logs" `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"
```
- 結果:
  - runtime monitor: Pass（`max_consecutive=0`）
  - docs sync: Pass
  - run summary: `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_custom_20260225_174453.json`

### 既定ログ探索（`-LogDir` 未指定）確認
```powershell
./tools/check_runtime_bootstrap_missing.ps1 `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_monitor_default_20260225.json"
```
- 結果: Pass（`max_consecutive=0`）
- 補足: `USERPROFILE` 未設定環境でも `C:\Users\*\AppData\LocalLow\DefaultCompany\project\logs` 探索で実行可能。

### 一括実行の最新証跡
- `Unity_PJ/artifacts/manual-check/u8_operations_operationalization_20260225_162538.md`
- `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_custom_20260225_174453.json`

### 推奨プロファイル実行証跡（2026-02-25 17:43）
- Daily: `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_daily_20260225_174307.json`
- Gate: `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_gate_20260225_174307.json`
- Gate strict docs sync 単体: `Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_pass_20260225.json`

### Task Scheduler 実行可否（この環境）
- `register_u8_ops_checks_task.ps1 -DryRun` / `unregister_u8_ops_checks_task.ps1 -DryRun` は実行可能。
- 実登録（`-Force`）: 前回セッション（同日）ではモジュール不足エラーで失敗したが、延長セッション（2026-02-25）で実登録・`/QUERY` 確認・削除まで成功（断続的環境依存事象として記録）。
  - 事前診断（`diagnose_u8_scheduler.ps1`）は `can_register=true` を返した。
  - 登録確認: `\MascotDesktop_U8_DailyOpsChecks` で次回実行 `2026/02/26 09:00:00`、状態 `準備完了` を確認。
  - テスト後に `unregister_u8_ops_checks_task.ps1` で削除済み。
- 本番運用時は `diagnose_u8_scheduler.ps1` で事前確認してから実登録すること。

### U8 延長セッション 実行証跡（2026-02-25 21:34〜）
- 鮮度チェック T1 (Stale/default): `Unity_PJ/artifacts/manual-check/freshness_test1_stale_default.json`
- 鮮度チェック T2 (Pass/extended): `Unity_PJ/artifacts/manual-check/freshness_test2_stale_extended.json`
- 鮮度チェック T3 (NoArtifact): `Unity_PJ/artifacts/manual-check/freshness_test3_no_artifact.json`
- Scheduler 診断: `Unity_PJ/artifacts/manual-check/scheduler_diag_20260225_extension.json`（`can_register=true`）

### U8 監査修正 実行証跡（2026-02-25 22:04〜）
- 鮮度チェック互換性修正:
  - 原因: `Profile=Any` 実行時に、旧artifact（`u8_ops_checks_run_20260225_162419.json` など）の `profile` 欠損で例外停止。
  - 対処: `tools/check_u8_ops_freshness.ps1` で `Resolve-ProfileFromArtifact` を導入（欠損時 fallback）。
- 再監査 artifact:
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_default_20260225_220408.json`（Pass）
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_relaxed_20260225_220408.json`（Pass）
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_noartifact_20260225_220409.json`（Fail/期待どおり）
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_custom_20260225_220408.json`（Pass）
  - `Unity_PJ/artifacts/manual-check/audit_fix_scheduler_diag_20260225_220408.json`（`can_register=false`、環境依存blocker）

### U9 負債対応 実行証跡（2026-02-26 12:55〜）
- `run_u8_ops_checks.ps1` 失敗時 run summary 欠落の修正確認:
  - `Unity_PJ/artifacts/manual-check/debtfix-fail/u8_ops_checks_run_gate_20260226_125755.json`（Fail / `runtime_check_error` 記録あり）
  - `Unity_PJ/artifacts/manual-check/debtfix-pass/u8_ops_checks_run_gate_20260226_125744.json`（Pass）
- Scheduler 診断の再試行記録:
  - `Unity_PJ/artifacts/manual-check/u8_scheduler_diag_debtfix_20260226_1300.json`（`executable_probe_retries=2`, `attempts=2`）
- legacy docs 同期再確認:
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_debtfix_20260226_1300.json`（Pass）
