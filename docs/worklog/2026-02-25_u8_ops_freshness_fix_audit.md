# Worklog: U8 Ops Freshness Fix Audit (2026-02-25)

- Date: 2026-02-25
- Task: U8鮮度チェック互換性不具合の修正と監査再実行（Profile=Anyクラッシュ解消）
- Execution-Tool: codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - `tools/check_u8_ops_freshness.ps1`
  - `tools/diagnose_u8_scheduler.ps1`
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/worklog/2026-02-25_u8_ops_extension_closeout.md`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_20260225_162419.json`
  - `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_20260225_162538.json`
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_default_20260225_220408.json`
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_relaxed_20260225_220408.json`
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_noartifact_20260225_220409.json`
  - `Unity_PJ/artifacts/manual-check/audit_fix_freshness_custom_20260225_220408.json`
  - `Unity_PJ/artifacts/manual-check/audit_fix_scheduler_diag_20260225_220408.json`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-25_u8_ops_freshness_fix_audit.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260225_2206.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u8, operations, freshness, audit, fix]

## Summary

監査で再現した `check_u8_ops_freshness.ps1` のクラッシュ（`profile` 欠損artifact）を修正し、`Profile=Any` を含む再実行ケースで異常終了しないことを確認した。Scheduler診断は本実行環境では `can_register=false` で、モジュール不足エラーが継続している。

## Findings（重大度順）

| # | 重大度 | 内容 | 対処 |
|---|---|---|---|
| F1 | Medium (Fixed) | `Profile=Any` 実行時、旧artifactの `profile` 欠損で `$json.profile` 参照が例外停止（StrictMode）。 | `Resolve-ProfileFromArtifact` を追加し、`profile` 欠損時は filename 推定（不可なら `Unknown`）に fallback。 |
| F2 | Low (Noted) | Scheduler診断結果が環境依存（セッションにより `can_register=true/false`）。 | 最新監査結果（`can_register=false`）を docs に反映し、事前診断を必須運用として維持。 |

## Changes

1. `tools/check_u8_ops_freshness.ps1`
   - Diff intent: `profile` 欠損artifactを許容し、`Profile=Any` の走査で例外停止しないようにする。
   - `Resolve-ProfileFromArtifact` を追加。
   - `latest_profile` は `profile` 優先、欠損時は filename から `Custom|Daily|Gate` 推定、不可なら `Unknown`。

2. `docs/NEXT_TASKS.md`
   - Diff intent: 改訂履歴 `R34` を追加し、U8-T6（鮮度チェック互換性修正）を Done で記録。

3. `docs/05-dev/dev-status.md`
   - Diff intent: U8-T6 完了を追記し、Scheduler診断の環境依存（true/false実績）と最新監査結果（false）を反映。

4. `docs/05-dev/u8-operations-automation.md`
   - Diff intent: Check 3 の `profile` 欠損互換ルールと、2026-02-25 22:04 監査証跡を追記。

## Commands

```powershell
# 再現/確認
./tools/check_u8_ops_freshness.ps1 -ArtifactPath "Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_default_20260225_220408.json"
./tools/check_u8_ops_freshness.ps1 -ThresholdHours 9999999 -ArtifactPath "Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_relaxed_20260225_220408.json"
./tools/check_u8_ops_freshness.ps1 -ArtifactDir "Unity_PJ/artifacts/manual-check/__no_such_dir_for_test" -ArtifactPath "Unity_PJ/artifacts/manual-check/audit_fix_freshness_noartifact_20260225_220409.json"
./tools/check_u8_ops_freshness.ps1 -Profile Custom -ArtifactPath "Unity_PJ/artifacts/manual-check/audit_fix_freshness_custom_20260225_220408.json"
./tools/diagnose_u8_scheduler.ps1 -ArtifactPath "Unity_PJ/artifacts/manual-check/audit_fix_scheduler_diag_20260225_220408.json"
```

## Tests

| テスト | 結果 | artifact |
|---|---|---|
| T1: Freshness Any default | Pass (exit=0) | `audit_fix_freshness_any_default_20260225_220408.json` |
| T2: Freshness Any relaxed | Pass (exit=0) | `audit_fix_freshness_any_relaxed_20260225_220408.json` |
| T3: Freshness NoArtifact | Pass (期待Fail, exit=1) | `audit_fix_freshness_noartifact_20260225_220409.json` |
| T4: Freshness Custom | Pass (exit=0) | `audit_fix_freshness_custom_20260225_220408.json` |
| T5: Scheduler diagnose | Pass (期待Fail, exit=1 / can_register=false) | `audit_fix_scheduler_diag_20260225_220408.json` |

## Rationale (Key Points)

- 旧artifactを Invalid として除外するより、互換fallbackで継続可能にする方が既存証跡運用に適合する。
- `run_at_utc` 選定ルールは維持し、今回の修正は `latest_profile` 解決の安全化に限定した。
- Scheduler は環境依存の断続事象が続くため、ドキュメント上は単一結果（成功のみ）で固定しない。

## Next Actions

1. 日次運用で `./tools/run_u8_ops_checks.ps1 -Profile Daily` 実行後に `./tools/check_u8_ops_freshness.ps1` を併用する。
2. Scheduler 登録前は毎回 `./tools/diagnose_u8_scheduler.ps1` を実行し、`can_register=true` のセッションのみ登録する。

## Rollback

- `tools/check_u8_ops_freshness.ps1` の `Resolve-ProfileFromArtifact` 追加分を逆適用する。
- `docs/NEXT_TASKS.md`, `docs/05-dev/dev-status.md`, `docs/05-dev/u8-operations-automation.md` の追記分を逆適用する。
- 本worklogとObsidianログは削除せず、`Rolled back` 注記で履歴を保持する。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
