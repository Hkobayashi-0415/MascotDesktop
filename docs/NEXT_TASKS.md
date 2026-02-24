# NEXT_TASKS (Unity Migration Plan)

## 改訂履歴
| Rev | Date | 変更内容 | 根拠 |
|---|---|---|---|
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
