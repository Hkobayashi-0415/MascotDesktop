# Diff Note: 2025-12-26 add public_files refs

## 変更点
- refs-policy に public_files-main を追記（参照のみ、コード移植禁止）。
- UI/Window/Avatar設計に public_files の挙動を要約したドキュメントを追加。
- マッピング表に public_files を追加（SPEC_ONLY）。
- 仕様書(spec/latest/spec.md)に改訂履歴と Mode3 状態遷移/Window要件を反映。

## 理由
- 新たな参照リポジトリ(public_files)を設計インプットとして明文化し、実装タスクに落とすため。

## 影響範囲
- UI/Window/Avatar設計方針、マッピング表、仕様書改訂履歴。
- 実装は依然としてデスクトップUI（非Web）方針。PyQt依存は持ち込まない。

## 未確定/TODO
- Mode1/Mode2(3D/動画)への状態イベント適用範囲。
- Topmost/透過/ホットキーの最終キー割り当て。
- enum/インデックス/embedding保存方式の詳細。

## 次アクション
- IPC/DTO定義を interfaces/ に追加。
- AvatarRenderer/WindowControllerの受入テストケースを実装計画に落とす。
- DDLのenumチェック・インデックス案を追記。
