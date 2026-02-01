# Error Codes (PoC)

## 形式ルール
- フォーマット: `CORE.<FEATURE>.<CATEGORY>.<DETAIL>`
- FEATURE: GENERAL, IPC, CONFIG, CHAT, HEALTH
- CATEGORY: VALIDATION, IO, STATE, INTERNAL, BAD_REQUEST, NOT_FOUND
- 成功時: `ok=true`, `error_code` は省略または `null`。失敗時は `ok=false` かつ `error_code` を必須。
- レスポンス共通フィールド（追加分）: `ok`, `error_code?`, `retryable?`, `message?`, `request_id`

## カタログ
| error_code | cause | retryable | action |
| --- | --- | --- | --- |
| CORE.GENERAL.UNHANDLED | 予期しない例外 | false | ログを確認、再現手順を洗い出し |
| CORE.IPC.BAD_REQUEST.INVALID_JSON | JSONパース失敗 | false | リクエストフォーマットを修正 |
| CORE.IPC.NOT_FOUND | 未知エンドポイント | false | パス/メソッドを確認 |
| CORE.CONFIG.VALIDATION.MISSING_FIELD | entries が欠落/空/非dict | false | 必須フィールド(entries)を送る |
| CORE.CONFIG.IO.WRITE_FAILED | config 保存に失敗 | true | 再試行またはファイルロックを解除 |
| CORE.CHAT.VALIDATION.MISSING_MESSAGE | message/content が欠落 | false | 必須フィールドを送る |
| CORE.HEALTH.UNAVAILABLE | 内部依存が落ちている | true | サービス再起動、依存確認 |

## 返却スキーマ（追加分）
- 既存レスポンスに以下を追加:  
  - `ok: bool` (必須)  
  - `error_code?: string` (失敗時必須)  
  - `retryable?: bool` (失敗時推奨)  
  - `message?: string` (失敗時推奨)  
  - `request_id: string` (必須)  
- 既存フィールド（config/chat本体）はそのまま維持する。

## 運用メモ
- Healthは 200 固定で `ok=true/false` を返す運用でもよい（PoCでは常に ok=true）。
- ログには `error_code` と `request_id` を必ず出し、payload全文は記録しない（メタのみ）。
