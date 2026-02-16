# Worklog: PMX Review Follow-up Fix Pack

- Date: 2026-02-10
- Task: レビュー指摘に基づく `.bmp` 整合・候補抽出/表示経路補強・計画書同期・調査ログ導入
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ModelFormatRouter.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/Assets/Tests/EditMode/ModelFormatRouterTests.cs
  - Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
  - Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs.meta
  - docs/reports/2026-02-09_factor-isolation-execution-plan.md
  - Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
  - D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260210_0032.md
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, pmx]

## Summary

レビュー指摘のうち、`.bmp` 不整合、画像アスペクト比崩れ、2件前提計画の不整合、rescan期待差、テクスチャ解決調査手段、回帰テスト不足を対象に修正を実装した。`mmd/texture` 削除は方針未確定のため未実施で保留した。

## Changes

- `.bmp` を `ModelFormatRouter` の Image 判定に追加し、候補表示時の Unsupported 落ちを解消。
- `SimpleModelBootstrap` に `BuildModelCandidatesFromRelativePaths` を追加し、候補抽出ロジックを純粋関数化。
- `SimpleModelBootstrap` に `ComputeImagePlaneScale` を追加し、画像平面をアスペクト比維持で表示。
- `RuntimeDebugHud` に rescan の挙動明示 (`Model: rescan(list)` / candidates only) を追加。
- `TextureLoader` / `MaterialLoader` に `MASCOTDESKTOP_PMX_TEXTURE_TRACE` 連動の一時トレースログを追加。
- `docs/reports/2026-02-09_factor-isolation-execution-plan.md` を PMX 8件 + 候補40件前提へ更新。
- `pmx-validation-procedure-and-record.md` に rescan 表記同期と trace ログ手順を追記。
- EditMode テストを追加/更新:
  - `ModelFormatRouterTests` に `.bmp` ケース追加
  - `SimpleModelBootstrapTests` 新規作成（候補抽出4件上限、.bmp候補、画像スケール）

## Commands

- `git rev-parse --show-toplevel` (failed: git not found)
- `git remote -v` (failed: git not found)
- `git status --short` (failed: git not found)
- `Get-Content .git/HEAD`
- `Get-Content .git/config`
- `Get-ChildItem -Path Unity_PJ -Recurse -File ...`
- `Select-String -Path ... -Pattern ...`
- `Get-Content <target files>`
- `Get-Date -Format "yyMMdd_HHmm"`
- `& 'C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe' -batchmode ... -runTests ...` (failed)

## Tests

- EditMode batch test command:
  - `Unity.exe -batchmode -nographics -projectPath D:\dev\MascotDesktop\Unity_PJ\project -runTests -testPlatform EditMode -testFilter MascotDesktop.Tests.EditMode.ModelFormatRouterTests,MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests -testResults D:\dev\MascotDesktop\Unity_PJ\artifacts\test-results\editmode-model-tests.xml -logFile D:\dev\MascotDesktop\Unity_PJ\artifacts\test-results\editmode-model-tests.log -quit`
- Result: **Failed to start Unity process**（`指定されたモジュールが見つかりません`）
- Static verification:
  - `.bmp` が候補拡張子と分類に一致していることをコード確認
  - `BuildModelCandidates` 相当ロジックで `3D 8 + image 32 = 40` を再計測確認

## Rationale (Key Points)

- 候補抽出と分類がずれると実行時フォールバックが必発になるため、`.bmp` の入口と分類を一致させた。
- アスペクト比崩れは再発しやすいため、表示スケール計算を独立メソッド化してテストで固定化した。
- かなた白化調査は恒常的な大量ログを避けるため、環境変数トグルで必要時のみ有効化する方式にした。
- `mmd/texture` 削除は `.tga` 参照運用判断前に行うと破綻リスクが高いため、今回スコープ外とした。

## Rollback

1. 変更ファイルを単位ごとに巻き戻す:
   - `ModelFormatRouter.cs` の `.bmp` case を除去
   - `SimpleModelBootstrap.cs` の新規 static メソッドと呼び出しを除去
   - `RuntimeDebugHud.cs` の表記変更を戻す
   - `TextureLoader.cs` / `MaterialLoader.cs` の trace 追加を除去
   - 新規テスト `SimpleModelBootstrapTests.cs(.meta)` を削除
2. ドキュメントを元版へ戻す:
   - `docs/reports/2026-02-09_factor-isolation-execution-plan.md`
   - `Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions

1. Unity Editor 実機で EditMode テストを再実行し、XML結果を保存する。
2. かなたPMX検証時のみ `MASCOTDESKTOP_PMX_TEXTURE_TRACE=1` を有効化してログ採取する。
3. `.tga` 運用方針（維持 or fallback復帰）確定後に `mmd/texture` 整理可否を判断する。
