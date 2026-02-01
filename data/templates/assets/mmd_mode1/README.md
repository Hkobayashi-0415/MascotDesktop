# MMD Mode1 Asset Template

目的: Git管理外の実アセットを安全に配置するためのガイド。ここには実体を入れず、利用者が data/assets_user/ 以下にコピーする。

推奨配置（例）
```
data/assets_user/characters/<slug>/mmd/
  model.pmx           # または .pmd
  textures/           # テクスチャ類（モデルからの相対パスを維持）
  motions/            # VMDなど（任意）
  readme.txt          # 出典/ライセンスを明記（必須）
```

規約
- パスは ASCII のみ（日本語・全角禁止）。
- 拡張子: .pmx / .pmd / .vmd / .png / .jpg。
- テクスチャはモデルからの相対パスを壊さない。
- サイズ上限: 100MB 未満を推奨（PoC性能のため）。
- Git管理外: `.gitignore` で `data/assets_user/` を除外済み。テンプレのみ許可。

利用手順（PoC想定）
1. 上記構造で assets_user に配置する。
2. Avatar Viewer PoC の起動時に `--model-path` などで指定する（デフォルトはこのテンプレのダミーパス）。
3. ライセンス/出典を readme.txt に必ず記載する。ライセンス不明モデルは使用しない。

備考
- ネットワーク不要で動くよう、依存するJS/CSS/ライブラリはローカル配布（後続の実装で対応）。
- 現時点では Three.js MMDLoader を想定。後で Mode2/Mode3 と同居できるよう base_dir + relative_path で扱う。*** End Patch*** End Patch**? Wait extra. Need ensure apply patch format.**``` Actually patch has duplicate end markers. Need correct. Let's re-run simpler.***
