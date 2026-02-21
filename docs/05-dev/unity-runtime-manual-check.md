# Unity Runtime Manual Check

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-20
- Scope: Unity Editor での画面確認（Runtime HUD + 基本操作）

## 根拠
- `Unity_PJ/docs/05-dev/phase3-parity-verification.md:25`
- `Unity_PJ/docs/05-dev/phase3-parity-verification.md:30`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:98`
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:50`
- `docs/05-dev/unity-character-switch-operations.md:25`

## Preconditions
1. Unity project path: `D:\dev\MascotDesktop\Unity_PJ\project`
2. アセット配置: `Unity_PJ/data/assets_user/characters/<slug>/...`
3. 実行環境エラー時は `docs/05-dev/unity-test-environment-recovery.md` を参照

## 手順
1. Unity Editor で `Unity_PJ/project` を開く。
2. Play Mode に入る（シーンは任意）。
3. 画面左上に `MascotDesktop Runtime HUD` が表示されることを確認する。
4. 以下の順で最小操作を行う:
   - `Model: rescan(list)`
   - `Model: next`（または `Model: prev`）
   - `State: happy`
   - `Motion: wave`
   - `Toggle Topmost`
   - `Hide/Show`

## 合格条件
- HUD が表示される。
- モデル切替後に `Model Path` が更新される。
- Console に `avatar.model.displayed` が出力される。
- 操作時にエラーコード付きログで失敗原因を追える。

## 失敗時の記録
```markdown
| run_at | check_item | result | cause | evidence |
|---|---|---|---|---|
| 2026-02-20 01:20:00 | HUD appears | Failed | Play Mode start failed | editor screenshot + console |
```

## 参照
- `docs/05-dev/QUICKSTART.md`
- `docs/05-dev/unity-character-switch-operations.md`
- `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
