# Report: Unity Runtime Logging and Minimal Model Bootstrap (MascotDesktop)

## Summary
- Unity runtimeで原因特定可能な構造化ログ（`request_id`, `error_code`, `path`, `source_tier`）を追加した。
- Asset path ルール実装と EditMode テストを追加した。
- 起動時に簡易表示へ到達する `SimpleModelBootstrap` を追加した。

## Files
- Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs
- Unity_PJ/docs/NEXT_TASKS.md
- docs/worklog/2026-02-07_unity-runtime-logging-and-model-bootstrap_2136.md
