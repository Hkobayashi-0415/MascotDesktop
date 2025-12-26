# Avatar Renderer (Mode3: PNGTuber focus)

## 状態とイベント（public_files-main/desktop_image.py を参照）
| 状態 | 発火イベント | 遷移先 | 備考 |
|---|---|---|---|
| 01_normal | 起動/リセット | 01_normal | デフォルト |
| 01_normal | クリック累積>=5 | 02_smile or 03_oko | ランダム選択、10秒後リセット |
| 02_smile / 03_oko | タイマー10s | 01_normal | click_countリセット |
| 04_sleep | 無操作15分 | 04_sleep | 入眠 |
| 04_sleep | クリック | 05_on | 10秒後にnormal |
| 05_on | タイマー10s | 01_normal |  |

## リソース構造案
- `assets/pngtuber/<character>/<state>/*.png`（stateは01_normal,02_smile,03_oko,04_sleep,05_on 等を推奨）
- 直前画像を除外したランダム選択（fallback:全体から選択）
- 無操作タイマーと状態別タイマーを実装（無操作→sleep, sleep→clickでon, on/smile/oko→timer戻し）

## 本プロジェクトへの反映
- Must: 状態別フォルダ構造とランダム表示、直前除外、タイマー復帰、無操作スリープを実装。
- Should: 状態セットを拡張（notify/reminder/talk）し、イベントにマッピング。
- Could: キャラごとに状態セットを拡張可能にし、優先度/重みを設定。

## 他モードとの整合
- Mode1/2（3D/動画）でも同じ状態イベントをハブとして共有。状態イベントの発火は Core/Shell イベントバス経由で通知。

## 参照
- DTO: AvatarSetStateRequest.json
- アセットテンプレ: data/templates/assets/pngtuber_mode3/README.md
