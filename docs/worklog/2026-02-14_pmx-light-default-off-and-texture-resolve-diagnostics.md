# Worklog: pmx-light-default-off-and-texture-resolve-diagnostics

- Date: 2026-02-14
- Task: ライト既定ONの無効化とテクスチャ解決ログの比較強化
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Report-Path: docs/worklog/2026-02-14_pmx-light-default-off-and-texture-resolve-diagnostics.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260214_2114.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, texture, lighting]

## Summary
白飛び原因として指摘されたライト既定ON挙動をコード側でデフォルト無効化した。
同時に、AZKi対照比較に使えるようテクスチャ解決ログ（fallback/recursive/fail）を追加し、欠損原因の追跡情報を強化した。

## Changes
- 更新: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
  - `autoConfigureSceneLight` (default: false)
  - `createSceneLightIfMissing` (default: false)
- 更新: `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - ライト自動設定を `autoConfigureSceneLight` で制御。
  - `autoConfigureSceneLight=false` 時は `SimpleModelLight` が有効なら自動無効化。
  - ログ: `avatar.render.light.autoconfig_skipped` に既存ライト情報を出力。
- 更新: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
  - fallback解決ログ、recursive解決ログ、解決失敗ログを追加。
  - 失敗時に `textureDirExists/texturesDirExists/parent...Exists` を出力。
- 更新: `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
  - `Implementation Update (2026-02-14)` を追記。

## Commands
- `apply_patch Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs`
- `apply_patch Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `apply_patch Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- `apply_patch Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 実行時刻:
  - `2026-02-14 21:13:03`
  - `2026-02-14 21:13:36`
  - `2026-02-14 21:14:00`
- 結果:
  - この環境では Unity.com 起動で `指定されたモジュールが見つかりません` のため失敗。
  - 今回分 `.log/.xml` は未生成。
  - Unity.com 成否はユーザー手動実行で確認継続。

## Rationale (Key Points)
- 白飛びはUI上の Light ON/OFF で即変化しており、まず既定ライト挙動の固定を解除するのが妥当。
- 欠損/グレーは設定値よりテクスチャ解決結果の差分（解決戦略と失敗理由）を比較する方が根拠として強い。
- AZKi を正常対照として、同じログキーで kanata/nene_BEA と差分比較できる状態を作ることを優先した。

## Next Actions
1. ユーザー環境で F0 固定のまま 3モデル（kanata/AZKi/nene_BEA）を再表示し、`[TextureLoader] resolve ...` ログ差分を採取。
2. `resolve failed` が出るマテリアルの `requested` と実ファイル配置を突合し、パス不一致/実ファイル不足/拡張子差を分類。
3. 分類結果に応じて `TextureLoader` 探索規則追加 or アセット配置是正のどちらを採用するか決定。

## Rollback
1. `SimpleModelConfig.cs` の lighting項目2つを削除。
2. `SimpleModelBootstrap.cs` の `autoConfigureSceneLight` 分岐と `SimpleModelLight` 自動無効化を削除し、旧挙動へ戻す。
3. `TextureLoader.cs` の追加ログを削除して元の解決処理に戻す。
4. `pmx-validation-procedure-and-record.md` の `Implementation Update (2026-02-14)` を削除。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
