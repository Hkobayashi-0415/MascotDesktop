# Worklog: MascotDesktop_missing_spec_material_names_logging

- Date: 2026-02-15
- Task: nurse_taso診断向け missing_spec マテリアル名ログ追加
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: diagnostics-enhancement, minimal-diff-implementation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_missing_spec_material_names_logging.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0318.md
- Tags: [agent/codex, model/gpt-5, tool/codex, diagnostics, missing_spec]

## Summary
- `mainTex=False + missing_spec` の該当マテリアルを特定しやすくするため、専用ログイベントを追加した。
- 既存の `avatar.model.missing_main_textures` は維持し、互換性を壊さない最小差分で対応した。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `missingMainTextureSpecSamples` を追加
- `missing_spec` 判定時に `material.name`, `shader`, `transparentReason` を収集
- 新ログイベント追加:
  - `avatar.model.missing_spec_materials`
  - message: `missingMainTexSpecMats=<count>, samples=<material(shader=..., reason=...)...>`

## Commands
- `Get-Content ...SimpleModelBootstrap.cs`（該当箇所確認）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_031614.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_031614.xml`

## Rationale (Key Points)
- 提示ログは `missing_resolve=0` で参照解決失敗でないため、`missing_spec` の内訳可視化が次の判断に直結する。
- モデル修正や閾値変更前に、対象マテリアル単位での観測を優先。

## Rollback
- `SimpleModelBootstrap.cs` から `missingMainTextureSpecSamples` と `avatar.model.missing_spec_materials` 出力ブロックを削除。

## Next Actions
1. Unity.comで `nurse_taso` を再表示し、`avatar.model.missing_spec_materials` ログを採取。
2. 取得したマテリアル名に基づき、PMX仕様由来か補正対象かを分類。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
