# PNGTuber Mode3 Asset Template

## フォルダ構成（例）
```
pn.gtuber_mode3/
  states/
    01_normal.png
    02_smile.png
    03_oko.png
    04_sleep.png
    05_on.png
```
- state名は spec/docs と揃える（01_normal, 02_smile, 03_oko, 04_sleep, 05_on）。追加状態はサフィックスで拡張可（notify, talk 等）。
- 参照パスは相対パス＋ベースディレクトリ合成。絶対パス禁止。ASCIIパス推奨。

## ファイル要件
- 拡張子: .png（透過必須）
- 解像度上限: 1024x1024 推奨（大きすぎる場合はUI側で縮小）
- 背景透過必須、アルファ付き。

## 使用時の解決方法
- character_assets.file_path に `states/01_normal.png` のような相対パスを格納。
- ベースディレクトリはアプリ設定またはキャラ配置ディレクトリから合成。

## 動作メモ
- 直前画像を除外したランダム選択を推奨（同一stateでの連続重複を回避）。
- 無操作15分で sleep、clickで on → 10秒で normal に戻す遷移をデフォルトとする（public_files準拠）。
