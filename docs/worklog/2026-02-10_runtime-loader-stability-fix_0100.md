# Worklog: Runtime Loader Stability Fix (BMP/PMD/Swap)

- Date: 2026-02-10
- Task: BMPデコード経路統一・.pmd分類整合・モデル切替の黒画面低減
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs
  - Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs
  - Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
  - D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md
  - D:/dev/00_repository_templates/ai_playbook/skills/code-review/SKILL.md
  - D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-10_runtime-loader-stability-fix_0100.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260210_0100.md
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, pmx]

## Summary

ユーザー報告の「青カプセル（BMP失敗）」「黒画面体感（PMX切替）」に対して、表示経路を安定化する修正を実装した。BMPは LibMMD の BMP ローダー優先とし、`.pmd` 分類漏れを解消、モデル切替は成功時差し替え方式へ変更した。

## Changes

- `ReflectionModelLoaders`:
  - `ImageLoadAttemptResult` を追加
  - `TryLoadImageTexture(string)` を追加
  - `.bmp` は `LibMMD.Unity3D.TextureLoader.LoadBmp`（reflection, non-public static）を優先
  - Unity `LoadImage` 失敗時 `Texture2D` を `Destroy`
- `SimpleModelBootstrap`:
  - `TryDisplayAsImagePlane` を `TryLoadImageTexture` 経由へ切替
  - `ReloadModel` の先行 `ClearActiveModelRoot` を削除
  - `RegisterActiveModelRoot` で新モデル登録後に旧モデルを破棄するスワップ方式へ変更
  - 未使用 `ClearActiveModelRoot` を削除
- `ModelFormatRouter`:
  - `.pmd` を `ModelAssetKind.Pmx` に分類
- テスト:
  - `ModelFormatRouterTests` に `.pmd` 分類テストを追加
  - `SimpleModelBootstrapTests` に `.pmd` 候補包含確認を追加

## Commands

- `Get-Content ...\SKILL.md` (bug-investigation/code-review/worklog-update)
- `Get-Content` / `Select-String` for target runtime files
- `apply_patch` (5 files)
- Unity test attempt:
  - `Unity.exe -batchmode -nographics -projectPath D:\dev\MascotDesktop\Unity_PJ\project -runTests -testPlatform EditMode -testFilter MascotDesktop.Tests.EditMode.ModelFormatRouterTests,MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests -testResults ... -logFile ... -quit`

## Tests

- EditMode batch run: **Failed to start Unity process**
  - Error: `指定されたモジュールが見つかりません` (`Unity.exe` 起動失敗)
- Static verification:
  - `.bmp` / `.pmd` 分類反映確認
  - 画像ロード経路が `TryLoadImageTexture` 経由になっていることを確認
  - スワップ方式 (`Destroy(previousRoot)`) を確認

## Rationale (Key Points)

- ログで `ASSET.READ.DECODE_FAILED` が発生していたBMPを、LibMMD経路優先にすることで既存PMX資産との互換を優先した。
- 切替時の黒画面体感は、先行破棄が主要因のため、表示中モデルを保持したまま成功時に差し替える方式に変更した。
- `.pmd` は候補化済みなのに未分類だったため、分類側を合わせて不整合を解消した。

## Rollback

1. `ReflectionModelLoaders.cs` の `TryLoadImageTexture` 追加差分を戻す。
2. `SimpleModelBootstrap.cs` の `ReloadModel` / `RegisterActiveModelRoot` 差分を戻す。
3. `ModelFormatRouter.cs` の `.pmd` case を戻す。
4. 追加したテスト差分を戻す。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions

1. Unity Editor 実機で `ModelFormatRouterTests` / `SimpleModelBootstrapTests` を実行する。
2. `characters/momone_nene_official_v1/mmd/texture/TGA/S_1.bmp` でデコード成功確認を実施する。
3. PMX切替中の黒画面体感を再検証し、必要なら非同期ロード表示（Loading状態）を追加する。
