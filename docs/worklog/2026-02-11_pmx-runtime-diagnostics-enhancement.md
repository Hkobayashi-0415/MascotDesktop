# Worklog: pmx-runtime-diagnostics-enhancement

- Date: 2026-02-11
- Task: PMX白飛びのコード外要因切り分けのための runtime 診断ログ強化
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, tools/run_unity_tests.ps1, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md, D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-runtime-diagnostics-enhancement.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1447.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, diagnostics, unity]

## Summary
`missingMainTexResolveMats=0` でも白飛びが残るケースに対応するため、SimpleModelBootstrap に runtime 診断イベントを追加した。これにより、ディレクトリ構造・材質パラメータ・Unity描画環境を同時にログ比較できる。

## Changes
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - 追加イベント: `avatar.model.path_structure`
    - モデルディレクトリにおける `texture`/`textures` の存在、画像ファイル数（top/nested）を記録。
  - 追加イベント: `avatar.model.material_diagnostics`
    - 材質総数、Toon/SphereAdd/SphereMul 欠損数、`_Shininess` 高値数、`_SpecularColor` 高値数、`_Color` 高値数、サンプル材質情報を記録。
  - 追加イベント: `avatar.render.environment_diagnostics`
    - `QualitySettings.activeColorSpace`, HDR/MSAA, AA, LOD bias, near/far clip, ambient/light を記録。
  - PMXロード前に path structure 診断を呼び出すよう変更。
  - 診断用ヘルパー追加: `IsTextureFilePath`, `BuildMaterialDiagnosticSample`, `MaxColorChannel`。
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `DiagnosticsHelpers_IsTextureFilePath_DetectsKnownExtensions` を追加。

## Commands
- `rg -n "render_diagnostics|missing_main_textures|TryDisplayPmx|EnsureCameraAndLight|ApplyRuntimeRenderQuality" Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs -S`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`（該当行確認）
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `apply_patch`（SimpleModelBootstrap.cs, SimpleModelBootstrapTests.cs）
- `& .\tools\run_unity_tests.ps1 -TestPlatform EditMode -TestFilter MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests -Quit`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_144646.xml`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_144646.log`

## Tests
- Target: `MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests`
- Attempt: 実行コマンドは起動
- Result: **Failed**
  - Unity起動失敗: `指定されたモジュールが見つかりません` (ResourceUnavailable)
  - XML/log生成: `False/False`
  - 補足: `run_unity_tests.ps1` の終了コードは0だったが、実際はUnityプロセス起動失敗

## Rationale (Key Points)
- 白飛びは texture resolve だけで説明できないため、観測点を runtime に増やして原因を一意化する必要がある。
- ユーザー指摘どおり、ディレクトリ構造・Unity設定・材質パラメータを同時観測できるログが有効。
- 既存の `render_diagnostics` と新規イベントを組み合わせることで A/B/C 分岐（resolve / shader-material / spec）の判断精度を上げる。

## Rollback
1. `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` の追加テストを戻す
2. `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` の新規診断ロジックを戻す

## Next Actions
1. Unity実機で `SimpleModelBootstrapTests` を再実行して pass を確認。
2. 8モデルを F0固定で再ロードし、新規イベント3種を採取。
3. AZKi/Nurse系の `material_diagnostics` を比較し、shader係数の次段修正に進む。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
