# Worklog: MascotDesktop_warning_cleanup_findobject_rsp_lineendings

- Date: 2026-02-16
- Task: コンパイル警告の一次解消（mcs.rsp廃止警告、FindObjectOfType系警告、CS0162、shader改行混在）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/csc.rsp, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AvatarStateController.cs, Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs, Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial-CullBack.shader
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260216_0037.md
- Tags: [agent/codex, model/gpt-5, tool/codex, warning-fix, unity, runtime]

## Summary
- `mcs.rsp` を `csc.rsp` に移行し、廃止警告対象を解消した。
- Runtime配下の `FindObjectOfType` / `FindObjectsOfType` を `FindFirstObjectByType` / `FindObjectsByType` へ置換した。
- `TextureLoader` の `CS0162` 原因（`const false` 分岐）を環境変数読取の `static readonly` へ変更した。
- `MeshPmdMaterial*.shader` 17ファイルの改行を CRLF に統一し、改行混在警告の直接原因を除去した。

## Changes
1. `Unity_PJ/project/Assets/mcs.rsp` -> `Unity_PJ/project/Assets/csc.rsp`
- `mcs.rsp(.meta)` を `csc.rsp(.meta)` にリネーム。
- 内容は `-unsafe` を維持。

2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `FindObjectOfType<T>` を `FindFirstObjectByType<T>` に置換（4箇所）。
- `FindObjectsOfType<Light>()` を `FindObjectsByType<Light>(FindObjectsSortMode.None)` に置換。

3. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AvatarStateController.cs`
- `FindObjectOfType<T>` 2箇所を `FindFirstObjectByType<T>` に置換。

4. `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `FindObjectOfType<RuntimeConfig>()` を `FindFirstObjectByType<RuntimeConfig>()` に置換。

5. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `FindObjectOfType<T>` 8箇所を `FindFirstObjectByType<T>` に置換。

6. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- `EnableTgaPngFallback` を `static readonly` 化。
- `ReadTgaPngFallbackFlag()` を追加し、`MASCOTDESKTOP_PMX_TGA_PNG_FALLBACK` から設定読取。

7. `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterial*.shader`（17ファイル）
- 全ファイルを CRLF 改行へ正規化。

## Commands
- `Get-ChildItem ... -Filter mcs.rsp`
- `Select-String ... FindObjectOfType<|FindObjectsOfType<`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- `Move-Item Unity_PJ/project/Assets/mcs.rsp .../csc.rsp`
- `Move-Item Unity_PJ/project/Assets/mcs.rsp.meta .../csc.rsp.meta`
- shader改行正規化（PowerShell一括変換）
- `Select-String` による置換確認
- 改行混在検出（`(?<!\r)\n`）

## Tests
- Unityコンパイル/テストは未実行（この環境では Unity 起動不可の既知制約）。
- 静的確認:
  - Runtime配下 `FindObjectOfType`/`FindObjectsOfType` ヒット 0
  - `csc.rsp` 存在確認
  - shader改行混在検出 `mixed_count=0`

## Rationale (Key Points)
- 警告群は機能不具合と無関係なものを先に消して、中央線/欠損の観測ノイズを減らす狙い。
- `TextureLoader` の `CS0162` は定数分岐が原因で、可変設定化が最小差分。
- shader改行混在は直近編集で発生しやすく、ビルド時の行番号ずれ要因を排除した。

## Rollback
1. `csc.rsp(.meta)` を `mcs.rsp(.meta)` へ戻す。
2. 置換した `FindFirstObjectByType` / `FindObjectsByType` を元APIへ戻す。
3. `TextureLoader.cs` の `EnableTgaPngFallback` を `const false` に戻し、`ReadTgaPngFallbackFlag` を削除する。
4. shaderファイルを変更前状態へ戻す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. ユーザー環境で再コンパイルし、今回列挙の警告が消えるか確認する。
2. その後、中央線/欠損再発の再現ログ（`avatar.model.material_diagnostics`）を同一モデルで再採取する。
