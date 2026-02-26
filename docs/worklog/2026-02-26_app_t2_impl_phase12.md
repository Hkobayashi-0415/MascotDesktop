# Worklog: APP-T2 実装 Phase 1-2（2026-02-26）

- Date: 2026-02-26
- Task: APP-T2 Full Core 接続の実装着手（設定切替/通信契約/degraded管理）
- Execution-Tool: Codex
- Execution-Agent: codex-gpt5
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs`
  - `docs/05-dev/app-t2-full-core-design.md`
  - `docs/05-dev/app-spec-and-roadmap.md`
  - `docs/NEXT_TASKS.md`
  - `docs/05-dev/dev-status.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/phase-planning/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-26_app_t2_impl_phase12.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260227_0020.md
- Tags: [agent/codex-gpt5, model/gpt-5, tool/codex, app-dev, app-t2, implementation, full-core]

## Summary

APP-T2 実装を 3分割計画で開始し、Phase 1-2 を反映した。`RuntimeConfig` へ `mode=loopback|core` と endpoint別 timeout/retry を追加、`LoopbackHttpClient` へ retry/backoff と拡張スキーマ（`core_request_id/error_name/attempt`）を実装、`CoreOrchestrator` に機能単位 degraded enter/exit 管理を追加した。HUD には runtime mode 表示と管理者向け degraded 詳細表示を追加した。

## Findings（重大度順）

| # | 重大度 | 内容 | 対応 |
|---|---|---|---|
| F1 | High | APP-T2 実装前のコードは loopback単一設定（`loopbackBaseUrl/httpTimeoutMs`）で、endpoint別 retry/backoff 契約が未実装。 | `RuntimeConfig` と `LoopbackHttpClient` を設計仕様へ拡張。 |
| F2 | Med | Core 側で機能単位 degraded 状態（enter/exit）を保持する実装がなかった。 | `CoreOrchestrator` に feature health 状態と `core.degraded_enter/exit` を追加。 |
| F3 | Med | Unity 実行環境でテスト起動前失敗（`指定されたモジュールが見つかりません`）が再発し、EditMode 実行を完了できない。 | `run_unity_tests.ps1` 実行結果を証跡化し、テスト継続を次アクションへ移管。 |

## Changes

1. `RuntimeConfig.cs`
   - `RuntimeMode`（`Loopback/Core`）追加
   - `coreBaseUrl`、endpoint別 timeout/retry、`adminDebugMode` 追加
   - `ResolveBridgeBaseUrl` / `GetTimeoutMsForPath` / `GetMaxRetriesForPath` 追加

2. `LoopbackHttpClient.cs`
   - 結果DTOへ `CoreRequestId` / `ErrorName` / `Attempt` を追加
   - endpoint別 timeout/retry の利用
   - 5xx/timeout/通信失敗に限定した自動リトライ実装
   - 指数バックオフ + jitter 実装
   - レスポンス解析に `core_request_id/error_name/attempt` を追加

3. `CoreOrchestrator.cs`
   - feature別 degraded 状態管理（Core/LLM/TTS/STT）
   - `core.degraded_enter` / `core.degraded_exit` ログ追加
   - HUD参照用 `IsFeatureDegraded` / `DegradedSummary` 追加

4. `RuntimeDebugHud.cs`
   - `Runtime Mode` / `Degraded` 表示追加
   - 管理者モード時の feature別 degraded 詳細表示追加
   - `config/set` 操作導線を削除（APP-T2 read-only 方針）
   - orchestrator 呼び出し時に `error_name/core_request_id/attempt` を伝搬

5. `LoopbackHttpClientTests.cs`
   - `runtimeMode=Core` の base URL 反映を検証
   - エラー応答で `core_request_id/error_name/attempt` を検証
   - mismatch 応答で `CoreRequestId` / `ErrorName` を検証

6. 状態同期ドキュメント
   - `docs/NEXT_TASKS.md` に R40 追加、APP-T2 状態を In Progress へ更新
   - `docs/05-dev/dev-status.md` に APP-T2 実装着手を追記
   - `docs/05-dev/app-spec-and-roadmap.md` の F-09 状態を In Progress へ更新

## Commands

```powershell
# 実行環境確認
Get-ChildItem Unity_PJ/project/Assets/Scripts/Runtime/Config -File
Get-ChildItem Unity_PJ/project/Assets/Scripts/Runtime/Core -File
Get-ChildItem Unity_PJ/project/Assets/Scripts/Runtime/Ipc -File

# テスト実行（Loopback 単体）
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "LoopbackHttpClientTests" -RequireArtifacts -Quit

# 変更ファイル静的確認
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Ipc/LoopbackHttpClient.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
Get-Content Unity_PJ/project/Assets/Tests/EditMode/LoopbackHttpClientTests.cs
```

## Tests

| テスト | コマンド | exit code | 結果 | 根拠 |
|---|---|---|---|---|
| T1: LoopbackHttpClientTests 実行 | `run_unity_tests.ps1 -TestFilter "LoopbackHttpClientTests"` | 1 | FAIL（環境要因） | Unity.exe/Unity.com とも起動前失敗（指定されたモジュールが見つかりません） |
| T2: 静的整合確認 | `Get-Content` で変更ファイルを確認 | 0 | PASS | 主要変更の構文・参照整合を確認 |

## Rationale（Key Points）

- APP-T2設計（`app-t2-full-core-design.md`）の固定点を優先実装し、まず設定切替と通信契約を収束させた。
- 自動リトライは設計通り「通信失敗/timeout/5xx のみ」に限定し、429や業務エラーの無条件リトライを避けた。
- `request_id` 固定 + `attempt` 加算、`core_request_id` 受領を実装し、Unity/Core の相関可能性を高めた。

## Rollback

- 変更対象5ファイル（RuntimeConfig/CoreOrchestrator/LoopbackHttpClient/RuntimeDebugHud/LoopbackHttpClientTests）を差分逆適用。
- 状態同期3文書（`NEXT_TASKS` / `dev-status` / `app-spec-and-roadmap`）の APP-T2 更新を逆適用。
- `docs/worklog/2026-02-26_app_t2_impl_phase12.md` と Obsidian ログは削除しない。
- ロールバック時は本ファイルへ理由（何を・なぜ戻したか）を追記し、Obsidian ログへ `Rolled back` / `Superseded` を追記する。

## Record Check

- Report-Path exists: True（`docs/worklog/2026-02-26_app_t2_impl_phase12.md`）
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes（`D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260227_0020.md`）
- Execution fields recorded: Yes（Execution-Tool / Execution-Agent / Execution-Model）
- Tags include agent/model/tool: Yes

## Next Actions

1. Unity 実行環境を復旧し、EditMode 4スイート（LLM/TTS/STT/Loopback）を実行する。
2. 手動確認（通常/劣化/復帰、管理者/通常表示差）を実施する。
3. NF-01（timeout秒数調整）の解除要否を判定し、APP-T2 DoD 判定を更新する。
