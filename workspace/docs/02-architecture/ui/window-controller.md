# Window Controller (desktop UI) - Acceptance Criteria

## 要件サマリ
- フレームレス＋透過背景。ドラッグ移動で位置変更。終了時に位置保存、起動時復元。
- Topmost ON/OFF を設定・ショートカットで切替（後述受入条件）。
- クリック透過はオプション。透過中に解除できる安全ホットキーを用意。
- 最小/最大サイズガード。閉じる操作で設定保存を保証。

## 受入条件 (Given/When/Then)
1. Given アプリ初回起動、When 位置未保存, Then 既定位置(例:100,100)で表示する。
2. Given 位置が保存済み, When 再起動, Then 保存された位置/サイズで復元する。
3. Given フレームレス/透過, When 左ドラッグ, Then ウィンドウが追従し位置が更新される。
4. Given ドラッグで移動後, When ウィンドウを閉じる, Then 現在位置/サイズが設定ストアに保存される。
5. Given Topmost=OFF, When ユーザが切替(設定orホットキー), Then ウィンドウが最前面化し、状態が設定に保存される。
6. Given Topmost=ON, When ユーザが切替, Then 最前面解除し、設定に保存される。
7. Given クリック透過=ON, When 安全ホットキーを押下, Then 透過が解除され操作可能になる。
8. Given 無操作タイマーON, When 一定時間操作なし, Then 省リソースモード(描画/更新抑制)へ遷移する。
9. Given 最小/最大サイズガード, When ユーザが極端なリサイズを試みる, Then ガード範囲内に制限される。
10. Given 終了処理, When ウィンドウを閉じる, Then 保存失敗時はエラーをログ/通知しつつ終了またはキャンセルを選択できる。
## 対応DTO/イベント
- WindowEvent.json (topmost_toggle, clickthrough_toggle, safe_unlock)
- WindowPositionSave.json (位置/サイズ保存)
- ConfigSet/ConfigGet (Topmost/透過/スクショ除外等)
