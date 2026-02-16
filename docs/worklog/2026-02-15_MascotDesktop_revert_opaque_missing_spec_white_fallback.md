# Worklog: MascotDesktop_revert_opaque_missing_spec_white_fallback

- Date: 2026-02-15
- Task: Opaque `missing_spec` 白フォールバック撤回（白化を防止し根本原因観測を優先）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: rollback, minimal-diff, diagnostics-first, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_revert_opaque_missing_spec_white_fallback.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0348.md
- Tags: [agent/codex, model/gpt-5, tool/codex, rollback, missing_spec, nurse_taso]

## Summary
- 直前に導入した `missing_spec + opaque` 白テクスチャ補完は、欠けを白化して根本原因を隠すため撤回した。
- `shadow` 名材質（低alpha）向けの限定フォールバックのみ維持。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `ShouldUseWhiteFallbackMainTexture` を shadow 限定に戻した。
- `opaque_spec_missing` 分岐を削除。
- fallback reason は `optional_shadow_spec_missing` のみを返す構成に整理。

2. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- テスト期待値を修正:
  - `MaterialLoaderWhiteFallback_DoesNotUseOpaqueMissingSpecFallback` は `False` を期待。

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`

## Validation
- 本環境では Unity 実行不可（Unity.exe 起動時モジュール不足）のため、Unity.com 側で再表示確認が必要。

## Next Actions
1. nurse_taso を再表示し、`avatar.model.missing_spec_materials` / `avatar.model.material_diagnostics` を再採取。
2. `mainTex=False` 材質名を確定し、PMX仕様（材質にtexture指定なし）か読込不整合かを判定。
