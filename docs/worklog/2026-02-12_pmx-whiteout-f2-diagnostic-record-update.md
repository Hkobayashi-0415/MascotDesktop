# Worklog: pmx-whiteout-f2-diagnostic-record-update

- Date: 2026-02-12
- Task: ユーザー共有の remediation_hint（F0/F2）再採取ログを記録表へ反映
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update, bug-investigation
- Repo-Refs: AGENTS.md, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_1955.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-f2-diagnostic-record-update.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_2039.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, diagnostics, f2]

## Summary
ユーザー共有の最新 `avatar.model.remediation_hint`（F0/F2・3モデル）を記録に反映した。F2の3モデル値はF0と同傾向で、hint分類に変化は見られなかったため、既存の暫定方針（shader優先）を維持する。

## Changes
- 更新: `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - `Record Sheet E: F2 Diagnostic Snapshot` を追加。
  - 3モデル（kanata/AZKi/nene_BEA）の F2 値を記録。

## Captured Values
- F2 `amane_kanata_v1`: `hint=shader_lighting_candidate`, `transparentRatio=0.813`, `textureAlphaShare=0.231`, `edgeAlphaShare=0.923`, `highShininessRatio=1`, `brightDiffuseRatio=0.938`, `missingResolveTotal=0`
- F2 `AZKi_4th`: `hint=materialloader_threshold_candidate`, `transparentRatio=0.485`, `textureAlphaShare=0.875`, `edgeAlphaShare=0.125`, `highShininessRatio=0`, `brightDiffuseRatio=1`, `missingResolveTotal=0`
- F2 `momone_nene_BEA`: `hint=shader_lighting_candidate`, `transparentRatio=0.895`, `textureAlphaShare=0.529`, `edgeAlphaShare=0.706`, `highShininessRatio=0.789`, `brightDiffuseRatio=0.947`, `missingResolveTotal=0`

## Commands
- `apply_patch Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-f2-diagnostic-record-update.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_2039.md`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity.com 起動時に `指定されたモジュールが見つかりません` のため実行不可。
  - `editmode-20260212_203859.log/.xml` は未生成。
- 参考:
  - ユーザー環境での手動実行は `docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_1955.md` にて Success。

## Rationale (Key Points)
- F2値がF0と同傾向であるため、shader cap導入後も hint分類の大枠は維持されている。
- 現時点では方針変更より、F3再採取を含む見た目差分（補正前後）の確認を優先すべき。

## Next Actions
1. Shader補正後の F3・3モデル（kanata/AZKi/nene_BEA）を再採取して `Record Sheet D` を更新。
2. `Record Sheet B` に白飛び/欠け/質感の目視差分を補正前後で追記。
3. 必要なら cap値（specular 0.32/0.45, edge 0.55/0.75）の再調整案を作成。

## Rollback
1. `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md` から `Record Sheet E` を削除。
2. `docs/worklog/2026-02-12_pmx-whiteout-f2-diagnostic-record-update.md` を削除。
3. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_2039.md` を削除。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
