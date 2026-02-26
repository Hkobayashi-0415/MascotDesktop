# Worklog: APP-T2 Full Core 接続 実装前詳細設計確定（2026-02-26）

- Date: 2026-02-26
- Task: APP-T2（Full Core接続）の実装前設計確定。設計書新規作成と状態同期文書更新。
- Execution-Tool: Codex
- Execution-Agent: codex-gpt5
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `docs/05-dev/app-t2-full-core-design.md`
  - `docs/05-dev/app-spec-and-roadmap.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/05-dev/u5-core-integration-plan.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-26_app_t2_full_core_design.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_2344.md
- Tags: [agent/codex-gpt5, model/gpt-5, tool/codex, app-dev, app-t2, full-core, design]

## Summary

APP-T2（Full Core接続）の実装前詳細設計を `docs/05-dev/app-t2-full-core-design.md` に確定した。Endpoint契約、共通スキーマ、エラー運用、Runtime/Core責務、degraded mode、DoD、テスト戦略、non-blocking凍結ルールまで固定し、`NEXT_TASKS` / `dev-status` / `app-spec-and-roadmap` を同期した。

## Findings（重大度順）

| # | 重大度 | 内容 | 対応 |
|---|---|---|---|
| F1 | High | APP-T2 実装前に Endpoint契約・degraded復帰条件・DoD が単一文書で未固定。 | `app-t2-full-core-design.md` を新規作成し、Q1-Q36 の合意事項を決定ログとして固定。 |
| F2 | Med | APP-T2 状態が `NEXT_TASKS` / `dev-status` / `app-spec-and-roadmap` で設計確定状態として明示されていなかった。 | 3文書を同期し、APP-T2 を「Design Done / Impl Not Started」で整合。 |
| F3 | Low | `git` / `rg` が環境で利用不可。 | AGENTS.md 代替手順に従い `.git/HEAD` と `.git/config`、PowerShell標準コマンドで検証を実施。 |

## Changes

1. `docs/05-dev/app-t2-full-core-design.md`（新規）
   - Decision Log（Q1-Q36）
   - Endpoint設計（LLM/TTS/STT/health/config）
   - 共通 request/response schema（`request_id`/`core_request_id`/`error_code`/`retryable`/`attempt`）
   - timeout/retry/backoff と fallback 規則
   - エラー分類と HTTP マッピング
   - Runtime/Core 境界責務、`RuntimeConfig` 切替、degraded mode
   - DoD とテスト戦略（EditMode 4スイート + 手動確認）

2. `docs/05-dev/app-spec-and-roadmap.md`（更新）
   - 根拠に APP-T2 設計書を追加
   - F-09 状態を `Design Done / Impl Not Started` に更新
   - APP-T2 引き渡し条件に「詳細設計確定済み」を追加

3. `docs/NEXT_TASKS.md`（更新）
   - 改訂履歴 R39 を追加
   - APP-T2 状態を `Not Started（Design Done）` に更新
   - 完了条件へ設計書整合を追加

4. `docs/05-dev/dev-status.md`（更新）
   - 現状サマリーへ APP-T2 設計確定を追加
   - 次アクション APP-T2 前提に設計書参照を追加

5. `docs/worklog/2026-02-26_app_t2_full_core_design.md`（新規）
6. `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_2344.md`（新規）

## Commands

```powershell
# リポジトリ同定（git不可のため代替）
Get-Content .git/HEAD
Get-Content .git/config

# 根拠文書確認
Get-Content docs/05-dev/app-spec-and-roadmap.md
Get-Content docs/NEXT_TASKS.md
Get-Content docs/05-dev/dev-status.md
Get-Content docs/02-architecture/interfaces/ipc-contract.md
Get-Content docs/05-dev/u5-core-integration-plan.md

# 設計文書検証
Test-Path docs/05-dev/app-t2-full-core-design.md
Select-String -Path docs/05-dev/app-t2-full-core-design.md -Pattern "^## 2\. Decision Log|^## 3\. Endpoint|^## 7\. APP-T2 完了条件|未確定事項: なし"
Select-String -Path docs/NEXT_TASKS.md,docs/05-dev/dev-status.md,docs/05-dev/app-spec-and-roadmap.md -Pattern "app-t2-full-core-design\.md|Not Started（Design Done）|Design Done / Impl Not Started"
```

## Tests

| テスト | コマンド | exit code | 結果 | 根拠 |
|---|---|---|---|---|
| T1: 設計文書生成確認 | `Test-Path docs/05-dev/app-t2-full-core-design.md` | 0 | PASS (`True`) | 設計文書が存在 |
| T2: 設計必須セクション確認 | `Select-String ... "Decision Log|Endpoint|完了条件|未確定事項: なし"` | 0 | PASS | 全ヒット確認 |
| T3: 状態同期確認 | `Select-String ... "app-t2-full-core-design.md|Not Started（Design Done）|Design Done / Impl Not Started"` | 0 | PASS | 3文書で同期確認 |
| T4: リポジトリ同定（代替） | `Get-Content .git/HEAD`, `Get-Content .git/config` | 0 | PASS | `feature/U7`, origin=`MascotDesktop.git` を確認 |

## Rationale（Key Points）

- 既存の U5 IPC 契約と APP-T1 の引き渡し条件を維持しつつ、APP-T2 で必要な拡張（`core_request_id`、共通エラー規約、degraded復帰規則）を追加した。
- `/v1` 互換性方針（破壊的変更禁止、必要時 `/v2`）を設計時点で固定し、実装時の判断ブレを排除した。
- timeout/retry 初期値は固定したが、Q24ルールに従い「理由・解除条件・見直し期限」付き non-blocking 凍結を設け、実測調整余地を管理化した。

## Rollback

- `docs/05-dev/app-t2-full-core-design.md` を削除し、関連3文書の該当差分（R39, APP-T2状態更新, F-09更新）を逆適用すればロールバック可能。
- `docs/worklog/2026-02-26_app_t2_full_core_design.md` と Obsidianログは削除しない。
- ロールバック時は本ファイルに理由（何を・なぜ戻したか）を追記し、Obsidianログには `Rolled back` / `Superseded` を追記する。

## Record Check

- Report-Path exists: True（`docs/worklog/2026-02-26_app_t2_full_core_design.md`）
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes（`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_2344.md`）
- Execution fields recorded: Yes（Execution-Tool / Execution-Agent / Execution-Model）
- Tags include agent/model/tool: Yes

## Next Actions

1. APP-T2 実装を開始し、`RuntimeConfig.mode=core` で endpoint 切替を実装する。
2. EditMode 4スイートと手動確認（通常/劣化/復帰）を実施し、必要なら timeout 秒数を NF-01 解除条件で調整する。
3. 実装完了時に `NEXT_TASKS` / `dev-status` / worklog を再同期する。
