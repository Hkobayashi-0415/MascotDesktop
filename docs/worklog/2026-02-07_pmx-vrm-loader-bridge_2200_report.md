# Report: PMX/VRM Loader Bridge (MascotDesktop)

## Summary
- モデル拡張子ルーティングを追加し、`Image/VRM/PMX/Unsupported` の読込経路を分離した。
- VRM/PMXの反射ローダーブリッジを追加し、ローダー未導入時の専用エラーコードを実装した。
- `SimpleModelBootstrap` を更新し、VRM/PMXロード試行後のフォールバックに接続した。

## Files
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs
- Unity_PJ/docs/NEXT_TASKS.md
- docs/worklog/2026-02-07_pmx-vrm-loader-bridge_2200.md
