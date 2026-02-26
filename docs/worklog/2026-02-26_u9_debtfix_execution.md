# Worklog: U9 Debt Fix Execution（2026-02-26）

- Date: 2026-02-26
- Task: U9クローズ後の技術的負債対応（運用ゲート失敗時run summary欠落の解消、Scheduler診断再試行化、文書同期）。
- Execution-Tool: Codex
- Execution-Agent: codex-gpt5
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - `tools/run_u8_ops_checks.ps1`
  - `tools/diagnose_u8_scheduler.ps1`
  - `tools/check_unity_legacy_docs_sync.ps1`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/05-dev/u8-operations-automation.md`
  - `Unity_PJ/artifacts/manual-check/debtfix-pass/u8_ops_checks_run_gate_20260226_125744.json`
  - `Unity_PJ/artifacts/manual-check/debtfix-fail/u8_ops_checks_run_gate_20260226_125755.json`
  - `Unity_PJ/artifacts/manual-check/u8_scheduler_diag_debtfix_20260226_1300.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_debtfix_20260226_1300.json`
  - `Unity_PJ/artifacts/manual-check/u8_freshness_debtfix_20260226_1300.json`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-26_u9_debtfix_execution.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_1258.md
- Tags: [agent/codex-gpt5, model/gpt-5, tool/codex, u9, debt-fix, operations]

## Summary

U9完了後に残っていた負債候補のうち、運用ゲートの証跡一貫性とScheduler断続事象の診断粒度を実装で改善した。合わせて、legacy docs 非同期リスクは同期チェックPassにより解消済みへ更新した。

## Findings（重大度順）

| # | 重大度 | 内容 | 対応 |
|---|---|---|---|
| F1 | Med | `run_u8_ops_checks` が異常系で途中停止すると run summary が欠落し得る。 | try/catch + 安全な exit code 取得を導入し、Fail時も run summary を必ず出力。 |
| F2 | Low | Scheduler診断が単発判定で断続事象の観測情報が不足。 | executable probe 再試行（既定2回）と試行履歴のartifact化を追加。 |
| F3 | Low | legacy docs 非同期が負債候補として残っていた。 | `check_unity_legacy_docs_sync -RequireSameDay` Pass を再採取し、非同期リスク扱いを解消。 |

## Changes

1. `tools/run_u8_ops_checks.ps1`
   - `Get-SafeLastExitCode` を追加。
   - runtime/docs 各チェックを try/catch で実行し、失敗時も `runtime_check_error` / `docs_sync_check_error` を run summary に記録。
   - Fail終了時は標準エラー出力 + exit 1 で終了（summary生成後）。
2. `tools/diagnose_u8_scheduler.ps1`
   - `-ExecutableProbeRetries` / `-ExecutableProbeRetryIntervalMs` を追加。
   - `schtasks /?` probe を再試行化し、`diagnostics.executable_probe_*` をartifact出力。
3. `docs/NEXT_TASKS.md`
   - R37 を追加し、負債対応の根拠artifactを追記。
   - U9完了項目に「失敗時run summary欠落の解消 / 再試行診断追加」を反映。
   - 移行阻害根拠から legacy docs 非同期行を削除（同期確認済み）。
4. `docs/05-dev/dev-status.md`
   - U9負債対応を追記。
   - U9クローズ詳細を debtfix artifact へ更新。
   - 残件表記を F1 のみへ整理。
5. `docs/05-dev/u8-operations-automation.md`
   - run summary 常時生成と error field 記録を明記。
   - Scheduler probe 再試行オプションを追記。
   - U9負債対応証跡を追記。

## Commands

```powershell
./tools/run_u8_ops_checks.ps1 -Profile Gate -ArtifactDir "Unity_PJ/artifacts/manual-check/debtfix-pass"
./tools/run_u8_ops_checks.ps1 -Profile Gate -LogDir "D:\dev\MascotDesktop\Unity_PJ\artifacts\manual-check\_nonexistent_logs_for_gate_fail_v6" -ArtifactDir "Unity_PJ/artifacts/manual-check/debtfix-fail"
./tools/diagnose_u8_scheduler.ps1 -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_scheduler_diag_debtfix_20260226_1300.json"
./tools/check_unity_legacy_docs_sync.ps1 -RequireSameDay -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_debtfix_20260226_1300.json"
./tools/check_u8_ops_freshness.ps1 -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_freshness_debtfix_20260226_1300.json"
```

## Tests

| テスト | コマンド | exit code | 結果 | artifact |
|---|---|---|---|---|
| T1: 最終ゲート正常系 | `run_u8_ops_checks.ps1 -Profile Gate -ArtifactDir debtfix-pass` | 0 | PASS | `debtfix-pass/u8_ops_checks_run_gate_20260226_125744.json` |
| T2: 最終ゲート異常系 | `run_u8_ops_checks.ps1 -Profile Gate -LogDir <missing> -ArtifactDir debtfix-fail` | 1 | FAIL（期待どおり）+ run summary生成を確認 | `debtfix-fail/u8_ops_checks_run_gate_20260226_125755.json` |
| T3: Scheduler診断 | `diagnose_u8_scheduler.ps1 -ArtifactPath ...1300.json` | 1 | `can_register=false`（probe 2回の履歴記録あり） | `u8_scheduler_diag_debtfix_20260226_1300.json` |
| T4: legacy docs同期 | `check_unity_legacy_docs_sync.ps1 -RequireSameDay -ArtifactPath ...1300.json` | 0 | PASS | `u8_docs_sync_debtfix_20260226_1300.json` |
| T5: 鮮度チェック | `check_u8_ops_freshness.ps1 -ArtifactPath ...1300.json` | 0 | PASS | `u8_freshness_debtfix_20260226_1300.json` |

## Rationale (Key Points)

- `run_u8_ops_checks` は運用ゲートの主導線のため、失敗時にも run summary を確実に残すことが最優先。
- Scheduler は根本原因がセッション依存で揺れるため、判定の再試行と診断ログ増強で「原因不明」から「管理可能」へ寄せる。
- legacy docs は同期チェックPassが確認できたため、負債項目から除外した。

## Rollback
- 変更ファイルを差分単位で逆適用すればロールバック可能。
- Obsidianログは削除しない。
- ロールバック理由（何を・なぜ戻したか）を本ファイルへ追記する。
- Obsidianログには `Rolled back` / `Superseded` 注記を追記して履歴を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- APP-T1 を開始し、機能仕様と受入条件を固定する。
- APP-T2 は APP-T1 確定後に endpoint 切替計画を具体化する。
