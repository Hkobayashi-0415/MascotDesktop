# 2025-12-30 topmost UI change

## 変更概要
- Topmost切替をキーボード依存からUI操作（ボタン/右クリックメニュー）に変更。`t`キーは無効化。
- set_topmost を単一入口にし、状態変化時のみ config.set(reason=topmost_toggle) を送信（idempotent化）。
- ドラッグ中送信なし・リリース1回送信の方針は維持。

## 理由
- キーリピートによる多重発火（1回目無反応/2回目複数行）が発生していたため、フォーカスや入力依存を排除する。
- UI操作の方がクリック透過や将来のホットキー変更に強い。

## 影響
- run-poc の操作手順が変わる（Topmostはボタン/右クリック）。
- ログは 1操作=1行 (window.topmost_toggle) かつ config.set は状態が変わるときだけ送信。

## TODO
- MMD Viewer をShellと統合する際のWindowController設計に反映。
- キー操作を戻す場合はオプション設定で明示し、デバウンス/リピート無効化を合わせて実装する。
