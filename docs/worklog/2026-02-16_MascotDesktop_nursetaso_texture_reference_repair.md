# Worklog: MascotDesktop_nursetaso_texture_reference_repair

- Date: 2026-02-16
- Task: NurseTaso 白表示箇所の解消（数値調整前にテクスチャ形式/パス/参照を再検証し、PMX参照を修正）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx, Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_diffuse_20260215_164240, docs/worklog/2026-02-15_MascotDesktop_nurse_taso_diffuse_normalization_patch.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_nursetaso_texture_reference_repair.md
- Obsidian-Log: 未実施（このセッションではリポジトリ内修正と検証を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex, nurse_taso, pmx, texture, regression-guard]

## Summary
- テクスチャの「形式不良 / パス不一致 / 実体欠落」は再調査で否定した。
- `NurseTaso.pmx` をバイナリ解析した結果、白表示に寄与しうる 7 材質が `TextureIndex=-1`（main texture 未割当）だった。
- 最小差分として、同梱バックアップ `NurseTaso.pmx.bak_diffuse_20260215_164240` 相当の参照に置換し、7 材質のみ main texture 参照を追加した。
- 既存固定済み状態（toon missing 対策 / `_MascotEdgeContributionCap` 条件 / F0 Baseline数値）はコード無変更で維持。

## Investigation Evidence (Texture First)
1. 実ファイル存在と形式
- `mmd_pkg/mmd/textures` 内の参照対象 7 ファイルは全て存在。
- `System.Drawing` デコード確認: 全ファイル `OK`（PNG/BMP とも読込可能）。

2. PMX参照テーブル（BinaryReader解析）
- `NurseTaso.pmx` の texture table は以下 7 件:
  - `textures\\cloth_texture_diffuse.png`
  - `textures\\face.png`
  - `textures\\facetoon.bmp`
  - `textures\\body_texture_diffuse.png`
  - `textures\\epron_texture_diffuse.png`
  - `textures\\hair_tex_diffuse.png`
  - `textures\\skirt_texture_diffuse.png`
- 全件 `mmd_pkg/mmd` 基点で実在確認 `exists=True`。

3. 問題点（current PMX）
- 以下 7 材質が `TextureIndex=-1`（main texture 未割当）:
  - `舌`, `口`, `歯`, `まつげ`, `眉`, `脚`, `靴`

## Changes
1. `Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- 変更前をセッションバックアップ:
  - `NurseTaso.pmx.bak_session_texturefix_20260216_203642`
- 置換元:
  - `NurseTaso.pmx.bak_diffuse_20260215_164240`
- 反映差分（材質参照のみ 7 件）:
  - `舌`: `-1` -> `textures\\face.png`
  - `口`: `-1` -> `textures\\face.png`
  - `歯`: `-1` -> `textures\\face.png`
  - `まつげ`: `-1` -> `textures\\face.png`
  - `眉`: `-1` -> `textures\\face.png`
  - `脚`: `-1` -> `textures\\body_texture_diffuse.png`
  - `靴`: `-1` -> `textures\\body_texture_diffuse.png`

2. `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texture_reference_repair.md`
- 本記録を新規作成。

## Commands
- `.git` 代替同定:
  - `Get-Content .git/HEAD`
  - `Get-Content .git/config`
- スキル参照:
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `Get-Content D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- 調査:
  - `Get-ChildItem ...nurse_taso_v1/mmd_pkg/mmd -Recurse -File`
  - `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - `Add-Type -AssemblyName System.Drawing` + 画像デコード確認
  - PowerShell `BinaryReader` で PMX texture/material テーブル抽出
- 適用:
  - `Copy-Item NurseTaso.pmx NurseTaso.pmx.bak_session_texturefix_20260216_203642 -Force`
  - `Copy-Item NurseTaso.pmx.bak_diffuse_20260215_164240 NurseTaso.pmx -Force`
- 反映検証:
  - PowerShell `BinaryReader` で before/after material texture diff（7件）確認
- テスト:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. Static/Binary verification
- PMX再パース結果: `diff_count=7`（想定した 7 材質のみ変更）。
- toon参照ロジック・edge cap・lighting設定のコードは未変更（回帰リスク最小化）。

2. Unity EditMode test
- 実行コマンド: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果: 失敗（Unity起動不可）
- エラー: `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
- 生成物: `editmode-20260216_203714.log/.xml` は未作成

## avatar.model.material_diagnostics Before/After
- Before（ユーザー提示）:
  - request_id: `req-01671cdf44e24f74abe5bb5d4bfea622`
  - `toonMissingMats=0`, `toonMissingSpecMats=0`
  - `toonStatus=loaded_fallback_white` を複数材質で確認
  - `brightDiffuseMats=1`
- After（この環境）:
  - Unity 実行不可のため新規採取不可
  - 代替として PMX の材質参照差分は確認済み（未割当7材質 -> texture割当あり）

## Rationale (Key Points)
- ユーザー要求どおり、数値調整の前に texture 固有要因を先に排除した。
- `TextureLoader` / `MaterialLoader` コード改変は既存固定済み状態を壊すリスクが高いため回避。
- モデル固有不具合に対して、モデル資産（PMX参照）を最小差分で修正する方針を採用。

## Risk / Alternatives
- リスク:
  - NurseTaso の材質意図に対して texture 割当が過剰な場合、顔周辺の見え方が変わる可能性。
- 代替案:
  - current PMX のまま、MaterialLoader にモデル依存分岐を追加（今回は保守性悪化のため不採用）。

## Rollback
1. セッション前状態へ戻す:
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_session_texturefix_20260216_203642 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

2. または既知バックアップへ戻す:
- `Copy-Item Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx.bak_20260215_161533 Unity_PJ/data/assets_user/characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx -Force`

## Next Actions
1. ユーザー環境で `NurseTaso.pmx` を reload し、見た目（白表示箇所）を再確認する。
2. `avatar.model.material_diagnostics` を再採取し、Before（`req-01671cdf44e24f74abe5bb5d4bfea622`）との差分を提示する。
3. なお白飛びが残る場合は、材質ごとの `transparentReason` と `samples` を基に透明経路のみ追加切り分けする（数値変更は最後）。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
