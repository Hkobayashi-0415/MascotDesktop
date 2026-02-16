# Worklog: MascotDesktop_character_variant_mmd_pkg_migration

- Date: 2026-02-15
- Task: character/variant/mmd_pkg への移行（nurse_tasoをamane配下variant化）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: asset-structure-migration, variant-segmentation, safe-archive-cleanup, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/data/assets_user/characters, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs, Unity_PJ/data/_cleanup_archive/20260215_0310_variant_reorg
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_character_variant_mmd_pkg_migration.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0258.md
- Tags: [agent/codex, model/gpt-5, tool/codex, asset-migration, variants, mmd_pkg]

## Summary
- ディレクトリを `characters/<character_id>/<variant>/mmd_pkg` へ移行した。
- `nurse_taso` は `amane_kanata/nurse_taso_v1` として統合し、同キャラ内のvariant管理に切り替えた。

## Changes
1. 新しい active 構成（PMX）
- `characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx`
- `characters/amane_kanata/nurse_taso_v1/mmd_pkg/mmd/NurseTaso.pmx`
- `characters/momosuzu_nene/bea_v1/mmd_pkg/mmd/PMX/momosuzu_nene_BEA.pmx`
- `characters/momosuzu_nene/std_v1/mmd_pkg/mmd/PMX/momosuzu_nene_STD.pmx`
- `characters/azki/official_v1/mmd_pkg/mmd/AZKi_4th_src/AZKi_4th.pmx`
- `characters/sakamata_chloe/official_v1/mmd_pkg/mmd/SakamataChloe_src/SakamataChloe.pmx`

2. 旧ルートの退避
- 旧ディレクトリ（`*_v1`, `momone_nene_official_v1` など）は
  `Unity_PJ/data/_cleanup_archive/20260215_0310_variant_reorg` へ退避。

3. 既定モデルパス更新
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
  - `modelRelativePath` を
  `characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx` に変更。

4. nurse_taso の扱い
- `nurse_taso` を `amane_kanata` の variant として配置。
- 提示ログの `missing_spec/mainTex=False` は `missing_resolve` ではなく、
  「テクスチャ参照未指定マテリアル」を示す値であり、参照失敗とは別系統と判断。

## Commands
- `Copy-Item`（旧ルート -> 新variant構造）
- `Move-Item`（ネスト解消・旧ルート/不要複製の退避）
- `Get-ChildItem -Recurse -Filter *.pmx`（移行結果検証）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_025529.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_025529.xml`

## Rationale (Key Points)
- 仕様どおり `character_id/variant` へ切ることで、将来の衣装追加時も衝突を局所化できる。
- 同キャラ内の共有texture誤読を防ぐため、variant単位で独立パッケージ化した。

## Rollback
- `Unity_PJ/data/_cleanup_archive/20260215_0310_variant_reorg` の旧ディレクトリを `Unity_PJ/data/assets_user/characters` へ戻す。
- `SimpleModelConfig.cs` の `modelRelativePath` を旧値へ戻す。

## Next Actions
1. Unity.comでモデル候補一覧が新パスで正しく列挙されるか確認。
2. `nurse_taso` の表示確認（欠け箇所が残る場合は対象マテリアル名の採取へ進む）。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
