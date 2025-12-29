# IPC Contract (PoC draft)

## 方針
- ローカル専用。Transportは未確定（HTTP localhost or named pipe）。DTOはJSON前提、`dto_version`で将来のProtobuf移行を吸収。
- すべてのリクエストに `request_id`、`timestamp`, `actor` (user/local) を付与。
- エラー形式: `{ dto_version, request_id, status: "error", code, message, details? }`。監査対象イベントでは必ずログ発火。
- PoC最小セットと将来拡張を分離。

## PoC最小セット (Phase1/2)
- ChatSendRequest/Response: テキスト送信、LLM応答（音声クリップ優先/TTSフォールバックのメタ含む）。
- AudioPlayRequest: クリップID or カテゴリ指定、割込み許可フラグ、音量。
- AvatarSetStateRequest: state遷移要求（idle/talk/notify/sleep/on/smile/oko 等）、avatar_mode指定可。
- WindowEvent: drag/move/position-save/topmost-toggle/clickthrough-toggle/safe-unlock。
- ReminderUpsertRequest: リマインド登録/更新/削除フラグ。
- HealthCheck: service=core/memory/shell/audio/avatar/scheduler。
- ConfigGet/Set: スカラー設定（Topmost, clickthrough, screenshot exclusion, thresholds 等）。

## 将来拡張
- Streamingチャット（分割トークン）、ツール/MCP呼び出し、メモリ検索API、キャラCRUD/公開、プロンプトバージョン操作、インポート/エクスポート。
- STTイベント (wakeword detected, partial transcript)、話者認識結果。

## 監査ログ発火点
- キャラ/プロンプトの変更、インポート/エクスポート、設定変更(Topmost/透過/鍵含む)、リマインドCRUD、オーディオ再生要求、ウィンドウ制御要求。

## バージョニング
- `dto_version`: string (例 "0.1.0")。破壊的変更時にメジャー更新。
- Transportは後日確定。エンドポイント/pipe名は `core/chat`, `core/config`, `shell/window`, `shell/avatar`, `shell/audio`, `scheduler/reminder`, `health` などを想定。
