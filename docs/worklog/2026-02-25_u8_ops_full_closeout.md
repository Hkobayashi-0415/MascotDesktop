# Worklog: U8 Ops Full Closeout (2026-02-25)

- Date: 2026-02-25
- Task: U8運用チェックを一気通貫でクローズ（プロファイル + Scheduler導線まで）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `tools/run_u8_ops_checks.ps1`
  - `tools/register_u8_ops_checks_task.ps1`
  - `tools/unregister_u8_ops_checks_task.ps1`
  - `tools/check_runtime_bootstrap_missing.ps1`
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_daily_20260225_174307.json`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_gate_20260225_174307.json`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_custom_20260225_174453.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_pass_20260225.json`
  - `Unity_PJ/artifacts/manual-check/u8_ops_full_closeout_20260225_1745.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-25_u8_ops_full_closeout.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260225_1745.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u8, operations, closeout]

## Summary
- U8運用チェックを `Daily/Gate/Custom` プロファイルで実行可能にし、実ログで全プロファイル Pass を確認した。
- Task Scheduler 登録/削除スクリプトを追加し、DryRun 検証を完了した。
- `schtasks.exe` 実登録はこの環境ではモジュール不足で失敗することを確認し、運用文書に制約を明記した。

## Findings
1. Medium (Fixed): `run_u8_ops_checks` の artifact 名が同秒並列実行で衝突する。
- Fix: artifact 名に `profile` トークンを追加して衝突を回避。

2. Medium (Fixed): Scheduler 導線が未整備で、日次運用を標準化できていなかった。
- Fix: `register/unregister_u8_ops_checks_task.ps1` を追加し、運用手順へ反映。

3. Low (Open): この実行環境では `schtasks.exe` 起動時にモジュール不足（`指定されたモジュールが見つかりません`）。
- Impact: 実登録は未完了。スクリプト自体は完成し、DryRunでコマンド妥当性は確認済み。

## Changes
1. `tools/run_u8_ops_checks.ps1`
- Diff intent: `Profile`（Custom/Daily/Gate）を追加し、推奨設定を固定。artifact名に profile を含めて衝突回避。

2. `tools/register_u8_ops_checks_task.ps1`
- Diff intent: 日次監視タスク登録を自動化。`DryRun` と `schtasks` 解決/エラーハンドリングを実装。

3. `tools/unregister_u8_ops_checks_task.ps1`
- Diff intent: 登録タスク削除を自動化。`DryRun` と `schtasks` 解決/エラーハンドリングを実装。

4. 文書同期
- Files:
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Diff intent: 推奨プロファイル、Task Scheduler導線、環境制約、最新artifactを反映。

5. 実行証跡
- File: `Unity_PJ/artifacts/manual-check/u8_ops_full_closeout_20260225_1745.md`
- Diff intent: 実行コマンドと Pass/Blocked を1ファイルに集約。

## Commands
```powershell
./tools/run_u8_ops_checks.ps1 -Profile Daily  -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/run_u8_ops_checks.ps1 -Profile Gate   -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/run_u8_ops_checks.ps1 -Profile Custom -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/check_unity_legacy_docs_sync.ps1 -RequireSameDay -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_pass_20260225.json"
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -DryRun
./tools/unregister_u8_ops_checks_task.ps1 -DryRun
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -Force
```

## Tests
- Daily profile: Pass
  - `u8_ops_checks_run_daily_20260225_174307.json`
- Gate profile: Pass
  - `u8_ops_checks_run_gate_20260225_174307.json`
- Custom profile: Pass
  - `u8_ops_checks_run_custom_20260225_174453.json`
- Strict docs sync (`-RequireSameDay`): Pass
  - `u8_docs_sync_strict_pass_20260225.json`
- Scheduler dry-run:
  - register: Pass
  - unregister: Pass
- Scheduler actual register:
  - Blocked-Environment（`schtasks.exe` 起動時モジュール不足）

## Rationale (Key Points)
- 日次監視と受入判定を同一スクリプトで運用するため、`Profile` 固定が最も誤操作を減らせる。
- Scheduler 実登録は環境依存のため、スクリプト側で DryRun と明示エラーを持たせる方が運用時の切り分けが速い。
- `Last Updated` を 4文書同日に揃えることで Gate strict 判定を通る状態に整えた。

## Rollback
- 戻す対象:
  - `tools/run_u8_ops_checks.ps1`
  - `tools/register_u8_ops_checks_task.ps1`
  - `tools/unregister_u8_ops_checks_task.ps1`
  - `tools/check_runtime_bootstrap_missing.ps1`（既定探索拡張）
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8_ops_full_closeout_20260225_1745.md`
- 手順:
  - 対象差分を逆適用し、U8-T4 未反映状態へ戻す。
  - `docs/worklog` にロールバック理由（何を・なぜ戻したか）を追記する。
  - Obsidianログは削除せず `Rolled back` / `Superseded` 注記を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. 通常端末で `register_u8_ops_checks_task.ps1 -Force` を実行し、Scheduler 実登録を完了させる。
2. 運用では日次 `Profile Daily` / 受入時 `Profile Gate` を固定して artifact を蓄積する。

