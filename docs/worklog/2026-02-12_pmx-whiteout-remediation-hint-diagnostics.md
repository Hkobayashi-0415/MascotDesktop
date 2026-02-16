# Worklog: pmx-whiteout-remediation-hint-diagnostics

- Date: 2026-02-12
- Task: PMX白飛び調査の次判断向け remediation hint 診断追加と検証手順書更新
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md, docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0129.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:\dev\00_repository_templates\ai_playbook\skills\phase-planning\SKILL.md, D:\dev\00_repository_templates\ai_playbook\skills\bug-investigation\SKILL.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-remediation-hint-diagnostics.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0147.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, diagnostics, remediation-hint]

## Summary
`avatar.model.material_diagnostics` の詳細値を「次の修正方針判断」に直接使えるよう、`avatar.model.remediation_hint` を追加した。`MaterialLoader` 閾値再調整と Shader側補正のどちらを優先するかを、モデルごとの定量値（ratio/share）で比較可能にした。

## Changes
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `avatar.model.remediation_hint` ログを追加。
  - 出力項目: `hint`, `transparentRatio`, `textureAlphaShare`, `edgeAlphaShare`, `highShininessRatio`, `brightDiffuseRatio`, `missingResolveTotal`。
  - 追加ヘルパー: `ComputeRatio(...)`, `BuildRemediationHint(...)`。
  - hint 判定:
    - `asset_resolution_first`
    - `materialloader_threshold_candidate`
    - `shader_lighting_candidate`
    - `mixed_followup`
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `BuildRemediationHint` の判定テスト5件を追加。
- `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - Procedure に `avatar.model.remediation_hint` 採取要件を追記。
  - `Record Sheet D`（Diagnostic Snapshot）を追加。

## Commands
- `Get-Content AGENTS.md`
- `Get-Content docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md`
- `Get-Content Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Select-String Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `Select-String Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-remediation-hint-diagnostics.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0147.md`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity 起動時に `指定されたモジュールが見つかりません` で失敗。
  - 対象実行の成果物 `Unity_PJ/artifacts/test-results/editmode-20260212_014744.log/.xml` は未生成。
- 補足:
  - 直近のユーザー実行結果（`docs/worklog/2026-02-12_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Success_0129.md`）は pass を確認済み。

## Rationale (Key Points)
- 既存観測では Light ON/OFF で白飛びが改善し、`missingMainTexResolveMats=0` が継続しているため、単純な解決失敗より材質・シェーダ経路の寄与が疑わしい。
- ただし `transparentMats` 単独では反例があり、次判断には「透明理由の内訳比率 + 輝度寄与比率 + resolve欠損有無」を同時に見る必要がある。
- 上記を1イベントで固定出力することで、F0/F3 とモデル間比較を同一軸で実施できる。

## Next Actions
1. ユーザー環境で `SimpleModelBootstrapTests` を Unity.com で再実行し、追加5テストを含む pass を確認する。
2. `Candidate Mode: Model` 固定で `amane_kanata_v1` / `AZKi_4th` / `momone_nene_BEA` を F0・F3 で再採取し、`Record Sheet D` を埋める。
3. 判定ルール:
   - `hint=shader_lighting_candidate` が白飛びモデルで優勢なら Shader側補正を優先。
   - `hint=materialloader_threshold_candidate` が優勢なら MaterialLoader 閾値A/Bを優先。
   - `hint=asset_resolution_first` が出る場合はパス解決側を優先。

## Rollback
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` から `avatar.model.remediation_hint` 出力と `ComputeRatio` / `BuildRemediationHint` を削除する。
2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` の `BuildRemediationHint` テスト5件と `InvokeBuildRemediationHint` を削除する。
3. `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md` の `avatar.model.remediation_hint` 採取行と `Record Sheet D` を削除する。
4. `docs/worklog/2026-02-12_pmx-whiteout-remediation-hint-diagnostics.md` を削除する。
5. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0147.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
