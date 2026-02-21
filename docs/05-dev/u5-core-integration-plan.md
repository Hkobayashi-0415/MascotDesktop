# U5 Core Integration Plan (Unity Boundary)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-21
- Scope: U5（Core統合）の実行計画、Unity境界契約、段階移行受入条件

## 根拠
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `Unity_PJ/spec/latest/spec.md:49`
- `Unity_PJ/spec/latest/spec.md:53`
- `Unity_PJ/spec/latest/spec.md:102`
- `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md:33`
- `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs:47`
- `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs:48`

## 1. 目的（U5で確定すること）
1. LLM/TTS/STT 連携の Unity統合方針を確定する。
2. Unity境界の IPC契約（`request_id` / `error_code`）を固定する。
3. 段階移行（Minimal Core -> Full Core）の受入条件を定義する。

## 2. 現在ベースライン（U4完了時点）
- Unity Runtime は in-process で `CoreOrchestrator` / `AvatarStateController` / `MotionSlotPlayer` を持つ。
- `LoopbackHttpClient` は loopback HTTP 呼び出し機能を持つが、Core統合運用は未確立。
- 画面確認は Runtime HUD ベースで可能（モデル切替、状態遷移、motion、window操作）。

## 3. 段階移行（実行フェーズ）

### U5-P1: Minimal Core Bridge
- 目的:
  - Unityから loopback HTTP 経由で最小エンドポイント（health/chat/config）へ接続し、`request_id` を一貫伝搬。
- 画面上の到達状態:
  - HUD操作時に `request_id` が UnityログとIPCログで同一追跡できる。
  - chat要求が local rule のみでなく外部Core応答の受信経路を持つ。
- 受入条件:
  - `X-Request-Id` と body `request_id` の一致。
  - 非成功応答時に `error_code` 付きで Unity側ログへ反映。

### U5-P2: LLM Integration
- 目的:
  - Chat応答を外部Core経由に切替し、state遷移連携を維持。
- 画面上の到達状態:
  - chat操作で応答結果に応じた state 遷移が維持される。
- 受入条件:
  - chat応答失敗時に fallback state 遷移と明示エラーを確認。
  - request_id 単位で chat要求から state反映まで追跡可能。

### U5-P3: TTS/STT Integration
- 目的:
  - TTS再生要求と STTイベント入力を Unity境界へ統合。
- 画面上の到達状態:
  - TTS要求が再生系に到達し、STTイベントが state/motion 遷移へ反映可能。
- 受入条件:
  - TTS失敗時に retryable 判定を含む `error_code` が返る。
  - STT partial/final を区別したイベント記録が残る。

### U5-P4: Full Core Cutover
- 目的:
  - Minimal Core 経路を置換し、U5運用を標準化。
- 画面上の到達状態:
  - Runtime HUD からの主要操作（chat/state/motion）が外部Core連携で一貫動作。
- 受入条件:
  - 主要操作すべてで `request_id` / `error_code` / `retryable` が整合。
  - 失敗時復旧手順が `docs/05-dev` に反映済み。

## 4. 境界契約（要点）
- Transport: loopback HTTP (`127.0.0.1`) + JSON DTO
- Correlation: `X-Request-Id` と body `request_id` を必須一致
- Error payload: `status`, `error_code`, `message`, `retryable`, `request_id`
- Logging: payload本文は保存せずメタ情報中心（キー/サイズ/処理時間）

詳細は `docs/02-architecture/interfaces/ipc-contract.md` を正とする。

## 5. テスト方針（U5）
- EditMode:
  - contract validation（`request_id`一致、error schema、timeout mapping）
  - orchestrator と bridge の連携単体
- PlayMode/Manual:
  - Runtime HUD 操作時の end-to-end 相関確認
  - 失敗系（非成功応答、timeout）での表示・ログ確認
- Artifact:
  - `Unity_PJ/artifacts/test-results/` に XML/LOG を保存

## 6. リスクと対策
- 外部Core未起動で連携失敗:
  - `IPC.HTTP.DISABLED` / `IPC.HTTP.REQUEST_FAILED` を明示して fallback維持。
- 契約不一致（DTO drift）:
  - `dto_version` と契約テストを gate 化。
- 運用複雑化:
  - U5-P1 -> P4 の段階移行を厳守し、一括切替を避ける。

## 7. U5-T4 運用導線
- 段階統合と失敗時復旧は `docs/05-dev/u5-llm-tts-stt-operations.md` を正とする。
- U5-P2/P3/P4 実行時は同ドキュメントのフェーズ定義と記録テンプレートを使用する。

## 8. 完了定義（U5ドキュメント実行）
- U5方針文書が存在し、段階移行と受入条件が定義されている。
- IPC契約が Unity境界向けに再定義されている。
- `NEXT_TASKS` と `dev-status` に U5実行状態が同期されている。

