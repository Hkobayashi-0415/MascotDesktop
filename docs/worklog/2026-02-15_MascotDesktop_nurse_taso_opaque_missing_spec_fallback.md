# Worklog: MascotDesktop_nurse_taso_opaque_missing_spec_fallback

- Date: 2026-02-15
- Task: nurse_taso の `missing_spec`（Opaque材質）欠け対策として白テクスチャ補完を拡張
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: minimal-diff-implementation, diagnostics-driven-fix, test-augmentation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_nurse_taso_opaque_missing_spec_fallback.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0340.md
- Tags: [agent/codex, model/gpt-5, tool/codex, nurse_taso, missing_spec, materialloader]

## Summary
- `mainTexture == null` かつ `main texture status == missing_spec` のうち、Opaque材質にも白テクスチャ補完を適用するようにした。
- 既存の shadow 用補完は維持した。
- 補完判定のユニットテストを追加した。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `ConfigMaterial` 内の白補完条件を拡張:
  - 変更前: `missing_spec` + `shadow` 名材質（低alpha）
  - 変更後: 上記に加えて `missing_spec` + `transparentReason=opaque` + `diffuse alpha >= 0.9999`
- 追加メソッド:
  - `ShouldUseWhiteFallbackMainTexture(MmdMaterial, string transparentReason)`
  - `ShouldUseShadowWhiteFallback(MmdMaterial)`
  - `IsOpaqueTransparentReason(string)`
  - `ResolveWhiteFallbackMainTextureReason(MmdMaterial, string)`
- トレースログの fallback reason を `optional_shadow_spec_missing` / `opaque_spec_missing` で出力。

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- 追加テスト:
  - `MaterialLoaderWhiteFallback_UsesOpaqueMissingSpecFallback`
  - `MaterialLoaderWhiteFallback_DoesNotUseOpaqueFallbackForTransparentMaterial`
  - `MaterialLoaderWhiteFallback_KeepsShadowFallbackForLowAlphaShadowMesh`
- private helper を reflection で追加:
  - `InvokeMaterialLoaderShouldUseWhiteFallbackMainTexture(MmdMaterial, string)`

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_033523.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_033523.xml`

## Rationale (Key Points)
- 提示ログでは `missing_resolve=0` でパス解決失敗ではなく、`missing_spec` が主要因。
- `NurseTaso.pmx` の欠けは Opaque 材質で `mainTex=False` が複数あるため、最小差分では白テクスチャ補完が副作用を抑えつつ有効。

## Rollback
- `MaterialLoader.cs` の Opaque fallback 分岐を除去し、shadow fallback のみに戻す。
- 追加テスト 3件と helper を削除。

## Next Actions
1. Unity.com で `characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx` を再表示。
2. `avatar.model.missing_spec_materials` の `missingMainTexSpecMats` が減少/解消しているか確認。
3. 見た目欠けが残る場合は、該当材質名を元に PMX 側材質定義を個別確認する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
