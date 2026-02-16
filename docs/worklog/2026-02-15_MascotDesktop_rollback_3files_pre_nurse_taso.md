# Worklog: MascotDesktop_rollback_3files_pre_nurse_taso

- Date: 2026-02-15
- Task: nurse_taso調査セッション前の状態への3ファイル復元（amane_kanata顔中央線解消）
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: claude-sonnet-4-20250514
- Used-Skills: rollback, diff-analysis, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_rollback_3files_pre_nurse_taso.md
- Obsidian-Log: 未実施:本作業は復元のみでコード新規作成なし
- Tags: [agent/antigravity, model/claude-sonnet-4-20250514, tool/antigravity, rollback, face-line, amane_kanata]

## Summary
- 前セッション(Codex/gpt-5)のnurse_taso調査で変更された3ファイルを問題発生前のバックアップに復元した。
- amane_kanata official_v1 の顔中央線の原因は、`MaterialLoader.cs`の透明判定変更（`edge_alpha`を透明トリガーから除外）が最有力。
- コンパイルエラーは0件（CS1056も現在のファイルに存在しない）。

## Changes

### Before (現行 → .bak_current に退避)
| ファイル | 行数 |
|---------|------|
| MaterialLoader.cs | 488 |
| SimpleModelBootstrap.cs | 1838 |
| SimpleModelBootstrapTests.cs | 684 |

### After (バックアップから復元)
| ファイル | 復元元 | 行数 |
|---------|-------|------|
| MaterialLoader.cs | .bak3 | 407 |
| SimpleModelBootstrap.cs | .bak2 | 1615 |
| SimpleModelBootstrapTests.cs | .bak3 | 445 |

### 差分サマリ (bak → 現行の間に追加されていたもの、今回除去)
1. **MaterialLoader.cs** (bak3→現行: 81行追加)
   - 透明判定からedge_alpha除外 → **顔中央線の最有力原因**
   - ShouldUseWhiteFallbackMainTexture (shadow限定白fallback)
   - CalculateRequiredTransparentPixels (整数ベースの閾値計算)
   - RatioThresholdEpsilon, MainTextureStatusLoadedFallbackWhite
2. **SimpleModelBootstrap.cs** (bak2→現行: 223行追加)
   - toon/sphere テクスチャの詳細 missing/not_used 分類
   - BuildRemediationHint, ComputeRatio, IsTextureStatus, CountMissingTextureStatus
   - ライティング autoConfigureSceneLight 制御
3. **SimpleModelBootstrapTests.cs** (bak3→現行: 239行追加)
   - 上記新APIに対応するテスト追加

## Commands
```powershell
# 退避
Copy-Item MaterialLoader.cs MaterialLoader.cs.bak_current -Force
Copy-Item SimpleModelBootstrap.cs SimpleModelBootstrap.cs.bak_current -Force
Copy-Item SimpleModelBootstrapTests.cs SimpleModelBootstrapTests.cs.bak_current -Force

# 復元
Copy-Item MaterialLoader.cs.bak3 MaterialLoader.cs -Force
Copy-Item SimpleModelBootstrap.cs.bak2 SimpleModelBootstrap.cs -Force
Copy-Item SimpleModelBootstrapTests.cs.bak3 SimpleModelBootstrapTests.cs -Force
```

## Tests
- Command: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests" -Quit`
- Log: `Unity_PJ/artifacts/test-results/editmode-20260215_202818.log`
- Result: コンパイルエラー0件 (`LogAssemblyErrors (0ms)`×2, `error CS`ヒットなし)
- テスト実行: ライセンス問題でXML未生成 → Playモードで目視確認が必要

## Rationale (Key Points)
- 顔中央線の原因はedge_alpha除外による透明→不透明シェーダー切替が最有力
- 3ファイルをセットで戻す必要があった（テストが新APIを参照していたため）
- 現行ファイルは.bak_currentに退避済みで、いつでも戻せる

## Rollback
```powershell
Copy-Item MaterialLoader.cs.bak_current MaterialLoader.cs -Force
Copy-Item SimpleModelBootstrap.cs.bak_current SimpleModelBootstrap.cs -Force
Copy-Item SimpleModelBootstrapTests.cs.bak_current SimpleModelBootstrapTests.cs -Force
```

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes (未実施:復元のみ)
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unityで amane_kanata official_v1 を表示し、顔中央線の消失を確認する
2. 顔中央線が消えた場合、nurse_taso の表示も影響がないか確認する
3. 今後 edge_alpha の扱いを改善する場合は、顔マテリアルへの影響を個別テストすること
