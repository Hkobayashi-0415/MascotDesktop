# Worklog: MascotDesktop_multi_model_directory_cleanup_phase1

- Date: 2026-02-15
- Task: 他モデルのディレクトリ整理（Phase1: 未使用系の安全退避）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: asset-inventory, safe-archive-cleanup, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters, Unity_PJ/data/_cleanup_archive/20260215_0229
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_multi_model_directory_cleanup_phase1.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0221.md
- Tags: [agent/codex, model/gpt-5, tool/codex, asset-dedup, cleanup]

## Summary
- `amane_kanata` の整理は既に実行済み（`amane_kanata_official_v1` はアーカイブ退避済み）であることを再確認した。
- 他モデルについては、PMXを持たない未使用系ディレクトリを `_cleanup_archive` へ退避し、走査対象を縮小した。

## Changes
1. 既存整理の再確認
- `amane_kanata` は `amane_kanata_v1` のみが active で、`amane_kanata_official_v1` は既に退避済み。

2. Phase1 退避（未使用系）
- 退避先: `Unity_PJ/data/_cleanup_archive/20260215_0229`
- 退避対象:
  - `Unity_PJ/data/assets_user/characters/amane_kanata_v2_nurse`
  - `Unity_PJ/data/assets_user/characters/momone_nene`
  - `Unity_PJ/data/assets_user/characters/momone_nene_v2`
  - `Unity_PJ/data/assets_user/characters/demo`

3. 未対応（意図的）
- `SakamataChloe_v1` の `mmd/TEX`, `mmd/SakamataChloe/TEX`, `mmd/SakamataChloe_src/TEX` 三重構成は、参照相対パス破壊リスクがあるため Phase2 で別途対応。

## Commands
- `Get-ChildItem ...characters -Recurse -Filter *.pmx`
- 重複棚卸しスクリプト（PMX数/textureDir/同名重複）
- `Move-Item`（4ディレクトリを `_cleanup_archive/20260215_0229` へ）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_021907.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_021907.xml`

## Rationale (Key Points)
- まずは「PMXなし」で実行経路に入らない安全対象のみ整理し、誤破壊リスクをゼロに近づけた。
- Sakamata は重複が多いが、同名異ハッシュも混在するため、即時統合は危険と判断した。

## Rollback
- `Unity_PJ/data/_cleanup_archive/20260215_0229` から各ディレクトリを `Unity_PJ/data/assets_user/characters/` に戻す。

## Next Actions
1. Unity.com で候補モデル一覧に欠落/誤消失がないか確認。
2. `SakamataChloe_v1` は PMX の実参照パスを採取後、`mmd/SakamataChloe_src/TEX` を正本に統一する Phase2 を実施。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
