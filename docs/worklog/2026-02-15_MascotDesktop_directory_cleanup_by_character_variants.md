# Worklog: MascotDesktop_directory_cleanup_by_character_variants

- Date: 2026-02-15
- Task: キャラ/バリエーション定義に基づく不要ディレクトリ整理
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: asset-inventory, safe-archive-cleanup, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters, Unity_PJ/data/_cleanup_archive/20260215_0238
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_directory_cleanup_by_character_variants.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0242.md
- Tags: [agent/codex, model/gpt-5, tool/codex, asset-dedup, cleanup, variants]

## Summary
- ユーザー定義（amane+nurse, nene(BEA/STD), azki単体, Sakamata単体）を基準に active 構成を再棚卸しした。
- `SakamataChloe_v1` 内で PMXを持たない重複ディレクトリ `mmd/SakamataChloe` を退避し、不要重複を削減した。

## Changes
1. 認識確認
- `amane_kanata` 整理済み状態を確認（activeは `amane_kanata_v1` のみ）。

2. 退避実施（今回）
- 退避先: `Unity_PJ/data/_cleanup_archive/20260215_0238`
- 退避対象:
  - `Unity_PJ/data/assets_user/characters/SakamataChloe_v1/mmd/SakamataChloe`

3. 維持対象（安全優先）
- `Unity_PJ/data/assets_user/characters/SakamataChloe_v1/mmd/SakamataChloe_src`（PMX実体あり）
- `Unity_PJ/data/assets_user/characters/SakamataChloe_v1/mmd/TEX`（PMX相対参照確証が取れるまで保持）

## Commands
- `Get-ChildItem ...characters -Directory`
- 棚卸しスクリプト（PMX数/重複グループ/同一ハッシュ判定）
- `Move-Item`（Sakamata重複ディレクトリの退避）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_024003.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_024003.xml`

## Rationale (Key Points)
- PMX実体を持たない重複フォルダのみを先に退避し、モデル参照破壊リスクを回避した。
- Sakamataは同名異ハッシュが一部あり、軽率な TEX 統合は表示破綻を起こすため段階対応にした。

## Rollback
- `Unity_PJ/data/_cleanup_archive/20260215_0238/SakamataChloe_v1_mmd_SakamataChloe` を `Unity_PJ/data/assets_user/characters/SakamataChloe_v1/mmd/SakamataChloe` に戻す。

## Next Actions
1. Unity.com で SakamataChloe 表示確認（欠け/崩れ有無）
2. 問題なければ Phase2 で `mmd/TEX` と `mmd/SakamataChloe_src/TEX` の参照実測を取り、片系統化を実施

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
