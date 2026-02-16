# Worklog: Safe Mode Compile Fix

- Date: 2026-02-10
- Task: Unity Safe Mode発生（コンパイルエラー）への即時復旧
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-10_safe-mode-compile-fix_1552.md
- Obsidian-Log: 未実施（緊急復旧対応を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, safe-mode, compile]

## Summary

Safe Modeの直接原因は、`MascotDesktop.Runtime` asmdef 参照境界を越えて `SimpleModelBootstrap` が `LibMMD.Unity3D.MaterialLoader` を直接参照したこと。`using LibMMD.Unity3D;` を除去し、必要な判定定数を Runtime 側ローカル定数へ置換して復旧した。

## Changes

- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `using LibMMD.Unity3D;` を削除
  - `MainTextureStatusTag` / `MainTextureStatusMissingSpec` / `MainTextureStatusMissingResolve` をローカル定数として追加
  - `material.GetTag(...)` の参照先を `MaterialLoader` 定数からローカル定数へ変更

## Commands

- `Get-ChildItem -Path Unity_PJ/project/Assets -Recurse -Filter *.asmdef`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef`
- `apply_patch` (SimpleModelBootstrap.cs)
- `Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/**/*.cs -Pattern 'using LibMMD|MaterialLoader\.'`

## Tests

- 静的確認:
  - Runtime配下から `using LibMMD` / `MaterialLoader.` 参照が消えていることを確認
- Unity実行テスト:
  - このセッションでは未実施（Unity.exe 起動環境不備: 指定されたモジュールが見つかりません）

## Rationale (Key Points)

- asmdef 参照が空の `MascotDesktop.Runtime` から default assembly 側の型へ直接参照するとコンパイル不能になる。
- 状態文字列はランタイム診断用途のため、依存を増やさず文字列定数を共通化して運用可能。

## Next Actions

1. Unityを再オープンし Safe Mode で再コンパイル実行。
2. Console に残るエラーがあれば先頭3件（ファイル/行番号）を採取。
3. Play Mode で `avatar.model.render_diagnostics` と `avatar.model.missing_main_textures` を確認。

## Rollback

1. `SimpleModelBootstrap.cs` のローカル定数追加と参照置換を戻す。
2. `using LibMMD.Unity3D;` を復元（ただし再び asmdef 境界エラーになる可能性あり）。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
