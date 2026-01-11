# Cleanroom Review (2025-12-30)

## 構造チェック
- Gitルート: `workspace/` 直下に apps/docs/spec/data。旧二重構造 `workspace/workspace` を発見し、`attic/legacy-impl/workspace-duplicate` へ退避済み。
- refs はリポジトリ外運用方針を維持（編集禁止）。

## 責務境界の懸念
- apps/shell で Topmost処理が複雑化し、多重 config.set を発火させやすい（キーリピート）。デバウンス強化と「KeyPressのみ」処理に絞る方針で再実装予定。
- apps/core は単機能HTTPのみで循環参照なし。共通ログ基盤 (apps/common/observability) は分離済み。

## 暗黙仕様の混入リスク
- prototype/public_files 由来の状態遷移・config保存フローが混在し、実装の単純化を阻害。クリーンルームでは Shell/Core を仕様準拠の最小機能に再構築する。

## ユーザ体感の詰まり候補
- Topmostトグルの連続発火で config.set が大量発生（ログ確認済）。ドラッグ中の送信抑止はできているが、キーリピート抑止が不十分。
- 日本語パス（OneDrive配下）での不安定化リスク。ASCIIパスへ移行し警告を表示するガードを実装。

## ログ・相関
- component/feature/request_id は出力されるが、Topmost連打で行数が膨らみ追跡が煩雑。クリーンルームでは1操作=1ログを徹底する。
