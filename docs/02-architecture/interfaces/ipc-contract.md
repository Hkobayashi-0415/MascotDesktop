# IPC Contract (Unity Boundary, U5 Baseline)

- Status: active
- Last Updated: 2026-02-20
- Scope: Unity Runtime と optional companion/Core 間の loopback HTTP 契約

## 根拠
- `Unity_PJ/spec/latest/spec.md` (`UR-007`, `UR-008`, `UR-009`, `UR-011`)
- `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md`
- `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs`

## 1. 境界方針（固定事項）
- Transport は loopback HTTP (`127.0.0.1`) + JSON DTO を採用する。
- `request_id` は HTTP ヘッダ (`X-Request-Id`) と Body の双方で必須とする。
- エラー応答は `error_code` / `message` / `retryable` / `request_id` を安定フィールドとして扱う。
- payload 本文はログに保存せず、メタ情報（キー、サイズ、処理時間、ステータス）を記録する。

## 2. 共通 DTO 契約

### 2.1 Request Envelope
```json
{
  "dto_version": "1.0.0",
  "request_id": "req-20260220-0001",
  "timestamp_utc": "2026-02-20T00:00:00Z",
  "actor": "runtime",
  "payload": {}
}
```

### 2.2 Response Envelope (Success)
```json
{
  "dto_version": "1.0.0",
  "request_id": "req-20260220-0001",
  "status": "ok",
  "payload": {}
}
```

### 2.3 Response Envelope (Error)
```json
{
  "dto_version": "1.0.0",
  "request_id": "req-20260220-0001",
  "status": "error",
  "error_code": "CORE.TIMEOUT",
  "message": "upstream timeout",
  "retryable": true
}
```

## 3. Endpoint Namespace（U5最小セット）
- `POST /health`
- `POST /v1/chat/send`
- `POST /v1/config/get`
- `POST /v1/config/set`

## 4. U5拡張予約エンドポイント
- `POST /v1/avatar/state`
- `POST /v1/avatar/motion`
- `POST /v1/tts/play`
- `POST /v1/stt/event`

## 5. 相関と検証ルール
- 送信時: `X-Request-Id` と body `request_id` は同一値であること。
- 受信時: レスポンスの `request_id` が送信 `request_id` と一致しない場合は契約違反として扱う。
- 再試行時: idempotent な要求は同一 `request_id` で再送し、重複適用を避ける。

## 6. エラー契約と現行Unityクライアント挙動

### 6.1 サーバー側必須エラー項目
- `status: "error"`
- `error_code`
- `message`
- `retryable`
- `request_id`

### 6.2 Unity側の最低保証（現時点）
- loopback bridge disabled: `IPC.HTTP.DISABLED`
- non-success status: `IPC.HTTP.NON_SUCCESS`
- transport/timeout/exception: `IPC.HTTP.REQUEST_FAILED`
- RuntimeConfig 欠落時は `IPC.HTTP.CONFIG_MISSING` をログ出力する。

## 7. セキュリティ/公開範囲
- バインド先は loopback のみ（LAN/WAN 公開禁止）。
- 認証情報・個人情報を payload として送る場合、ログには値を残さない。
- 本契約はローカル統合チャネル用途であり、外部公開 API として運用しない。

## 8. バージョニング
- `dto_version` は semantic version を採用する。
- 破壊的変更は major 更新。
- minor/patch 更新では既存フィールド互換を維持する。

## 9. U5受入チェック（契約観点）
- [ ] `request_id` が header/body/response/log で一貫している。
- [ ] 非成功時に `error_code` と `retryable` が収集できる。
- [ ] `docs/05-dev/unity-test-result-collection-template.md` で artifact と原因を記録できる。
