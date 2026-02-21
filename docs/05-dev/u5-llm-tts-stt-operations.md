# U5 LLM/TTS/STT Operations (Kickoff)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-21
- Scope: U5-T4（LLM/TTS/STT 段階統合と失敗時復旧の運用化）

## 根拠
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/05-dev/u5-core-integration-plan.md`
- `docs/02-architecture/interfaces/ipc-contract.md`
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/05-dev/unity-test-result-collection-template.md`

## 1. 運用方針（U5-T4）
- U5-T4 は一括切替しない。`LLM -> TTS -> STT` の順で段階統合する。
- 各段階で `request_id` / `error_code` / `retryable` の可観測性を確認してから次段階へ進む。
- 失敗時は段階ごとに rollback し、前段階の安定状態へ戻す。

## 2. 実施フェーズ

### Phase A: LLM Integration
1. Chat 応答経路を外部Core優先に切替する。
2. 応答失敗時は Runtime fallback（state遷移維持）を確認する。
3. `request_id` で chat 要求から state 反映まで追跡する。

完了条件:
- chat 成功/失敗の両系で `error_code` が記録される。
- `retryable` 判定を記録できる。

### Phase B: TTS Integration
1. 応答テキストから TTS 再生要求までの経路を接続する。
2. TTS 失敗時に `retryable=true/false` が区別されることを確認する。
3. 再試行方針を運用ルールに明記する。

完了条件:
- 再生成功時に state/motion が期待どおり反映される。
- 失敗時の復旧手順で運用継続できる。

### Phase C: STT Integration
1. STT partial/final 入力イベントを Runtime に接続する。
2. partial/final の扱い差（確定トリガー有無）を明文化する。
3. 誤認識時の復旧フロー（再入力/無視）を定義する。

完了条件:
- partial/final を区別したログが残る。
- 誤認識時に state が破綻しない。

## 3. 失敗時復旧手順（標準）
1. 失敗操作の `request_id` を取得する（HUD表示/RuntimeLog）。
2. `error_code` / `retryable` を確認して一次分類する。
3. `retryable=true` の場合は同一操作を1回のみ再試行する。
4. 再試行失敗または `retryable=false` の場合、該当フェーズを rollback して前段階へ戻す。
5. `docs/worklog` に原因・影響・戻した範囲を記録する。

## 4. 判定マトリクス
| category | 判定基準 | 標準対応 |
|---|---|---|
| Retryable transport | `IPC.HTTP.REQUEST_FAILED` 等で `retryable=true` | 1回再試行し、失敗なら前段階へ戻す |
| Non-retryable contract | `IPC.HTTP.REQUEST_ID_MISMATCH` / schema不整合 | 即時停止、契約テストを先に修正 |
| Non-retryable business | `retryable=false` の Core エラー | fallback 運用へ切替し、実装修正まで凍結 |

## 5. 実行結果記録テンプレート（U5-T4）
```markdown
| run_at | phase | operation | request_id | pass_fail | error_code | retryable | artifact_xml | artifact_log | cause |
|---|---|---|---|---|---|---|---|---|---|
| 2026-02-21 12:00:00 | Phase A | chat/send | req-xxxx | Passed | - | - | `Unity_PJ/artifacts/test-results/....xml` | `...log` | all checks passed |
```

## 6. U5-T4 完了条件
- Phase A/B/C の運用手順が `docs/05-dev` に明文化されている。
- 失敗時復旧手順（retryable 判定含む）が運用可能な粒度で定義されている。
- `NEXT_TASKS` / `dev-status` / `worklog` が同一状態に同期されている。

## 参照
- `docs/05-dev/u5-core-integration-plan.md`
- `docs/02-architecture/interfaces/ipc-contract.md`
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/05-dev/unity-test-result-collection-template.md`
- `docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md`
- `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md`
