# Session Handoff: Runtime Visibility / Texture Resolution (2026-02-10)

## 1) Current Situation

- 症状:
  - `avatar.model.displayed` は成功ログが出る。
  - 一部モデルで非表示・白飛びが継続。
- 重要観測:
  - `avatar.model.render_diagnostics` で `missingMainTexMats=1` が継続。
  - 同ログで `enabled=1, active=1, skinned=1, bounds=1.617x2.4x0.491` のため、Renderer/Boundsは成立。
- 結論:
  - 主因は「マテリアルの main texture 解決失敗」の可能性が最も高い。

## 2) What Was Changed in This Session

- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - 読込直後の配置安定化を追加（`LateUpdate` + 画面外時の再センタリング）。
  - `RegisterActiveModelRoot` を拡張し、PMX/VRMでのみ一定フレーム安定化を有効化。
- `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/BitmapLoader.cs`
  - BITFIELDS BMP (compression=3) 対応（16/32bit）。
  - 低bpp行バイト計算修正。
  - パレット件数計算修正。
- `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - ファイル名ベースの再帰探索（base/parent/grandParent）追加。
  - 探索結果キャッシュを追加。
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - main/sub/toonテクスチャ欠落の警告ログを追加。

## 3) What To Verify First (Priority Order)

1. 問題モデル読込時に以下ログが出るか確認
- `MMD main texture missing: material=..., requested=...`
- `MMD sub texture missing: ...`
- `MMD toon texture missing: ...`

2. 同一タイミングの診断ログを採取
- `avatar.model.render_diagnostics`
- 値として `missingMainTexMats` / `lowAlphaMats` / `bounds`

3. `requested` 参照先の実在確認
- 実ファイル名・拡張子・階層差（例: `texture/` vs `textures/`）
- 同名ファイルが複数ある場合は誤解決の可能性

## 4) Decision Rules for Next Session

- ケースA: `missingMainTexMats > 0`
  - 参照解決ロジックを優先して修正。
  - 必要なら `TextureLoader.ResolveTexturePath` に候補ログ（request/base/resolved）を追加。
- ケースB: `missingMainTexMats = 0` かつ 白飛び継続
  - `MaterialLoader` の shader/property（_Color/_Opacity/_MainTex）を書き換え前後で追跡。
  - 必要なら render factor適用時に shader別分岐を追加。
- ケースC: `bounds` が無効 or 極端値
  - `StabilizeActiveModelPlacement` の閾値調整・bounds再計算を優先。

## 5) Known Risks

- ファイル名再帰探索は誤マッチの可能性あり（同名テクスチャ複数）。
- キャッシュにより初回誤解決が残る可能性があるため、検証時はUnity再起動でクリア推奨。

## 6) Related Logs / Docs

- Worklog (this session):
  - `docs/worklog/2026-02-10_runtime-visibility-texture-followup_0405.md`
- Prior worklogs:
  - `docs/worklog/2026-02-10_runtime-loader-stability-fix_0100.md`
  - `docs/worklog/2026-02-10_candidate-mode-separation_0144.md`

## 7) Ready-to-use Validation Checklist

- [ ] `MMD main texture missing` の `requested=` を1件以上取得
- [ ] 同一モデルの `avatar.model.render_diagnostics` を取得
- [ ] 問題モデル3体（桃鈴ねね_BEA / 桃鈴ねね_STD / SakamataChloe）で比較
- [ ] `missingMainTexMats` の有無で分岐し、次修正方針を確定
