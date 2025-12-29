# Phase1 PoC 実行手順 (Windows, ローカル)

## 前提
- Gitルート: `workspace/`
- 依存: Python 3.10+ (標準ライブラリのみ使用)
- 画像テンプレ: `data/templates/assets/pngtuber_mode3/states/01_normal.png` （未存在なら自動生成される）

## 実行
```powershell
cd "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop\workspace"
python apps/core/poc/poc_core_http.py   # 先に起動（別ターミナルで）
python apps/shell/poc/poc_shell.py      # 後で起動
python apps/avatar/poc/poc_avatar_mmd_viewer.py  # Avatar Mode1 viewer（別ターミナル可、表示のみPoC）
```
- Escで終了。キー `t` でTopmost切替。

### 詰まり検知用スモーク
```powershell
cd "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop\workspace"
python apps/core/poc/poc_core_http.py   # ターミナルAで起動
# もう1つのターミナルで
scripts/dev/smoke_ipc.ps1           # health / bad json / missing fields を叩く
```
- 3ケースとも JSON が返り、`request_id` が付くこと。
- 異常系は `error_code` が付くこと（例: CORE.IPC.BAD_REQUEST.INVALID_JSON）。
- ログ追跡は `logs/core/ipc.log` と `logs/shell/ipc.log` を `request_id` で grep/jq。
- Avatar viewer を起動して `/avatar/health` を叩くと `logs/avatar/avatar.health.log` に記録される（現状はプレースホルダ表示のみ）。`/avatar/load` でモデルパスを送るとロード完了メタを返す。

## 期待動作（受入条件チェック）
- フレームレスで画像が表示される（01_normal）。
- 左ドラッグでウィンドウ移動。
- ドラッグ中は `/v1/config/set` を送らず、ボタンを離した瞬間に1回だけ送信される（reason=drag_end を Coreログで確認）。
- 終了時に位置/サイズが `data/user/config.json` に保存され、再起動で復元。
- `t`キーでTopmost ON/OFFが切り替わり、状態は終了時に保存。
- Core連携: 起動時に `/health`、`/v1/config/get` を叩き、位置/Topmostを反映。移動/Topmost変更で `/v1/config/set` を送る（reason=drag_end/topmost_toggle）。
- ログ追跡: drag_end/topmost は `logs/shell/window.log` と `logs/core/config.log` に同じ `request_id` が出る。
- エラー確認: 失敗時はレスポンスに `error_code` が入る（例: CORE.IPC.BAD_REQUEST.INVALID_JSON）。`logs/core/config.log` などに同じ `request_id` と `error_code` が出る。

## トラブルシュート
- 透過しない: WindowsテーマやGPU設定で透明色が効かない場合あり。背景色が白であることを確認。
- OneDriveパス: 日本語パスに起因する不具合があれば、ASCIIパス側に丸ごとコピーして実行。
- 画像なし: `data/templates/assets/pngtuber_mode3/states/01_normal.png` が無ければ自動生成のプレースホルダを使用。
- ポート競合: Coreデフォルト8765が埋まっている場合は `poc_core_http.py` を修正、または環境変数で上書き。
- Avatar viewer のデフォルトポートは 8770。競合時は `poc_avatar_mmd_viewer.py` を修正。
