# Worklog: pmx-whiteout-record-sheet-d-and-interim-direction

- Date: 2026-02-12
- Task: Record Sheet D 記入と次修正方針の暫定判断記録
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update, bug-investigation
- Repo-Refs: AGENTS.md, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0151.md, docs/worklog/2026-02-12_pmx-whiteout-remediation-hint-diagnostics.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-record-sheet-d-and-interim-direction.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1804.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, record-sheet-d, direction]

## Summary
ユーザー提供の `avatar.model.remediation_hint` ログ6件（F0/F3 x 3モデル）を `Record Sheet D` に反映した。記録結果から、白飛び系モデル（かなた/ねね_BEA）で `shader_lighting_candidate` が一貫し、AZKi は `materialloader_threshold_candidate` で対照になっているため、暫定的に Shader 側補正優先と判断した。

## Changes
- 更新: `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - `Record Sheet D` を6行すべて埋めた。
  - `Interim Decision (2026-02-12)` を追記し、優先方針を明記した。

## Evidence Used
- ユーザー実行ログ（会話内共有）
  - F0/F3: `amane_kanata_v1`, `AZKi_4th`, `momone_nene_BEA`
- ユーザーテスト成功記録
  - `docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0151.md`

## Commands
- `Get-Content Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `apply_patch`（Record Sheet D と Interim Decision 追記）
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-record-sheet-d-and-interim-direction.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1804.md`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity 起動時に `指定されたモジュールが見つかりません` で実行不可。
  - 対象実行の `editmode-20260212_180325` / `editmode-20260212_180353` ログ・XMLは未生成。
- 参考:
  - ユーザー環境では `2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0151.md` にて Success を確認済み。

## Rationale (Key Points)
- かなた/ねね_BEAはF0/F3で一貫して `shader_lighting_candidate`。
- AZKiはF0/F3で一貫して `materialloader_threshold_candidate` だが、比較対象として安定表示寄り。
- 3モデルすべてでF3時 `brightDiffuseRatio=0` まで低下しており、照明寄与の支配性を示す。
- よって次の実装修正は Shader 側補正を先行し、MaterialLoader 閾値調整は副次A/Bとして扱うのが妥当。

## Next Actions
1. Shader側補正の最小実装案を作成（specular/edge寄与上限とF0整合を維持）。
2. 補正適用後に F0/F3 x 3モデルを再採取し、`Record Sheet B/D` を差分比較する。
3. AZKiを基準に、質感劣化が許容範囲かを確認する。

## Rollback
1. `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md` の `Record Sheet D` 値と `Interim Decision` を編集前に戻す。
2. `docs/worklog/2026-02-12_pmx-whiteout-record-sheet-d-and-interim-direction.md` を削除する。
3. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1804.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
