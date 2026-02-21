# 作業ログ: U5-T1 文書化実行（方針/IPC契約/受入条件）

- 日付: 2026-02-20
- タスク: U5の実行開始として方針・契約・受入条件を文書化し、`NEXT_TASKS` と `dev-status` を同期
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `docs/05-dev/u5-core-integration-plan.md`
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md`
  - `Unity_PJ/spec/latest/spec.md`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-20_u5_execution_docs.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0926.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, ipc-contract, planning]

## 変更内容
1. `docs/05-dev/u5-core-integration-plan.md` を U5実行計画として新規作成。
   - Minimal Core -> Full Core の段階移行（U5-P1..P4）を定義。
   - 画面到達状態と受入条件（`request_id`/`error_code`/`retryable`）を明文化。
2. `docs/02-architecture/interfaces/ipc-contract.md` を Unity境界向け契約へ更新。
   - transport 固定（loopback HTTP + JSON）、ヘッダ/Body 相関、エラー契約、エンドポイント方針を定義。
3. `docs/NEXT_TASKS.md` を更新。
   - 改訂履歴 `R7` を追加。
   - `U5` を `In Progress` に更新し、定義済み項目を完了化。
   - 実装継続用タスク `U5-T2..T4` を追加。
4. `docs/05-dev/dev-status.md` を更新。
   - `U5-T1` 完了と U5実行フェーズ開始を反映。
   - 次アクションを `U5-T2..T4` へ更新。

## 実行コマンド
- `if (Get-Command git ...) { ... } else { Get-Content .git/HEAD; Get-Content .git/config }`
- `Get-Content docs/NEXT_TASKS.md`
- `Get-Content docs/05-dev/dev-status.md`
- `Get-Content docs/02-architecture/interfaces/ipc-contract.md`
- `Get-Content docs/05-dev/u5-core-integration-plan.md`
- `Get-Content Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md`
- `Get-Content Unity_PJ/spec/latest/spec.md`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs`
- `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
- `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- `Get-Content D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- `apply_patch`（`docs/02-architecture/interfaces/ipc-contract.md` 更新）
- `apply_patch`（`docs/NEXT_TASKS.md` 更新）
- `apply_patch`（`docs/05-dev/dev-status.md` 更新）

## テスト結果
- 文書変更タスクのため Unityコードテストは未実行。
- 整合確認:
  - `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` で U5 状態が同期していることを確認。
  - `docs/02-architecture/interfaces/ipc-contract.md` が loopback HTTP 固定方針へ更新されたことを確認。
  - `docs/05-dev/u5-core-integration-plan.md` の段階移行定義を確認。

## 判断理由（要点）
- U5開始時点で最大リスクは「契約未固定のまま実装が分岐すること」であり、先に契約と受入条件を固定する必要がある。
- `runtime-boundary-and-ipc` と `spec` で transport は既に固定済みのため、`ipc-contract` の旧草案状態を解消した。
- `U5` を `Done` にすると実装未着手が埋もれるため、`In Progress` のまま定義完了と実装タスクを分離した。

## 次アクション
- `U5-T2`: Minimal Core Bridge を Runtime HUD 導線に接続。
- `U5-T3`: 契約検証 EditMode テストを追加。
- `U5-T4`: LLM/TTS/STT 段階統合と運用手順更新。

## ロールバック方針
- 変更ファイルを変更前内容へ戻す:
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/05-dev/dev-status.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/u5-core-integration-plan.md`（必要なら削除ではなく `superseded` 運用も可）
- Obsidianログは削除しない（履歴保持）。
- ロールバック実施時は理由（何を・なぜ戻したか）を `docs/worklog/` に追記し、`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260220_0926.md` に `Rolled back` または `Superseded` 注記を追加する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model: Yes
- Tags include agent/model/tool: Yes
