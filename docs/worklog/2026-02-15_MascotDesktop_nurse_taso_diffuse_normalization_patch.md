# Worklog: MascotDesktop_nurse_taso_diffuse_normalization_patch

- Date: 2026-02-15
- Task: NurseTaso の灰色化補正（PMX Diffuse係数の正規化）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: pmx-binary-edit, root-cause-analysis, asset-normalization, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_nurse_taso_diffuse_normalization_patch.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_1645.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, diffuse, nurse_taso]

## Summary
- `mainTex`/`toon` 欠損は解消済みだが、見た目は灰色。
- PMX解析で白系パーツの `Diffuse` が 0.75〜0.80（靴は0.38）と低く、これが直接減衰要因。
- 該当材質のみ Diffuse RGB を 1.0 に正規化した。

## Evidence
- Before patch:
  - 服/エプロン/スカート/フリル: dmax=0.75
  - グローブ/脚: dmax=0.80
  - 靴: dmax=0.38
- toonMissing は既に 0（fallback適用後）

## Applied Fix
- Target file:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- Backup:
  - `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_diffuse_20260215_164240`
- Patched materials (RGB only, alpha kept):
  - `服`, `エプロン`, `スカート`, `フリル`, `グローブ`, `脚`, `靴`
  - `(r,g,b) -> (1.0,1.0,1.0)`

## Verification
- Re-parse confirmed patched values:
  - all 7 target materials are `(1.00,1.00,1.00,1.00)`

## Rollback
- Restore backup:
  - `Copy-Item "...NurseTaso.pmx.bak_diffuse_20260215_164240" "...NurseTaso.pmx" -Force`

## Next Actions
1. Unityで `NurseTaso.pmx` を reload。
2. 見た目比較（白衣・脚・靴の灰色が解消するか）。
3. 必要なら `髪` だけ 0.8 のまま維持/1.0へ調整を選択。
