# Phase1 PoC 実行手順 (Windows, ローカル)

## 前提
- Gitルート: `workspace/`
- 依存: Python 3.10+ (標準ライブラリのみ使用)
- 画像テンプレ: `data/templates/assets/pngtuber_mode3/states/01_normal.png` （未存在なら自動生成される）

## 実行
```powershell
cd "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop\workspace"
python -m apps.core.poc.poc_core_http         # 先に起動（別ターミナルで）
python -m apps.shell.poc.poc_shell            # 後で起動
python -m apps.avatar.poc.poc_avatar_mmd_viewer  # Avatar Mode1 viewer（別ターミナル可、表示のみPoC）
```
- Escで終了。Topmost(Pinned)はUI操作（右上の「PIN」ボタン、右クリックメニュー）で切替。`t`キーは無効化。

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
- Dedup確認（config.set 保険）
  - 同じ `request_id` で `/v1/config/set` を2回送っても設定適用は1回のみ。2回目は Core ログに `config.set.dedup` が1行出るだけ（200/ok=true）。
  - Shellで万一2行出ても Core は1行＋dedup1行であることを `logs/core/config.log` で確認。

## 期待動作（受入条件チェック）
- フレームレスで画像が表示される（01_normal）。
- 右上オーバーレイ `[PIN][FIT][X]` が見える。PINで常時前面/通常、FITで contain/cover 切替、Xで終了。
- 右クリックメニューでも PIN/Normal, Fit(Contain/Cover), Quit が選べる。
- 左ドラッグでウィンドウ移動。
- ドラッグ中は `/v1/config/set` を送らず、ボタンを離した瞬間に1回だけ送信される（reason=drag_end を Coreログで確認）。
- 終了時に位置/サイズが `data/user/config.json` に保存され、再起動で復元。
- Topmost(Pinned/Normal)は「PINボタン/右クリック」1操作で1ログ・1送信（reason=topmost_toggle）、状態は終了時に保存。
- 画像リサイズ: ウィンドウを縮小/拡大すると画像が追従する（デフォルトはcontain）。`logs/assets.log` に `assets.image_resized` が1回出る。
- Core連携: 起動時に `/health`、`/v1/config/get` を叩き、位置/Topmostを反映。移動/Topmost変更で `/v1/config/set` を送る（reason=drag_end/topmost_toggle）。
- ログ追跡: drag_end/topmost は `logs/shell/window.log` と `logs/core/config.log` に同じ `request_id` が出る。
- エラー確認: 失敗時はレスポンスに `error_code` が入る（例: CORE.IPC.BAD_REQUEST.INVALID_JSON）。`logs/core/config.log` などに同じ `request_id` と `error_code` が出る。

## トラブルシュート
- 透過しない: WindowsテーマやGPU設定で透明色が効かない場合あり。背景色が白であることを確認。
- OneDriveパス: 日本語パスに起因する不具合があれば、ASCIIパス側に丸ごとコピーして実行。
- 画像なし: `data/templates/assets/pngtuber_mode3/states/01_normal.png` が無ければ自動生成のプレースホルダを使用。
- ポート競合: Coreデフォルト8765が埋まっている場合は `poc_core_http.py` を修正、または環境変数で上書き。
- Avatar viewer のデフォルトポートは 8770。競合時は `poc_avatar_mmd_viewer.py` を修正。
