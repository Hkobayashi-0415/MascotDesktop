# Worklog: Runtime Render Total Review

- Date: 2026-02-10
- Task: 白飛び/テクスチャ不足継続に対するリポジトリ総レビュー（shader/material/texture観点）
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: code-review
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial.shader
  - Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc
  - Unity_PJ/project/Assets/LibMmd/Reader/PmxReader.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs
  - docs/worklog/2026-02-10_runtime-visibility-texture-followup_0405.md
  - docs/reports/2026-02-10_session-handoff-runtime-visibility-texture.md
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-10_runtime-render-total-review_1655.md
- Obsidian-Log: 未実施（レビュー内容を本worklogへ集約）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/review, runtime, pmx, texture, shader]

## Summary

- コード変更は行わず、現行実装とユーザー提示ログを根拠に課題を整理した。
- `missingMainTexResolveMats` が 0 のモデルでも白飛びが発生しており、テクスチャ解決以外（shader/material適用系）の問題が混在している。
- 一方で `missingMainTexSpecMats` が高いモデル（NurseTaso系）は、見た目不良と診断値が一致している。

## Changes

- 変更なし（レビューのみ）。

## Commands

- `Get-Content` / `Select-String`（Runtime, LibMmd, Shader, Reader, docs）
- `Get-ChildItem`（候補ファイル探索）
- `.git/HEAD` / `.git/config` 確認（git不在環境のため）

## Tests

- 実行テスト: 未実施（レビューのみ）
- 静的確認: 実装行・ログ値・既存worklogを突合

## Rationale (Key Points)

- `SimpleModelBootstrap` 診断では `missingMainTexSpec/Resolve/Unknown` の3系統で欠落が分離されるが、unknownが常に1出るモデル群があり、`MmdGameObject` の分割描画構造（root renderer残存）と整合する。
- `TextureLoader` の再帰探索は回復力を上げる一方、`grandParent` 探索 + キャッシュで同名テクスチャ誤解決リスクが残る。
- `MeshPmdMaterialSurface.cginc` の sphere UV 計算は `o.Normal` 依存のため、モデル差による見た目偏差の候補として残る。

## Rollback

- コード変更なしのため不要。

## Next Actions

1. `missingMainTexResolveMats` が 0 でも白飛びするモデル（AZKi等）を shader/material 経路で重点調査する。
2. `missingMainTexSpecMats` が高いモデル（NurseTaso系）は PMX材質仕様/参照の実在確認を優先する。
3. `missingMainTexUnknownMats` のノイズを減らすため、分割描画時の root renderer を診断対象から除外する方針を検討する。

## External-Refs

- https://zenn.dev/hololab/articles/3b8fb92c5971f7 （参考可否の確認対象。実装根拠はリポジトリ内を優先）

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes (n/a)
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
