# Unity Character Switch Operations (U4-T3)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-20
- Scope: Unity Runtime HUD を使ったキャラクター（モデル）切替の標準運用

## 根拠
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:69`
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:74`
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs:98`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:158`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:280`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:346`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs:9`
- `Unity_PJ/spec/latest/spec.md:73`
- `Unity_PJ/docs/02-architecture/assets/asset-layout.md:9`
- `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md:23`

## 前提
1. アセットは `Unity_PJ/data/assets_user/characters/<slug>/...` 配下に配置されていること。
2. Unity Play Mode で `MascotDesktop Runtime HUD` が表示されていること。
3. 失敗時の環境復旧は `docs/05-dev/unity-test-environment-recovery.md` に従うこと。

## 標準手順
1. Play Mode に入り、HUD に `Model Candidates` が表示されることを確認する。
2. `Model: rescan(list)` を1回実行して候補一覧を再取得する。
3. `Mode: models` を選択し、モデル候補モードに固定する。
4. `Model: next` / `Model: prev` で対象モデルへ切り替える。
5. 必要に応じて `Model: reload` で再描画する。

## 手動パス指定が必要な場合
- `SimpleModelConfig.modelRelativePath` は `data/assets_user` からの相対パスで指定する。
- 既定値例: `characters/amane_kanata/official_v1/mmd_pkg/mmd/amane_kanata.pmx`
- `modelRelativePath` 更新後に `Model: reload` を実行する。

## 検証ポイント
- HUD の `Model Path` が切替先の相対パスに変わっていること。
- Console で以下を確認すること（PMX検証手順と同じ基準）:
  - `avatar.model.loader_selected` が `Pmx`
  - `avatar.model.displayed` が `pmx model displayed`
- 切替失敗時は fallback 表示の有無とエラーコードを記録する。

## 記録フォーマット（運用）
```markdown
| run_at | from_model | to_model | candidate_count | result | cause | evidence |
|---|---|---|---|---|---|---|
| 2026-02-20 01:00:00 | characters/a/...pmx | characters/b/...pmx | 8 | Passed | switched by HUD next | avatar.model.displayed |
```

## トラブルシュート
- 候補が0件:
  - `Unity_PJ/data/assets_user/characters/` 配下の実ファイル配置を確認する。
- 切替後に表示されない:
  - Console の `avatar.model.resolve_failed` / `avatar.model.fallback_used` を確認する。
- 旧ルート参照が混入:
  - `ASSET.PATH.LEGACY_FORBIDDEN` を確認し、`Old_PJ/workspace` 参照を除去する。

## 参照
- `docs/05-dev/dev-status.md`
- `docs/NEXT_TASKS.md`
- `docs/05-dev/unity-test-environment-recovery.md`
- `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
