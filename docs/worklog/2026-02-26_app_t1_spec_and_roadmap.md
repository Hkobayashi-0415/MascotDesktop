# Worklog: APP-T1 アプリケーション機能仕様とロードマップ確定（2026-02-26）

- Date: 2026-02-26
- Task: APP-T1 — アプリケーション機能仕様・ロードマップ・受入条件の確定。`docs/05-dev/app-spec-and-roadmap.md` 新規作成および関連文書同期。
- Execution-Tool: claude-code
- Execution-Agent: claude-sonnet-4-6
- Execution-Model: claude-sonnet-4-6
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `docs/05-dev/app-spec-and-roadmap.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/spec/latest/spec.md`
  - `docs/05-dev/u5-core-integration-plan.md`
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/PACKAGING.md`
  - `docs/RESIDENT_MODE.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_1400.md
- Tags: [agent/claude-sonnet-4-6, model/claude-sonnet-4-6, tool/claude-code, app-dev, spec, roadmap, app-t1]

---

## Summary

U9完了・App Dev移行可判定後の初手タスクとして、アプリケーション機能仕様（F-01〜F-12）・Phase分割（MVP/Phase 2）・受入条件（DoD）・依存関係・リスク対策を `docs/05-dev/app-spec-and-roadmap.md` に確定した。APP-T2（Full Core接続）と APP-T3（配布パッケージング）が着手可能な粒度で定義済み。non-blocking 凍結 3件（F-10 配布、F-11 トレイ常駐、Scheduler 断続障害）を明示し、残件ゼロを確認。

---

## Findings（重大度順）

| # | 重大度 | 内容 | 対応 |
|---|---|---|---|
| F1 | High | `docs/05-dev/app-spec-and-roadmap.md` が未存在で、APP-T2/T3 の設計根拠が欠如していた。 | 本タスクで新規作成し、F-01〜F-12 / DoD / Phase分割 / リスクを確定。 |
| F2 | High | Full Core接続（F-09）の受入条件・前提条件・依存関係が未定義だった。 | `app-spec-and-roadmap.md` §8「APP-T2引き渡し条件」として明文化。 |
| F3 | Med | 配布パッケージング（F-10）は実装未着手（旧 PyInstaller 手順のみ）。legacy docs 非同期リスクは解消済み（同期チェック Pass 確認済み）。 | APP-T3 スコープとして凍結。non-blocking 管理を明記。 |
| F4 | Low | トレイ常駐（UR-003 tray icon）は現 Unity では未実装（Hide/Show = SW_MINIMIZE/RESTORE のみ）。 | Phase 2 候補として non-blocking 凍結。現行実装で代替運用可能を明記。 |
| F5 | Low | Scheduler 断続障害（`schtasks.exe` モジュール不足）は U9 から管理済み制約（non-blocking）として継続。 | `app-spec-and-roadmap.md` §7 R5 にリスクと回避策を明記。APP-T1 完了報告の non-blocking 件数に算入（計3件）。 |

---

## Changes

1. **`docs/05-dev/app-spec-and-roadmap.md`**（新規作成）
   - UR 要件トレーサビリティ表（UR-001〜UR-012 対応）
   - 機能一覧 F-01〜F-12（既存継承 + APP-T2/T3/Phase2 対象）
   - Phase 1（MVP: F-01〜F-09, F-12）/ Phase 2（F-10, F-11）分割
   - 機能別受入条件（DoD）— テスト可能な形で定義
   - 依存関係グラフ（U9 → APP-T1 → APP-T2 → APP-T3）
   - リスクと回避策（R1〜R5）
   - APP-T2 引き渡し条件（F-09 前提）

2. **`docs/NEXT_TASKS.md`**（更新）
   - 改訂履歴に R38 を追加
   - `### 初手タスク（最大3件）` を `### APP タスク一覧` に拡張し、APP-T1=Done・APP-T2/T3 の前提・依存・完了条件を追加
   - 参照セクションに `app-spec-and-roadmap.md` / worklog を追加

3. **`docs/05-dev/dev-status.md`**（更新）
   - ファイルヘッダ日付を `2026-02-26` に更新
   - 現状サマリーに APP-T1 完了を追記
   - 次アクションセクションで APP-T1 を Done として打ち消し、APP-T2/T3 の前提参照を追加

---

## Commands

```powershell
# T1: 仕様文書存在確認
Test-Path "docs/05-dev/app-spec-and-roadmap.md"
# → True

# T2: 機能IDと受入条件の定義有無
Select-String -Path "docs/05-dev/app-spec-and-roadmap.md" -Pattern "^#### F-" | Measure-Object | Select-Object -ExpandProperty Count
# → 12

# T3: APP-T2/T3 の前提・依存・完了条件の明記確認
Select-String -Path "docs/05-dev/app-spec-and-roadmap.md" -Pattern "APP-T2|APP-T3|引き渡し条件"
# → 複数ヒット

# T4: NEXT_TASKS / dev-status で APP-T1=Done 同期確認
Select-String -Path "docs/NEXT_TASKS.md","docs/05-dev/dev-status.md" -Pattern "APP-T1"
# → Done が両ファイルに存在

# T5: legacy docs 同期チェック（実チェック実行 + artifact採取）
./tools/check_unity_legacy_docs_sync.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_appt1_fix_20260226_1430.json"
# → exit 0 / PASS: status=PASS unity_latest=2026-02-25 max_lag_days=0
# → QUICKSTART=2026-02-25 manual-check=2026-02-25 PACKAGING=2026-02-25 RESIDENT_MODE=2026-02-25

# T6: worklog Record Check 確認
Select-String -Path "docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md" -Pattern "Record Check|Report-Path|Obsidian-Log"
# → 全項目存在
```

---

## Tests

| テスト | コマンド | exit code | 結果 | artifact / 根拠 |
|---|---|---|---|---|
| T1: 仕様文書存在確認 | `Test-Path docs/05-dev/app-spec-and-roadmap.md` | 0 (True) | PASS | `docs/05-dev/app-spec-and-roadmap.md` 生成済み |
| T2: 機能ID数確認（12件） | `Select-String -Pattern "^#### F-" ... \| Measure-Object` | 0 | PASS | F-01〜F-12 全件定義（Phase1: F-01〜F-09/F-12、Phase2: F-10/F-11） |
| T3: APP-T2/T3 引き渡し条件確認 | `Select-String -Pattern "APP-T2\|引き渡し条件"` | 0 | PASS | `app-spec-and-roadmap.md` §8 に明文化済み |
| T4: NEXT_TASKS/dev-status 同期確認 | `Select-String -Pattern "APP-T1" ...両ファイル` | 0 | PASS | NEXT_TASKS R38 / dev-status に Done 反映済み |
| T5: legacy docs 同期チェック（実チェック実行） | `./tools/check_unity_legacy_docs_sync.ps1 -ArtifactPath ...1430.json` | 0 | **PASS** | `Unity_PJ/artifacts/manual-check/u8_docs_sync_appt1_fix_20260226_1430.json` / status=PASS, unity_latest=2026-02-25, all docs in sync |
| T6: worklog Record Check | `Select-String -Pattern "Record Check"` | 0 | PASS | 本ファイル末尾の Record Check 参照 |

---

## Rationale（Key Points）

- APP-T1 を「文書のみ」で完結させ、実装コードは一切変更しないことで、AGENTS.md の仕様変更禁止・コード変更最小化方針を遵守した。
- 機能 F-01〜F-08 は U0〜U9 で実装・検証済みの状態をそのまま継承し、APP フェーズの出発点として明示した。
- F-09（Full Core接続）は APP-T2 のスコープとして分離し、引き渡し条件を §8 で明文化することで APP-T2 の設計根拠を確立した。
- F-10/F-11 および U9 から継続する Scheduler 断続障害（F5）の計3件を「non-blocking 凍結」として明示し、未整理の保留を残さない（解決 or 凍結方針明記）原則を満たした。

---

## Rollback

- `docs/05-dev/app-spec-and-roadmap.md` を削除すればロールバック可能（新規作成のみ）。
- `docs/NEXT_TASKS.md` / `docs/05-dev/dev-status.md` の変更差分のみ逆適用する（R38の削除 + 該当行を R37 時点の状態へ戻す）。
- `docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md` と Obsidian ログは削除しない。
  - worklog にロールバック理由を追記する。
  - Obsidian ログには `Rolled back` / `Superseded` 注記を追記して履歴を保持する。

---

## Record Check

- Report-Path exists: True（`docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md`）
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes（`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_1400.md`）
- Execution fields recorded: Yes（Execution-Tool / Execution-Agent / Execution-Model）
- Tags include agent/model/tool: Yes

---

## Next Actions

1. APP-T2 を開始: Full Core 接続計画を `docs/05-dev` に作成し、`RuntimeConfig` のエンドポイント切替設計を確定する。
2. APP-T3 は APP-T2 完了後に着手: `docs/PACKAGING.md` を Unity 向けに更新し、Unity Windows Standalone Player ビルド導線を確立する。
3. F-12（運用監視）を継続: `./tools/run_u8_ops_checks.ps1 -Profile Daily` を定期実行する。
