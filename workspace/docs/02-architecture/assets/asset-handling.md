# Asset Handling

## 管理方針
- 実アセット（ユーザ配布物）は Git に含めない。`data/assets_user/` は gitignore。
- テンプレ/サンプル/プレースホルダは `data/templates/assets/` に置く（軽量・安全なもののみ）。
- 探索優先順位:
  1. `data/assets_user/...` （実アセット）
  2. `data/templates/assets/...` （テンプレ/プレースホルダ）
  3. コード内フォールバック
- 絶対パスは禁止。必ずベースディレクトリ＋相対パスで解決。ASCIIパス推奨（非ASCIIはWARNで継続）。

## 許可拡張子と検証
- 画像: .png（推奨/透過可）, .webp  
- 動画: .mp4, .webm  
- 3D/MMD: .pmx, .pmd, .vmd, .vrm（将来検討）  
- 音声: .wav, .ogg, .mp3（クリップ）、TTSはストリーム/キャッシュ

## サイズ・性能制約
- プレースホルダ: 1ファイル 1MB 以下推奨（最小の単色PNGを同梱）。
- ユーザーアセット: 制限なしだが、動画は30fps/1080p目安、画像は 1024x1024 目安、MMDはポリ数/テクスチャを抑える。

## 表示モード（画像フィット）
- `window.image_fit_mode`: `"contain"`（既定。縦横比維持で全体を収める余白あり） / `"cover"`（縦横比維持で埋める。はみ出しは中央トリミング）。
- 描画領域サイズを基準にリサイズし、サイズが変わらない場合は再生成しない（キャッシュ）。

## 置き場所（推奨）
- `data/assets_user/`（Git除外）
  - `characters/<slug>/pngtuber_mode3/states/*.png`
  - `characters/<slug>/mmd/{model.pmx,textures/,motions/*.vmd}`
  - `characters/<slug>/video/*.mp4`
  - `characters/<slug>/audio_clips/*.wav`
- `data/templates/assets/`（Git管理OK）
  - `placeholders/` 仮画像一式（CC0/自作）
  - `pngtuber_mode3/` 規約・空ディレクトリ（.gitkeep）
  - `mmd_mode1/` 規約・example/.gitkeep（実モデルは置かない）

## パス解決
- character_assets.file_path は相対パスを保持し、BaseDir（キャラ配置 or アセットルート）と結合して解決。
- 例: BaseDir=`data/assets_user/characters/alice` + `pngtuber_mode3/states/01_normal.png`

## ログとフォールバック
- 読み込み失敗時は `data/templates/assets/placeholders/avatar_placeholder_256.png` を使い、`assets.placeholder_used` を INFO で出す。
- 探索結果（user/templates/placeholder）をログに残すと追跡しやすい。

## 参照
- Mode3テンプレ: `data/templates/assets/pngtuber_mode3/README.md`
- Placeholders: `data/templates/assets/placeholders/README.md`
- AvatarRenderer: `docs/02-architecture/avatar/avatar-renderer.md`
