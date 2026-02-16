# Worklog: MascotDesktop_missing_spec_shadow_fallback

- Date: 2026-02-15
- Task: `missing_spec=1` 継続時の最小補正（untextured shadow material fallback）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: diagnostics, runtime-fix, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/amane_kanata.pmx
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_missing_spec_shadow_fallback.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0120.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, missing_spec, shadow]

## Summary
- `missing_spec=1` の原因材質を PMX 直接解析で特定した。
- 原因は `MAT_PPT_Hair_Shadow`（textureIndex=-1, diffuseAlpha=0.2, surfaces=564）で、未解決ではなく「仕様上未指定」材質。
- 表示安定化のため、該当パターンに対して `Texture2D.whiteTexture` を mainTex として補完する最小補正を追加した。

## Investigation
- PMX解析結果:
  - materials=16, textures=14
  - missing_main_texture_spec_count=1
  - idx=14, name=`MAT_PPT_Hair_Shadow`, surfaces=564, texIdx=-1, diffA=0.2

## Changes
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  1. `MainTextureStatusLoadedFallbackWhite = "loaded_fallback_white"` を追加
  2. main texture status 判定後、以下条件で white fallback を適用
     - `mainTexture == null`
     - `mainTextureStatus == missing_spec`
     - 材質名に `shadow` を含む
     - `diffuse alpha <= 0.25`
  3. helper `ShouldUseWhiteFallbackMainTexture(MmdMaterial)` を追加

## Commands
- PMX解析（PowerShell BinaryReader）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
- EditMode test command executed
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_012024.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_012024.xml`

## Rationale (Key Points)
- `missing_resolve=0` のため読込失敗ではなく仕様未指定材質が残件。
- `MAT_PPT_Hair_Shadow` は low-alpha shadow overlay と判断でき、white fallback は色/alpha を維持しつつ shader の null-texture 挙動差を回避できる。
- 既存の texture 解決ロジック/材質判定を崩さない最小差分を優先した。

## Rollback
1. `MaterialLoader.cs` の `MainTextureStatusLoadedFallbackWhite` を削除
2. `ShouldUseWhiteFallbackMainTexture` helper を削除
3. `ConfigMaterial` 内 fallback 分岐を削除

## Next Actions
1. Unity実機で `avatar.model.missing_main_textures` が 0 になるか確認。
2. 見た目で灰色欠けが残る場合、`MAT_PPT_Hair_Shadow` の opacity/edge 設定を段階的に評価。
3. `Input Manager` の deprecation 警告は本件と分離して別タスク化。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
