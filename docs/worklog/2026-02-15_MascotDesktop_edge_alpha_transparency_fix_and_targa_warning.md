# Worklog: MascotDesktop_edge_alpha_transparency_fix_and_targa_warning

- Date: 2026-02-15
- Task: edge_alpha単独での透明化抑止 + TargaImage CS0162警告解消
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: rendering-diagnostics, minimal-diff-fix, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_edge_alpha_transparency_fix_and_targa_warning.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0209.md
- Tags: [agent/codex, model/gpt-5, tool/codex, libmmd, transparency, tga]

## Summary
- `MaterialLoader` の透明判定を修正し、`edge_alpha` 単独では Transparent シェーダーに落ちないように変更した。
- `TargaImage.cs` の到達不能コード (`CS0162`) を `#if LIBMMD_VERBOSE_TGA_LOGS` で条件コンパイル化して解消した。

## Changes
1. `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `ResolveTransparentReason` の判定を変更。
- 変更前: `isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha`
- 変更後: `isTransparent = byDiffuseAlpha || byTextureAlpha`
- 補足: `edge_alpha` は「透明化トリガー」ではなく「理由タグ/輪郭寄与」に残す方針。

2. `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs`
- `const bool enableVerboseTgaLogs = false` による到達不能分岐を削除。
- ログ出力を `#if LIBMMD_VERBOSE_TGA_LOGS` ブロックへ置換。

## Commands
- `Get-Content ...MaterialLoader.cs`
- `Get-Content ...TargaImage.cs`
- `Select-String ...TargaImage.cs enableVerboseTgaLogs`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
1. EditMode targeted run
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_020830.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_020830.xml`

## Rationale (Key Points)
- 提供ログでは `missing_main_textures` が主因ではなく、`transparentByEdgeAlphaMats` が高く、`edge_alpha` 過検出が欠けに直結していた。
- 修正はローダーの透明判定1点に限定し、他の質感補正（specular/edge cap）は維持して副作用を最小化した。
- `CS0162` は機能影響がないがビルドノイズになるため、最小差分で整理した。

## Rollback
1. `MaterialLoader.cs` の `ResolveTransparentReason` を旧判定式へ戻す。
2. `TargaImage.cs` の `#if LIBMMD_VERBOSE_TGA_LOGS` ブロックを旧 `enableVerboseTgaLogs` 実装へ戻す。

## Next Actions
1. Unity.com で `amane_kanata_v1` を再表示し、`transparentByEdgeAlphaMats` と欠けの改善有無を確認する。
2. まだ欠ける場合は `transparentByTextureAlphaMats` 対象マテリアルのサンプル名を採取し、閾値調整の必要性を判定する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
