# Logging & Monitoring (PoC)

## 基本方針
- 1行1JSON (JSON Lines)。共通項目: timestamp, level, message, component, feature, event, request_id, session_id, character_id。
- ログはコンポーネント/機能別に分離しつつ request_id で横断追跡できる。
- 秘密情報（APIキー、全文プロンプト、個人情報）は記録しない。payloadはメタのみ（キー一覧・サイズ）。

## 出力先構成（例）
- `logs/core/core.log` (集約), `logs/core/config.log`, `logs/core/chat.log`, `logs/core/health.log`, ...
- `logs/shell/shell.log` (集約), `logs/shell/window.log`, `logs/shell/ipc.log`, `logs/shell/config.log`
- ローテーション: RotatingFileHandler, maxBytes=5MB, backupCount=5。Git管理外。

## イベント例
- http.request/response（Core）: status_code, duration_ms。ヘルスは feature=health。
- config.get / config.set（Core/Shell）: changed_keys、reason（drag_end, topmost_toggle 等）、payload全文は記録しない。
- window.drag_end / window.topmost_toggle（Shell）: 位置/サイズ/Topmostをメタで記録。
- chat.send（Core）: request_id/session_id/character_id を必須。message content は長さのみ。

## 追跡方法
- request_id で grep: `rg "req-123" logs/core logs/shell`
- jqで絞り込み（例）: `jq 'select(.request_id=="req-123")' logs/core/config.log`
- error_codeでgrep: `rg "CORE.CONFIG" logs/core/config.log`

## 禁止事項
- 秘密情報の出力（APIキー、プロンプト全文、個人情報）。
- ログをリポジトリにコミットしない（logs/ は .gitignore 済み）。

## Revision History

### v2.0 (PoC, 2025-12-30~)
- JSON Lines形式の詳細ガイドに拡張
- 出力先構成、イベント例、追跡方法、禁止事項を追加
- 1行1JSON形式、request_id による横断追跡の詳細を記載
- 旧バージョン（v1.0）は基本方針のみ（8行）

### v1.0 (baseline, ~2025-12-30)
- 基本方針のみ定義（ルートレベル `docs/03-operations/logging.md` に存在）
- レベル、チャネル、ヘルス、プライバシー、トレーサビリティの概要のみ
