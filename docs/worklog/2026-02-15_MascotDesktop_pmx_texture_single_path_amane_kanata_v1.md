# Worklog: MascotDesktop_pmx_texture_single_path_amane_kanata_v1

- Date: 2026-02-15
- Task: PMX参照先に合わせたテクスチャ配置1系統化（amane_kanata_v1）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: asset-layout-normalization, diagnostics, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/amane_kanata.pmx, Unity_PJ/data/assets_user/characters/amane_kanata_v1/texture, Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_pmx_texture_single_path_amane_kanata_v1.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0111.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, texture, asset-layout]

## Summary
- `amane_kanata_v1` のテクスチャ配置を `texture/` 正本の1系統に統一した。
- `mmd/texture` と `mmd` 直下の重複テクスチャを除去し、`mmd` 直下は `amane_kanata.pmx` のみとした。
- 欠けていた `TEX_PPT_Face.tga` を `texture/` に配置し、`..\texture\...` 参照を満たす状態にした。

## Changes
1. 移動
- `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/texture/TEX_PPT_Face.tga`
  -> `Unity_PJ/data/assets_user/characters/amane_kanata_v1/texture/TEX_PPT_Face.tga`

2. 削除（重複・ハッシュ一致）
- `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/texture/*`（全テクスチャ重複）
- `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/S_metal.png`
- `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/TEX_PPT_Hyojo.png`
- 空ディレクトリ `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/texture`

3. 構成結果
- `Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd` には `amane_kanata.pmx` のみ
- テクスチャは `Unity_PJ/data/assets_user/characters/amane_kanata_v1/texture` に集約
- テクスチャ重複グループ: `14 -> 0`

## Commands
- 重複調査/一覧:
  - `Get-ChildItem ... -Recurse -Directory`
  - `Group-Object Name | Where-Object { $_.Count -gt 1 }`
- ハッシュ確認:
  - `Get-FileHash -Algorithm SHA256`
- 移動/削除:
  - `Move-Item`
  - `Remove-Item`
- 変更後検証:
  - `Test-Path .../texture/TEX_PPT_Face.tga`
  - `Test-Path .../mmd/texture`

## Tests
1. EditMode test
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: 実行環境制約で Unity 起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_011052.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_011052.xml`

## Rationale (Key Points)
- PMX基準ディレクトリ（`mmd`）からの `..\texture\...` 参照を優先し、正本を `texture/` に統一。
- `mmd` 側重複を残すと、将来の探索順変更やフォールバックで再び揺らぐため除去。
- ファイル削除は全てハッシュ一致を確認した重複のみ実施。

## Rollback
1. 必要ファイルを `texture/` から `mmd/texture` に戻す。
2. `mmd` 直下に `S_metal.png` / `TEX_PPT_Hyojo.png` を復元。
3. `mmd/texture` ディレクトリを再作成し復元。

## Next Actions
1. Unityで再読み込みし、`[TextureLoader] resolve fallback` が減ることを確認する。
2. かなた表示時に灰色化/欠けが残る場合、`avatar.model.missing_main_textures` の samples を再採取する。
3. 同様の1系統化を `amane_kanata_official_v1` に適用するか判断する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
