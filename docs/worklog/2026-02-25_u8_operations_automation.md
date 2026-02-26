# Worklog: U8 Operations Automation (2026-02-25)

- Date: 2026-02-25
- Task: U7完了後の運用自動化（bootstrap_missing監視 + 文書同期チェック）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `tools/check_runtime_bootstrap_missing.ps1`
  - `tools/check_unity_legacy_docs_sync.ps1`
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-pass.jsonl`
  - `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-fail.jsonl`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_pass.json`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_fail.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_pass.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_fail.json`
  - `Unity_PJ/artifacts/manual-check/u8_operations_automation_20260224.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-25_u8_operations_automation.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260225_0227.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u8, operations, automation]

## Summary
- `ui.hud.bootstrap_missing` 連続監視を `tools/check_runtime_bootstrap_missing.ps1` として追加した。
- Unity導線文書更新時の legacy文書同期漏れ検知を `tools/check_unity_legacy_docs_sync.ps1` として追加した。
- 判定基準/運用手順を `docs/05-dev/u8-operations-automation.md` に明文化し、`NEXT_TASKS` / `dev-status` を U8 Done へ同期した。

## Findings
1. Medium (Fixed): `ui.hud.bootstrap_missing` の連続出力監視が手動確認依存で、自動fail基準が未定義。
- Fix: `tools/check_runtime_bootstrap_missing.ps1` を追加し、`ThresholdConsecutive=3` / `MaxGapSeconds=5` / reset event を実装。

2. Medium (Fixed): `QUICKSTART` / `unity-runtime-manual-check` 更新時に `PACKAGING` / `RESIDENT_MODE` 同期漏れを機械判定できない。
- Fix: `tools/check_unity_legacy_docs_sync.ps1` を追加し、`Last Updated` と Unity参照リンクの整合チェックを実装。

## Changes
1. Runtime監視チェック追加
- File: `tools/check_runtime_bootstrap_missing.ps1`
- Diff intent: `runtime-*.jsonl` を走査し、`ui.hud.bootstrap_missing` 連続回数と時間間隔を評価して fail/pass を返す。artifact JSON 出力に対応。

2. 文書同期チェック追加
- File: `tools/check_unity_legacy_docs_sync.ps1`
- Diff intent: `QUICKSTART` / `unity-runtime-manual-check` / `PACKAGING` / `RESIDENT_MODE` の `Last Updated` と参照リンク整合を検証し、同期漏れを fail 判定する。

3. 運用基準の文書化
- File: `docs/05-dev/u8-operations-automation.md`
- Diff intent: しきい値・判定基準・実行例・オプションを定義し、再実行可能な運用手順を固定する。

4. 導線文書と状態同期
- Files:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Diff intent: 自動チェック導線を追加し、U8タスク完了状態を同期する。

5. 実行例artifact追加
- Files:
  - `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-pass.jsonl`
  - `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-fail.jsonl`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_pass.json`
  - `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_fail.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_pass.json`
  - `Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_fail.json`
  - `Unity_PJ/artifacts/manual-check/u8_operations_automation_20260224.md`
- Diff intent: PASS/FAIL 両ケースの再現可能な証跡を残す。

## Commands
```powershell
# repo identification fallback
Get-Content .git/HEAD
Get-Content .git/config

# skill/doc context
Get-Content D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md
Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
Get-Content docs/05-dev/QUICKSTART.md
Get-Content docs/05-dev/unity-runtime-manual-check.md

# automation checks (artifacts)
./tools/check_runtime_bootstrap_missing.ps1 -LogDir "Unity_PJ/artifacts/manual-check/u8-runtime-log-samples" -LogPattern "runtime-20260224-pass.jsonl" -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_pass.json"
./tools/check_runtime_bootstrap_missing.ps1 -LogDir "Unity_PJ/artifacts/manual-check/u8-runtime-log-samples" -LogPattern "runtime-20260224-fail.jsonl" -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_fail.json"
./tools/check_unity_legacy_docs_sync.ps1 -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_pass.json"
./tools/check_unity_legacy_docs_sync.ps1 -RequireSameDay -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_fail.json"

# sync verification
Select-String -Path docs/NEXT_TASKS.md -Pattern "R30|U8: 運用自動化|U8-T1|U8-T2"
Select-String -Path docs/05-dev/dev-status.md -Pattern "U8運用自動化|現在フェーズ（U8|check_runtime_bootstrap_missing|check_unity_legacy_docs_sync"
```

## Tests
- `check_runtime_bootstrap_missing`:
  - Pass sample: Pass（`max_consecutive=2`, threshold=3）
  - Fail sample: Fail（expected, `max_consecutive=3`, threshold=3）
- `check_unity_legacy_docs_sync`:
  - Default mode: Pass（Unity latest=2026-02-24, legacy=2026-02-25）
  - `-RequireSameDay`: Fail（expected, strict mode mismatch）
- 文書同期: `NEXT_TASKS` / `dev-status` の U8反映を静的確認。
- 未実施: Unity実機再実行（本タスクは運用自動化スクリプト/文書更新が対象のため、既存ログと静的検証で代替）。

## Rationale (Key Points)
- runtime欠損ログは2秒間隔のため、3連続閾値で誤検知を抑えつつ継続異常を検知できる。
- legacy同期漏れは `Last Updated` と参照リンクの2軸で判定することで、更新漏れと参照欠落の両方を検知できる。
- PASS/FAIL 両方のartifactを固定化し、環境差異下でもチェック仕様の再現性を担保した。

## Rollback
- 戻す対象:
  - `tools/check_runtime_bootstrap_missing.ps1`
  - `tools/check_unity_legacy_docs_sync.ps1`
  - `docs/05-dev/u8-operations-automation.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/artifacts/manual-check/u8_*`
  - `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/*`
- 手順:
  - 対象差分を逆適用して U8自動化追加前の状態へ戻す。
  - `docs/worklog` にロールバック理由（何を・なぜ戻したか）を追記する。
  - Obsidianログは削除せず `Rolled back` / `Superseded` を追記する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. 実ランタイムログディレクトリ（`AppData/LocalLow/.../logs`）を対象に `check_runtime_bootstrap_missing` を定期実行する。
2. Unity導線文書更新PRに `check_unity_legacy_docs_sync` 実行結果artifactを添付する。

