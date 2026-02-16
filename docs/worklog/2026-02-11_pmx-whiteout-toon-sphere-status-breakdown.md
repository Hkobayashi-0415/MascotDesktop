# Worklog: pmx-whiteout-toon-sphere-status-breakdown

- Date: 2026-02-11
- Task: PMX白飛び調査の継続として toon/sphere 不足理由の診断分解を追加
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md, docs/worklog/2026-02-11_pmx-whiteout-transparent-reason-diagnostics.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-whiteout-toon-sphere-status-breakdown.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1918.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, diagnostics, toon, sphere, texture-status]

## Summary
`avatar.model.material_diagnostics` を拡張し、toon/sphere の missing を `spec/resolve/unknown` に分解、さらに sphere は `not_used` を分離計上できるようにした。これにより、白飛び後に残るグレーアウト/欠けの原因を「実不足」と「未使用」を分離して比較できる。

## Input Observations (from this run)
- SimpleModelLight OFF で白飛びが改善する。
- ただし、テクスチャ不足・グレーアウト・欠けは残る。
- AZKi は比較的正常で、対照モデルとして有効。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - 追加タグ:
    - `MASCOT_TOON_TEX_STATUS`
    - `MASCOT_SPHERE_ADD_TEX_STATUS`
    - `MASCOT_SPHERE_MUL_TEX_STATUS`
  - 追加ステータス:
    - `not_used`
  - toon/sphere について `loaded/missing_spec/missing_resolve/not_used` を材質タグとして設定。
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - toon/sphere status tag の読取を追加。
  - 新規カウント追加:
    - `toonMissingSpecMats`, `toonMissingResolveMats`, `toonMissingUnknownMats`
    - `sphereAddMissingSpecMats`, `sphereAddMissingResolveMats`, `sphereAddMissingUnknownMats`, `sphereAddNotUsedMats`
    - `sphereMulMissingSpecMats`, `sphereMulMissingResolveMats`, `sphereMulMissingUnknownMats`, `sphereMulNotUsedMats`
  - material sample に `toonStatus/sphereAddStatus/sphereMulStatus` を追加。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `CountMissingTextureStatus` 分類テスト追加。
  - `IsTextureStatus` 大文字小文字非依存テスト追加。

## Commands
- `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak2 -Force`
- `Copy-Item Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs.bak2 -Force`
- `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak2 -Force`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-11_pmx-whiteout-toon-sphere-status-breakdown.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1918.md`

## Tests
- EditMode targeted test (`SimpleModelBootstrapTests`) を1回試行。
- 結果: 実行失敗（Unity起動時に `指定されたモジュールが見つかりません`）。
- 判定根拠: PowerShell出力で Unity process 起動失敗。`Unity_PJ/artifacts/test-results/editmode-20260211_191837.log/.xml` は未生成。

## Rationale (Key Points)
- 現在は白飛びと不足テクスチャが混在しているため、修正優先度を誤らないよう不足理由の粒度を上げる必要がある。
- sphere の `not_used` 分離により、欠け/不足の過剰カウントを抑えて比較精度を上げる。
- AZKi を対照にすることで、同じ F0 条件で異常モデルとの差分を機械的に比較できる。

## Next Actions
1. AZKi / かなた / ねね_BEA を `Candidate Mode: Model` + `F0 Baseline` 固定で再採取し、新規カウントを比較表化する。
2. `toonMissingResolveMats` / `sphere*MissingResolveMats` が0のままなら、欠け主因を spec未指定 or shading 経路に寄せて判断する。
3. Light ON/OFF 差分と `highShininessMats` を併記し、Shader補正（ライト寄与抑制）を次修正候補として確定する。

## Rollback
1. `Copy-Item Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak2 Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs -Force`
2. `Copy-Item Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs.bak2 Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -Force`
3. `Copy-Item Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.bak2 Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs -Force`
4. `Remove-Item docs/worklog/2026-02-11_pmx-whiteout-toon-sphere-status-breakdown.md`
5. `Remove-Item D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1918.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
