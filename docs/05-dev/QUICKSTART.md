# QUICKSTART (Unity Runtime)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-20
- Scope: Unity Runtime の最短起動と画面確認。

## Steps (Unity First)

1) Open Unity project:

```text
D:\dev\MascotDesktop\Unity_PJ\project
```

2) Enter Play Mode in Unity Editor (scene is arbitrary):
- `SimpleModelBootstrap` が自動起動する。
- 画面左上に `MascotDesktop Runtime HUD` が表示されることを確認する。

3) Quick screen checks:
- `Model: rescan(list)` -> `Model: next/prev`（モデル切替）
- `State: happy/sleepy`
- `Motion: wave`
- `Toggle Topmost`
- `Hide/Show`

## Expected
- Runtime HUD が表示される。
- モデル表示に成功すると `avatar.model.displayed` ログが出る。
- モデル切替が `Model Path` に反映される。

## Related Docs
- Runtime手動確認: `docs/05-dev/unity-runtime-manual-check.md`
- キャラクター切替運用: `docs/05-dev/unity-character-switch-operations.md`
- Unity資産配置: `Unity_PJ/docs/02-architecture/assets/asset-layout.md`
- Unity要件: `Unity_PJ/spec/latest/spec.md`

## Legacy Reference (Read-Only)
- 旧PoC実行手順: `docs/05-dev/run-poc.md`
