# 2025-12-30 pinned/normal topmost change

## 変更概要
- Topmost操作を「Pinned（常に前面）/Normal」の2モードに整理。UIボタン/右クリックメニューで切替。tキーは無効化。
- set_pinned を単一入口にして idempotent 化。同状態なら何もしない。状態変化時だけ window.log と config.log に1行ずつ、/v1/config/set も1回。
- config の正は `pinned`。旧 `topmost` が存在する場合のみ読み替える後方互換を追加。

## 影響
- キーボード依存を除去し、クリック透過やフォーカスに左右されない操作に統一。
- ログ/IPC が 1操作=1回に安定。連打しても状態変化しない限り送信されない。

## TODO
- UIボタンの状態表示改善（Pinned時は色やアイコンを変える）。
- Shell起動時に pinned 状態を表示に反映する（現状は属性に適用のみ）。
- 将来のクリック透過/安全ホットキー設計に合わせてメニュー拡張を検討。
