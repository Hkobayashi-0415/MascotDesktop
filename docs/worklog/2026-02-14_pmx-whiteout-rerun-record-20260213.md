# Worklog: pmx-whiteout-rerun-record-20260213

- Date: 2026-02-14
- Task: 2026-02-13 再採取 remediation_hint の記録反映
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update, bug-investigation
- Repo-Refs: AGENTS.md, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, docs/worklog/2026-02-12_pmx-whiteout-f2-diagnostic-record-update.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md
- Report-Path: docs/worklog/2026-02-14_pmx-whiteout-rerun-record-20260213.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_0021.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, rerun, diagnostics]

## Summary
ユーザー共有の 2026-02-13 再採取ログ（F0/F2・3モデル）を `pmx-validation-procedure-and-record.md` に反映した。既存の `Record Sheet D/E` と完全一致し、hint/ratio 値の再現性を確認した。

## Changes
- 更新: `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - `Record Sheet F: Re-run Snapshot 2026-02-13 (F0/F2)` を追加。
  - 6行（F0/F2 x 3モデル）を追記。

## Captured Values (2026-02-13)
- F0: kanata=`shader_lighting_candidate`, AZKi=`materialloader_threshold_candidate`, nene_BEA=`shader_lighting_candidate`
- F2: kanata=`shader_lighting_candidate`, AZKi=`materialloader_threshold_candidate`, nene_BEA=`shader_lighting_candidate`
- いずれも既存 `Record Sheet D/E` の値と同値。

## Commands
- `apply_patch Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-14_pmx-whiteout-rerun-record-20260213.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_0021.md`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity.com 起動時に `指定されたモジュールが見つかりません` で実行不可。
  - `editmode-20260214_002101.log/.xml` は未生成。

## Rationale (Key Points)
- 数値再現が取れたため、現在の診断軸（hint + ratio/share）は安定しており、次はF3再採取と目視差分を優先すべき。
- 2026-02-13時点でも `shader_lighting_candidate`（kanata/nene_BEA）と `materialloader_threshold_candidate`（AZKi）の対照関係は維持された。

## Next Actions
1. F3の3モデル再採取ログ（hint + ratio/share）を取得して `Record Sheet D` に照合追記する。
2. `Record Sheet B` に補正前後の見た目（白飛び/欠け/グレー）を追記し、最終判断を確定する。
3. 必要なら specular/edge cap の微調整案を作成する。

## Rollback
1. `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md` の `Record Sheet F` を削除する。
2. `docs/worklog/2026-02-14_pmx-whiteout-rerun-record-20260213.md` を削除する。
3. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_0021.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
