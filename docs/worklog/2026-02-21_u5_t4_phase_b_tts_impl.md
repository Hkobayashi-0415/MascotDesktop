# Worklog: U5-T4 Phase B（TTS統合）実装着手

- Date: 2026-02-21
- Task: U5-T4 Phase B（TTS統合）: chat導線から `/v1/tts/play` を連結し、成功/失敗時の motion 反映と retryable 可観測性を追加
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs.meta`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `tools/run_unity_tests.ps1`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-21_u5_t4_phase_b_tts_impl.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_1746.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t4, phase-b, tts-integration]

## Summary
U5-T4 Phase B として `CoreOrchestrator.SendTtsWithBridgeResult` を追加し、`RuntimeDebugHud` の chat導線から `/v1/tts/play` への連結を実装した。
TTS bridge 成功時は `wave` motion、失敗時は `idle` fallback motion を要求し、`error_code/retryable` をログ記録する。
テスト実行はこの環境で `Unity.exe`/`Unity.com` とも起動前失敗（モジュール不足）となり、artifact は未生成だった。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
- `SendTtsWithBridgeResult(text, requestId, bridgeSucceeded, bridgeErrorCode, bridgeRetryable)` を追加。
- 成功時: `core.tts.bridge_success` を記録し、`RequestMotionSlot("wave")` を実行。
- 失敗時: `core.tts.bridge_fallback` を記録し、`RequestMotionSlot("idle")` を実行。

2. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `SendChatWithBridgeAsync` の末尾で `SendTtsWithBridgeAsync(message, requestId)` を呼び出し、Phase B 連結を追加。
- `SendTtsWithBridgeAsync` を新規追加し、`/v1/tts/play` bridge 結果を `SendTtsWithBridgeResult` へ渡す。

3. `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`（新規）
- 成功時 motion/request_id 検証（`wave`）
- 失敗時 retryable=true の fallback 検証（`idle` + ログ）
- 失敗時 retryable=false の fallback 検証（`idle` + ログ）

4. `docs/NEXT_TASKS.md`
- R14 を追加し、Phase B 着手と検証保留を反映。
- U5チェックリストと U5-T4 状態を `Phase B Started` に更新。

5. `docs/05-dev/dev-status.md`
- Phase B 着手を反映。
- 17:45 実行失敗（artifact 未生成）をリスクに追記。
- 次アクションを「環境復旧→Phase B検証完了→Phase C」へ更新。

## Commands
```powershell
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorTtsIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174541.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174541.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174543.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174543.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174545.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_174545.log
```

## Tests
| run_at | suite | pass_fail | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|
| 2026-02-21 17:45 | CoreOrchestratorTtsIntegrationTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_174541.xml`) | missing (`editmode-20260221_174541.log`) |
| 2026-02-21 17:45 | CoreOrchestratorLlmIntegrationTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_174543.xml`) | missing (`editmode-20260221_174543.log`) |
| 2026-02-21 17:45 | LoopbackHttpClientTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_174545.xml`) | missing (`editmode-20260221_174545.log`) |

## Tests 追補（Antigravity による再実行確認）
- Execution-Tool: Antigravity / Execution-Model: gemini-2.5-pro

| run_at | suite | pass_fail | artifact_xml |
|---|---|---|---|
| 2026-02-21 17:58 | CoreOrchestratorTtsIntegrationTests | Passed 3/3 | `editmode-20260221_175856.xml` |
| 2026-02-21 18:12 | CoreOrchestratorLlmIntegrationTests | Passed 5/5 | `editmode-20260221_181200.xml` |
| 2026-02-21 18:17 | LoopbackHttpClientTests | Passed 5/5 | `editmode-20260221_181704.xml` |

run_unity_tests.ps1 の自動フォールバックにより環境復旧不要で通過。

## Rationale (Key Points)
- 重大リスク: TTS結果の可観測性が無いまま Phase C へ進むと、音声導線の障害切り分けが困難になる。
- 対策: Phase B で `core.tts.bridge_success` / `core.tts.bridge_fallback` と `retryable` を固定ログ化。
- 差分意図: Core は bridge依存を持たず、HUD から bridge結果を注入する形でテストしやすさを維持。

## Rollback
- 変更戻し対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorTtsIntegrationTests.cs.meta`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Obsidianログは削除しない。
- ロールバック理由（何を・なぜ戻したか）を本worklogへ追記し、Obsidianに `Rolled back` / `Superseded` 注記を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. ~~Unity環境復旧→Phase B 3スイート再実行~~ 完了（Antigravity にて検証済み、上記Tests追補参照）
2. Phase B 完了判定: TtsIntegrationTests 3/3 Passed（motion反映・retryable区別確認済み）
3. Phase C（STT統合）へ進む。

