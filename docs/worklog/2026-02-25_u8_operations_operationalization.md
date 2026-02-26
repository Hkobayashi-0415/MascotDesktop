# Worklog: U8 Operations Operationalization (2026-02-25)

- Date: 2026-02-25
- Task: U8自動化の運用定着（実ログ接続 + 一括実行 + 失敗時手順）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `tools/check_runtime_bootstrap_missing.ps1`
  - `tools/run_u8_ops_checks.ps1`
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_summary_20260225_162419.json`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_summary_20260225_162538.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_summary_20260225_162538.json`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_20260225_162538.json`
  - `Unity_PJ/artifacts/manual-check/u8_operations_operationalization_20260225_162538.md`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_default_20260225.json`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-25_u8_operations_operationalization.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260225_1626.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u8, operations, automation]

## Summary
- `tools/run_u8_ops_checks.ps1` を追加し、runtime監視と文書同期チェックの一括実行を可能にした。
- `check_runtime_bootstrap_missing` の既定ログ探索を強化し、`USERPROFILE` 未設定環境でも `C:\Users\*\AppData\LocalLow\DefaultCompany\project\logs` から実行できるようにした。
- 実ログ（`C:\Users\sugar\...`）で一括実行を実施し、Pass artifact を採取した。

## Findings
1. Medium (Fixed): `USERPROFILE` 未設定環境で `check_runtime_bootstrap_missing.ps1` の既定ログ探索が失敗する。
- Fix: 既定探索に `HOMEDRIVE/HOMEPATH` と `C:\Users\*\AppData\LocalLow\DefaultCompany\project\logs` の走査を追加。

2. Medium (Fixed): U8の2チェックを毎回個別実行する必要があり、運用手順が分散していた。
- Fix: `run_u8_ops_checks.ps1` を追加し、artifact出力込みで一括実行を標準化。

## Changes
1. `tools/check_runtime_bootstrap_missing.ps1`
- Diff intent: 既定ログ探索を環境依存に強化し、`-LogDir` 未指定でも実行可能にした。

2. `tools/run_u8_ops_checks.ps1`
- Diff intent: runtime監視 + docs同期を1コマンドで実行し、3種artifact（runtime/docs/run-summary）を生成する。

3. `docs/05-dev/u8-operations-automation.md`
- Diff intent: ラッパー導線、失敗時一次対応、実行証跡（2026-02-25）を追記。

4. `docs/05-dev/QUICKSTART.md` / `docs/05-dev/unity-runtime-manual-check.md`
- Diff intent: 一括実行ラッパー `run_u8_ops_checks` への運用導線を追記。

5. `docs/NEXT_TASKS.md` / `docs/05-dev/dev-status.md`
- Diff intent: U8-T3完了と最新実行証跡を同期。

6. `Unity_PJ/artifacts/manual-check/u8_operations_operationalization_20260225_162538.md`
- Diff intent: 実ログ一括実行の証跡を再利用しやすい形式で保存。

## Commands
```powershell
# 実ログ候補探索
Get-ChildItem C:\Users -Directory | ForEach-Object {
  $p = Join-Path $_.FullName 'AppData\LocalLow\DefaultCompany\project\logs'
  if (Test-Path $p) { $p }
}

# 一括実行（明示LogDir）
./tools/run_u8_ops_checks.ps1 `
  -LogDir "C:\Users\sugar\AppData\LocalLow\DefaultCompany\project\logs" `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"

# 既定探索（-LogDir未指定）確認
./tools/check_runtime_bootstrap_missing.ps1 `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_monitor_default_20260225.json"

./tools/run_u8_ops_checks.ps1 `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"
```

## Tests
- `run_u8_ops_checks`（明示LogDir）:
  - runtime monitor: Pass
  - docs sync: Pass
  - artifact: `u8_ops_checks_run_20260225_162419.json`
- `check_runtime_bootstrap_missing`（既定探索、`-LogDir` 未指定）:
  - Pass
  - artifact: `u8_runtime_monitor_default_20260225.json`
- `run_u8_ops_checks`（既定探索、`-LogDir` 未指定）:
  - runtime monitor/docs sync とも Pass
  - artifact: `u8_ops_checks_run_20260225_162538.json`
- Unity実行:
  - 未実施（本タスクは運用チェック実装と実ログ解析が対象。Unity起動なしで要件検証可能）。

## Rationale (Key Points)
- 実運用では環境変数未設定があり得るため、ログ探索は実パススキャンを含める方が堅牢。
- 一括実行ラッパーを追加することで、チェック漏れと手順差異を抑えられる。
- Fail時一次対応を文書化することで、判定失敗から復旧までの運用を標準化できる。

## Rollback
- 戻す対象:
  - `tools/run_u8_ops_checks.ps1`
  - `tools/check_runtime_bootstrap_missing.ps1`（既定探索拡張分）
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8_*20260225_1625*.json`
  - `Unity_PJ/artifacts/manual-check/u8_operations_operationalization_20260225_162538.md`
- 手順:
  - 対象差分を逆適用し、U8-T3未実施状態へ戻す。
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
1. 日次または受入タイミングで `run_u8_ops_checks` を定期実行し、artifactを `docs/worklog` に紐付ける。
2. strict同日同期（`-RequireSameDay`）を運用基準に含めるかを次フェーズで決定する。

## Legacy Artifact Note

- 本 worklog（U8-T3）は `run_u8_ops_checks.ps1` に profile 機能が追加される **前** の実行記録である。
- このため Repo-Refs および Tests 内の artifact 名には profile トークンが含まれていない。
- U8-T4 以降の現行命名規則（`u8_ops_checks_run_<profile>_<timestamp>.json`）との対応は以下の通り:

| 旧 artifact 名（profile 無し） | 相当プロファイル | 現行命名の等価 artifact |
|---|---|---|
| `u8_ops_checks_run_20260225_162419.json` | Custom 相当 | `u8_ops_checks_run_custom_20260225_174453.json` |
| `u8_ops_checks_run_20260225_162538.json` | Custom 相当 | `u8_ops_checks_run_custom_20260225_174453.json` |
| `u8_runtime_monitor_summary_20260225_162419.json` | Custom 相当 | `u8_runtime_monitor_summary_custom_20260225_174453.json` |
| `u8_runtime_monitor_summary_20260225_162538.json` | Custom 相当 | `u8_runtime_monitor_summary_custom_20260225_174453.json` |
| `u8_docs_sync_summary_20260225_162538.json` | Custom 相当 | `u8_docs_sync_summary_custom_20260225_174453.json` |

- 旧 artifact ファイル自体は `Unity_PJ/artifacts/manual-check/` に実在するため、上記 Repo-Refs の参照は有効である。
- 新しい実行証跡は profile 付き命名（現行）を参照すること（`docs/05-dev/u8-operations-automation.md` § 推奨プロファイル実行証跡）。

