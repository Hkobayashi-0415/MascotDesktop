# Dev Status (2026-02-23)

## 現状サマリー（Unity移行後）
- `U0`/`U1`/`U2` は完了（移行基盤、Runtime基盤、エラーハンドリング/ログ強化）。
- `U3-T1`/`U3-T2` は完了（環境差異復旧手順、結果収集テンプレを `docs/05-dev` に整備）。
- `U4-T1` は完了（`NEXT_TASKS` と本ファイルの同期更新）。
- `U4-T3` は完了（キャラクター切替導線の標準運用手順を追加）。
- `U4-T4` は完了（`docs/05-dev` の主導線を Unity前提へ統一し、legacy手順を分離）。
- `U5-T1` は完了（Core統合方針、IPC契約、段階移行受入条件を文書化）。
- `U5-T2` は完了（Runtime HUD の health/chat/config bridge 導線追加とテスト通過を確認）。
- `U5-T3` は完了（`LoopbackHttpClientTests` 5/5 Pass、`LogAssert.Expect` 追加で失敗2件を解消）。
- `U5-T4 Phase A` は完了（chat経路の外部Core優先化・fallback維持・request_id/error_code/retryable 可観測性確保）。
- `U5-T4 Phase B` は完了（`/v1/tts/play` 連結、TTS統合テスト 3/3 Pass を確認）。
- `U5-T4 Phase C` は完了（STT統合テスト 4/4 Pass、回帰3スイートも通過）。
- `U5-T4` は完了（Phase A/B/C のartifact確認完了）。
- `U6` は完了（回帰品質ゲート運用）。`-RequireArtifacts` 実装と未生成検知確認に加え、運用手順/記録テンプレートを `docs/05-dev/u6-regression-gate-operations.md` に標準化。
- リリース完了計画を開始（旧P0-P6参照で Gate 定義を `docs/05-dev/release-completion-plan.md` に作成）。
- RLS-S1 Release Gate Execution（R1-R4）を実施（2026-02-22）:
  - R1: Conditional Pass（Unity側起動導線は整合、旧PoC配布手順は未更新）
  - R2: Conditional Pass（Runtime HUD 導線は整合、旧PoC常駐は未更新）
  - R3: **Pass**（Unity.exe直接実行で全スイート Passed。STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）
  - R4: **Done**（R1/R2 Conditional Pass をリリース完了扱いとして確定）
- 最新再実行（2026-02-23 00:07-00:08）: `run_unity_tests.ps1 -RequireArtifacts` で4スイート全て Passed（STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）。artifact（xml/log）全件生成を確認。

## 現状OK
- 品質ゲート対象テストは直近の通過実績あり:
  - `RuntimeErrorHandlingAndLoggingTests`: 7/7 Passed
  - `SimpleModelBootstrapTests`: 34/34 Passed
  - 直近手動実行（2026-02-20）:
    - `editmode-20260220_000605.xml`（RuntimeErrorHandlingAndLoggingTests）
    - `editmode-20260220_001245.xml`（SimpleModelBootstrapTests）
    - `editmode-20260220_104153.xml`（RuntimeErrorHandlingAndLoggingTests, 7/7）
    - `editmode-20260220_150020.xml`（SimpleModelBootstrapTests, 34/34）
  - U5-T2 bridge テスト（2026-02-20 10:29）:
    - `editmode-20260220_102912.xml`（LoopbackHttpClientTests: 1 Passed, 2 Failed）
  - U5-T3 契約テスト再実行（2026-02-21 01:38）:
    - `editmode-20260221_013844.xml`（LoopbackHttpClientTests: 5/5 Passed）
  - U5-T4 Phase A 事前確認（2026-02-21 15:09-15:28）:
    - `editmode-20260221_150919.xml`（LoopbackHttpClientTests: 5/5 Passed）
    - `editmode-20260221_151713.xml`（RuntimeErrorHandlingAndLoggingTests: 7/7 Passed）
    - `editmode-20260221_152801.xml`（SimpleModelBootstrapTests: 34/34 Passed）
  - U5-T4 Phase A 新規テスト（2026-02-21 15:39）:
    - `editmode-20260221_153948.xml`（CoreOrchestratorLlmIntegrationTests: 3/3 Passed）
- U5-T4 Phase A 回帰確認（2026-02-21 15:42）:
  - `editmode-20260221_154251.xml`（LoopbackHttpClientTests: 5/5 Passed）
 - U5-T4 Phase B 検証追補（2026-02-21 17:58-18:17）:
   - `editmode-20260221_175856.xml`（CoreOrchestratorTtsIntegrationTests: 3/3 Passed）
   - `editmode-20260221_181200.xml`（CoreOrchestratorLlmIntegrationTests: 5/5 Passed）
   - `editmode-20260221_181704.xml`（LoopbackHttpClientTests: 5/5 Passed）
 - U5-T4 Phase C 検証追補（2026-02-21 19:50-20:05）:
   - `editmode-20260221_195019.xml`（CoreOrchestratorSttIntegrationTests: 4/4 Passed）
   - `editmode-20260221_195037.xml`（CoreOrchestratorTtsIntegrationTests: 3/3 Passed）
   - `editmode-20260221_195406.xml`（CoreOrchestratorLlmIntegrationTests: 5/5 Passed）
   - `editmode-20260221_200517.xml`（LoopbackHttpClientTests: 5/5 Passed）
 - U6-T1 `-RequireArtifacts` 検証追補（2026-02-21 21:32-21:34）:
   - `editmode-20260221_213239.xml`（CoreOrchestratorSttIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_213407.xml`（CoreOrchestratorTtsIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_213441.xml`（CoreOrchestratorLlmIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_213446.xml`（LoopbackHttpClientTests: missing / exit 1 検知）
 - RLS-T2 最終バッチ回帰（2026-02-21 22:23-22:29）:
   - `editmode-20260221_222310.xml`（CoreOrchestratorSttIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_222441.xml`（CoreOrchestratorTtsIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_222831.xml`（CoreOrchestratorLlmIntegrationTests: missing / exit 1 検知）
   - `editmode-20260221_222910.xml`（LoopbackHttpClientTests: missing / exit 1 検知）
  - 根拠: `docs/worklog/2026-02-19_test_run_debug.md`, `docs/worklog/2026-02-19_manual_test_exec.md`, `docs/worklog/2026-02-20_u5_t2_minimal_core_bridge.md`, `docs/worklog/2026-02-21_u5_t3_logassert_fix.md`, `docs/worklog/2026-02-21_u5_t4_phase_a_llm_impl.md`, `docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md`, `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md`
- Unity実行環境: UNITY_COM（`Unity.com`）で回避できるケースはあるが、同一エラーの再発事例が継続している。
- Unityテスト運用ドキュメントを標準化済み:
  - 復旧手順: `docs/05-dev/unity-test-environment-recovery.md`
  - 結果収集テンプレ: `docs/05-dev/unity-test-result-collection-template.md`
- Unity運用導線（U4）:
  - キャラクター切替手順: `docs/05-dev/unity-character-switch-operations.md`
- U5基準ドキュメントを整備:
  - 統合計画: `docs/05-dev/u5-core-integration-plan.md`
  - 運用手順: `docs/05-dev/u5-llm-tts-stt-operations.md`
  - IPC契約: `docs/02-architecture/interfaces/ipc-contract.md`
- U6/Release 基準ドキュメントを整備:
  - 回帰品質ゲート運用: `docs/05-dev/u6-regression-gate-operations.md`
  - リリース完了計画: `docs/05-dev/release-completion-plan.md`
- U5-T2 実装差分:
  - HUD bridge 導線: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - bridge挙動テスト: `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- U5-T3 実装差分:
  - 契約反映: `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
  - 契約テスト追加: `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
- U5-T4 Phase A 実装差分:
  - Core拡張: `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`（`SendChatWithBridgeResult` 追加）
  - HUD修正: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`（bridge成功/失敗/unavailable分岐）
  - Phase Aテスト: `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorLlmIntegrationTests.cs`（3/3 Passed）
- U5-T4 Phase B 実装差分:
  - Core拡張: `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`（`SendTtsWithBridgeResult` 追加）
  - HUD修正: `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`（chat導線から `/v1/tts/play` 連結）
  - Phase Bテスト: `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`（新規）
- U6-T1 実装差分:
  - テスト実行ガード: `tools/run_unity_tests.ps1`（`-RequireArtifacts` 追加、xml/log 未生成時を fail 判定）
- U6-T2 実装差分:
  - 回帰運用標準: `docs/05-dev/u6-regression-gate-operations.md`（実行手順/判定ルール/記録テンプレート）
- リリース計画差分:
  - 旧P0-P6対応のゲート定義: `docs/05-dev/release-completion-plan.md`

## 次セクション計画（RLS-S1 Release Gate Execution）
- 目的:
  - `R1-R4` を実行可能タスクへ分解し、判定根拠の記録先（`worklog` / `release-completion-plan` / 状態文書）を固定する。
  - Unity起動前失敗事象を「ユーザー実行環境で再実行する対象」として管理し、R3/R4 の実行主体を統一する。
- スコープ:
  - 過去計画（P0-P6 対比 + U0-U6）を基に、全体像/開発計画/機能一覧をリリース判定軸で整理。
  - R1-R4 の ID・優先度・依存関係・完了条件を定義して実行順を固定。
- 非スコープ:
  - Unity実行環境の再インストールや新機能実装。
  - CI/通知の新規自動化。

### 機能一覧サマリー（リリース判定対応）
| 機能ID | 機能 | 状態 | 関連Gate | 備考 |
|---|---|---|---|---|
| F-01 | Unity起動とRuntime HUD表示 | Ready | R1/R2 | 起動導線整合をR1で判定 |
| F-02 | モデル切替/状態遷移/motion | Ready | R2 | Runtime Manual の合格条件で判定 |
| F-03 | 常駐導線（Show/Hide/Exit/Logs） | Conditional Pass | R2 | 旧PoC常駐は未更新、Unity側導線は整合 |
| F-04 | LLM連携 | Done (Passed) | R3 | LLM 5/5 Passed（`editmode-20260222_180700.xml`） |
| F-05 | TTS連携 | Done (Passed) | R3 | TTS 3/3 Passed（`editmode-20260222_180600.xml`） |
| F-06 | STT連携 | Done (Passed) | R3 | STT 4/4 Passed（`editmode-20260222_180500.xml`） |
| F-07 | Loopback契約検証 | Done (Passed) | R3 | Loopback 5/5 Passed, artifact 生成確認済み |
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

### R1-R4 タスク分解（同期用）
| ID | Gate | 優先度 | 状態 | 依存関係 | ブロッカー | 完了条件 |
|---|---|---|---|---|---|---|
| RLS-R1-01 | R1 | High | Done | - | - | `PACKAGING` と `QUICKSTART` の差分確認結果を `worklog` に記録 |
| RLS-R1-02 | R1 | High | Done (Conditional Pass) | RLS-R1-01 | - | R1判定を `NEXT_TASKS` / `dev-status` / `release-completion-plan` に同期 |
| RLS-R2-01 | R2 | High | Done | - | - | Resident/Runtime 導線の確認結果を `worklog` に記録 |
| RLS-R2-02 | R2 | Med | Done (Conditional Pass) | RLS-R2-01 | - | R2判定と残課題を状態文書へ同期 |
| RLS-R3-01 | R3 | High | Done | - | - | 実行コマンドと記録テンプレートを `worklog` に確定 |
| RLS-R3-02 | R3 | High | Done (Passed: 直接実行) | RLS-R3-01 | - | STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5。注: `run_unity_tests.ps1` は 20:54-20:58/22:23-22:24/2026-02-23 00:07-00:08 は exit 0、21:31-21:32 は起動前失敗で exit 1 |
| RLS-R3-03 | R3 | High | Done | RLS-R3-02 | - | 回帰結果テーブルを `worklog` / `dev-status` に同期 |
| RLS-R4-01 | R4 | High | Done | RLS-R1-02, RLS-R2-02, RLS-R3-03 | - | R1/R2 Conditional Pass を「リリース完了扱い（Unity Scope）」として確定し、R4=Done を3文書で一致させる。 |
| RLS-R4-02 | R4 | Med | Done | RLS-R4-01 | - | 最終判定履歴（根拠/残課題/ロールバック方針）を `worklog` に確定し、Report-Path/Obsidian-Log/Record Check を充足する。 |

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

## 現状NG / リスク
- Unity実行環境差異により、テスト起動前に `指定されたモジュールが見つかりません` が発生する場合がある。
  - `tools/run_unity_tests.ps1` は Unity.exe 失敗時に Unity.com フォールバックを実装済みだが、この環境では Unity.com 側も同エラーで起動失敗する場合がある。
- 上記起動前失敗時は `Unity_PJ/artifacts/test-results` に xml/log artifact が生成されない。
- `tools/run_unity_tests.ps1` は `-RequireArtifacts` で判定を補強済み。最終判定はユーザー実行環境の再実行結果を受領して行う。
- Unity実行環境差異の再発リスクは継続監視が必要（過去に `editmode-20260220_094003.*` / `094011.*` / `152126.*` 未生成事象あり）。
- 2026-02-20 15:57 の U5-T3 検証実行でも起動エラーが再発し、`editmode-20260220_155717.*` / `editmode-20260220_155723.*` は未生成。
- 2026-02-21 12:39 の再実行でも 3 スイートすべて起動前失敗（`指定されたモジュールが見つかりません`）。`editmode-20260221_123901.*` / `123903.*` / `123905.*` は未生成。
- 2026-02-21 17:45 の Phase B 検証実行でも 3 スイートすべて起動前失敗（Unity.exe / Unity.com とも `指定されたモジュールが見つかりません`）。`editmode-20260221_174541.*` / `174543.*` / `174545.*` は未生成。
- 2026-02-21 19:39-19:40 の Phase C 検証実行でも 4 スイートすべて起動前失敗（Unity.exe / Unity.com とも `指定されたモジュールが見つかりません`）。`editmode-20260221_193956.*` / `194017.*` / `194025.*` / `194027.*` は未生成。
- 同日 19:50 以降の再実行では 4 スイート全て通過し、artifact 生成を確認（`editmode-20260221_195019.xml` / `195037.xml` / `195406.xml` / `200517.xml`）。
- 2026-02-21 21:24 の U6-T1 回帰実行（STT/TTS/LLM/Loopback）でも 4 スイートすべて起動前失敗（Unity.exe / Unity.com とも `指定されたモジュールが見つかりません`）。`editmode-20260221_212416.*` / `212425.*` は未生成。
- 2026-02-21 21:32-21:34 の U6-T1 追補検証は、当時記録として `-RequireArtifacts` が artifact 未生成を全4スイートで `exit 1` として検知（`213239` / `213407` / `213441` / `213446`）。
- 2026-02-22 17:43-17:47 の RLS-S1 R3-02 実行は、当時記録として `-RequireArtifacts` が全件 exit 1（`174300` / `174535` / `174556` / `174722`）。
- 同タイムスタンプ artifact（`174300` / `174535` / `174556` / `174722`）は現在存在し、XMLは全件 Passed。
- 2026-02-22 18:02-18:07: Unity.exe 直接実行で4スイート全て Passed（R3 判定根拠）。
- 2026-02-22 20:54-20:58: `run_unity_tests.ps1` 再テストで artifact 待機が機能し、4スイート exit 0（`script_retest_*.txt`）。
- 2026-02-22 21:31-21:32: 同スクリプト再実行で Unity.exe/Unity.com 起動前失敗が再発し、4スイート exit 1（`review_run_*_20260222_1.txt`）。
- 2026-02-22 22:23-22:24: 同スクリプト再実行で4スイート全て Passed（`editmode-20260222_222339.xml` / `222350.xml` / `222401.xml` / `222412.xml`）。
- 2026-02-23 00:07-00:08: 同スクリプト再実行で4スイート全て Passed（`editmode-20260223_000743.xml` / `000753.xml` / `000803.xml` / `000813.xml`）。

## Gate 判定サマリー（2026-02-22 RLS-S1 最終）
| Gate | 判定 | 根拠 | 残課題 |
|---|---|---|---|
| R1 | Conditional Pass | Unity側起動導線は `QUICKSTART.md` で整合 | 旧PoC配布(`PACKAGING.md`)は未更新 |
| R2 | Conditional Pass | Runtime HUD 導線は `unity-runtime-manual-check.md` で整合 | 旧PoC常駐(`RESIDENT_MODE.md`)は未更新 |
| R3 | **Pass** | 4スイート全て Passed, artifact 全件生成（直接実行） | スクリプト経由の再現性は不安定（成功/起動前失敗が混在） |
| R4 | Done | R1-R3 結果集約済み + R1/R2 の Unity Scope 基準確定 | なし（次フェーズ管理のみ） |

## 標準運用（Unityテスト）
- 起動導線（Unity First）: `docs/05-dev/QUICKSTART.md`
- 実行前/失敗時の復旧: `docs/05-dev/unity-test-environment-recovery.md`
- 実行結果の記録形式: `docs/05-dev/unity-test-result-collection-template.md`
- 4スイート回帰実行時は `tools/run_unity_tests.ps1 -RequireArtifacts` を使用し、artifact未生成をfailとして扱う。
- キャラクター切替運用: `docs/05-dev/unity-character-switch-operations.md`
- Runtime画面確認: `docs/05-dev/unity-runtime-manual-check.md`
- 進捗とタスク状態: `docs/NEXT_TASKS.md`

## 次アクション
- `run_unity_tests.ps1` の成功/失敗混在（20:54-20:58 成功 / 21:31-21:32 失敗 / 22:23-22:24 成功 / 2026-02-23 00:07-00:08 成功）を事実/仮説分離で継続監視する。
- 旧PoC文書（`PACKAGING.md` / `RESIDENT_MODE.md`）の更新要否を次フェーズバックログで管理する。
- R4 Done 判定の根拠一式を `worklog` / Obsidian ログで固定し、追補時は `Superseded` 注記ルールを維持する。
