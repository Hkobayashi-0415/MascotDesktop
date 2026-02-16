# Worklog: unity-test-framework-and-asmdef-fix (MascotDesktop)

- Date: 2026-02-07
- Task: UnityのNUnit未解決コンパイルエラーを、Test Framework導入とasmdef分離で修正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: Unity_PJ/project/Packages/manifest.json, Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef, Unity_PJ/project/Assets/Tests/EditMode/MascotDesktop.Tests.EditMode.asmdef, Assets/Tests/EditMode/AssetPathResolverTests.cs, Assets/Tests/EditMode/ModelFormatRouterTests.cs
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-07_unity-test-framework-and-asmdef-fix_2347_report.md
- Obsidian-Log: 未実施: 本セッションではObsidianログを新規作成していないため
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
Unityコンパイルログの `NUnit` 未解決 (`CS0246`) を根拠に、`com.unity.test-framework` を導入し、`Runtime` と `EditMode Tests` を asmdef で分離した。これにより、テストコードが `Assembly-CSharp` ではなくテストアセンブリとして解決される構成に修正した。

## Changes
- `Unity_PJ/project/Packages/manifest.json` に `com.unity.test-framework: 1.5.1` を追加
- `Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef` を追加
- `Unity_PJ/project/Assets/Tests/EditMode/MascotDesktop.Tests.EditMode.asmdef` を追加

## Commands
- Get-ChildItem -Path "Unity_PJ\project\Packages" -Force | Select-Object Name,Mode
- Get-Content -Path "Unity_PJ\project\Packages\manifest.json"
- Get-Content -Path "Unity_PJ\project\Packages\packages-lock.json"
- apply_patch: Unity_PJ/project/Packages/manifest.json
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef
- apply_patch: Unity_PJ/project/Assets/Tests/EditMode/MascotDesktop.Tests.EditMode.asmdef
- Get-Content -Path "Unity_PJ\project\Assets\Scripts\Runtime\MascotDesktop.Runtime.asmdef"
- Get-Content -Path "Unity_PJ\project\Assets\Tests\EditMode\MascotDesktop.Tests.EditMode.asmdef"

## Tests
- JSON parse check:
  - `manifest.json`: Pass
  - `MascotDesktop.Tests.EditMode.asmdef`: Pass
- Unity batch compile/test: Not Run (この実行環境では Unity CLI 実行不可のため、ユーザー端末で再実行が必要)

## Rationale (Key Points)
- エラー原因は `NUnit` の参照不足であり、`Assets/Tests` が通常アセンブリに混在していたことが直接要因。
- Test Framework package と test asmdef (`optionalUnityReferences: TestAssemblies`) を入れるのが最小かつ標準的な修正。

## Rollback
- 追加/変更ファイルを差し戻す:
  - `Unity_PJ/project/Packages/manifest.json`
  - `Unity_PJ/project/Assets/Scripts/Runtime/MascotDesktop.Runtime.asmdef`
  - `Unity_PJ/project/Assets/Tests/EditMode/MascotDesktop.Tests.EditMode.asmdef`
  - `docs/worklog/2026-02-07_unity-test-framework-and-asmdef-fix_2347.md`
  - `docs/worklog/2026-02-07_unity-test-framework-and-asmdef-fix_2347_report.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- ユーザー端末でUnity batchmodeを再実行し、`NUnit` エラー消失と EditMode テスト実行可否を確認する。
