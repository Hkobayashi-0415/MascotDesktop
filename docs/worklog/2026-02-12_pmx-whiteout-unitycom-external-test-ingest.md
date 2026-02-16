# Worklog: pmx-whiteout-unitycom-external-test-ingest

- Date: 2026-02-12
- Task: ユーザー実行 Unity.com テスト失敗結果の取り込みと裏取り
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: worklog-update, bug-investigation
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Failed_2009.md, docs/worklog/2026-02-11_pmx-whiteout-toon-sphere-status-breakdown.md, Unity_PJ/artifacts/test-results/editmode-20260211_200355.log, Unity_PJ/artifacts/test-results/editmode-20260211_200355.xml
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-unitycom-external-test-ingest.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0107.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, unity.com, tests, ingest]

## Summary
ユーザー提供の `Unity.com` 実行結果を取り込み、`editmode-20260211_200355.xml` により `SimpleModelBootstrapTests` が `total=22, passed=20, failed=2` であることを確認した。失敗テストは透明判定しきい値に関する2件で、いずれも `Expected: True, But was: False`。

## Evidence
- 受領レポート: `docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Failed_2009.md`
- 実行結果XML: `Unity_PJ/artifacts/test-results/editmode-20260211_200355.xml`
  - `<test-run ... total="22" passed="20" failed="2" ...>`
  - failed test #1: `MaterialLoaderTransparencyHeuristic_UsesSemiTransparentRatioThreshold` (`SimpleModelBootstrapTests.cs:261`)
  - failed test #2: `MaterialLoaderTransparencyHeuristic_UsesStrongTransparentRatioThreshold` (`SimpleModelBootstrapTests.cs:279`)
- 実行ログ: `Unity_PJ/artifacts/test-results/editmode-20260211_200355.log`
  - `Test run completed. Exiting with code 2 (Failed). One or more tests failed.`

## Changes
- コード変更なし。
- 取り込み記録を新規作成:
  - `docs/worklog/2026-02-12_pmx-whiteout-unitycom-external-test-ingest.md`
  - `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0107.md`

## Commands
- `Get-Content docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_UnityCom_Failed_2009.md`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_200355.log`
- `Test-Path Unity_PJ/artifacts/test-results/editmode-20260211_200355.xml`
- `Select-String Unity_PJ/artifacts/test-results/editmode-20260211_200355.log`
- `Select-String Unity_PJ/artifacts/test-results/editmode-20260211_200355.xml`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-unitycom-external-test-ingest.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0107.md`

## Tests
- Test execution: 未実施（本作業は外部実行結果の取り込みのみ）。
- Evidence validation:
  - `editmode-20260211_200355.log` exists: True
  - `editmode-20260211_200355.xml` exists: True
  - XML failure count (`failed=2`) と受領レポートの一致: Yes

## Rationale (Key Points)
- 現時点の正式テスト状態は「Unity起動失敗」ではなく「実行済みで2件失敗」。
- 失敗テストはどちらも `MaterialLoader` 透明判定閾値の境界条件で、今後の判断点は「テスト期待値を調整するか、実装閾値を戻すか」。
- 既存の白飛び調査（ライト寄与、テクスチャ不足分解）と独立した回帰要因として追跡する必要がある。

## Next Actions
1. `IsTextureTransparentByPixels` のしきい値意図（0.10/0.032, 0.59/0.10）を仕様として確定する。
2. 仕様に合わせて failing test 2件の期待値を調整するか、実装側を調整するかを選択する。
3. ユーザー実行で再テストし、`failed=0` かを確認する。

## Rollback
1. `Remove-Item docs/worklog/2026-02-12_pmx-whiteout-unitycom-external-test-ingest.md`
2. `Remove-Item D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_0107.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
