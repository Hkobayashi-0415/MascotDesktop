# QUICKSTART (Unity Runtime)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-24
- Scope: Unity Runtime の最短起動と画面確認。

## Steps (Unity First)

1) Open Unity project:

```text
D:\dev\MascotDesktop\Unity_PJ\project
```

2) Enter Play Mode in Unity Editor (scene is arbitrary):
- `SimpleModelBootstrap` が自動起動する。
- 画面左上に `MascotDesktop Runtime HUD` が表示されることを確認する。

## Windows Standalone 前提（重要）

- Runtime は `modelRelativePath` を以下の順で探索する:
  1. `<Unity_PJ>/data/assets_user`
  2. `<Player>_Data/StreamingAssets`
- Build 出力先が `Unity_PJ` 配下なら、親ディレクトリ探索で `Unity_PJ/data/assets_user` を自動検出できる。
- Build 出力先が `Unity_PJ` 外の場合は、`<Player>_Data/StreamingAssets` 側に同等の `characters/...` 構成を配置する。

3) Quick screen checks:
- HUD の高さより項目が多い場合はパネル内をスクロールして下段ボタン（`Motion: wave` / `Toggle Topmost` / `Hide/Show`）を表示する
- `Model: rescan(list)` で候補一覧を再読込し、`Model: next/prev` で `Model Path` が変わることを確認
- `State: happy/sleepy` 実行後に HUD の `Avatar State` と Console の `avatar.state.transitioned` が更新されることを確認
- `Motion: wave` 実行後に HUD の `Motion Slot` と Console の `avatar.motion.slot_played` が更新されることを確認
- `Toggle Topmost` / `Hide/Show` は Windows Standalone Player でネイティブ効果を確認（Unity Editor ではシミュレーションログ確認）

## Expected
- Runtime HUD が表示される。
- モデル表示に成功すると `avatar.model.displayed` ログが出る。
- モデル切替が `Model Path` に反映される。
- `Model: rescan(list)` は候補リスト更新のみ（モデル自体は切替えない）。

## Troubleshooting (Standalone)
- `Model Candidates: 0` / `Image Candidates: 0`:
  - `avatar.model.candidates.discovered` ログの `canonical_exists` / `streaming_exists` を確認。
  - `avatar.paths.assets_roots_checked` ログで `selected_canonical` / `streaming` の実パスを確認。
  - `false` になっている側の配置を修正する。
  - `ui.hud.bootstrap_missing` が出る場合は bootstrap初期化失敗。最新版ビルドで再確認し、`ui.hud.bootstrap_recovered` の有無を確認する。
- 画面がマゼンタのカプセル:
  - モデル解決失敗でフォールバック表示中の可能性が高い。
  - `avatar.model.resolve_failed` / `avatar.model.fallback_used` の `error_code` を確認。

## Related Docs
- Runtime手動確認: `docs/05-dev/unity-runtime-manual-check.md`
- キャラクター切替運用: `docs/05-dev/unity-character-switch-operations.md`
- Unity資産配置: `Unity_PJ/docs/02-architecture/assets/asset-layout.md`
- Unity要件: `Unity_PJ/spec/latest/spec.md`

## Legacy Reference (Read-Only)
- 旧PoC実行手順: `docs/05-dev/run-poc.md`
