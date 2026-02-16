# Worklog: MascotDesktop_restore_bak_current_and_revert_edge_alpha

- Date: 2026-02-15
- Task: bak_current復元 + edge_alpha透明判定除外の最小修正(顔中央線対策)
- Execution-Tool: Antigravity
- Execution-Agent: Antigravity
- Execution-Model: claude-sonnet-4-20250514
- Used-Skills: worklog-review, rollback, minimal-diff-fix
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-15_MascotDesktop_restore_bak_current_and_revert_edge_alpha.md
- Obsidian-Log: 未実施:復元+最小修正のみ
- Tags: [agent/antigravity, model/claude-sonnet-4-20250514, tool/antigravity, rollback, face-line, edge-alpha, amane_kanata]

## Summary
- 前回のbak3/bak2復元が古すぎた（2/12のremediation hint、shader cap、toon詳細分類等が全て欠落）ため、bak_current（前セッション最終状態）に復元し直した。
- 全worklog(2/12, 2/15計27件)をレビューし、変更時系列を正確に把握。
- 顔中央線の原因である `MaterialLoader.cs` の `ResolveTransparentReason` edge_alpha透明判定除外（2/15の変更）のみを元に戻す最小修正を適用。
- コンパイルエラー0件を確認。

## Changes

### MaterialLoader.cs (修正1箇所のみ)
```diff
# ResolveTransparentReason メソッド (L237-248)
-// Edge alpha controls outline contribution, but edge-only materials should not
-// be forced into transparent surface rendering.
-var bySurfaceTransparency = byDiffuseAlpha || byTextureAlpha;
-isTransparent = bySurfaceTransparency;
-if (!isTransparent)
-{
-    return TransparentReasonOpaque;
-}
+isTransparent = byDiffuseAlpha || byEdgeAlpha || byTextureAlpha;
```

### SimpleModelBootstrap.cs, SimpleModelBootstrapTests.cs
- bak_current（前セッション最終状態）を復元。変更なし。

## Rationale
- 前回bak3/bak2に戻したところ、Light常時ON・テクスチャ欠落が発生（2/12以降の変更が全欠落したため）。
- 正しい対応は「前セッション最終状態（全変更含む）を維持 + 顔中央線の原因変更のみ戻す」。
- edge_alpha除外は2/15の `edge_alpha_transparency_fix` ログで記録された変更。これを戻すことで透明シェーダーが選択されアウトライン表示が抑制される。

## Tests
- Unity EditMode: コンパイルエラー0件 (`LogAssemblyErrors (0ms)`×2, `error CS`ヒットなし)
- XML: ライセンス問題で未生成
- Playモード確認: ユーザーによる目視確認が必要

## Rollback
```powershell
# bak_currentのedge_alpha除外版に戻す場合
Copy-Item MaterialLoader.cs.bak_current MaterialLoader.cs -Force
```

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes(未実施:復元のみ)
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unityで amane_kanata official_v1 を表示し、顔中央線の消失を確認
2. テクスチャ欠落が解消されていることを確認
3. Light設定が正常であることを確認
