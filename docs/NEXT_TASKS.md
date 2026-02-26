# NEXT_TASKS (Unity Migration Plan)

## 改訂履歴
| Rev | Date | 変更内容 | 根拠 |
|---|---|---|---|
| R40 | 2026-02-26 | APP-T2 実装着手（Phase 1-2）。`RuntimeConfig` の `mode=loopback|core` と endpoint別 timeout/retry、`LoopbackHttpClient` の retry/backoff・拡張スキーマ（`core_request_id/error_name/attempt`）、`CoreOrchestrator` degraded管理を反映。テスト実行は Unity起動前失敗で未完了。 | `Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs`, `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`, `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`, `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`, `docs/worklog/2026-02-26_app_t2_impl_phase12.md` |
| R39 | 2026-02-26 | APP-T2 実装前詳細設計を確定。Endpoint契約、Runtime/Core境界、degraded mode、DoD、テスト戦略を `app-t2-full-core-design.md` に固定し、実装着手条件を明文化。 | `docs/05-dev/app-t2-full-core-design.md`, `docs/worklog/2026-02-26_app_t2_full_core_design.md` |
| R38 | 2026-02-26 | APP-T1 完了。アプリケーション機能仕様（`app-spec-and-roadmap.md`）を新規作成し、F-01〜F-12 の受入条件・Phase分割・依存関係・リスク対策を確定。APP-T2/T3 が着手可能な粒度で定義。 | `docs/05-dev/app-spec-and-roadmap.md`, `docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md` |
| R37 | 2026-02-26 | 技術的負債対応。`run_u8_ops_checks.ps1` を異常系でも run summary を必ず出力する実装へ修正し、`diagnose_u8_scheduler.ps1` に executable probe 再試行診断を追加。legacy docs 同期チェックを再実行し同期済みを確認。 | `tools/run_u8_ops_checks.ps1`, `tools/diagnose_u8_scheduler.ps1`, `Unity_PJ/artifacts/manual-check/debtfix-pass/u8_ops_checks_run_gate_20260226_125744.json`, `Unity_PJ/artifacts/manual-check/debtfix-fail/u8_ops_checks_run_gate_20260226_125755.json`, `Unity_PJ/artifacts/manual-check/u8_docs_sync_debtfix_20260226_1300.json` |
| R36 | 2026-02-26 | U9最終クローズを再検証。運用ゲート1コマンド判定の正常/異常証跡を再採取し、Scheduler診断の最新結果（`can_register=false`）を管理済み制約として再固定。App Dev移行可判定を維持。 | `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_gate_20260226_000657.json`, `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_summary_gate_20260226_000716.json`, `Unity_PJ/artifacts/manual-check/u8_scheduler_diag_closure_20260226_000657.json`, `docs/worklog/2026-02-26_u9_final_closure_revalidation.md` |
| R35 | 2026-02-25 | U9最終安定化クローズ。環境依存Schedulerを「管理済み制約（non-blocking）」として整理（今回セッションは `can_register=true` ）、最終ゲートチェック（Gate profile Pass）証跡採取、App Dev移行判定を文書化。 | `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_gate_20260225_232337.json`, `docs/worklog/2026-02-25_u9_final_closure.md` |
| R34 | 2026-02-25 | U8鮮度チェックの互換性不具合を修正（`Profile=Any` + 旧artifactの`profile`欠損で例外停止）。再監査でクラッシュ解消と再実行性を確認。 | `tools/check_u8_ops_freshness.ps1`, `Unity_PJ/artifacts/manual-check/audit_fix_freshness_any_default_20260225_220408.json`, `docs/worklog/2026-02-25_u8_ops_freshness_fix_audit.md` |
| R33 | 2026-02-25 | U8残件4項目一括実装（証跡整合クリーンアップ / 鮮度チェック / Fail記録テンプレ / Scheduler事前診断）。Scheduler実登録成功（断続的環境依存）を確認し文書化。 | `tools/check_u8_ops_freshness.ps1`, `tools/diagnose_u8_scheduler.ps1`, `docs/worklog/_templates/u8_ops_fail_template.md`, `docs/05-dev/u8-operations-automation.md`, `docs/worklog/2026-02-25_u8_ops_extension_closeout.md` |
| R32 | 2026-02-25 | U8運用チェックを一気通貫化。`Daily/Gate` プロファイルを追加し、Task Scheduler 登録/削除スクリプトを整備。 | `tools/run_u8_ops_checks.ps1`, `tools/register_u8_ops_checks_task.ps1`, `tools/unregister_u8_ops_checks_task.ps1`, `docs/05-dev/u8-operations-automation.md`, `docs/worklog/2026-02-25_u8_ops_full_closeout.md` |
| R31 | 2026-02-25 | U8運用定着を実施。実ログパス接続で一括チェック（`run_u8_ops_checks`）を実行し、失敗時一次対応を文書化。 | `tools/run_u8_ops_checks.ps1`, `docs/05-dev/u8-operations-automation.md`, `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_custom_20260225_174453.json`, `docs/worklog/2026-02-25_u8_operations_operationalization.md` |
| R30 | 2026-02-24 | U8運用自動化を実装。`ui.hud.bootstrap_missing` 連続監視チェックと Unity導線/legacy文書同期チェックを追加し、運用基準を文書化。 | `tools/check_runtime_bootstrap_missing.ps1`, `tools/check_unity_legacy_docs_sync.ps1`, `docs/05-dev/u8-operations-automation.md`, `docs/worklog/2026-02-24_u8_operations_automation.md` |
| R29 | 2026-02-25 | 旧PoC文書（`PACKAGING.md` / `RESIDENT_MODE.md`）を legacy-reference として明示し、現行 Unity導線への参照を追記。U7完了後の文書整合タスクを同期。 | `docs/PACKAGING.md`, `docs/RESIDENT_MODE.md`, `docs/worklog/2026-02-25_legacy_docs_alignment.md`, `docs/05-dev/dev-status.md` |
| R28 | 2026-02-25 | U7-T5 差分に対する EditMode 再実行（Unity Editor 終了後）を完了。4スイート 50/50 Passed を確認し、worklog 追補・dev-status 同期。 | `docs/worklog/2026-02-24_u7_t4_t5_execution.md`（追補）, `docs/05-dev/dev-status.md` |
| R27 | 2026-02-24 | U7-T4/U7-T5 を実施完了。Standalone runtime証跡（必須イベント5種）と bootstrap recovery 非依存化（HUD自己回復の撤去、運用手順同期）を反映。 | `docs/worklog/2026-02-24_u7_t4_t5_execution.md`, `docs/05-dev/QUICKSTART.md`, `docs/05-dev/unity-runtime-manual-check.md`, `docs/05-dev/dev-status.md` |
| R26 | 2026-02-24 | 根本治療リファクタ（`AssetCatalogService` / `WindowNativeGateway`）と U7 4スイート検証結果（最終 50/50 Passed）を反映し、リリース後安定化フェーズ `U7` と残タスクを追加。 | `docs/worklog/2026-02-24_root_cause_refactor_execution.md`, `docs/worklog/2026-02-24_test_execution_u7_four_suites.md`, `docs/05-dev/dev-status.md` |
| R25 | 2026-02-23 | `run_unity_tests.ps1 -RequireArtifacts` を4スイートで再実行（00:07-00:08）。STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5 で全 Passed、artifact（xml/log）全件生成を確認。R3 Pass 根拠を追補し、R4 Done 判定を維持。 | `Unity_PJ/artifacts/test-results/editmode-20260223_000743.xml`, `Unity_PJ/artifacts/test-results/editmode-20260223_000753.xml`, `Unity_PJ/artifacts/test-results/editmode-20260223_000803.xml`, `Unity_PJ/artifacts/test-results/editmode-20260223_000813.xml`, `docs/worklog/2026-02-23_r4_postclosure_script_rerun_sync.md`, `docs/05-dev/dev-status.md`, `docs/05-dev/release-completion-plan.md` |
| R24 | 2026-02-22 | R1/R2 Conditional Pass の「リリース完了扱い（Unity Scope）」基準を明文化し、R4 を Done 化。RLS-R4-01/02 の state/blocker/completion condition を3文書で一致。 | `docs/05-dev/release-completion-plan.md`, `docs/05-dev/dev-status.md`, `docs/worklog/2026-02-22_r4_closure_fullquality.md` |
| R23 | 2026-02-22 | `run_unity_tests.ps1 -RequireArtifacts` で4スイート再実行（22:23-22:24）。Loopback 5/5, STT 4/4, TTS 3/3, LLM 5/5 で全 Passed、artifact（xml/log）全件生成を確認。R3 Pass 根拠を補強。R4 は R1/R2 Conditional Pass のリリース完了扱い未確定のため In Progress 継続。 | `Unity_PJ/artifacts/test-results/editmode-20260222_222339.xml`, `Unity_PJ/artifacts/test-results/editmode-20260222_222350.xml`, `Unity_PJ/artifacts/test-results/editmode-20260222_222401.xml`, `Unity_PJ/artifacts/test-results/editmode-20260222_222412.xml`, `docs/worklog/2026-02-22_rls_docsync_script_success_sync.md`, `docs/05-dev/dev-status.md`, `docs/05-dev/release-completion-plan.md` |
| R22 | 2026-02-22 | R3/R4 判定根拠を再整理。Unity.exe 直接実行（18:02-18:07）で STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5 全 Passed。`run_unity_tests.ps1` は 20:54-20:58 の再テストで artifact 待機が機能した一方、21:31-21:32 の再実行では Unity.exe/Unity.com とも起動前失敗（exit 1）。R4 は R1/R2 Conditional Pass のリリース完了扱い未確定のため In Progress。 | `docs/worklog/2026-02-22_unity_recovery_r3_pass.md`, `docs/worklog/2026-02-22_deepfix_rls_docsync.md`, `docs/05-dev/dev-status.md`, `docs/05-dev/release-completion-plan.md` |
| R21 | 2026-02-22 | RLS-S1 Release Gate Execution 実施。R1/R2 を Conditional Pass 判定（旧PoC手順は未更新、Unity側導線は整合）。R3-02 4スイートは当時記録で起動前失敗・artifact未生成（`174300`/`174535`/`174556`/`174722`）。その後、同タイムスタンプ artifact が存在し XML は全件 Passed であることを確認。R4 は当時 R3 Pass 待ちで Pending。 | `docs/worklog/2026-02-22_rls_s1_gate_execution.md`, `docs/05-dev/dev-status.md`, `docs/05-dev/release-completion-plan.md` |
| R20 | 2026-02-21 | RLS-T2 最終バッチ回帰を追補。4スイート（STT/TTS/LLM/Loopback）が起動前失敗（`指定されたモジュールが見つかりません`）で artifact 未生成となり、`-RequireArtifacts` が全件 `exit 1` を検知（`222310` / `222441` / `222831` / `222910`）。 | `docs/worklog/2026-02-21_u6_completion_and_release_planning.md`, `docs/worklog/2026-02-21_rls_t2_result_sync.md`, `docs/05-dev/dev-status.md` |
| R19 | 2026-02-21 | U6を完了化。U6-T2（回帰品質ゲート運用手順/記録テンプレート）を追加し、U6のDoDを更新。続けて旧計画（P0-P6）参照のリリース完了計画を新設。最終バッチ回帰はユーザー実行に引き継ぎ。 | `docs/05-dev/u6-regression-gate-operations.md`, `docs/05-dev/release-completion-plan.md`, `docs/05-dev/dev-status.md`, `docs/worklog/2026-02-21_u6_completion_and_release_planning.md` |
| R18 | 2026-02-21 | U6-T1 の `-RequireArtifacts` 動作確認を追補。4スイート（STT/TTS/LLM/Loopback）すべてで起動前失敗時の artifact 未生成を `exit 1` として検知（`213239` / `213407` / `213441` / `213446`）。 | `docs/worklog/2026-02-21_u6_t1_kickoff.md`, `docs/05-dev/dev-status.md` |
| R17 | 2026-02-21 | U6（回帰品質ゲート運用）を開始。目的/スコープ/DoDを新設し、U6-T1 として `run_unity_tests.ps1` に `-RequireArtifacts` を追加。4スイート回帰はこの環境で起動前エラー（Unity.exe / Unity.com: `指定されたモジュールが見つかりません`）となり、artifact未生成を記録。 | `tools/run_unity_tests.ps1`, `docs/05-dev/dev-status.md`, `docs/worklog/2026-02-21_u6_t1_kickoff.md` |
| R16 | 2026-02-21 | U5-T4 Phase C（STT統合）の4スイート検証が完了（STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）。Phase A/B/C 全てartifact確認済みとして U5-T4 を Done 化。 | `Unity_PJ/artifacts/test-results/editmode-20260221_195019.xml`, `Unity_PJ/artifacts/test-results/editmode-20260221_195037.xml`, `Unity_PJ/artifacts/test-results/editmode-20260221_195406.xml`, `Unity_PJ/artifacts/test-results/editmode-20260221_200517.xml`, `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md`, `docs/worklog/2026-02-21_u5_t4_phase_c_progress_sync.md` |
| R15 | 2026-02-21 | U5-T4 Phase C（STT統合）を実装着手。`SendSttWithBridgeResult` と HUD の `/v1/stt/event` 導線、STT統合テストを追加。検証はこの環境で起動前エラーにより保留。 | `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`, `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`, `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorSttIntegrationTests.cs`, `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md` |
| R14 | 2026-02-21 | U5-T4 Phase B（TTS統合）に着手。`SendTtsWithBridgeResult` と HUD の `/v1/tts/play` 連結を追加し、TTS統合テストを新規作成（この環境で実行検証は起動前エラーにより保留）。 | `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`, `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`, `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`, `docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md` |
| R13 | 2026-02-21 | U5-T4 Phase A（LLM統合）を完了。`CoreOrchestrator.SendChatWithBridgeResult` 追加・`RuntimeDebugHud` bridge結果連携・`CoreOrchestratorLlmIntegrationTests` 3/3 Pass。 | `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`, `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`, `docs/worklog/2026-02-21_u5_t4_phase_a_llm_impl.md` |
| R12 | 2026-02-21 | U5-T4 を実行開始。段階統合/復旧手順を `docs/05-dev` に新設し、状態同期を更新。 | `docs/05-dev/u5-llm-tts-stt-operations.md`, `docs/worklog/2026-02-21_u5_t4_operations_kickoff.md` |
| R11 | 2026-02-21 | U5-T3 完了。`LoopbackHttpClientTests` 5/5 Pass（`LogAssert.Expect` 追加で全件通過）。 | `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`, `docs/worklog/2026-02-21_u5_t3_logassert_fix.md` |
| R10 | 2026-02-20 | U5-T3 契約テスト自動化に着手。`LoopbackHttpClient` に `retryable` / response `request_id` 検証を実装し、EditMode テストを追加（環境エラーで検証保留）。 | `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`, `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`, `docs/worklog/2026-02-20_u5_t3_contract_test_automation.md` |
| R9 | 2026-02-20 | U5-T2 を完了。Loopback/RuntimeErrorHandling/SimpleModelBootstrap の3スイート再実行で全通過を確認。 | `docs/worklog/2026-02-20_u5_t2_minimal_core_bridge.md`, `Unity_PJ/artifacts/test-results/editmode-20260220_154422.xml`, `Unity_PJ/artifacts/test-results/editmode-20260220_154445.xml`, `Unity_PJ/artifacts/test-results/editmode-20260220_154500.xml` |
| R8 | 2026-02-20 | U5-T2 実装に着手。Runtime HUD から health/chat/config の bridge 導線を追加（環境エラーにより検証は保留）。 | `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`, `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`, `docs/worklog/2026-02-20_u5_t2_minimal_core_bridge.md` |
| R7 | 2026-02-20 | U5-T1（方針/契約/受入条件定義）を完了し、U5を実行フェーズ（In Progress）へ更新。 | `docs/05-dev/u5-core-integration-plan.md`, `docs/02-architecture/interfaces/ipc-contract.md`, `docs/worklog/2026-02-20_u5_execution_docs.md` |
| R6 | 2026-02-20 | U4-T4 を完了。`docs/05-dev` の主導線を Unity前提へ統一し、legacy手順を参照分離。 | `docs/05-dev/QUICKSTART.md`, `docs/05-dev/unity-runtime-manual-check.md`, `docs/worklog/2026-02-20_u4_docs_unification.md` |
| R5 | 2026-02-20 | U4のキャラクター切替導線を標準手順化し、U4継続タスクを再定義。 | `docs/05-dev/unity-character-switch-operations.md`, `docs/worklog/2026-02-20_u4_character_switch_kickoff.md` |
| R4 | 2026-02-19 | U3-T1/U3-T2/U4-T1 を完了。Unityテスト環境復旧手順・結果収集テンプレ・dev-status 同期を反映。 | `docs/05-dev/unity-test-environment-recovery.md`, `docs/05-dev/unity-test-result-collection-template.md`, `docs/worklog/2026-02-19_unity_test_ops_standardization.md` |
| R3 | 2026-02-19 | 既知モデル課題を解消済みへ更新し、関連タスク状態を反映。 | `docs/worklog/2026-02-19_test_run_debug.md` |
| R2 | 2026-02-19 | 旧PoC中心の `P0-P6` 計画を Unity移行後の計画に更新。旧計画は対比表に集約。 | `docs/worklog/2026-02-07_unity-pj-restructure.md`, `docs/worklog/2026-02-07_unity-pj-cutover.md`, `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md` |
| R1 | 2026-01-08 | 旧PoC向け `P0-P6` タスクを更新（manifest/smoke/packaging/resident）。 | 旧版 `docs/NEXT_TASKS.md` |

## 旧計画との対比（Pre-Unity vs Unity）
| 旧計画 | 旧計画の主題 | Unity移行後の対応計画 | 現在状態 | 根拠 |
|---|---|---|---|---|
| P0-P2 | PoC起動導線、manifest、packaging準備 | U0: Unity基盤切替とリポジトリ再編 | 完了 | `docs/worklog/2026-02-07_unity-pj-restructure.md`, `docs/worklog/2026-02-07_unity-pj-cutover.md` |
| P3-P4 | Launcher/Resident UX | U1: Unity Runtime基盤（Avatar表示、基本操作、診断） | 完了 | `docs/worklog/2026-02-07_unity-pj-cutover.md`, `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md` |
| P5 | キャラ切替・運用改善 | U4: Unity上の運用UX整備（キャラ切替導線、運用手順） | 完了 | 本ファイル「U4」 |
| P6 | Core統合（LLM/TTS/STT） | U5: Core統合フェーズ | 完了 | 本ファイル「U5」 |
| 追加課題 | 旧計画にない障害解析基盤 | U2: エラーハンドリング/ログ強化 | 完了 | `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`, `docs/worklog/2026-02-19_test_run_debug.md` |

## Unity移行計画（現行）

### U0: 移行基盤と切替（Done）
- [x] Active root を Unity前提へ切替
- [x] 旧workspaceを物理分離（legacy参照）
- [x] Unity用 spec/docs 構造の再初期化

### U1: Runtime基盤（Done）
- [x] Runtime側モデルロード基盤の整備
- [x] PMX/VRM表示の基本ルートを運用可能化
- [x] 障害切り分け用の診断ログ導線を整備

### U2: エラーハンドリング/ログ強化（Done）
- [x] 失敗情報 DTO の統一
- [x] `request_id` の縦断伝搬
- [x] Reflection fallback hardening（whitelist + cache）
- [x] RuntimeLog 強化（非同期/rotation/retention/level/filter）
- [x] silent catch の可観測化
- [x] 追加テストの整備と通過確認

### U3: 品質ゲート整備（Done）
- [x] `RuntimeErrorHandlingAndLoggingTests` の安定通過（7/7）
- [x] `SimpleModelBootstrapTests` の回帰解消と通過（34/34）
- [x] Unityテスト実行環境の依存差分吸収手順を標準化（`docs/05-dev/unity-test-environment-recovery.md`）
- [x] 実行環境差異時の artifact 取得手順を統一（`docs/05-dev/unity-test-result-collection-template.md`）

### U4: Unity運用UX整備（Done）
- [x] キャラクター切替導線の Unity運用版整理（`docs/05-dev/unity-character-switch-operations.md`）
- [x] 運用ドキュメントの Unity前提一本化（`docs/05-dev/QUICKSTART.md`, `docs/05-dev/unity-runtime-manual-check.md`）
- [x] 既知モデル課題の追跡テンプレート統一
- [x] `NEXT_TASKS` / `dev-status` の同期更新（U4-T1）

### U5: Core統合（Done）
- [x] LLM/TTS/STT 連携の Unity統合方針を確定（`docs/05-dev/u5-core-integration-plan.md`）
- [x] IPC/契約（error_code/request_id）を Unity境界で再定義（`docs/02-architecture/interfaces/ipc-contract.md`）
- [x] 段階移行（Minimal Core -> Full Core）の受入条件を定義（`docs/05-dev/u5-core-integration-plan.md`）
- [x] Minimal Core Bridge 実装（health/chat/config の loopback 経路を Runtime HUD 操作に接続）
- [x] 契約テスト整備（request_id 一致、error schema、timeout mapping）— `LoopbackHttpClientTests` 5/5 Pass
- [x] Phase A: LLM統合（chat経路の外部Core優先化・fallback維持・request_id/error_code/retryable 可観測性）— `CoreOrchestratorLlmIntegrationTests` 5/5 Pass
- [x] Phase B: TTS統合（chat導線から `/v1/tts/play` 連結、成功/失敗時 motion 反映、retryable可観測化）— `CoreOrchestratorTtsIntegrationTests` 3/3 Pass（`docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md` の追補）
- [x] Phase C: STT統合（partial/final 分離、誤認識時復旧、chat導線連結）— `CoreOrchestratorSttIntegrationTests` 4/4 Pass（`editmode-20260221_195019.xml`）
- [x] Phase B/C: TTS/STT の段階統合と運用手順反映（`docs/05-dev/u5-llm-tts-stt-operations.md`）

### U6: 回帰品質ゲート運用（Done）
- 目的: 主要4スイート（STT/TTS/LLM/Loopback）の回帰運用を継続可能化し、起動前失敗やartifact欠落を見落とさない。
- スコープ:
  - `tools/run_unity_tests.ps1` へ artifact 必須判定（`-RequireArtifacts`）を実装する。
  - 指定4スイート実行時に、起動前失敗内容と artifact 生成有無を必ず記録する。
  - 運用手順と結果記録テンプレートを `docs/05-dev` に標準化する。
  - `NEXT_TASKS` / `dev-status` / `worklog` の状態同期を維持する。
- 非スコープ:
  - Unity実行環境の再構築・再インストール。
  - 自動通知・CI連携の実装。
- 完了条件（DoD）:
  - [x] U6の目的/スコープ/DoD が `docs/NEXT_TASKS.md` に定義されている。
  - [x] U6-T1 が実装され、`tools/run_unity_tests.ps1` に `-RequireArtifacts` が追加されている。
  - [x] `-RequireArtifacts` の artifact 未生成検知が4スイートで確認され、`docs/worklog/2026-02-21_u6_t1_kickoff.md` に追補済み。
  - [x] U6-T2（運用手順/記録テンプレート）が `docs/05-dev/u6-regression-gate-operations.md` に定義されている。
  - [x] 最終バッチ回帰実行をユーザー実行として引き継ぐ方針が `dev-status` / `worklog` に明記されている。

### U7: リリース後安定化・保守性強化（Done）
- [x] 高頻度候補探索の根本治療（`AssetCatalogService`: cache + empty-scan backoff）
- [x] Windowing ネイティブ境界の責務統合（`WindowNativeGateway` 導入）
- [x] U7対象4スイート（AssetCatalog/WindowNativeGateway/SimpleModelBootstrap/RuntimeLog）最終通過確認（50/50）
- [x] Standalone 実機の安定稼働確認（Hide/Show/Topmost/Drag + ログ証跡）を最新実装で再採取
- [x] Runtime 初期化時の `ui.hud.bootstrap_recovered` 発生条件を運用側で解消（通常起動は bootstrap recovery 非依存）
- [x] 旧PoC文書（`PACKAGING.md` / `RESIDENT_MODE.md`）に legacy-reference と Unity導線参照を明記

### U8: 運用自動化（Done）
- [x] `runtime-*.jsonl` の `ui.hud.bootstrap_missing` 連続出力監視を自動化（既定: 3連続, gap 5秒）
- [x] Unity導線更新時の legacy文書同期漏れチェックを自動化（`QUICKSTART` / `unity-runtime-manual-check` vs `PACKAGING` / `RESIDENT_MODE`）
- [x] 判定基準と再実行手順を `docs/05-dev/u8-operations-automation.md` に明文化
- [x] 一括実行ラッパー（`run_u8_ops_checks`）と失敗時一次対応を運用手順へ反映し、実ログで実行証跡を採取
- [x] 推奨運用プロファイル（`Daily`/`Gate`）と Task Scheduler 導線（登録/削除）を追加

## 新規タスク（2026-02-19 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U3-T1 | Unityテスト環境差異の標準復旧手順を `docs/05-dev` に明文化 | High | Done | `docs/05-dev/unity-test-environment-recovery.md` を作成し、モジュール不足時手順を定義済み |
| U3-T2 | テスト実行結果の収集フォーマット統一（artifact path, pass/fail, cause） | High | Done | `docs/05-dev/unity-test-result-collection-template.md` を作成し、再利用テンプレを定義済み |
| U4-T1 | Unity移行後の運用タスクを `NEXT_TASKS` と `dev-status` で同期 | Med | Done | `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` の状態表現を同期済み |
| U4-T2 | 既知モデル課題の管理粒度を統一（症状/原因/回避/検証） | Med | Done | 既知課題を解消済みへ更新し、追跡状態を明確化した |

## 追加タスク（2026-02-20 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U4-T3 | キャラクター切替導線の Unity運用版手順を `docs/05-dev` に標準化 | High | Done | `docs/05-dev/unity-character-switch-operations.md` が作成され、切替手順と検証ポイントが定義されている |
| U4-T4 | 運用ドキュメント導線を Unity前提で一本化（legacy手順の分離と参照整理） | Med | Done | `docs/05-dev` の主要運用導線を Unity手順に統一し、`run-poc` をlegacy参照へ分離済み |

## U5タスク（2026-02-20 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U5-T1 | U5方針/契約/受入条件の定義を文書化 | High | Done | `docs/05-dev/u5-core-integration-plan.md` と `docs/02-architecture/interfaces/ipc-contract.md` が作成・更新済み |
| U5-T2 | Minimal Core Bridge（health/chat/config）を Runtime から実行可能化 | High | Done | Runtime HUD 経由で loopback 呼び出しが実行され、`request_id` の相関確認ができる |
| U5-T3 | 契約テスト（request_id/error_code/retryable）を EditMode で自動化 | High | Done | `LoopbackHttpClientTests` 5/5 Pass。`LogAssert.Expect` 追加で全件通過確認済み |
| U5-T4 | LLM/TTS/STT の段階統合と失敗時復旧手順を運用化 | Med | Done | Phase A/B/C の全検証が完了（LLM 5/5, TTS 3/3, STT 4/4, Loopback 5/5）し、artifact採取済み。 |

## U6タスク（2026-02-21 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U6-T1 | `tools/run_unity_tests.ps1` に `-RequireArtifacts` を追加し、artifact未生成を fail 判定できるようにする | High | Done | `-RequireArtifacts` 実装済みで、指定4スイート実行時の起動前失敗/artifact未生成を記録済み（`docs/worklog/2026-02-21_u6_t1_kickoff.md`） |
| U6-T2 | 回帰品質ゲートの標準運用（実行手順/記録テンプレート/判定ルール）を `docs/05-dev` に定義する | Med | Done | `docs/05-dev/u6-regression-gate-operations.md` が作成され、4スイート運用手順と記録テンプレートが明記されている |

## U7タスク（2026-02-24 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U7-T1 | 候補探索の根本治療（cache/backoff + 強制再探索導線）を実装 | High | Done | `AssetCatalogService` 追加、`SimpleModelBootstrap` / `RuntimeDebugHud` 連携、`AssetCatalogServiceTests` が存在する |
| U7-T2 | Windowing ネイティブ呼び出しを gateway 化して責務を統合 | High | Done | `WindowNativeGateway` 追加、`ResidentController` / `WindowController` が gateway 経由、`WindowNativeGatewayTests` が存在する |
| U7-T3 | U7対象4スイートの EditMode 検証を完了 | High | Done | AssetCatalog 4/4, WindowNativeGateway 1/1, SimpleModelBootstrap 36/36, RuntimeLog 9/9 で最終 Passed（`docs/worklog/2026-02-24_test_execution_u7_four_suites.md`） |
| U7-T4 | Standalone 安定稼働の受入確認（Hide/Show/Topmost/Drag + 必須ログ）を再採取 | Med | Done | 最新 `runtime-20260224-03.jsonl` で必須イベント（`avatar.model.displayed`, `avatar.motion.slot_played`, `window.resident.hidden`, `window.resident.restored`, `window.topmost.changed`）を確認し、証跡を `docs/worklog/2026-02-24_u7_t4_t5_execution.md` に記録 |
| U7-T5 | Runtime 初期化導線の簡素化（bootstrap recovery 依存の恒常運用を解消） | Med | Done | `RuntimeDebugHud` の bootstrap自己回復導線を撤去し、通常起動を `SimpleModelBootstrap` 自動初期化に固定。`QUICKSTART` / `unity-runtime-manual-check` を同期済み |

## U8タスク（2026-02-24 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U8-T1 | `runtime-*.jsonl` から `ui.hud.bootstrap_missing` の連続出力を検知する自動チェックを追加 | High | Done | `tools/check_runtime_bootstrap_missing.ps1` が追加され、しきい値/判定基準が `docs/05-dev/u8-operations-automation.md` に記載されている |
| U8-T2 | Unity導線更新時の legacy文書同期漏れ（`PACKAGING` / `RESIDENT_MODE`）を検知する軽量チェックを追加 | Med | Done | `tools/check_unity_legacy_docs_sync.ps1` が追加され、`Last Updated` 基準と参照リンク基準で Fail 判定できる |
| U8-T3 | U8チェックの一括実行導線を追加し、実ログで運用証跡を採取する | Med | Done | `tools/run_u8_ops_checks.ps1` が追加され、`u8_ops_checks_run_custom_20260225_174453.json` が生成されている |
| U8-T4 | U8運用の推奨設定（Daily/Gate）と定期実行導線を整備する | Med | Done | `run_u8_ops_checks` に `Profile` を追加し、`register/unregister_u8_ops_checks_task.ps1` が利用可能 |
| U8-T5 | U8残件4項目（証跡整合 / 鮮度チェック / Fail記録テンプレ / Scheduler診断）を実装する | Med | Done | `tools/check_u8_ops_freshness.ps1` / `tools/diagnose_u8_scheduler.ps1` / `docs/worklog/_templates/u8_ops_fail_template.md` が追加され、証跡整合クリーンアップと文書更新が完了。Scheduler実登録・確認・削除の実績あり。 |
| U8-T6 | `check_u8_ops_freshness` の旧artifact互換性不具合（`profile`欠損で例外）を修正し、再監査で確認する | Med | Done | `Resolve-ProfileFromArtifact` により `profile` 欠損を許容し、`Profile=Any` 実行でクラッシュしない。`audit_fix_freshness_any_default_20260225_220408.json` で再確認済み。 |

### U9: 最終安定化クローズ（Done）
- [x] 環境依存制約（Scheduler断続障害）を「管理済み制約 (non-blocking)」として整理（最新診断 `2026-02-26 00:07` は `can_register=false`。過去実績として `can_register=true` も保持。事前診断 + 手動日次代替を必須運用として固定）
- [x] 最終ゲートチェック実行（`run_u8_ops_checks.ps1 -Profile Gate`）と証跡採取（正常: `u8_ops_checks_run_gate_20260226_000657.json` / 異常: `u8_runtime_monitor_summary_gate_20260226_000716.json`）
- [x] 技術的負債対応（`run_u8_ops_checks` の失敗時run summary欠落を解消、`diagnose_u8_scheduler` に再試行診断を追加）
- [x] App Dev 移行条件の明文化と文書同期（NEXT_TASKS / dev-status）

## 次セクション: アプリケーション開発

### 移行判定
- U0〜U9 完了・RLS-S1 R1〜R4 Done・50/50 テスト Pass・運用チェック正常
- 残件ゼロ（または凍結済み non-blocking）確認済み
- **判定: アプリケーション開発へ移行可 ✅**

### 移行を妨げない根拠（非ゼロ残件の場合）
| 残件 | 重大度 | 根拠 |
|---|---|---|
| Scheduler 断続障害 | Low (管理済み制約) | 代替手順（手動日次）が `u8-operations-automation.md` に固定済み。診断スクリプトで事前確認可能。アプリ機能に直接影響しない。 |

### APP タスク一覧

| ID | タスク | 優先度 | 状態 | 前提 | 完了条件 |
|---|---|---|---|---|---|
| APP-T1 | アプリケーション機能仕様とロードマップの確定（優先機能リスト/受入条件） | High | **Done** | U9 Done | `docs/05-dev/app-spec-and-roadmap.md` に F-01〜F-12・Phase分割・DoD・依存・リスクが定義され、NEXT_TASKS / dev-status が同期済み |
| APP-T2 | Full Core接続（loopbackダミーから実LLM/TTS/STTエンドポイントへ切り替え） | High | In Progress（Phase 1-2実装済み / テスト継続） | APP-T1 Done | `docs/05-dev/app-t2-full-core-design.md` に整合した実装であること。`RuntimeConfig` でエンドポイントが切替可能であること。全4スイート（LLM/TTS/STT/Loopback）が Full Core 接続状態で Pass すること。切替手順が `docs/05-dev` に記載されていること。 |
| APP-T3 | 配布パッケージング整備（PACKAGING.md 更新・インストーラー導線確立） | Med | Not Started | APP-T1 Done | Unity Windows Standalone Player ビルドが成功すること。`docs/PACKAGING.md` が Unity 向け手順へ更新されていること。配布テスト手順が `docs/05-dev` に記載されていること。 |

## リリース完了タスク（2026-02-21 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| RLS-T1 | 旧計画（P0-P6）参照でリリース完了ゲートを策定する | High | Done | `docs/05-dev/release-completion-plan.md` に旧P0-P6対応とリリース判定ゲートが定義されている |
| RLS-T2 | 最終バッチ回帰（STT/TTS/LLM/Loopback）を実行し、artifactを採取する | High | Done | Unity.exe 直接実行で全スイート Passed（STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）。artifact 全件生成確認済み。`docs/worklog/2026-02-22_unity_recovery_r3_pass.md` |
| RLS-T3 | リリース候補の運用導線（Packaging/Resident/Runtime Manual）を最終確認して判定する | Med | Done (Conditional Pass) | R1/R2 判定完了。旧PoC手順（`PACKAGING.md` / `RESIDENT_MODE.md`）は未更新だがUnity側導線は整合。Unity Scope 基準でリリース完了扱いへ確定。`docs/worklog/2026-02-22_r4_closure_fullquality.md` に記録済み。 |

## 次セクション定義（2026-02-22 作成）: RLS-S1 Release Gate Execution
### 目的
- リリース完了ゲート `R1-R4` を実行可能な粒度に分解し、判定根拠を `worklog` と状態同期ドキュメントに残せる状態へ固定する。
- Unity起動前失敗（`指定されたモジュールが見つかりません`）を「ユーザー実行環境で再実行する対象」として明示管理し、R3/R4 の実行主体を明確化する。

### スコープ
- 過去計画（`U0-U6` と `P0-P6 対比`）を根拠に、アプリケーション全体像・開発計画・機能一覧をリリース判定軸へ整理する。
- `R1-R4` を実行タスクへ分解し、ID/優先度/依存関係/ブロッカー/完了条件を定義する。
- `NEXT_TASKS` / `dev-status` の同期対象として、次セクションの状態表現を統一する。

### 非スコープ
- Unity実行環境の再インストールやOSレベル修復作業そのもの。
- 新機能実装（Runtime/Core/UI/IPC のコード変更）。
- CI自動化や通知導線の新規実装。

### 完了条件（DoD）
- [x] 次セクションの目的/スコープ/非スコープ/DoD が `NEXT_TASKS` と `dev-status` で一致している。
- [x] `R1-R4` の実行タスクに ID・優先度・完了条件・依存関係・ブロッカーが定義されている。
- [x] Unity起動前失敗事象の扱いが明記されている（断続的環境依存事象として記録、直接実行で回避し再実行完了）。
- [x] 判定結果の記録先（`worklog` / `release-completion-plan` / `dev-status`）がタスクに紐付いている。

### アプリケーション全体像（過去計画ベース）
| 領域 | 全体像（機能観点） | 根拠 | 現在状態 |
|---|---|---|---|
| Runtime UX | Unity Runtime HUD でモデル表示/状態遷移/motion/ウィンドウ操作を提供する。 | `Unity_PJ/spec/latest/spec.md`（UR-002/004/005/006）, `docs/05-dev/QUICKSTART.md`, `docs/05-dev/unity-runtime-manual-check.md` | 稼働中（U1/U4 完了） |
| Resident Operation | 常駐運用（Show/Hide/Exit/ログ導線）を提供し、Runtime導線と運用手順を接続する。 | `Unity_PJ/spec/latest/spec.md`（UR-003）, `docs/RESIDENT_MODE.md`, `docs/05-dev/release-completion-plan.md` | Conditional Pass（Gate R2） |
| Core Integration | LLM/TTS/STT を loopback HTTP 境界で統合し、段階運用（LLM->TTS->STT）で品質を担保する。 | `docs/05-dev/u5-core-integration-plan.md`, `docs/05-dev/u5-llm-tts-stt-operations.md`, 本ファイル U5 | 実装完了（U5 Done） |
| Observability & Gate | `request_id`/`error_code` と artifact必須判定で回帰品質ゲートを運用する。 | `Unity_PJ/spec/latest/spec.md`（UR-007/008/009）, `docs/05-dev/u6-regression-gate-operations.md`, 本ファイル U6 | 運用中（U6 Done、RLS-T2 Done） |
| Packaging/Distribution | 配布導線と起動導線を整合させ、リリース判定へ接続する。 | `docs/PACKAGING.md`, `docs/05-dev/QUICKSTART.md`, `docs/05-dev/release-completion-plan.md` | Conditional Pass（Gate R1） |

### 開発計画サマリー（過去計画 -> 現行）
| 計画レイヤー | 主題 | 現行対応 | 状態 |
|---|---|---|---|
| P0-P2 | 起動導線/manifest/packaging | U0 + R1 | U0 Done / R1 Conditional Pass |
| P3-P4 | Launcher/Resident UX | U1 + U4 + R2 | U1/U4 Done / R2 Conditional Pass |
| P5 | キャラ切替と運用改善 | U4 + Runtime Manual | U4 Done |
| P6 | Core統合（LLM/TTS/STT） | U5 + U6 + R3 | U5/U6 Done / R3 Pass |
| Release Closure | 最終判定と状態同期 | R4 | Done（R1/R2 Conditional Pass をリリース完了扱いとして確定） |

### 機能一覧（リリース判定対応）
| 機能ID | 機能 | 現在状態 | 関連Gate | 判定時の確認ポイント |
|---|---|---|---|---|
| F-01 | Unity起動とRuntime HUD表示 | Ready | R1/R2 | `docs/05-dev/QUICKSTART.md` と Runtime Manual 手順一致 |
| F-02 | モデル切替/状態遷移/motion操作 | Ready | R2 | `unity-runtime-manual-check` の合格条件を満たす |
| F-03 | 常駐導線（Show/Hide/Exit/Logs） | Conditional Pass | R2 | 旧PoC常駐は未更新、Unity側導線は整合 |
| F-04 | LLM連携（chat + fallback） | Done (Passed) | R3 | LLM 5/5 Passed (`editmode-20260222_180700.xml`) |
| F-05 | TTS連携（`/v1/tts/play`） | Done (Passed) | R3 | TTS 3/3 Passed (`editmode-20260222_180600.xml`) |
| F-06 | STT連携（partial/final） | Done (Passed) | R3 | STT 4/4 Passed (`editmode-20260222_180500.xml`) |
| F-07 | Loopback契約検証（request_id/error schema） | Done (Passed) | R3 | Loopback 5/5 Passed (`editmode-20260222_180230.xml`) |
| F-08 | 判定同期（NEXT_TASKS/dev-status/worklog） | Done | R4 | R1/R2 Conditional Pass のリリース完了扱い基準を確定し、R4判定を同期済み |

### R1/R2 Conditional Pass のリリース完了扱い基準（Unity Scope）
- Unityスコープ受入条件:
  - R1: `docs/05-dev/QUICKSTART.md` の起動導線で運用可能。
  - R2: `docs/05-dev/unity-runtime-manual-check.md` の Runtime/Resident 導線で運用可能。
- 旧PoC文書未更新の扱い:
  - `docs/PACKAGING.md` / `docs/RESIDENT_MODE.md` は legacy 参照（read-only）として扱い、Unityスコープ判定では non-blocking。
  - Unityスコープ手順との矛盾が確認された場合のみ blocking に昇格。
- 残課題の管理方針:
  - Unityリリース機能に直接影響しない残課題は次フェーズ管理（worklog + 次アクション）へ移管し、R4を停止しない。
  - Gate失敗、artifact欠損、重大不具合のようにUnityリリース機能へ直接影響する証跡がある場合のみ R4 を停止する。
- 判定:
  - 上記を満たす場合、R1/R2 の Conditional Pass を「リリース完了扱い（Unity Scope）」として確定する。

### R1-R4 実行タスク分解（次セクション）
| ID | Gate | タスク | 優先度 | 状態 | 依存関係 | ブロッカー | 完了条件 |
|---|---|---|---|---|---|---|---|
| RLS-R1-01 | R1 | 配布導線とUnity起動導線の差分確認（`PACKAGING` vs `QUICKSTART`） | High | Done | - | - | 差分有無と是正方針が `worklog` に記録されている |
| RLS-R1-02 | R1 | Gate R1 判定（Pass/Fail/保留）を `release-completion-plan` と状態文書へ同期 | High | Done (Conditional Pass) | RLS-R1-01 | - | R1判定結果と根拠参照が3文書で一致している |
| RLS-R2-01 | R2 | Runtime/Resident 運用導線の手順確認（参照切れ含む） | High | Done | - | - | `RESIDENT_MODE` / `unity-runtime-manual-check` の確認結果が `worklog` にある |
| RLS-R2-02 | R2 | Gate R2 判定を記録し、運用可否を状態同期 | Med | Done (Conditional Pass) | RLS-R2-01 | - | R2判定と残課題が `NEXT_TASKS` / `dev-status` で一致 |
| RLS-R3-01 | R3 | 4スイート再実行の前提条件（コマンド/記録形式）を確定する | High | Done | - | - | 実行コマンドと記録テンプレートが `worklog` に確定している |
| RLS-R3-02 | R3 | 4スイート最終バッチ回帰を `-RequireArtifacts` 付きで実行する | High | Done (Passed: 直接実行) | RLS-R3-01 | - | STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5。artifact 全件生成。注: `run_unity_tests.ps1` は 20:54-20:58/22:23-22:24/2026-02-23 00:07-00:08 は exit 0、21:31-21:32 は起動前失敗で exit 1。 |
| RLS-R3-03 | R3 | 実行結果（pass/fail, exit_code, artifact）を状態文書へ同期 | High | Done | RLS-R3-02 | - | `worklog` テーブルと `dev-status` 記載が一致 |
| RLS-R4-01 | R4 | Gate R1-R3 結果を集約して最終判定を記録 | High | Done | RLS-R1-02, RLS-R2-02, RLS-R3-03 | - | R1/R2 Conditional Pass を「リリース完了扱い（Unity Scope）」として確定し、R4=Done を3文書で一致させる。 |
| RLS-R4-02 | R4 | リリース判定履歴（根拠/残課題/ロールバック方針）を `worklog` に確定 | Med | Done | RLS-R4-01 | - | 最終判定履歴（根拠/残課題/ロールバック方針）を `worklog` に確定し、Report-Path/Obsidian-Log/Record Check を充足する。 |

### 依存関係とブロッカー
- `CTX-USER-EXECUTION`: 解除済み。
- `run_unity_tests.ps1` に関する事実:
  - 2026-02-22 20:54-20:58 の再テストでは、artifact 待機ログ出力後に check passed（`Unity_PJ/artifacts/test-results/script_retest_*.txt`）。
  - 2026-02-22 21:31-21:32 の再実行では Unity.exe/Unity.com とも起動前失敗で exit 1（`Unity_PJ/artifacts/test-results/review_run_*_20260222_1.txt`）。
  - 2026-02-22 22:23-22:24 の再実行では `run_unity_tests.ps1 -RequireArtifacts` で4スイート全て Passed（Loopback 5/5, STT 4/4, TTS 3/3, LLM 5/5）し、artifact（xml/log）全件生成。
  - 2026-02-23 00:07-00:08 の再実行でも `run_unity_tests.ps1 -RequireArtifacts` で4スイート全て Passed（STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）し、artifact（xml/log）全件生成。
  - 2026-02-22 17:43-17:47 実行分（`174300` / `174535` / `174556` / `174722`）は当時記録が missing/exit1 だが、現存 artifact XML は全件 Passed。
- `run_unity_tests.ps1` に関する仮説:
  - 当時の exit 1 は artifact 判定タイミング要因と環境依存の起動前失敗が重なっていた可能性がある。単一原因は未確定。
- R1/R2 は Conditional Pass（Unity Scope 基準でリリース完了扱い確定）。R3 は Pass（直接実行 + 22:23-22:24/2026-02-23 00:07-00:08 再実行成功）。R4 は Done。

## 既知の課題（モデル関連）
- 2026-02-19時点: 解消済み（未解決項目なし）
- 継続方針: 新規課題が発生した場合のみ本セクションへ追加

## 参照
- `docs/worklog/2026-02-07_unity-pj-restructure.md`
- `docs/worklog/2026-02-07_unity-pj-cutover.md`
- `docs/worklog/2026-02-07_unity-migration-integrated-review.md`
- `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`
- `docs/worklog/2026-02-19_test_run_debug.md`
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/05-dev/unity-test-result-collection-template.md`
- `docs/worklog/2026-02-19_unity_test_ops_standardization.md`
- `docs/05-dev/unity-character-switch-operations.md`
- `docs/worklog/2026-02-20_u4_character_switch_kickoff.md`
- `docs/05-dev/unity-runtime-manual-check.md`
- `docs/worklog/2026-02-20_u4_docs_unification.md`
- `docs/05-dev/u5-core-integration-plan.md`
- `docs/02-architecture/interfaces/ipc-contract.md`
- `docs/worklog/2026-02-20_u5_execution_docs.md`
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- `docs/worklog/2026-02-20_u5_t2_minimal_core_bridge.md`
- `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
- `docs/worklog/2026-02-20_u5_t3_contract_test_automation.md`
- `docs/worklog/2026-02-21_u5_t3_contract_test_rerun.md`
- `docs/worklog/2026-02-21_u5_t3_logassert_fix.md`
- `docs/05-dev/u5-llm-tts-stt-operations.md`
- `docs/worklog/2026-02-21_u5_t4_operations_kickoff.md`
- `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
- `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`
- `docs/worklog/2026-02-21_u5_t4_phase_a_llm_impl.md`
- `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`
- `docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md`
- `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorSttIntegrationTests.cs`
- `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md`
- `docs/worklog/2026-02-21_u5_t4_phase_c_progress_sync.md`
- `tools/run_unity_tests.ps1`
- `docs/worklog/2026-02-21_u6_t1_kickoff.md`
- `docs/05-dev/u6-regression-gate-operations.md`
- `docs/05-dev/release-completion-plan.md`
- `docs/PACKAGING.md`
- `docs/RESIDENT_MODE.md`
- `docs/worklog/2026-02-21_u6_completion_and_release_planning.md`
- `docs/worklog/2026-02-21_rls_t2_result_sync.md`
- `docs/worklog/2026-02-22_rls_s1_gate_execution.md`
- `docs/worklog/2026-02-22_unity_recovery_r3_pass.md`
- `docs/05-dev/app-spec-and-roadmap.md`
- `docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md`
