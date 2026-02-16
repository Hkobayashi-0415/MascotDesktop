# Worklog: MascotDesktop_amane_duplicate_cleanup_and_inputsystem_followup

- Date: 2026-02-15
- Task: Input deprecation対応の実行確認 + amane重複資産の1系統化（走査対象）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: input-migration-verification, asset-dedup, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/ProjectSettings/ProjectSettings.asset, Unity_PJ/project/Packages/manifest.json, Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef, Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs, Unity_PJ/data/assets_user/characters/amane_kanata_v1, Unity_PJ/data/_cleanup_archive/20260215_0222
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_amane_duplicate_cleanup_and_inputsystem_followup.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0159.md
- Tags: [agent/codex, model/gpt-5, tool/codex, inputsystem, asset-dedup, amane]

## Summary
- Input Manager deprecationの解消状態を再確認し、Input System参照の反映を確認した。
- 完全重複だった `amane_kanata_official_v1` を走査対象から退避し、`amane_kanata` のPMX/texture系を実質1系統へ整理した。

## Changes
1. Input deprecation 対応の確認
- `Unity_PJ/project/ProjectSettings/ProjectSettings.asset`
  - `activeInputHandler: 1` を確認
- `Unity_PJ/project/Packages/manifest.json`
  - `com.unity.inputsystem: 1.11.2` を確認
- `Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef`
  - `Unity.InputSystem` 参照を確認
- `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Keyboard.current[Key].wasPressedThisFrame` 実装を確認

2. amane重複資産の整理（走査対象1系統化）
- 退避先: `Unity_PJ/data/_cleanup_archive/20260215_0222`
- 退避内容:
  - `Unity_PJ/data/assets_user/characters/amane_kanata_official_v1`
  - `Unity_PJ/data/assets_user/characters/amane_kanata_v1/texture/TEX_PPT_Face.tga.bak`
- 結果:
  - `assets_user/characters` 配下の `amane_kanata` PMXは `amane_kanata_v1/mmd/amane_kanata.pmx` のみ

## Commands
- `Get-Content ...ProjectSettings.asset | Select-String activeInputHandler`
- `Get-Content ...manifest.json`
- `Get-Content ...MascotDesktop.Runtime.asmdef`
- `Get-Content ...ResidentController.cs`
- `Get-FileHash`（`amane_kanata_v1` と `amane_kanata_official_v1` の同一性確認）
- `Move-Item`（`amane_kanata_official_v1` と `.bak` を `_cleanup_archive` へ退避）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Note: この実行では log/xml は生成されず

## Rationale (Key Points)
- ユーザー要望どおり、白飛びの設定こね回しではなく、重複構成そのものの整理を優先した。
- 完全重複（PMXハッシュ一致）を走査対象外へ退避することで、モデル選択とテクスチャ解決の揺れを下げた。
- 削除ではなく退避方式にして、即時ロールバック可能にした。

## Rollback
1. `Unity_PJ/data/_cleanup_archive/20260215_0222/amane_kanata_official_v1` を `Unity_PJ/data/assets_user/characters/` へ戻す
2. `Unity_PJ/data/_cleanup_archive/20260215_0222/amane_kanata_v1_texture_TEX_PPT_Face.tga.bak` を `Unity_PJ/data/assets_user/characters/amane_kanata_v1/texture/TEX_PPT_Face.tga.bak` へ戻す

## Next Actions
1. Unity.comで `amane_kanata_v1` を再表示し、欠けの再現有無を確認する。
2. 欠けが続く場合は `avatar.model.material_diagnostics` の該当マテリアル名を採取し、transparent判定閾値側（MaterialLoader）を比較検証する。
3. 問題が `amane_kanata` のみで再現するなら、`amane_kanata_v1` を `mmd_pkg` 方式へ段階移行する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
