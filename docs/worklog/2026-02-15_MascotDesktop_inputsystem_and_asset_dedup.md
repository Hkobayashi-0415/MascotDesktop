# Worklog: MascotDesktop_inputsystem_and_asset_dedup

- Date: 2026-02-15
- Task: Input Manager deprecation解消 + 重複ディレクトリ整理（B方針の段階適用）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: input-migration, asset-dedup, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/ProjectSettings/ProjectSettings.asset, Unity_PJ/project/Packages/manifest.json, Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef, Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs, Unity_PJ/data/assets_user/characters/nurse_taso_v1, Unity_PJ/data/assets_user/characters/azki_v1
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_inputsystem_and_asset_dedup.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0155.md
- Tags: [agent/codex, model/gpt-5, tool/codex, inputsystem, asset-dedup]

## Summary
- Input deprecation対応として、プロジェクトを新Input System専用へ切り替え、`ResidentController` を InputSystem API に移行した。
- 重複PMX/テクスチャの構造整理として、`nurse_taso_v1` と `azki_v1` の重複内容を `assets_user` 走査対象外へアーカイブ退避した。

## Changes
1. Input deprecation対応
- `Unity_PJ/project/ProjectSettings/ProjectSettings.asset`
  - `activeInputHandler: 0 -> 1`
- `Unity_PJ/project/Packages/manifest.json`
  - `com.unity.inputsystem: 1.11.2` を追加
- `Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef`
  - 参照に `Unity.InputSystem` を追加
- `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Input.GetKeyDown(KeyCode)` を廃止
  - `Keyboard.current[Key].wasPressedThisFrame` へ移行

2. ディレクトリ整理（B方針の段階適用）
- アーカイブ先（走査対象外）:
  - `Unity_PJ/data/_cleanup_archive/20260215_015409`
- 退避した重複ディレクトリ:
  - `Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/NurseTaso_src_mmd`
  - `Unity_PJ/data/assets_user/characters/azki_v1/mmd/AZKi_4th`
- 退避した重複ファイル:
  - `Unity_PJ/data/assets_user/characters/azki_v1/mmd/AZKi_Body.png`
  - `Unity_PJ/data/assets_user/characters/azki_v1/mmd/AZKi_Face.png`
  - `Unity_PJ/data/assets_user/characters/azki_v1/mmd/AZKi_Hair.png`

## Post-state (Observed)
- `nurse_taso_v1`: `pmx=1`, `textureDirs=1`, `dupNameGroups=0`
- `azki_v1`: `pmx=1`, `textureDirs=0`, `dupNameGroups=0`
- `assets_user/characters` 配下 PMX は 7件（重複NurseTaso除去後）

## Commands
- `Get-Content ...ResidentController.cs`
- `Select-String ...ProjectSettings.asset activeInputHandler`
- `Select-String ...Assets/Scripts ... Input.Get`
- `Move-Item`（重複ディレクトリ/ファイルのアーカイブ退避）
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. Input変更後
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_015320.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_015320.xml`

2. 資産整理後
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（同上）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_015447.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_015447.xml`

## Rationale (Key Points)
- deprecation警告の直接原因は `activeInputHandler=0` と `Input.GetKeyDown` 利用。
- 重複除去は削除より安全な退避を採用（復旧容易、`assets_user` 走査から除外）。
- PMX参照を壊さないよう、正本ディレクトリは維持し、同一ハッシュ重複のみ処理した。

## Rollback
1. Input
- `ProjectSettings.asset` の `activeInputHandler` を元に戻す
- `manifest.json` から `com.unity.inputsystem` を削除
- `MascotDesktop.Runtime.asmdef` から `Unity.InputSystem` 参照を削除
- `ResidentController.cs` を旧 `Input.GetKeyDown` 実装へ戻す
2. Asset
- `Unity_PJ/data/_cleanup_archive/20260215_015409` から元パスへ戻す

## Next Actions
1. Unity側で deprecation 警告消失を確認する。
2. `F10/F12` のホットキー動作（常駐切替/終了）を実機確認する。
3. 次段で A方針（`mmd_pkg`）へ移行する場合、まず `nurse_taso_v1` で試験導入し、ローダー走査対象を限定する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
