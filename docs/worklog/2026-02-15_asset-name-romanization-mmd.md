# Worklog: asset-name-romanization-mmd

- Date: 2026-02-15
- Task: MMD 日本語名の指定マッピングをASCII名称へ変更
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Report-Path: docs/worklog/2026-02-15_asset-name-romanization-mmd.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0026.md
- Tags: [agent/codex, model/gpt-5, tool/codex, assets, rename, mmd]

## Summary
指定されたマッピングに基づいてMMD資産名をリネームした。
実ファイルリネームに加えて、既定モデルパスと主要ドキュメント参照を新名称へ更新した。

## Changes
- リネーム:
  - `Unity_PJ/data/assets_user/characters/amane_kanata_official_v1/mmd/PMX/天音かなた.pmx` -> `Unity_PJ/data/assets_user/characters/amane_kanata_official_v1/mmd/PMX/amane_kanata.pmx`
  - `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/天音かなた.pmx` -> `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/amane_kanata.pmx`
  - `Unity_PJ/data/assets_user/characters/momone_nene_official_v1/mmd/PMX/桃鈴ねね_BEA.pmx` -> `Unity_PJ/data/assets_user/characters/momone_nene_official_v1/mmd/PMX/momosuzu_nene_BEA.pmx`
  - `Unity_PJ/data/assets_user/characters/momone_nene_official_v1/mmd/PMX/桃鈴ねね_STD.pmx` -> `Unity_PJ/data/assets_user/characters/momone_nene_official_v1/mmd/PMX/momosuzu_nene_STD.pmx`
  - `Unity_PJ/data/assets_user/characters/amane_kanata_v2_nurse/mmd/ナースたそ` -> `Unity_PJ/data/assets_user/characters/amane_kanata_v2_nurse/mmd/amane_kanata_nurse`
- 更新:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`（default model path）
  - `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`（参照パス）
  - `Unity_PJ/docs/05-dev/asset-path-read-test-design.md`（参照文字列 / AP-005非ASCII例を整合化）
  - `docs/reports/2026-02-09_factor-isolation-execution-plan.md`（参照パス）

## Commands
- `Move-Item ...`（上記5件）
- `Set-Content` による参照文字列置換
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity.com 起動時に `指定されたモジュールが見つかりません` で失敗。
  - 今回分 `editmode-20260215_002536.log/.xml` は未生成。
  - 再試行 `editmode-20260215_002722.log/.xml` も未生成。

## Rationale (Key Points)
- ユーザー指定の名称マッピング（天音かなた/桃鈴ねね/ナースたそ）を優先。
- 実行時の既定モデル参照が旧名を向かないように `SimpleModelConfig` を同時更新。
- 履歴worklog本文は過去記録のため未改変。

## Next Actions
1. Unity側でモデル候補に新ファイル名が出ることを確認。
2. 旧名キャッシュが残る場合は `Model: rescan(list)` 実行。
3. 必要なら `docs/ASSETS_PLACEMENT.md` など運用文書も同命名へ統一。

## Rollback
1. 上記5件を逆方向にリネームして元名へ戻す。
2. `SimpleModelConfig.cs` とドキュメント参照文字列を旧名へ戻す。
3. 本 worklog と Obsidian ログを削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
