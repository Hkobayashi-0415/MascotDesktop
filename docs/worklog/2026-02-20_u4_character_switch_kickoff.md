# 作業ログ: U4 キャラクター切替導線の着手と標準化

- 日付: 2026-02-20
- タスク: U4 の「キャラクター切替導線」と運用ドキュメント導線統一の初回反映
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-character-switch-operations.md`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
  - `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_u4_character_switch_kickoff.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0108.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u4, unity-operations, character-switch]

## 変更内容
1. `docs/05-dev/unity-character-switch-operations.md` を新規作成。
   - Runtime HUD での `Model: prev/next` 切替と `Model: rescan(list)` を標準手順化。
   - 検証ポイント（`avatar.model.loader_selected`, `avatar.model.displayed`）と記録フォーマットを定義。
2. `docs/NEXT_TASKS.md` を更新。
   - 改訂履歴 `R5` を追加。
   - U4の「キャラクター切替導線」を完了へ更新。
   - `U4-T3`（Done）/`U4-T4`（In Progress）を追加。
3. `docs/05-dev/dev-status.md` を更新。
   - `U4-T3` 完了を反映。
   - 残課題を `U4-T4`（運用ドキュメント一本化）へ集約。
4. `docs/05-dev/QUICKSTART.md` に新規手順書への参照リンクを追加。

## 実行コマンド
- `Get-Content docs/NEXT_TASKS.md`
- `Get-Content docs/05-dev/dev-status.md`
- `Get-Content docs/05-dev/QUICKSTART.md`
- `Get-Content docs/05-dev/run-poc.md`
- `Get-Content Unity_PJ/README.md`
- `Get-Content Unity_PJ/spec/latest/spec.md`
- `Get-Content Unity_PJ/docs/02-architecture/assets/asset-layout.md`
- `Get-Content Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/...`（HUD/Bootstrap/Config の根拠抽出）
- `apply_patch`（docs 更新）

## テスト結果
- 文書更新タスクのためコードテスト実行なし。
- 整合確認:
  - `docs/NEXT_TASKS.md` の U4 状態更新と追加タスク表を確認。
  - `docs/05-dev/dev-status.md` の U4 反映を確認。
  - `docs/05-dev/unity-character-switch-operations.md` の参照先が実在することを確認。

## 判断理由（要点）
- U4未完項目のうち「キャラクター切替導線」は、Runtime HUD の実装根拠が既に揃っており、文書化で完了条件を満たせる。
- 一方で `docs/05-dev` には legacy前提手順が残るため、一本化は `U4-T4` として継続管理が必要。
- 進捗の見える化を優先し、`NEXT_TASKS` と `dev-status` を同時更新して状態の不一致を防止した。

## 次アクション
- `U4-T4` として `docs/05-dev` の legacy文脈手順を分離し、Unity導線を優先表示する。

## ロールバック方針
- 追加ファイルを個別削除:
  - `docs/05-dev/unity-character-switch-operations.md`
  - `docs/worklog/2026-02-20_u4_character_switch_kickoff.md`
- 更新ファイルを変更前内容へ戻す:
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/05-dev/QUICKSTART.md`
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0108.md` に `Rolled back` または `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
