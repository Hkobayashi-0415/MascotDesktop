# Worklog: pmx-vrm-loader-bridge (MascotDesktop)

- Date: 2026-02-07
- Task: PMX/VRMローダーブリッジを追加し、unsupported extension依存を縮小
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs, Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs, Unity_PJ/docs/NEXT_TASKS.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-07_pmx-vrm-loader-bridge_2200_report.md
- Obsidian-Log: 未実施: 本セッションではObsidian配下への新規ログ作成を実施していないため
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
モデル読込を `Image/VRM/PMX/Unsupported` に分離し、VRM/PMXについては導入済みローダーパッケージを反射で検出して実ロードを試行するブリッジを追加した。ローダー未導入時は専用エラーコードで失敗を表現し、原因切り分けを可能にした。

## Changes
- `ModelFormatRouter` を追加して拡張子ルーティングを明示化
- `ReflectionModelLoaders` を追加して `VRM.VRMImporterContext` / PMX候補型を反射で探索
- `SimpleModelBootstrap` をパイプライン化し、`TryDisplayVrm` / `TryDisplayPmx` を追加
- `ModelFormatRouterTests` を追加（EditMode）
- `Unity_PJ/docs/NEXT_TASKS.md` に PMX/VRM loader bridge の反映を追記

## Commands
- Get-ChildItem -Path "Unity_PJ\project\Assets\Scripts\Runtime\Avatar" -Recurse -File | Select-Object FullName
- Get-Content -Path "Unity_PJ\project\Assets\Scripts\Runtime\Avatar\SimpleModelBootstrap.cs"
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs
- apply_patch: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- apply_patch: Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs
- apply_patch: Unity_PJ/docs/NEXT_TASKS.md
- Select-String -Path "Unity_PJ\project\Assets\Scripts\Runtime\Avatar\SimpleModelBootstrap.cs" -Pattern "TryDisplayVrm|TryDisplayPmx|ModelFormatRouter"
- Select-String -Path "Unity_PJ\project\Assets\Scripts\Runtime\Avatar\ReflectionModelLoaders.cs" -Pattern "AVATAR.VRM.LOADER_NOT_FOUND|AVATAR.PMX.LOADER_NOT_FOUND|VRM.VRMImporterContext"
- Get-Command Unity.exe -ErrorAction SilentlyContinue | Select-Object Source,Name
- Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Directory -ErrorAction SilentlyContinue | Select-Object FullName

## Tests
- 追加ファイル存在確認: Pass
- 主要経路（VRM/PMX/Router）の文字列検証: Pass
- Unity EditMode実行: Not Run
  - `Unity.exe` 未検出のためこの環境では実行不可

## Rationale (Key Points)
- `ASSET.READ.UNSUPPORTED_EXTENSION` 一律では原因特定が弱いため、VRM/PMX専用エラーへ分離した。
- 依存パッケージ有無が環境差分になるため、反射検出でビルド依存を増やさず統合点を用意した。
- ローダー未導入時にも `AVATAR.VRM.LOADER_NOT_FOUND` / `AVATAR.PMX.LOADER_NOT_FOUND` で対処が明確になる。

## Rollback
- 以下の追加/更新を差し戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs`
  - `Unity_PJ/docs/NEXT_TASKS.md`
  - `docs/worklog/2026-02-07_pmx-vrm-loader-bridge_2200.md`
  - `docs/worklog/2026-02-07_pmx-vrm-loader-bridge_2200_report.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Unity環境にVRM/PMXローダーパッケージを導入した上でPlay実行し、`avatar.model.vrm_load_failed` / `avatar.model.pmx_load_failed` の挙動と成功経路ログを実測する。
- PMXローダーの既知APIに合わせて反射候補を具体化し、ロード成功率を上げる。
