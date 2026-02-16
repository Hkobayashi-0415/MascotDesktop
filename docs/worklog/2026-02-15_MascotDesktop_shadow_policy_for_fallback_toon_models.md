# Worklog: MascotDesktop_shadow_policy_for_fallback_toon_models

- Date: 2026-02-15
- Task: fallback toon モデルの灰色化対策を状態ベースポリシーへ変更
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: strategy-change, state-based-policy, runtime-render-pipeline-adjustment, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_1702.md
- Tags: [agent/codex, model/gpt-5, tool/codex, rendering-policy, fallback-toon]

## Strategy Change
- これまでの材質数値調整中心の対応を停止。
- `toonStatus=loaded_fallback_white` の存在をトリガーに、影の破綻を防ぐポリシーへ切替。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 追加定数:
  - `MainTextureStatusLoadedFallbackWhite = "loaded_fallback_white"`
- 表示フローに追加:
  - `ApplyModelShadowPolicy(result.Root, requestId, absolutePath, sourceTier)`
- 追加メソッド:
  - `ApplyModelShadowPolicy(...)`
    - `ToonTextureStatusTag == loaded_fallback_white` 材質が1つ以上あれば、モデル全Rendererで
      - `shadowCastingMode = Off`
      - `receiveShadows = false`
    - ログ出力:
      - `avatar.model.shadow_policy_applied`
  - `CountMaterialsWithTextureStatus(...)`

## Why
- 今回は `missing_main_textures` ではなく `toon fallback` に寄る破綻。
- fallback toon は非作者意図の簡易状態のため、影を維持すると色再現が崩れやすい。
- しきい値の微調整ではなく、状態ベースの一貫ルールにした。

## Validation
- Unity 実行確認はユーザー環境で実施（本環境では Unity.exe 起動不可）。
- 確認ログ:
  - `avatar.model.shadow_policy_applied`
  - `avatar.model.material_diagnostics`

## Next Actions
1. `NurseTaso.pmx` を reload。
2. `avatar.model.shadow_policy_applied` が出ることを確認。
3. 見た目比較し、まだ不一致なら「影以外（PMX UV/材質割当）」へ切り分け。
