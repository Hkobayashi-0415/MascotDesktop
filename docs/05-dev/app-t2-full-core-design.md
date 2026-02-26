# APP-T2 Full Core Integration Detailed Design

- Status: active
- Last Updated: 2026-02-26
- Owner/Agent: codex
- Scope: APP-T2（Full Core接続）の実装前詳細設計（契約・アーキテクチャ・運用・受入条件）

## 根拠
- `docs/05-dev/app-spec-and-roadmap.md`
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/02-architecture/interfaces/ipc-contract.md`
- `docs/05-dev/u5-core-integration-plan.md`

## 1. 目的 / スコープ / 非スコープ

### 目的
1. APP-T2 実装に着手可能な詳細設計を固定する。
2. Endpoint契約、Runtime-Core境界、degraded mode、受入条件、テスト戦略を確定する。
3. 設計未確定を残さず、未確定が必要な場合は non-blocking 凍結として管理する。

### スコープ
- LLM/TTS/STT/health/config の API 契約。
- request/response schema（`request_id`, `core_request_id`, `error_code`, `retryable`, `attempt`）。
- timeout/retry/fallback、エラー分類、HTTPマッピング。
- Unity Runtime と Core の責務境界、`RuntimeConfig` 切替、degraded mode 復帰規則。
- APP-T2 DoD と実装時テスト基準。

### 非スコープ
- 実装コード変更。
- APP-T3（配布パッケージング）実装。
- 外部公開 API としての認証設計。

## 2. Decision Log（確定）

| ID | 決定事項 |
|---|---|
| D01 | Endpoint 形は機能別 REST（Q1=A）。 |
| D02 | 相関IDは Unity `request_id` + Core `core_request_id` の二系統（Q2=C）。 |
| D03 | 部分障害時は機能単位で継続（Q3=B）。 |
| D04 | timeout/retry は endpoint 別最適化（Q4=B, Q13=B）。 |
| D05 | エラースキーマは共通化（Q5=A）。 |
| D06 | 設定優先順位は `default < file < env`（Q6=A）。 |
| D07 | エラーは業務 `error_code` を固定し HTTP と分離マップ（Q7=A, Q20=A, Q27=A）。 |
| D08 | 状態は `processing/partial/final/failed` を採用（Q8=A）。 |
| D09 | 破壊的変更は禁止。必要時は `/v2` を新設（Q9=A）。 |
| D10 | 自動リトライ対象は通信失敗/timeout/HTTP 5xx のみ（Q10=A）。 |
| D11 | 画面表示は通常時抑制、管理者/デバッグモードで詳細表示（Q11, Q14=A）。 |
| D12 | APP-T2 DoD は契約+テスト+文書同期まで必須（Q12=A, Q36=A）。 |
| D13 | エラーは `error_code`（例: E1001）と `error_name`（例: LLM_TIMEOUT）を別フィールド化（Q15, Q16=B）。 |
| D14 | バックオフは指数 + jitter（Q17=B, Q26=A）。 |
| D15 | `/health` は全体+コンポーネント別状態を返す（Q18=B）。 |
| D16 | retry/timeout 初期値は設計で固定し、必要時に実装フェーズで調整（Q19=A, Q22, Q25=B）。 |
| D17 | 再試行時は `request_id` を固定し `attempt` を加算（Q23=A）。 |
| D18 | 凍結項目は「理由・解除条件・見直し期限」を必須（Q24=A）。 |
| D19 | Unity がオーケストレーション主導、Core は機能提供 API（Q28=B）。 |
| D20 | `RuntimeConfig.mode=loopback|core` で明示切替（Q29=A）。 |
| D21 | degraded 復帰は自動 + 管理者固定化併用（Q30=C, Q34=B）。 |
| D22 | 成功/失敗レスポンスは共通 envelope（Q31=A）。 |
| D23 | 必須ログイベントは `received/start/retry/partial/final/error/degraded_enter/degraded_exit`（Q32=A）。 |
| D24 | `/v1/config` は読み取り専用運用（Q33=A）。 |
| D25 | JSON 命名規約は `snake_case`（Q35=A）。 |
| D26 | 未確定事項はなし（本書公開時点）。 |

## 3. Endpoint / 契約設計

### 3.1 Endpoint 一覧（APP-T2）

| 機能 | Method/Path | timeout | max_retry | fallback |
|---|---|---:|---:|---|
| health | `POST /health` | 3s | 0 | `core` 到達不可時は `degraded_enter` 判定のみ実施 |
| LLM | `POST /v1/chat/send` | 45s | 5 | LLM機能のみ degraded 化し、他機能は継続 |
| TTS | `POST /v1/tts/play` | 25s | 3 | TTS失敗時は音声再生をスキップして処理継続 |
| STT | `POST /v1/stt/event` | 15s | 2 | STT機能のみ degraded 化し、LLM/TTS は継続 |
| config(get) | `POST /v1/config/get` | 5s | 0 | ローカル有効設定スナップショットを返却 |

補足:
- `POST /v1/config/set` は APP-T2 では runtime から呼び出さない（read-only 運用）。
- 自動リトライは通信失敗/timeout/HTTP 5xx のみ。429 は自動リトライ対象外。

### 3.2 共通 Request / Response Schema

#### Request Envelope
```json
{
  "dto_version": "1.1.0",
  "request_id": "req-20260226-0001",
  "timestamp_utc": "2026-02-26T12:00:00Z",
  "actor": "runtime",
  "payload": {}
}
```

HTTP Header:
- `X-Request-Id`: body `request_id` と一致必須。

#### Success Response Envelope
```json
{
  "dto_version": "1.1.0",
  "request_id": "req-20260226-0001",
  "core_request_id": "core-20260226-9001",
  "attempt": 0,
  "status": "final",
  "data": {}
}
```

#### Error Response Envelope
```json
{
  "dto_version": "1.1.0",
  "request_id": "req-20260226-0001",
  "core_request_id": "core-20260226-9001",
  "attempt": 2,
  "status": "failed",
  "error_code": "E1001",
  "error_name": "LLM_TIMEOUT",
  "message": "upstream timeout",
  "retryable": true,
  "details": {}
}
```

### 3.3 エラー分類と HTTP マッピング

| 分類 | HTTP | retryable | 備考 |
|---|---:|---|---|
| malformed request | 400 | false | JSON構造不正 |
| validation error | 422 | false | 必須項目欠落、型不一致 |
| rate limit | 429 | false | 自動リトライ対象外 |
| core internal error | 500 | true | Core内部失敗 |
| core unavailable | 503 | true | Core未起動/到達不可 |
| upstream timeout | 504 | true | 上流処理タイムアウト |

初期エラーコード表:

| error_code | error_name | 代表HTTP | retryable |
|---|---|---:|---|
| E1001 | LLM_TIMEOUT | 504 | true |
| E1002 | LLM_UNAVAILABLE | 503 | true |
| E2001 | TTS_TIMEOUT | 504 | true |
| E2002 | TTS_PLAY_FAILED | 500 | true |
| E3001 | STT_TIMEOUT | 504 | true |
| E3002 | STT_EVENT_INVALID | 422 | false |
| E4001 | IPC_HTTP_DISABLED | 503 | true |
| E4002 | IPC_HTTP_REQUEST_FAILED | 500 | true |
| E4003 | IPC_HTTP_NON_SUCCESS | 500 | true |
| E4004 | IPC_HTTP_CONFIG_MISSING | 500 | false |
| E5001 | CONFIG_VALIDATION_FAILED | 422 | false |
| E5002 | HEALTH_COMPONENT_DOWN | 503 | true |

### 3.4 timeout / retry / backoff 規則

- リトライ対象: 通信失敗、timeout、HTTP 5xx。
- `request_id` は再試行で固定。`attempt` を `0..N` で加算。
- 待機時間:
  - `delay = min(8.0, 0.5 * 2^(attempt-1)) * random(0.8, 1.2)` seconds
- 上限超過時は `status=failed` で終了し、対象機能のみ degraded 化する。

## 4. アーキテクチャ設計

### 4.1 Unity Runtime ↔ Core 境界責務

| レイヤ | 主責務 |
|---|---|
| Unity Runtime | UI/HUD、状態遷移、オーケストレーション、`request_id` 発行、`RuntimeConfig` 反映、degraded表示制御 |
| Core | LLM/TTS/STT 実処理、`core_request_id` 発行、コンポーネント health 集約、統一エラー返却 |

### 4.2 RuntimeConfig 切替

- `mode=loopback|core` の明示切替。
- `mode=core` のときに endpoint 群（health/chat/tts/stt/config）を使用。
- 優先順位は `default < file < env`。

必須設定キー（論理名）:
- `runtime_mode`
- `core_base_url`
- `llm_timeout_sec`, `tts_timeout_sec`, `stt_timeout_sec`
- `admin_debug_mode`

### 4.3 degraded mode

- 入口条件:
  - Core 未起動/到達不可
  - timeout/5xx が retry 上限超過
  - コンポーネント個別 health が `down`
- 動作:
  - 失敗した機能のみ degraded 化し、他機能は継続。
  - 通常ユーザー表示は抑制（障害詳細は非表示）。
  - 管理者/デバッグモードではコンポーネント状態と直近エラーを表示。
- 復帰:
  - `health` 3回連続成功で自動復帰。
  - 管理者は手動で degraded 固定化/解除可能。

## 5. 詳細設計

### 5.1 状態遷移（成功/失敗/partial/final）

共通状態:
- `processing`: 処理開始
- `partial`: 中間結果（主に STT）
- `final`: 正常完了
- `failed`: リトライ上限到達または非リトライ対象失敗

遷移:
1. `processing -> final`（成功）
2. `processing -> partial -> final`（ストリーム）
3. `processing -> failed`（即時失敗）
4. `processing -> retry(n) -> final|failed`（再試行）

### 5.2 ログ / 可観測性

必須イベント:
- `received`
- `start`
- `retry`
- `partial`
- `final`
- `error`
- `degraded_enter`
- `degraded_exit`

必須フィールド:
- `request_id`
- `core_request_id`
- `endpoint`
- `attempt`
- `status`
- `error_code`
- `error_name`
- `retryable`
- `duration_ms`

方針:
- payload 本文は保存しない（サイズ/キーのみ）。
- `request_id` と `core_request_id` で Unity/Core 双方を相関可能にする。

### 5.3 設定優先順位

1. `default`（コード既定値）
2. `file`（RuntimeConfig ファイル）
3. `env`（環境変数）

同一キー競合時は上位を採用する。

## 6. 命名規約 / 互換性方針 / 運用ルール

- JSON フィールドは `snake_case`。
- エラーは `error_code` + `error_name` の二項固定。
- `/v1` で破壊的変更は行わない。
- 破壊的変更が必要な場合のみ `/v2` を新設。
- 設計・実装差分が出た場合は `docs/worklog/` に理由を記録し、`NEXT_TASKS` / `dev-status` を同期する。

## 7. APP-T2 完了条件（DoD）

1. 本書の endpoint/契約を実装へ反映し、`/v1` 非破壊を維持している。
2. `RuntimeConfig.mode=core` で実 endpoint へ切替できる。
3. degraded mode（通常表示抑制 + 管理者/デバッグ表示 + 自動復帰3回）が動作する。
4. エラー共通スキーマ（`error_code`, `error_name`, `retryable`）を全 endpoint で返却する。
5. `request_id` / `core_request_id` / `attempt` の相関がログで追跡できる。
6. `docs/NEXT_TASKS.md` / `docs/05-dev/dev-status.md` / `docs/worklog/` が同期される。

## 8. テスト戦略（APP-T2 実装時）

### 8.1 EditMode（必須 4 スイート）
- `CoreOrchestratorLlmIntegrationTests`（5/5）
- `CoreOrchestratorTtsIntegrationTests`（3/3）
- `CoreOrchestratorSttIntegrationTests`（4/4）
- `LoopbackHttpClientTests`（5/5）

### 8.2 手動確認（必須）
1. `mode=core` で LLM/TTS/STT 正常系が動作する。
2. Core停止時に機能別 degraded へ遷移する（全停止しない）。
3. 管理者/デバッグモードで障害詳細が可視化される。
4. 通常ユーザーでは障害詳細が非表示である。
5. health 3連続成功で自動復帰する。
6. エラー時に `error_code`/`error_name`/`retryable` が記録される。

## 9. non-blocking 凍結

| ID | 凍結項目 | 理由 | 解除条件 | 見直し期限 |
|---|---|---|---|---|
| NF-01 | timeout 秒数の微調整（45/25/15/3/5 を初期値） | 実 Core レイテンシは環境差があるため | APP-T2 実装で 4スイート + 手動確認後に必要時調整 | APP-T2 実装完了時 |

未確定事項: なし（本書時点）。
