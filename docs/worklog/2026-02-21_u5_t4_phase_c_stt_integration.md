# Worklog: U5-T4 Phase C（STT統合）実装着手と検証再試行

- Date: 2026-02-21
- Task: U5-T4 Phase C（STT統合）: STT partial/final 分離処理と HUD `/v1/stt/event` 導線を前提に、EditMode 4スイートで検証を実施
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/CoreOrchestratorSttIntegrationTests.cs`
  - `tools/run_unity_tests.ps1`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_1941.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t4, phase-c, stt-integration]

## Summary
U5-T4 Phase C の実装差分（`SendSttWithBridgeResult` / HUD STT導線 / STT統合テスト）を確認し、4スイート実行で検証を試行した。
この環境では Unity.exe と Unity.com の両方が起動前に `指定されたモジュールが見つかりません` で失敗し、xml/log artifact は未生成だった。
`NEXT_TASKS` と `dev-status` は Phase C 着手・検証保留の状態に同期した。

## Changes
1. `docs/NEXT_TASKS.md`
- Rev R15 を追加し、Phase C 実装着手と検証保留（起動前失敗）を反映。
- U5 チェックリストを更新し、Phase A 5/5 Pass、Phase B 3/3 Pass、Phase C 着手済みに同期。
- U5-T4 状態を `In Progress (Phase A/B Done, Phase C Started)` へ更新。

2. `docs/05-dev/dev-status.md`
- 現状サマリーを更新し、Phase B 完了・Phase C 実装着手を反映。
- 2026-02-21 19:39-19:40 の Phase C 検証失敗（4スイート、artifact未生成）を `現状NG / リスク` に追加。
- 次アクションを「Phase C 4スイート再検証」に更新。

## Commands
```powershell
Get-Content .git/HEAD
Get-Content .git/config
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorSttIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorTtsIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_193956.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_193956.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194017.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194017.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194025.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194025.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194027.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_194027.log
```

## Tests
| run_at | suite | pass_fail | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|
| 2026-02-21 19:39:56 | CoreOrchestratorSttIntegrationTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_193956.xml`) | missing (`editmode-20260221_193956.log`) |
| 2026-02-21 19:40:17 | CoreOrchestratorTtsIntegrationTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_194017.xml`) | missing (`editmode-20260221_194017.log`) |
| 2026-02-21 19:40:25 | CoreOrchestratorLlmIntegrationTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_194025.xml`) | missing (`editmode-20260221_194025.log`) |
| 2026-02-21 19:40:27 | LoopbackHttpClientTests | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_194027.xml`) | missing (`editmode-20260221_194027.log`) |

## Tests 追補（Antigravity による再実行確認）
- Execution-Tool: Antigravity / Execution-Model: gemini-2.5-pro

| run_at | suite | pass_fail | artifact_xml |
|---|---|---|---|
| 2026-02-21 19:50 | CoreOrchestratorSttIntegrationTests | Passed 4/4 | `editmode-20260221_195019.xml` |
| 2026-02-21 19:50 | CoreOrchestratorTtsIntegrationTests | Passed 3/3 | `editmode-20260221_195037.xml` |
| 2026-02-21 19:54 | CoreOrchestratorLlmIntegrationTests | Passed 5/5 | `editmode-20260221_195406.xml` |
| 2026-02-21 20:05 | LoopbackHttpClientTests | Passed 5/5 | `editmode-20260221_200517.xml` |

run_unity_tests.ps1 の自動フォールバックにより環境復旧不要で通過。

## Rationale (Key Points)
- 重大リスク: STT統合の検証が取れないまま進行すると、partial/final誤処理による state 破綻を見逃す。
- 対策: Phase C 専用テスト + 回帰3スイートを固定実行し、失敗時は起動エラーと artifact 未生成をセットで記録する。
- 差分意図: 実装進捗と検証ブロックを `NEXT_TASKS` / `dev-status` / worklog で同一表現に揃え、次回再実行の起点を一本化する。

## Rollback
- 変更戻し対象:
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `docs/worklog/2026-02-21_u5_t4_phase_c_stt_integration.md`
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
1. ~~Unity起動失敗の復旧 → Phase C 4スイート再実行~~ 完了（Antigravity にて検証済み、上記Tests追補参照）
2. Phase C 完了判定: SttIntegrationTests 4/4 Passed（partial無視・final成功・final失敗retryable・空final無視）
3. U5-T4 全Phaseがartifact確認済みとなったため、NEXT_TASKSのPhase C「検証保留」記載を更新すること。
