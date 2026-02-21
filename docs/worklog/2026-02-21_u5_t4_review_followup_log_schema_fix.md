# Worklog: U5-T4 レビューFollow-up（bridge_fallbackログ項目整合）

- Date: 2026-02-21
- Task: `CoreOrchestrator.SendChatWithBridgeResult` の fallback ログ引数順を修正し、path/source_tier のスキーマ整合を回復
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `tools/run_unity_tests.ps1`
  - `Unity_PJ/artifacts/test-results/`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Report-Path: docs/worklog/2026-02-21_u5_t4_review_followup_log_schema_fix.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260221_1656.md
- Tags: [agent/codex, model/gpt-5, tool/codex, u5, u5-t4, review-followup, logging]

## Summary
`core.chat.bridge_fallback` 記録時の `RuntimeLog.Warn` 引数順誤りを修正し、`path` と `source_tier` の意味を正しい位置に戻した。
検証テストを2スイート再実行したが、実行環境で `Unity.exe` / `Unity.com` の双方がモジュール不足で起動失敗し、artifact は未生成だった。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
- `RuntimeLog.Warn(...)` の引数を以下へ修正:
  - `path`: `"/v1/chat/send"`
  - `source_tier`: `"core"`
- これにより、`path="core"` となる誤記録を解消。

## Commands
```powershell
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests"
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests"
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_165541.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_165541.log
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_165543.xml
Test-Path Unity_PJ/artifacts/test-results/editmode-20260221_165543.log
```

## Tests
| suite | run_at | pass_fail | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|
| CoreOrchestratorLlmIntegrationTests | 2026-02-21 16:55 | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_165541.xml`) | missing (`editmode-20260221_165541.log`) |
| LoopbackHttpClientTests | 2026-02-21 16:55 | Failed (起動前) | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`editmode-20260221_165543.xml`) | missing (`editmode-20260221_165543.log`) |

## Rationale (Key Points)
- 監査ログでは `path` と `source_tier` の意味が固定されているため、引数順誤りは可観測性の劣化につながる。
- 本修正は挙動変更を伴わないが、障害解析時のクエリ精度に直接影響するため先に是正した。

## Rollback
- 対象は `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs` の1箇所。
- もしロールバックする場合は、`RuntimeLog.Warn` 呼び出しを修正前に戻す。
- Obsidianログは削除しない。`Rolled back` / `Superseded` 注記で履歴を残す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Tests 追補（Antigravity による再実行確認）
- Execution-Tool: Antigravity / Execution-Model: gemini-2.5-pro

| suite | run_at | pass_fail | artifact_xml |
|---|---|---|---|
| CoreOrchestratorLlmIntegrationTests | 2026-02-21 17:00 | Passed 5/5 | `editmode-20260221_170008.xml` |
| LoopbackHttpClientTests | 2026-02-21 17:06 | Passed 5/5 | `editmode-20260221_170654.xml` |

Unity自動フォールバック（run_unity_tests.ps1 修正済み）により環境復旧不要で通過。

## Next Actions
1. ~~Unity起動エラー復旧 → 再実行~~ 完了（Antigravity にて検証済み）
2. ~~artifact採取~~ 完了（上記Tests追補参照）
3. U5-T4 Phase B 実装へ進む。
