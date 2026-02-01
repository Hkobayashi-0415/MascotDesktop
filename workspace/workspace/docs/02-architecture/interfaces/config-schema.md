# config-schema (Shell ⇔ Core)

## 方針
- ローカル専用。`config.json` は `data/user/config.json` に保存。
- 未知キーは無視（後方互換性確保）。必須キーが欠けたらデフォルト補完。
- 値は整数/boolean/文字列のシンプル型で保持する。

## キー一覧（現行PoC）
- `window_x` (int, 必須): ウィンドウ左上X座標
- `window_y` (int, 必須): ウィンドウ左上Y座標
- `width`    (int, 必須): ウィンドウ幅
- `height`   (int, 必須): ウィンドウ高さ
- `topmost`  (bool, 必須): 最前面フラグ

## 保存トリガ（Shell側）
- `drag_end`: ドラッグ終了時に位置・サイズを送信
- `topmost_toggle`: `t`キーで最前面を切り替えたとき
- （将来）`close`: 終了時の最終状態保存を追加予定

## DTO例（/v1/config/set）
```json
{
  "dto_version": "0.1.0",
  "request_id": "cfg-set-pos",
  "reason": "drag_end",
  "entries": {
    "window_x": 100,
    "window_y": 200,
    "width": 320,
    "height": 320,
    "topmost": true
  }
}
```

## 互換性
- Coreは未知の`reason`や未使用キーを無視してもよい。
- 新キー追加時は必須にしない（欠損時はデフォルト補完）。
