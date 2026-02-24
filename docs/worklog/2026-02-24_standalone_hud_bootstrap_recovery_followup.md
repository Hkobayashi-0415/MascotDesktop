# Worklog: Standalone HUD Bootstrap Recovery Follow-up (2026-02-24)

- Date: 2026-02-24
- Task: candidates=0 継続と HUD rescan スパムの追跡修正
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Repo-Refs:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `docs/05-dev/QUICKSTART.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md`
  - `D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-24_standalone_hud_bootstrap_recovery_followup.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260224_1911.md
- Tags: [agent/codex, model/gpt-5, tool/codex, runtime-hud, standalone, bug-investigation, code-review]

## Summary
- ユーザー提供ログから `ui.hud.model_candidates_rescanned` が毎フレーム発生していることを確認。
- `SimpleModelBootstrap` の公開復旧 API と HUD 側の自己回復を追加。
- 候補自動再探索を 0.5 秒間隔へ制限し、ログスパムと無限再探索を抑制した。

## Findings
1. High: 候補0時に `EnsureModelCandidates()` がフレームごとに再探索し続ける
- Evidence: `runtime-20260224-05.jsonl` Tail に同一イベントが連続出力。
- Impact: ログスパム、原因切り分け困難、実効操作の観測性低下。

2. High: `SimpleModelBootstrap` 欠損時の復旧・識別ログ不足
- Evidence: `avatar.model.candidates.discovered` が見えない一方、HUD rescan だけが継続。
- Impact: runtime graph 欠損時に candidates が永続0となる。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `EnsureBootstrapForRuntime()` を public static で追加。
- `RuntimeInitializeOnLoadMethod` は上記APIを経由するよう整理。

2. `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
- `CacheDependencies()` で bootstrap 欠損時に `SimpleModelBootstrap.EnsureBootstrapForRuntime()` を呼び自己回復。
- 復旧成功時ログ `ui.hud.bootstrap_recovered` を追加。
- 欠損継続時ログ `ui.hud.bootstrap_missing` を追加（2秒間隔）。
- 自動 rescan を `AutoRescanIntervalSeconds=0.5f` でスロットル。

3. `docs/05-dev/QUICKSTART.md`
- Troubleshooting に `ui.hud.bootstrap_missing` / `ui.hud.bootstrap_recovered` の確認観点を追加。

## Commands
```powershell
# User log analysis (provided output basis)
Get-ChildItem C:\Users\sugar\AppData\LocalLow\DefaultCompany\project\logs -Filter "runtime-20260224-*.jsonl"
Select-String -Path "C:\Users\sugar\AppData\LocalLow\DefaultCompany\project\logs\runtime-20260224-*.jsonl" -Pattern "ui.hud.model_candidates_rescanned"
Get-Content "C:\Users\sugar\AppData\LocalLow\DefaultCompany\project\logs\runtime-20260224-05.jsonl" -Tail 80

# Static verification
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -Pattern "EnsureBootstrapForRuntime"
Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs -Pattern "bootstrap_recovered|bootstrap_missing|AutoRescanIntervalSeconds"
Select-String -Path docs/05-dev/QUICKSTART.md -Pattern "bootstrap_missing|bootstrap_recovered"
```

## Test Results
- 実施有無: 部分実施（静的検証）
- 実施内容:
  - 追加API/ログ/スロットル定数の存在確認。
  - ドキュメント追記の存在確認。
- 未実施:
  - Unity Runtime 実行確認（この環境では Unity 起動不可）。

## Next Actions
1. ユーザー環境で Standalone を再ビルド・再起動する。
2. `avatar.model.candidates.discovered` / `ui.hud.bootstrap_*` / `window.*` のログを再採取する。
3. candidates 復帰後に `State/Motion/Topmost/Hide/Show` を再確認する。

## Rollback Plan
- 戻す対象:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `docs/05-dev/QUICKSTART.md`
- 手順:
  - 差分を逆適用。
  - `docs/worklog/` に理由を追記。
  - Obsidianログは削除せず `Rolled back` / `Superseded` を追記。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Used-Skills recorded: Yes
- Tags include agent/model/tool: Yes
