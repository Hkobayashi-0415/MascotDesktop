# Worklog: MascotDesktop_fix_review_findings_1_to_4

- Date: 2026-02-16
- Task: レビュー指摘 1-4（中央線要因・実装不整合・診断識別性・ログノイズ）を同時解消
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: code-review, bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline-CullBack.shader, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260216_0021.md
- Tags: [agent/codex, model/gpt-5, tool/codex, fix, diagnostics, rendering]

## Summary
- `MaterialLoader` に edge_alpha 透明化方針の切替フラグを追加し、既定値を「透明化に含める」に戻した。
- shader cap 系メソッド (`ResolveSpecularContributionCap`, `ResolveEdgeContributionCap`) と shader 側プロパティ/適用処理を復元し、テスト期待値との不整合を解消した。
- 材質実名タグ (`MASCOT_MMD_MATERIAL_NAME`) を付与し、`SimpleModelBootstrap` の診断サンプルでタグ名優先表示へ変更した。
- `DiscoverAllRelativeAssetPaths` 経由の `avatar.paths.resolved` を抑止できるようにして、HUD再走査時の重複ノイズを低減した。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- 追加:
  - `MmdMaterialNameTag`
  - edge_alpha 透明化切替 env var: `MASCOTDESKTOP_PMX_EDGE_ALPHA_SURFACE_TRANSPARENCY`
  - cap 関連定数/メソッド:
    - `ResolveSpecularContributionCap(MmdMaterial)`
    - `ResolveEdgeContributionCap(MmdMaterial)`
  - `GetSafeMaterialName(...)`
- 変更:
  - `ResolveTransparentReason` の surface透明条件に `TreatEdgeAlphaAsSurfaceTransparency` を反映。
  - `ConfigMaterial` で材質名タグ付与/材質名設定。
  - cap プロパティ（`_MascotSpecularContributionCap`, `_MascotEdgeContributionCap`）を `SetFloat`。

2. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- `_MascotSpecularContributionCap` を追加し、specular加算に cap を適用。

3. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- `_MascotEdgeContributionCap` を追加し、outline alpha を `min(..., cap)` で制限。

4. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial*.shader`（17ファイル）
- `Properties` に `_MascotSpecularContributionCap` を追加。
- outline系には `_MascotEdgeContributionCap` も追加。

5. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `MASCOT_MMD_MATERIAL_NAME` タグ定数を追加。
- `GetMaterialDiagnosticName(Material)` を追加し、診断で材質タグ名優先化。
- `missingMainTexture*Samples` / `BuildMaterialDiagnosticSample` を材質タグ名対応へ変更。
- `ResolveProjectRoots` に `logResolvedInfo` 引数を追加し、`DiscoverAllRelativeAssetPaths` からは `false` 指定でノイズ抑制。

6. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `DiagnosticsHelpers_GetMaterialDiagnosticName_PrefersTaggedMmdName`
  - `DiagnosticsHelpers_GetMaterialDiagnosticName_FallsBackToMaterialName`
- 既存 cap テスト前提（`ResolveSpecularContributionCap` / `ResolveEdgeContributionCap`）と実装の整合を回復。

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `Get-ChildItem Unity_PJ/project/Assets/LibMmd/Resources/Shaders -Filter MeshPmdMaterial*.shader`
- shader properties 一括追記（PowerShell置換）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 失敗（Unity 起動不可）
- Error: `Unity.exe ... 指定されたモジュールが見つかりません`
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260216_002049.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260216_002049.xml`

## Rationale (Key Points)
- 中央線課題は `edge_alpha` と outline 経路の相互作用が大きく、既定値を「edge_alphaもsurface透明に含める」へ戻しつつ、env var で再比較可能にした。
- cap メソッド不在によりテストが壊れていたため、実装・shader・テスト期待値を同時に揃えた。
- 診断ログの材質識別は shader名依存だと切り分け効率が悪いため、MMD材質名タグをソースオブトゥルースにした。
- `avatar.paths.resolved` の重複は観測ノイズになるため、資産探索経路からの出力のみ抑制した。

## Rollback
1. `MaterialLoader.cs` の以下を戻す:
- `TreatEdgeAlphaAsSurfaceTransparency` 追加分
- `MmdMaterialNameTag` / cap 関連定数・メソッド・`SetFloat`
2. `MeshPmdMaterialSurface.cginc` の `_MascotSpecularContributionCap` 適用を戻す。
3. `MeshPmdMaterialVertFrag.cginc` の `_MascotEdgeContributionCap` 適用を戻す。
4. `MeshPmdMaterial*.shader` 追加プロパティを削除する。
5. `SimpleModelBootstrap.cs` の `GetMaterialDiagnosticName` と `ResolveProjectRoots(..., logResolvedInfo)` 変更を戻す。
6. `SimpleModelBootstrapTests.cs` の材質名診断テスト2件を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. ユーザー環境で `amane_kanata` を再表示し、中央線の改善有無を確認する。
2. A/B比較時は `MASCOTDESKTOP_PMX_EDGE_ALPHA_SURFACE_TRANSPARENCY=0/1` を切り替えて `avatar.model.material_diagnostics` を採取する。
3. Unity実行環境が復旧したら `SimpleModelBootstrapTests` を再実行し、結果をこのworklogへ追記する。
