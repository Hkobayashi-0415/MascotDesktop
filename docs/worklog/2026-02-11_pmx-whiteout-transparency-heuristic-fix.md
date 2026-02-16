# Worklog: pmx-whiteout-transparency-heuristic-fix

- Date: 2026-02-11
- Task: Gameタブ白飛び（Sceneとの差分）に対する透明判定ロジックの見直し
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-11_pmx-runtime-diagnostics-enhancement.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-whiteout-transparency-heuristic-fix.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1625.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, transparency, whiteout]

## Summary
`MaterialLoader` の透明判定が「alpha < 0.99 の画素が1つでもあれば透明」だったため、過剰に Transparent shader が選ばれる可能性があった。判定を「強透明画素数」または「透明画素比率」のしきい値に変更し、Game表示での過剰ブレンド白飛びリスクを下げた。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - 透明判定しきい値を追加:
    - `TransparentAlphaThreshold = 0.95`
    - `StrongTransparentAlphaThreshold = 0.50`
    - `TransparentPixelRatioThreshold = 0.01`
    - `StrongTransparentPixelCountThreshold = 16`
  - `IsTextureTransparent()` を `IsTextureTransparentByPixels()` 呼び出しに変更。
  - `IsTextureTransparentByPixels(Color[] pixels)` を新規追加。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `MaterialLoaderTransparencyHeuristic_IgnoresSparseAlphaNoise` を追加。
  - Reflection 経由で `LibMMD.Unity3D.MaterialLoader.IsTextureTransparentByPixels` を検証。

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `apply_patch` (MaterialLoader.cs)
- `apply_patch` (SimpleModelBootstrapTests.cs)
- `rg -n "IsTextureTransparentByPixels|TransparentPixelRatioThreshold|StrongTransparentPixelCountThreshold" ...`

## Tests
- Added: `MaterialLoaderTransparencyHeuristic_IgnoresSparseAlphaNoise`
- Runtime execution: 未実施（Unity.exe 起動失敗環境）
- 期待値:
  - alphaノイズ1画素程度では透明扱いしない
  - 透明画素が十分多い場合のみ transparent 扱い

## Rationale (Key Points)
- 提供ログで AZKi は `missingMainTex*=0` なのに白飛びしており、テクスチャ解決失敗では説明できない。
- 同時に多くの材質が透明系シェーダを使っており、Game camera での重なりブレンドが白化を起こす仮説と整合する。
- 判定緩和は「本来不透明な材質の透明化」を抑える対症として妥当。

## Rollback
1. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
2. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`

## Next Actions
1. 実機Unityで AZKi/Nurse の再読込比較（Before/After）
2. `avatar.model.render_diagnostics` と `avatar.model.material_diagnostics` を同時採取
3. まだ白飛びが残る場合は shader 側で `toonMissing` 時の係数補正を導入

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
