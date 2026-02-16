# Worklog: Render Quality Final Tuning

- Date: 2026-02-09
- Task: PMX表示は成功するが見た目が粗い問題の最終調整
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Report-Path: docs/reports/2026-02-09_render-quality-final-tuning.md
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本リポジトリ内 worklog/report を優先記録）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, fix/render-quality, unity/runtime]

## 変更内容

- `SimpleModelBootstrap` に実行時品質固定処理 `ApplyRuntimeRenderQuality()` を追加。
  - `masterTextureLimit=0`
  - `maximumLODLevel=0`
  - `lodBias>=2`
  - `anisotropicFiltering=ForceEnable`
  - `antiAliasing>=4`
  - `streamingMipmapsActive=false`
- カメラ解決ロジックを強化。
  - `Camera.main` -> 既存 `Camera` -> 新規作成の順に解決
  - `ConfigureCamera()` で FOV/MSAA/HDR/clip 範囲を統一適用
- モデル正規化の表示スケールを微調整。
  - `targetHeight: 1.8 -> 2.4`
  - `targetZ: 3.0 -> 2.5`
- 起動時確認ログ `avatar.render.quality.applied` を追加。

## 実行コマンド

- `git rev-parse --show-toplevel` / `git remote -v` / `git status -sb`（git 未導入を確認）
- `Get-Content` で関連ファイル確認
- `Select-String` で品質設定関連の呼び出し検索
- `apply_patch` で `SimpleModelBootstrap.cs` を更新

## テスト結果

- Unity実行テスト: 未実施（ユーザー担当）
- 静的確認: 実施
  - 変更コード反映を `Get-Content` で確認
  - 追加ログイベント文字列存在を確認

## 判断理由（要点）

- ログ上は PMX ロード成功のため、主問題はロード失敗ではなく描画品質経路。
- 実行時品質がシーン依存・環境依存だったため、起動時に明示固定した。
- カメラ未統一だと同一モデルでも見え方差が大きく、粗さの体感が悪化するため共通化した。
- モデルの画面占有率を上げることで、同じ出力解像度でも見た目解像感を改善できる。

## 次アクション

1. Unity Play で `avatar.render.quality.applied` の出力確認
2. Before/After のスクリーンショット比較（idle/happy/sleepy）
3. まだ粗い場合は `CameraFieldOfView` を 22-26 の範囲で再調整

## ロールバック方針

- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` の今回差分のみを戻す。

## Record Check

- Report-Path exists: True
- Repo-Refs recorded: Yes
- Obsidian-Refs recorded (or n/a): Yes (n/a)
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags recorded: Yes

## Follow-up Fix (11:18 UTC)

- 現象: モデルが画面上で見切れ、品質確認できない。
- 原因: 表示位置が `-bounds.min.y`（足基準）だったため、拡大時に頭部が上端でクリップ。
- 対応: `NormalizeLoadedModelTransform` のY配置を中心基準に変更。
  - 変更前: `y = -bounds.min.y`
  - 変更後: `y = -bounds.center.y`
- 変更ファイル:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- 期待結果: 全身が画面内に収まり、粗さ比較が可能になる。

## Follow-up Fix (11:28 UTC)

- 現象: 若干の白飛び（明部が飛ぶ、全体が白っぽい）。
- 対応:
  - `camera.allowHDR = false`
  - Directional Light intensity を `0.72` に調整
  - `RenderSettings.ambientMode = Flat`
  - `RenderSettings.ambientLight = (0.16, 0.16, 0.18)`
  - 既存ライトがある場合も上記設定を適用
- 変更ファイル:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

## Follow-up Fix (11:46 UTC)

- 現象: 露出調整後も若干の白飛び。
- 対応: PMDシェーダー側で白飛び抑制。
  - Ambient寄与を 35% に縮小
  - SphereAdd寄与を 35% に縮小
  - Sphere合成後の `o.Custom` を `saturate` でクランプ
- 変更ファイル:
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`

## Follow-up Fix (11:51 UTC)

- 現象: 白飛びがまだ僅かに残る。
- 追加対応:
  - KeyLightIntensity `0.58` へ減光
  - AmbientLightColor `(0.10,0.10,0.12)` へ減光
  - シェーダーの Ambient係数 `0.25`
  - SphereAdd係数 `0.20`
  - 最終色 `rgb *= 0.90`
- 変更ファイル:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
