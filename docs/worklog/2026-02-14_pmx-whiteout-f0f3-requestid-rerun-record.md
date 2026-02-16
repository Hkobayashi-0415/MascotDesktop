# Worklog: pmx-whiteout-f0f3-requestid-rerun-record

- Date: 2026-02-14
- Task: 2026-02-13 F0/F3 再採取ログ（request_id付き）を記録表へ反映
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-14_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0056.md, docs/worklog/2026-02-14_pmx-whiteout-f3-rerun-record-20260213.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Report-Path: docs/worklog/2026-02-14_pmx-whiteout-f0f3-requestid-rerun-record.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_0211.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, f0, f3, rerun]

## Summary
ユーザー共有の 2026-02-13 F0/F3 ログ（3モデル x 2）を request_id 付きで記録した。
Record Sheet B の目視所見と Record Sheet D/G の診断値が同じ傾向で再現されることを確認した。

## Changes
- 更新: `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - `Record Sheet A` に `R20260213_F0F3_3models_r2` を追加。
  - `Record Sheet H: Re-run Snapshot 2026-02-13 (F0/F3 with request_id)` を追加。
  - `Decision Update (2026-02-13 Re-run)` に再採取一致を追記。

## Captured Evidence
- F0 / kanata: `req-e880bff90c024cc287ae76ffec9bffdc` -> `shader_lighting_candidate`
- F3 / kanata: `req-1e81b032ed8842558d71f71b95dca97c` -> `shader_lighting_candidate`
- F0 / AZKi: `req-e5cd228568ec442aadc7ebb606e3b0af` -> `materialloader_threshold_candidate`
- F3 / AZKi: `req-c41b499b042943628ecdff194b169362` -> `materialloader_threshold_candidate`
- F0 / nene_BEA: `req-d847dee92f164e9586c1b2ff0815115f` -> `shader_lighting_candidate`
- F3 / nene_BEA: `req-a7b924008f4449208af9a65e6c0f401a` -> `shader_lighting_candidate`

## Commands
- `apply_patch Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260214_021001.log`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260214_021001.xml`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity.com 起動時に `指定されたモジュールが見つかりません` で失敗。
  - `editmode-20260214_021001.log/.xml` は未生成（存在確認: False / False）。
  - 直近の手動成功結果は `editmode-20260214_005040.log/.xml`（ユーザー実行）。

## Rationale (Key Points)
- F0/F3 両方で hint 分類は固定（kanata/nene_BEA: shader, AZKi: materialloader）で、再現性がある。
- F3 で `brightDiffuseRatio=0` が共通し、白飛び抑制方向の比較軸として有効。

## Next Actions
1. Shader側補正（specular/edge上限制御）適用後に、同じ `Record Sheet H` フォーマットで再採取する。
2. `Record Sheet B` の未記入対象（残り5モデル）を順次埋める。
3. 仕上げとして `Record Sheet E/F`（F2系）の扱いを整理し、意思決定向け記録を簡素化する。

## Rollback
1. `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md` から `R20260213_F0F3_3models_r2` と `Record Sheet H` 追記分を削除する。
2. `docs/worklog/2026-02-14_pmx-whiteout-f0f3-requestid-rerun-record.md` を削除する。
3. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_0211.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
