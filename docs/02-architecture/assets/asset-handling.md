# Asset Handling

## 管理方針
- 実アセット（ユーザ配布物）はGitに含めない。`data/assets_user/` 等はgitignore。
- テンプレ/サンプルは `data/templates/assets/` に置き、軽量・透過PNG等のみ。

## 許可拡張子と検証
- 画像: png (透過必須), jpg/jpeg (非推奨), webp (可)  
- 動画: mp4/webm（フレーム率上限 30fps、解像度上限 1080p）  
- 3D: vrm（デフォルト）、gltf/glb は将来検討  
- 音声: wav/ogg/mp3（クリップ）、TTSはストリーム/キャッシュ

## サイズ・性能制約
- 画像: 1024x1024以下推奨。  
- 動画: 30fps/1080p以下、ループ可。  
- 3D: ポリ数/テクスチャサイズは別途ガイド。  

## パス解決
- character_assets.file_path は相対パスを保持し、ベースディレクトリ（キャラ配置 or アセットルート）と結合して解決。絶対パスは禁止。ASCIIパス推奨。

## 参照
- Mode3テンプレ: `data/templates/assets/pngtuber_mode3/README.md`
- AvatarRenderer: `docs/02-architecture/avatar/avatar-renderer.md`
