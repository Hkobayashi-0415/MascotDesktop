# Worklog: pmx-whiteout-texture-diagnostics-fix

- Date: 2026-02-11
- Task: PMX表示の白飛び・テクスチャ不足の原因別切り分けと恒久対処（TextureLoader / Bootstrap診断 / Shader / 回帰テスト）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-whiteout-texture-diagnostics-fix.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_0247.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, texture, shader]

## Summary
PMX白飛びと missing_main_texture 診断のズレを分離するため、TextureLoaderの同名再帰探索を model近傍優先に改修し、SimpleModelBootstrapで root renderer 起因の unknown ノイズを除去した。MMD shader は specular 経路を安全側に最小調整し、診断/候補選定の回帰テストを追加した。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - 再帰探索の候補選定を `ScoreTexturePathForRequest` ベースへ変更（model近傍、requested path hint、texture/textures優先）。
  - rootごとの早期returnを廃止し、再帰roots全体で最良候補を選択。
  - キャッシュキーに `modelBaseDir` と `requestedPath` を含め、誤キャッシュを抑止。
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `ShouldSkipRendererForMissingMainTextureDiagnostics` を追加し、rootの空 `SkinnedMeshRenderer` を missing-main-texture 集計から除外。
  - `ShouldInspectMainTextureMaterial` を追加し、`_MainTex` を持たず status tag もない材質を missing-main-texture 集計対象外へ。
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
  - specularStrength/lightStrength の `saturate` 化。
  - shininess を `max(1.0, _Shininess)` で下限化。
  - dirSpecular に specularStrength を反映し、specular寄与を 0.65 倍で抑制。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - root renderer除外条件の回帰テスト追加。
  - material診断対象判定の回帰テスト追加。
  - TextureLoaderスコアの model近傍優先/texture folder優先テスト追加（reflection経由）。

## Commands
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `rg --files | rg "TextureLoader.cs|SimpleModelBootstrap.cs|MeshPmdMaterialSurface.cginc|SimpleModelBootstrapTests.cs|pmx-validation-procedure-and-record.md"`
- `Get-Content D:\dev\00_repository_templates\ai_playbook\skills\bug-investigation\SKILL.md`
- `Get-Content D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md`
- `Get-Content D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md`
- `apply_patch` (4 files)
- `& "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe" -batchmode -nographics -projectPath d:\dev\MascotDesktop\Unity_PJ\project -runTests -testPlatform EditMode -testFilter MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests ...`

## Tests
- Unity EditMode (filtered): `MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests`
- Result: **Failed to start Unity process**
  - Error: `Unity.exe` 起動時に `指定されたモジュールが見つかりません`（ResourceUnavailable）
  - Test XML: not generated
  - Log: process start failure before run

## Rationale (Key Points)
- `missingMainTexUnknownMats` の1件ノイズは、分割描画時に残る root `SkinnedMeshRenderer` の空Rendererを集計していた可能性が高い。
- 同名テクスチャの誤解決は、再帰探索時に folder種別優先のみで距離優先が弱いことが原因になり得るため、model近傍優先の合成スコアに変更。
- 白飛び対策は可逆最小差分を優先し、specular経路の安全化に限定した。

## Rollback
- 逆順ロールバックを想定:
  1. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  2. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
  3. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  4. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- まず shader のみ切り戻し、診断改善を残す段階的ロールバックを許容。

## Next Actions
- Unity実行環境修復後に EditMode test を再実行し、追加4テストの通過を確認。
- 8 PMX（F0固定）で `avatar.model.render_diagnostics` / `avatar.model.missing_main_textures` / スクリーンショットの Before/After 比較を実施。
- 白飛び主要3モデルで `resolve/spec/shader` の原因確定を記録。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
