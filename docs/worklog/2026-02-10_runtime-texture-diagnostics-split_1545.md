# Worklog: Runtime Texture Diagnostics Split

- Date: 2026-02-10
- Task: `1.2.3` 実行（texture trace実行確認 / 欠落材質ログ強化 / render diagnostics分離）
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - tools/run_unity.ps1
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-10_runtime-texture-diagnostics-split_1545.md
- Obsidian-Log: 未実施（本セッションは実装とリポジトリ内記録を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, pmx, diagnostics]

## Summary

ユーザー指定の `1.2.3` を実行。`1)` は `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1` を付与して `tools/run_unity.ps1` を起動したが、Unity実行環境の欠落（`Unity.exe` 起動時「指定されたモジュールが見つかりません」）で停止。`2)` `3)` はコード反映完了。

## Changes

- `MaterialLoader`:
  - main texture 状態タグを追加: `loaded / missing_spec / missing_resolve`。
  - `material.SetOverrideTag("MASCOT_MAIN_TEX_STATUS", ...)` を付与。
  - `MMD main/sub/toon texture missing` ログに `reason=` を追加。
  - `requested=(none)` の spec欠落ケースを texture trace時に記録。
- `SimpleModelBootstrap`:
  - `avatar.model.render_diagnostics` に内訳追加:
    - `missingMainTexSpecMats`
    - `missingMainTexResolveMats`
    - `missingMainTexUnknownMats`
  - 欠落サンプル材質名を `avatar.model.missing_main_textures` として追加出力。

## Commands

- `Get-Command Unity -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source`
- `Get-Command Unity.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source`
- `Get-Content tools/run_unity.ps1 -TotalCount 260`
- `$env:MASCOTDESKTOP_PMX_TEXTURE_TRACE='1'; ./tools/run_unity.ps1 -ExecuteMethod MascotDesktop.Editor.Automation.Ping`
- `Select-String` / `Get-Content` による差分確認
- `apply_patch`（2ファイル）

## Tests

- Unity実行テスト（Step 1）:
  - 実行: `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1` で `tools/run_unity.ps1` 実行
  - 結果: 失敗（`Unity.exe` 起動時 `指定されたモジュールが見つかりません`）
  - 補足: 新規 Unity log (`unity-20260210_154030.log`) は生成されず
- 静的確認（Step 2/3）:
  - `MaterialLoader` の新規タグ/ログ文言追加を確認
  - `SimpleModelBootstrap` の分離カウンタと追加イベントを確認

## Rationale (Key Points)

- `missingMainTexMats` 単一値では、PMX仕様上の未指定と解決失敗を区別できないため、分離指標を追加した。
- 材質名サンプルを同時出力することで、次セッションで対象材質の特定を1回で行えるようにした。
- 既存の `missingMainTexMats` は維持し、既存監視との互換を確保した。

## Next Actions

1. Unity実行環境復旧後に `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1` で再実行し、`avatar.model.missing_main_textures` を採取する。
2. `missingMainTexResolveMats > 0` のモデルのみ、`MMD ... texture missing` の `requested=` を突合する。
3. `missingMainTexResolveMats=0` で白飛びが残るモデルは shader/property 側へ進む。

## Rollback

1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs` の texture status tag と reason付きログ差分を戻す。
2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` の分離カウンタ・追加イベント差分を戻す。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
