# 作業ログ: ロールバック時のObsidianログ保持ルール永続化

- 日付: 2026-02-20
- タスク: ロールバック手順から Obsidianログ削除方針を廃止し、保持ルールを永続化
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `AGENTS.md`
  - `docs/worklog/_template.md`
  - `docs/worklog/2026-02-19_next_tasks_unity_refresh.md`
  - `docs/worklog/2026-02-19_unity_test_ops_standardization.md`
  - `docs/worklog/2026-02-20_manual_test_sync.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_rollback_policy_persistence.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0054.md
- Tags: [agent/codex, model/gpt-5, tool/codex, rollback-policy, worklog]

## 変更内容
1. 永続ルールを `AGENTS.md` に追加。
   - ロールバック時に Obsidianログを削除しないこと
   - `docs/worklog/` にロールバック理由を追記すること
   - Obsidianログに `Rolled back` / `Superseded` 注記を残すこと
2. `docs/worklog/_template.md` の Rollback 節を上記方針へ更新。
3. 既存3件の worklog で Obsidian削除記述を修正。
4. Obsidianテンプレート `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md` に Rollback Handling 節を追加。

## 実行コマンド
- `Get-ChildItem -Path . -Force`
- `Get-ChildItem -Path . -Recurse -Filter AGENTS.md`
- `Get-ChildItem -Path docs -Recurse -File | Where-Object { $_.Name -match 'template|TEMPLATE|worklog' }`
- `Get-Content AGENTS.md`
- `Get-Content docs/worklog/_template.md`
- `Get-Content docs/worklog/2026-02-19_next_tasks_unity_refresh.md`
- `Get-Content docs/worklog/2026-02-19_unity_test_ops_standardization.md`
- `Get-Content docs/worklog/2026-02-20_manual_test_sync.md`
- `apply_patch`（各ファイル修正）

## テスト結果
- 文書変更タスクのためコードテスト実行なし。
- 整合確認:
  - 対象3ファイルから「Obsidianログを削除」の指示が除去されたことを確認。
  - `AGENTS.md` と `docs/worklog/_template.md` に新方針が記載されたことを確認。

## 判断理由（要点）
- Obsidianログ削除は、障害時の判断根拠と時系列の追跡性を下げる。
- ロールバック理由を明示しつつログを保持する方が監査性・再現性に優れる。
- 永続化のため、実務で参照するルール本体（`AGENTS.md`）と記録テンプレートの両方を更新した。

## 次アクション
- 今後ロールバック発生時は、新方針に沿って `docs/worklog` へ理由を追記し、Obsidian側に `Rolled back` / `Superseded` 注記を入れる。

## ロールバック方針
- 本変更を戻す場合は repo 内差分のみを手動で取り消す。
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、関連 Obsidianログへ `Rolled back` / `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
