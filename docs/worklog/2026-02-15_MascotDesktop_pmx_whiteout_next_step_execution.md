# Worklog: MascotDesktop_pmx_whiteout_next_step (execution)

- Date: 2026-02-15
- Task: 白飛び再発防止とテクスチャ解決汚染防止の最小差分実装
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: runtime-hardening, bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_pmx_whiteout_next_step_execution.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0101.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, texture-resolution, lighting]

## Summary
- Lightの既定挙動を固定するため、`autoConfigureSceneLight=false` 時に既存シーンライトを無効化できる設定を追加（既定ON）。
- テクスチャ探索で別キャラ階層に到達しないよう、`TextureLoader` の再帰探索から `grandParentDir` を除外し、`baseDir + parentDir` のみに制限。

## Changes
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
- `disableExistingSceneLightsWhenAutoConfigOff` を追加（default: `true`）。

2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `autoConfigureSceneLight=false` の分岐で、上記フラグが有効時に `FindObjectsOfType<Light>()` の有効ライトを無効化。
- 診断ログ `avatar.render.light.autoconfig_skipped` に以下を追記:
  - `disableExistingSceneLightsWhenAutoConfigOff`
  - `disabledSceneLightsCount`

3. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- 再帰探索 root から `grandParentDir` を削除。
- これにより、`characters` 直下まで上がって他キャラの同名テクスチャを拾う経路を遮断。

## Commands
- `Get-Content AGENTS.md`
- `Get-Content docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md`
- `Get-Content Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `Get-Content Unity_PJ/docs/NEXT_TASKS.md`
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-15_MascotDesktop_pmx_whiteout_next_step_execution.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0101.md`

## Tests
1. EditMode test (change #1後)
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（`指定されたモジュールが見つかりません`）。この環境では実行不能。
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_010124.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_010124.xml`

2. EditMode test (change #2後)
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result: Unity起動不可（同上）。
- Log target: `Unity_PJ/artifacts/test-results/editmode-20260215_010145.log`
- XML target: `Unity_PJ/artifacts/test-results/editmode-20260215_010145.xml`

## Rationale (Key Points)
- 白飛びはユーザー観測で Light ON/OFF に強く依存しており、まず再発防止として既定ライト動作を固定するのが最短。
- テクスチャ欠損/灰色化は「探索範囲が広すぎる場合の誤解決」を排除してから比較する方が、AZKi対照比較の再現性が高い。
- 既存スコアリングは維持し、探索rootだけを制限して差分を最小化した。

## Rollback
1. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
   - `disableExistingSceneLightsWhenAutoConfigOff` 追加行を削除。
2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
   - 既存ライト無効化ロジックを削除し、旧ログ形式へ戻す。
3. `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
   - `grandParentDir` を再帰探索 roots に戻す。
4. 追加ログファイル削除:
   - `docs/worklog/2026-02-15_MascotDesktop_pmx_whiteout_next_step_execution.md`
   - `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260215_0101.md`

## Next Actions
1. Unity側で F0/F3・3モデルを再採取し、`avatar.render.light.autoconfig_skipped` の `disabledSceneLightsCount` を確認する。
2. かなたモデルで `[TextureLoader] resolve ... strategy=...` の変化を取得し、誤解決の減少を確認する。
3. まだ欠損が残る場合は、`amane_kanata_*` の PMX起点ディレクトリを1系統に整理する（資産レイアウト修正）。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
