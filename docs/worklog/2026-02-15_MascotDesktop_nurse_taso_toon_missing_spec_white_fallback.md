# Worklog: MascotDesktop_nurse_taso_toon_missing_spec_white_fallback

- Date: 2026-02-15
- Task: nurse_taso のグレー化対応（toon missing_spec の白フォールバック）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: root-cause-analysis, shader-material-fallback, pmx-inspection, test-update, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_nurse_taso_toon_missing_spec_white_fallback.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_1630.md
- Tags: [agent/codex, model/gpt-5, tool/codex, nurse_taso, toon, fallback]

## Summary
- `NurseTaso.pmx` の main texture 未割当は解消済みだが、見た目がグレー化。
- PMX解析で 14/15 材質が `toon=missing_spec`（未指定）と判明。
- Toon未指定時に `_ToonTex` が暗く効くケースを防ぐため、`missing_spec` のみ白 Toon を補完。

## Root Cause Evidence
- PMX materials:
  - `顔` だけ `toon=tex[2]=textures\\facetoon.bmp`
  - それ以外 14 材質が `toon=tex[-1]`
- `missing_spec_materials` が出ない一方で全体が灰色なのは Toon 側未指定の影響と整合。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- Toon設定時に以下を追加:
  - `toonTexture == null && toonTextureStatus == missing_spec` の場合、`Texture2D.whiteTexture` を補完
  - Toon status を `loaded_fallback_white` に更新
- 追加メソッド:
  - `ShouldUseWhiteFallbackToonTexture(string)`
  - `IsTextureStatus(string,string)`

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `MaterialLoaderToonFallback_UsesWhiteFallbackWhenMissingSpec`
  - `MaterialLoaderToonFallback_DoesNotUseWhiteFallbackWhenResolveMissing`
- reflection helper 追加:
  - `InvokeMaterialLoaderShouldUseWhiteFallbackToonTexture`

## Commands
- PMX 解析（PowerShell inline parser）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_162825.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_162825.xml`

## Next Actions
1. Unityで `NurseTaso.pmx` を reload。
2. 見た目比較（正解例と同等まで白化・灰化が解消されるか）。
3. `avatar.model.material_diagnostics` で `toonMissingSpecMats` が低下/解消しているか確認。
