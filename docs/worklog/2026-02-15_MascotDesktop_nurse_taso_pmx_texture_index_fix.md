# Worklog: MascotDesktop_nurse_taso_pmx_texture_index_fix

- Date: 2026-02-15
- Task: NurseTaso.pmx の missing_spec 根本修正（PMX材質の texIdx 補正）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: pmx-binary-inspection, root-cause-analysis, asset-fix, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_nurse_taso_pmx_texture_index_fix.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_1620.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, texture, nurse_taso]

## Summary
- `TextureLoader` 探索不一致ではなく、`NurseTaso.pmx` の材質7件が `texIdx=-1`（未割当）であることを確認。
- PMXを直接補正し、7材質の `texIdx` を設定した。

## Root Cause Evidence
- PMX texture table (7 entries):
  - `textures\\cloth_texture_diffuse.png`
  - `textures\\face.png`
  - `textures\\facetoon.bmp`
  - `textures\\body_texture_diffuse.png`
  - `textures\\epron_texture_diffuse.png`
  - `textures\\hair_tex_diffuse.png`
  - `textures\\skirt_texture_diffuse.png`
- Missingだった材質 (`texIdx=-1`):
  - `舌`, `口`, `歯`, `まつげ`, `眉`, `脚`, `靴`
- つまりフォルダ解決ミスではなく PMX定義欠損。

## Applied Fix
- Target: `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup created:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_20260215_161533`
- Patched texIdx:
  - `舌`: -1 -> 1 (`textures\\face.png`)
  - `口`: -1 -> 1
  - `歯`: -1 -> 1
  - `まつげ`: -1 -> 1
  - `眉`: -1 -> 1
  - `脚`: -1 -> 3 (`textures\\body_texture_diffuse.png`)
  - `靴`: -1 -> 3
- Re-parse result: `MissingCount=0`

## Commands
- PowerShell binary parser/patch script executed inline.
- Re-parse verification executed inline.

## Validation
- Local parsing confirms all 15 materials now have non-negative `texIdx`.
- Unity表示確認はユーザー環境で実施が必要（本環境は Unity 実行不可）。

## Rollback
- `NurseTaso.pmx` をバックアップから復元:
  - `Copy-Item "...NurseTaso.pmx.bak_20260215_161533" "...NurseTaso.pmx" -Force`

## Next Actions
1. Unityで `NurseTaso.pmx` を reload。
2. `avatar.model.missing_spec_materials` が消えているか確認。
3. 見た目が不自然な材質があれば、`舌/口/歯/まつげ/眉/靴` の割当先だけ個別再調整。
