# public_files UI notes (desktop_image.py, README)

## 画面/ウィンドウ要件
- PyQt6, フレームレス＋透過背景、ドラッグで移動可。Topmost/ホットキー/クリック透過はなし。
- コンボボックスでキャラ選択、閉じるボタンのみ（最小化/設定ダイアログなし）。
- 画像表示ラベルを中央配置、背景透過。

## 状態遷移（Mode3: PNGTuber的）
- STATES: 01_normal / 02_smile / 03_oko / 04_sleep / 05_on。
- クリック: normal時にクリック累積5回で smile/oko に遷移し10秒後にnormalへ。sleep時クリックで on(=05_on) に遷移し10秒後にnormalへ。
- 無操作: 15分で sleep へ遷移。sleepからの自動復帰はなし（クリックで復帰）。
- 画像選択: 状態フォルダからランダム（前回を除外して重複抑制）。

## 設定永続化
- 保存先: `%APPDATA%/CharacterApp/config.json` (win32の場合) を自動生成・読み書き。
- 保存項目: selected_character, window_x, window_y。ウィンドウ終了時に座標保存、起動時に復元。
- リソース: `desktop_image/image/<character>/<state>/*.png` を参照。EXCLUDE_FOLDER = "00_original" で除外。

## 本プロジェクト仕様への反映候補
- Must: フレームレス/透過、ドラッグ移動、ウィンドウ座標保存/復元、状態ごとの画像フォルダをランダム表示。
- Should: 無操作スリープ遷移、クリックで感情変化＋タイマーで元に戻す、キャラ選択UI。
- Could: 状態数の拡張（notify/reminder等）、前回画像除外ロジックの導入。
- 非採用: Topmost/ホットキー/クリック透過はソースに無し（必要なら自仕様で追加）。

## 本プロジェクトでの置換（Shell/UI実装示唆）
- WindowController: 位置保存/復元、フレームレス/透過、ドラッグ移動を実装。Topmost/透過/ホットキーは自仕様で追加。
- AvatarRenderer(Mode3 PNGTuber): 状態フォルダ構成とランダム表示・重複除外・タイマー復帰・無操作スリープを取り込む。リソース配置規約を `data/templates/assets/` 等に定義予定。
