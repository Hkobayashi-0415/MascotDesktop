# 作業ログ: U4-T4 運用ドキュメント導線のUnity前提一本化

- 日付: 2026-02-20
- タスク: `docs/05-dev` の主導線を Unity実行手順へ統一し、legacy手順を参照分離
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/run-poc.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/05-dev/dev-status.md`
  - `docs/NEXT_TASKS.md`
  - `docs/worklog/2026-02-20_u4_character_switch_kickoff.md`
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_u4_docs_unification.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0142.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u4, docs-unification, unity-runtime]

## 変更内容
1. `docs/05-dev/QUICKSTART.md` を Unity First 導線へ更新。
   - 起動入口を `Unity_PJ/project` + Play Mode + HUD確認に変更。
   - legacy実行手順は参照先へ分離。
2. `docs/05-dev/unity-runtime-manual-check.md` を新規作成。
   - Runtime HUD を使った画面確認手順と合格条件を定義。
3. `docs/05-dev/run-poc.md` を legacy reference 扱いへ更新。
   - 先頭に Unity現行導線リンクを追加。
4. `docs/NEXT_TASKS.md` を更新。
   - `R6` を追加。
   - U4を `Done`、`U4-T4` を `Done` に更新。
5. `docs/05-dev/dev-status.md` を更新。
   - `U4-T4` 完了を反映し、次アクションを U5準備へ変更。

## 実行コマンド
- `Get-Content docs/NEXT_TASKS.md`
- `Get-Content docs/05-dev/dev-status.md`
- `Get-Content docs/05-dev/QUICKSTART.md`
- `Get-Content docs/05-dev/run-poc.md`
- `Get-Content Unity_PJ/docs/05-dev/phase3-parity-verification.md`
- `Get-Content tools/run_unity.ps1`
- `Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/...`（HUD/Bootstrap 根拠抽出）
- `apply_patch`（ドキュメント更新）

## テスト結果
- 文書変更のためコードテストは実行なし。
- 整合確認:
  - `docs/05-dev` の主導線が Unity手順に更新されていることを確認。
  - `docs/05-dev/run-poc.md` が legacy reference として分離されていることを確認。
  - `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` の U4 状態が一致していることを確認。

## 判断理由（要点）
- 画面確認手順が `Unity_PJ/docs/...` 側に分散していたため、`docs/05-dev` から直接辿れる状態へ統一した。
- 旧PoC手順を削除せず参照分離したことで、履歴参照性を維持しつつ Unity導線の混在リスクを下げた。
- U4の未完項目は文書運用導線の統一が中心だったため、今回の反映で完了条件を満たすと判断した。

## 次アクション
- U5（Core統合）に向けて、受入条件と段階移行手順の具体化を開始する。

## ロールバック方針
- 追加ファイルを個別削除:
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `docs/worklog/2026-02-20_u4_docs_unification.md`
- 更新ファイルを変更前内容へ戻す:
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/run-poc.md`
  - `docs/05-dev/dev-status.md`
  - `docs/NEXT_TASKS.md`
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0142.md` に `Rolled back` または `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
