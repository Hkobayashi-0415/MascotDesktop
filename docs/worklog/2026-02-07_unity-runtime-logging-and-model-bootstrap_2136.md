# Worklog: unity-runtime-logging-and-model-bootstrap (MascotDesktop)

- Date: 2026-02-07
- Task: 原因追跡可能な実行ログ基盤と、簡易モデル表示までのUnity最小実装
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs, Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs, Unity_PJ/docs/NEXT_TASKS.md, Unity_PJ/docs/05-dev/asset-path-read-test-design.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-07_unity-runtime-logging-and-model-bootstrap_2136_report.md
- Obsidian-Log: 未実施: 本セッションではObsidian配下への新規ログ作成を実施していないため
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
Unity runtime向けに、`request_id` と `error_code` を含む検出可能な構造化ログ出力を追加した。合わせて asset path 解決ルールと EditMode テストを実装し、最小表示として画像平面またはプレースホルダ形状が自動表示されるブートストラップを追加した。

## Changes
- `RuntimeLog` を追加し、JSONログを Console と `persistentDataPath/logs/runtime.jsonl` に出力
- `AssetPathResolver` を追加し、絶対パス禁止・トラバーサル禁止・legacy禁止・non-ASCII警告・tier解決を実装
- `SimpleModelBootstrap` / `SimpleModelConfig` を追加し、起動時に簡易表示を自動生成
- `AssetPathResolverTests`（EditMode）を追加
- `Unity_PJ/docs/NEXT_TASKS.md` を更新して進捗反映

## Commands
- Get-ChildItem -Path "Unity_PJ\project\Assets" -Recurse -Force | Select-Object FullName,PSIsContainer
- Get-Content -Path "Unity_PJ\docs\05-dev\asset-path-read-test-design.md"
- Get-Content -Path "Unity_PJ\docs\NEXT_TASKS.md"
- Get-Command Unity.exe -ErrorAction SilentlyContinue | Select-Object Source,Name
- Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Directory -ErrorAction SilentlyContinue | Select-Object FullName
- Test-Path -Path "Unity_PJ\project\Assets\Tests\EditMode\AssetPathResolverTests.cs"
- Test-Path -Path "Unity_PJ\project\Assets\Scripts\Runtime\Avatar\SimpleModelBootstrap.cs"
- Test-Path -Path "Unity_PJ\project\Assets\Scripts\Runtime\Assets\AssetPathResolver.cs"
- Test-Path -Path "Unity_PJ\project\Assets\Scripts\Runtime\Diagnostics\RuntimeLog.cs"
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- apply_patch: Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs
- apply_patch: Unity_PJ/docs/NEXT_TASKS.md

## Tests
- File existence checks for new runtime/test scripts: Pass
- Resolver test source added (AP-001..AP-006 baseline): Pass (implementation completed)
- Unity Test Runner execution: Not Run
  - `Unity.exe` が環境で検出できず、バッチ実行不可

## Rationale (Key Points)
- 不具合原因追跡のため、`request_id/error_code/path/source_tier` をログの最小必須項目にした。
- PMX等の本番ローダー未導入状態でも、表示到達を優先して「画像平面表示 or プレースホルダ表示」を採用した。
- Asset path の失敗要因（絶対/トラバーサル/legacy/未検出）を個別エラーコード化し、切り分けを短縮した。

## Rollback
- 追加/更新ファイルを差し戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs`
  - `Unity_PJ/docs/NEXT_TASKS.md`
  - `docs/worklog/2026-02-07_unity-runtime-logging-and-model-bootstrap_2136.md`
  - `docs/worklog/2026-02-07_unity-runtime-logging-and-model-bootstrap_2136_report.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Unity Editor上でPlay確認し、`runtime.jsonl` に `request_id/error_code` が記録されることを実測確認する。
- 画像以外（PMX等）読み込みの実装方針を決め、`ASSET.READ.UNSUPPORTED_EXTENSION` の置き換えを段階的に進める。
