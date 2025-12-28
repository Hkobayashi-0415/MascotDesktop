# Phase1 PoC 実行手順 (Windows, ローカル)

## 前提
- Gitルート: `workspace/`
- 依存: Python 3.10+ (標準ライブラリのみ使用)
- 画像テンプレ: `data/templates/assets/pngtuber_mode3/states/01_normal.png` （未存在なら自動生成される）

## 実行
```powershell
cd "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop\workspace"
python apps/core/poc_core_http.py   # 先に起動（別ターミナルで）
python apps/shell/poc_shell.py      # 後で起動
```
- Escで終了。キー `t` でTopmost切替。

## 期待動作（受入条件チェック）
- フレームレスで画像が表示される（01_normal）。
- 左ドラッグでウィンドウ移動。
- ドラッグ中は `/v1/config/set` を送らず、ボタンを離した瞬間に1回だけ送信される（Coreログで確認）。
- 終了時に位置/サイズが `data/user/config.json` に保存され、再起動で復元。
- `t`キーでTopmost ON/OFFが切り替わり、状態は終了時に保存。
- Core連携: 起動時に `/health`、`/v1/config/get` を叩き、位置/Topmostを反映。移動/Topmost変更で `/v1/config/set` を送る。

## トラブルシュート
- 透過しない: WindowsテーマやGPU設定で透明色が効かない場合あり。背景色が白であることを確認。
- OneDriveパス: 日本語パスに起因する不具合があれば、ASCIIパス側に丸ごとコピーして実行。
- 画像なし: `data/templates/assets/pngtuber_mode3/states/01_normal.png` が無ければ自動生成のプレースホルダを使用。
- ポート競合: Coreデフォルト8765が埋まっている場合は `poc_core_http.py` を修正、または環境変数で上書き。
