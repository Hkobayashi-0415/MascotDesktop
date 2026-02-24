# Worklog: Runtime HUD Control Path Investigation + Code Review (2026-02-24)

- Date: 2026-02-24
- Task: Runtime HUD で報告された操作不達（State/Motion/Topmost/Hide/Show）と amane_kanata 中心線再発の調査、全体コードレビュー、運用手順同期
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AvatarStateController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/MotionSlotPlayer.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-24_runtime_hud_controlpath_investigation_review.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_1637.md
- Tags: [agent/codex, model/gpt-5, tool/codex, runtime-hud, bug-investigation, code-review]

## Summary
- 原因調査では、State/Motion の経路自体は生きている一方、`Motion` ボタンが HUD 内で二重経路（Orchestrator + 直接 PlaySlot）を持っていた。
- Topmost/Hide/Show は Windows Player ネイティブ機能であり、Unity Editor ではネイティブ効果が見えない設計だったが、手順書がその前提を十分に明示していなかった。
- 修正として、Motion の二重呼び出しを解消し、Editor シミュレーションログと手順書の判定条件を同期した。

## Findings (Code Review)
1. High: Motion 操作が1クリックで二重処理される可能性（HUDの直接 `PlaySlot` + Orchestrator 経路）
   - Evidence: `RuntimeDebugHud` motion button handlers と `AvatarStateController` の `MotionSlotRequested` 購読。
   - Risk: 1操作=1要求の相関が崩れ、重複ログ/重複副作用が発生しうる。
   - Fix: HUD は `TriggerMotionSlot` で単一経路に統一。
2. Med: Editor で Topmost/Hide のネイティブ効果が見えないのに「成功」に見える
   - Evidence: `WindowController` / `ResidentController` のプラットフォーム分岐。
   - Risk: 手動確認で false negative/false positive が発生しやすい。
   - Fix: `window.topmost.simulated` / `window.resident.*.simulated` ログ追加、HUD/手順書に明示。
3. Med: `Model: rescan(list)` の仕様が手順上あいまい
   - Evidence: rescan は候補更新のみ（モデル切替はしない）。
   - Risk: 動作不良と誤認。
   - Fix: 手順書へ仕様を明記し、HUD へ再スキャンログを追加。
4. Residual Risk (Med): UR-005 の「motion再生」が現在は slot 状態遷移中心で、視覚的再生保証は限定的
   - Evidence: `MotionSlotPlayer` は slot/state 更新とログ中心。
   - Action: 次タスクで視覚再生要件の定義/実装範囲を明確化。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- Motion ボタンを `TriggerMotionSlot` 経由へ変更し、直接 `PlaySlot` 呼び出しを除去。
- `Model: rescan(list)` / `Model switch` / `Candidate mode` の可観測ログ追加。
- HUD に Window 操作の実行モード表示（native/simulation）を追加。

2. `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- Editor/非Windows 実行時に `window.topmost.simulated` ログを追加。

3. `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
- Editor/非Windows 実行時に `window.resident.hide.simulated` / `window.resident.restore.simulated` ログを追加。

4. `docs/05-dev/QUICKSTART.md`
- `rescan(list)` の仕様、State/Motion の確認観点、Topmost/Hide の Standalone 判定条件を明記。
- `Last Updated` を 2026-02-24 に更新。

5. `docs/05-dev/unity-runtime-manual-check.md`
- 合格条件に State/Motion/Rescan の具体判定と Editorシミュレーションログ判定を追加。
- `Last Updated` を 2026-02-24 に更新。

6. `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
- Manual Check に Editor vs Standalone の判定注記を追加。
- `Last Updated` を 2026-02-24 に更新。

7. `docs/worklog/2026-02-24_runtime_hud_controlpath_investigation_review.md`（本ファイル）
- 調査/レビュー/修正/判定を記録。

8. `D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_1637.md`（新規）
- テンプレ準拠ログを記録。

## Commands
```powershell
# Repository identification fallback (git unavailable)
Get-Content .git/HEAD
Get-Content .git/config

# Investigation: code read
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Core/CoreOrchestrator.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/AvatarStateController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/MotionSlotPlayer.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs
Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs

# Spot checks
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs -Pattern '_motionSlotPlayer\?\.PlaySlot\("idle"\)|_motionSlotPlayer\?\.PlaySlot\("wave"\)'
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs -Pattern 'TriggerMotionSlot\("idle"\)|TriggerMotionSlot\("wave"\)|Window Ops:'
Select-String -Path docs/05-dev/QUICKSTART.md,docs/05-dev/unity-runtime-manual-check.md,Unity_PJ/docs/05-dev/phase3-parity-verification.md -Pattern 'Standalone Player|simulated|候補更新のみ'
```

## Tests
- 実施有無: Unity テスト未実施（ユーザー実行環境で実行予定のため）。
- 代替検証:
  - 直接呼び出し重複除去を `Select-String` で確認（HUD 内の `PlaySlot("idle"/"wave")` 直接呼び出しゼロ）。
  - 追加ログイベント名の存在確認。
  - 手順書3ファイルの判定文言同期確認。
- ユーザー環境での推奨実行:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorSttIntegrationTests" -RequireArtifacts`
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorTtsIntegrationTests" -RequireArtifacts`
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests" -RequireArtifacts`
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests" -RequireArtifacts`

## Rationale (Key Points)
- Motion の二重経路は request_id 一意性と1操作1イベントの追跡性を損なうため、運用上の誤判定を誘発する。
- Topmost/Hide/Show は実装上プラットフォーム依存であり、Editor での見え方のみを根拠に fail 判定すると誤る。
- 手順書と実装ログを同じ判定軸に揃えることで、今後の手動確認結果の再現性を上げる。
- Superseded/Rolled back 判断:
  - 既存 worklog の上書きは実施していないため `Superseded` / `Rolled back` 注記は不要。

## Rollback
- 変更戻し対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/ResidentController.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `docs/05-dev/unity-runtime-manual-check.md`
  - `Unity_PJ/docs/05-dev/phase3-parity-verification.md`
  - `docs/worklog/2026-02-24_runtime_hud_controlpath_investigation_review.md`
- ロールバック時の方針:
  - コード/文書を変更前に戻し、理由（何を・なぜ戻したか）を新規 worklog に追記する。
  - Obsidian ログは削除せず、`Rolled back` または `Superseded` を追記する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Used-Skills recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Windows Standalone Player で Topmost/Hide/Show のネイティブ効果を確認する。
2. 中心線再発時に `avatar.model.material_diagnostics` / `avatar.model.remediation_hint` を採取し、asset/lighting どちらが支配的かを判定する。
3. UR-005 の「視覚的 motion 再生」要件を明文化し、必要なら Motion 実再生の実装タスクを起票する。
