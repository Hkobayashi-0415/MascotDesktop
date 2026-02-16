# Report: PMX Texture 24-bit TGA Fix

## Status: ✅ Partial Success

## Changes Made

| File | Change |
|------|--------|
| `Utils.cs` | `Bgr24ToColors()` メソッド追加 |
| `TargaImage.cs` | 24-bit TGA処理を `SetPixels` に変更 |

## Results

- ✅ テクスチャファイルの読み込み成功
- ✅ 24-bit TGAのBGR→RGB変換適用
- ⚠️ モデルはまだ白っぽく表示（シェーダー/ライティング問題）

## Next Steps

1. シェーダー `MeshPmdMaterialSurface.cginc` の調査
2. ライティング設定の確認
3. `_AmbColor` の影響を減らす修正を検討
