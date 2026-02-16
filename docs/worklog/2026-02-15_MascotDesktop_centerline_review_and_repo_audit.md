# Worklog: MascotDesktop_centerline_review_and_repo_audit

- Date: 2026-02-15
- Task: 顔中央線の原因レビュー + 作業履歴ベースの全体レビュー評価
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: code-review, bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md, docs/worklog/2026-02-15_MascotDesktop_edge_alpha_transparency_fix_and_targa_warning.md, docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md, docs/worklog/2026-02-12_pmx-whiteout-shader-cap-implementation.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_2355.md
- Tags: [agent/codex, model/gpt-5, tool/codex, review, centerline, diagnostics]

## Summary
- 顔中央線は `MaterialLoader.ResolveTransparentReason` の `edge_alpha` 非透明化方針と、Outlineシェーダ経路の組み合わせが主因候補と判断した。
- 作業履歴と現行コードの整合を確認したところ、`2026-02-12` に記録された shader cap 実装が現行コードに存在せず、テスト期待値との不整合がある。
- 全体レビュー観点では、診断ログの粒度不足（材質実名不在）とノイズログ（`avatar.paths.resolved`）が問題発見経路のボトルネック。

## Changes
- コード変更なし（レビューのみ）。
- 作業記録として本worklogを追加。

## Commands
- `Get-Content .git\HEAD`
- `Get-Content .git\config`
- `Get-ChildItem docs/worklog -File`
- `Select-String Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs ...`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_edge_alpha_transparency_fix_and_targa_warning.md`
- `Get-Content docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md`
- `Get-Content docs/worklog/2026-02-12_pmx-whiteout-shader-cap-implementation.md`

## Tests
- 実行なし（レビューのみ・コード差分なし）。
- 注記: 現行環境では Unity 実行基盤不備の既知履歴があり、実機確認はユーザー環境が前提。

## Rationale (Key Points)
- 現行 `MaterialLoader.cs` は `edge_alpha` 単独を透明化トリガーから外しているため、`DrawEdge` 有効材質が不透明+Outline経路に残る。
- `MeshPmdMaterialVertFrag.cginc` の Outline頂点押し出しは旧実装で、法線差がある継ぎ目に線状アーティファクトを出しやすい。
- `SimpleModelBootstrap` 診断は豊富だが `material.name` が shader名に寄るため材質特定が難しく、修正の検証ループが遅い。
- worklog上は shader cap 実装が記録される一方、現行 `MaterialLoader.cs` / shader `.cginc` に該当コードがなく、履歴と現物の差分管理にリスクがある。

## Rollback
1. 本ファイルを削除する: `Remove-Item docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md`
2. Obsidianログを削除する: `Remove-Item D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_2355.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. `MaterialLoader.ResolveTransparentReason` の `edge_alpha` 取り扱いを feature flag 化し、顔中央線ケースで A/B 比較する。
2. `MaterialLoader.LoadMaterial` で `material.name = mmdMaterial.Name` と `SetOverrideTag("MASCOT_MMD_MATERIAL_NAME", ...)` を付与し、診断ログに材質実名を出す。
3. `SimpleModelBootstrapTests` と `MaterialLoader.cs` の不整合（`Resolve*ContributionCap`）を解消し、履歴差分を一本化する。
