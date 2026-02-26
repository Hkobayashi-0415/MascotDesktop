# Worklog: Legacy Docs Alignment After U7 (2026-02-25)

- Date: 2026-02-25
- Task: 旧PoC文書（PACKAGING/RESIDENT_MODE）を現行Unity導線へ整合
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: code-review, worklog-update
- Repo-Refs:
  - `docs/PACKAGING.md`
  - `docs/RESIDENT_MODE.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-25_legacy_docs_alignment.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260225_0013.md
- Tags: [agent/codex, model/gpt-5, tool/codex, docs, legacy, unity]

## Summary
- `docs/PACKAGING.md` と `docs/RESIDENT_MODE.md` に `legacy-reference` を明記し、旧PoC文書としての適用範囲を明確化した。
- 両文書に現行 Unity導線（`docs/05-dev/QUICKSTART.md`, `docs/05-dev/unity-runtime-manual-check.md`）への参照を追加した。
- `docs/NEXT_TASKS.md`（R29）と `docs/05-dev/dev-status.md` を同期し、文書整合タスク完了を反映した。

## Changes
1. `docs/PACKAGING.md`
- 先頭に `Status/Last Updated/Scope` を追加し、legacy-reference であることを明示。
- IMPORTANT 注記で現行 Unity導線優先を明記。
- 関連ドキュメントに `QUICKSTART` / `unity-runtime-manual-check` を追加。

2. `docs/RESIDENT_MODE.md`
- 先頭に `Status/Last Updated/Scope` を追加し、legacy-reference であることを明示。
- IMPORTANT 注記で現行 Unity導線優先を明記。
- `Unity Scope（Current）` セクションを追加し、Hide/Show（最小化/復帰）とログ確認観点を追記。
- 関連ドキュメントに `QUICKSTART` / `unity-runtime-manual-check` を追加。

3. `docs/NEXT_TASKS.md`
- 改訂履歴に `R29` を追加（legacy docs sync を記録）。
- U7 Done チェック項目に「旧PoC文書への legacy-reference 明記」を追記。

4. `docs/05-dev/dev-status.md`
- 現状サマリーに文書整合完了（2026-02-25）を追記。
- 次アクションを「更新要否バックログ」から「Unity導線更新時の同日同期」へ更新。

## Commands
```powershell
# 差分把握
Get-Content docs/PACKAGING.md
Get-Content docs/RESIDENT_MODE.md
Get-Content docs/05-dev/QUICKSTART.md
Get-Content docs/05-dev/unity-runtime-manual-check.md

# 静的検証
Select-String -Path docs/PACKAGING.md -Pattern 'Status: legacy-reference|Unity Runtime|QUICKSTART|unity-runtime-manual-check'
Select-String -Path docs/RESIDENT_MODE.md -Pattern 'Status: legacy-reference|Unity Scope（Current）|window.resident.hidden|window.topmost.changed|QUICKSTART|unity-runtime-manual-check'
Select-String -Path docs/NEXT_TASKS.md -Pattern 'R29|legacy-reference|U7: リリース後安定化・保守性強化（Done）'
Select-String -Path docs/05-dev/dev-status.md -Pattern '文書整合|次アクション|同日同期|PACKAGING.md|RESIDENT_MODE.md'
```

## Tests
- 実施: 静的検証（文書整合）
  - `PACKAGING.md` に legacy-reference と Unity導線参照が存在することを確認。
  - `RESIDENT_MODE.md` に legacy-reference / Unity Scope / ログ観点が存在することを確認。
  - `NEXT_TASKS.md` に R29 と U7完了チェック追記を確認。
  - `dev-status.md` に文書整合完了と次アクション更新を確認。
- 未実施: Unity/Runtime 実行テスト（本タスクは文書更新のみ）。

## Rationale (Key Points)
- 旧PoC手順を削除せず legacy-reference として残すことで、履歴参照性を維持しながら誤運用リスクを抑えられる。
- Unity運用導線への明示リンク追加により、現行手順への誘導を文書単体で完結できる。
- 状態文書の同期を同時実施することで、タスク管理上の完了判定と実文書のズレを防ぐ。

## Rollback
- 戻す対象:
  - `docs/PACKAGING.md`
  - `docs/RESIDENT_MODE.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- 手順:
  - 該当差分を逆適用して、legacy-reference追記とR29反映を取り消す。
  - `docs/worklog` にロールバック理由（何を・なぜ戻したか）を追記する。
  - Obsidianログは削除せず、`Rolled back` / `Superseded` 注記を追記する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unity導線（`QUICKSTART` / `unity-runtime-manual-check`）更新時に、同日で `PACKAGING` / `RESIDENT_MODE` の参照整合を確認する。
2. 旧PoC専用手順として残す情報と完全廃止できる情報を次フェーズで仕分ける。
