# Worklog: pmx-whiteout-handoff-next-session

- Date: 2026-02-11
- Task: PMX白飛び調査の結果整理と次セッション向け引継ぎ作成
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md, docs/worklog/2026-02-11_pmx-whiteout-context-handoff.md, docs/worklog/2026-02-11_pmx-transparency-ratio-heuristic-and-diagnostics.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1748.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, handoff]

## Summary
本セッションの実測により、`transparentMats` を使ってモデルごとの Transparent 材質数を定量把握できるようになった。`missingMainTexResolveMats=0` が継続する一方、白飛びモデルで Transparent 材質が多い傾向は確認されたが、正常寄りモデルでも Transparent 材質が多いケースがあり、単一要因では説明しきれない。

## Changes
- コード変更は本記録作業では実施なし（直前セッションで実施済みの結果を整理）。
- 作成ファイル:
  - `docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md`
- 参照した最新実装:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`（strong透明判定を比率化済み）
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`（`transparentMats` 出力追加済み）

## Results (Current Facts)
- 共通:
  - `missingMainTexResolveMats=0` / `missingMainTexUnknownMats=0` が継続
  - `assets.path.non_ascii` は `assets.path.resolved` 後に解決し、ロード阻害の主因ではない
- モデル別（F0/F3ログからの代表値）:
  - `amane_kanata_v1`: `materials=16, transparentMats=13, missingMainTexMats=1(spec=1)`
  - `AZKi_4th`: `materials=33, transparentMats=16, missingMainTexMats=0`
  - `momone_nene_BEA`: `materials=19, transparentMats=17, missingMainTexMats=1(spec=1)`
  - `momone_nene_STD`: `materials=18, transparentMats=16, missingMainTexMats=1(spec=1)`
  - `NurseTaso`: `materials=15, transparentMats=7, missingMainTexMats=7(spec=7)`
  - `NurseTaso_src`: `materials=15, transparentMats=7, missingMainTexMats=7(spec=7)`
  - `SakamataChloe`: `materials=36, transparentMats=17, missingMainTexMats=0`
- レンダーファクタ切替の観測:
  - `F3 Lower Light + Albedo88` で `brightDiffuseMats` は低下（0化するモデルあり）
  - ただし白飛び症状が残るケースがあり、明るさ係数だけでは説明不十分

## Interpretation
- 高優先の示唆:
  - 白飛びは「テクスチャ解決失敗」起因より「材質経路（Transparent化 + toon/sphere欠損 + モデル固有材質）」寄り。
- 未確定点:
  - `transparentMats` の絶対数だけでは白飛びを一意に説明できない（Chloe のような反例あり）。
- 評価上の注意:
  - 比較時に `Candidate Mode: Image` が混在したログがあるため、PMX比較では `Candidate Mode: Model` 固定が必須。

## Shared Info For Next Session
- 固定条件:
  - `Candidate Mode = Model`
  - `Render Factor = F0 Baseline`（補助でF3比較）
  - 同一カメラ/同一ポーズ
- 必須採取イベント:
  - `avatar.model.render_diagnostics`
  - `avatar.model.material_diagnostics`（`transparentMats` 含む）
  - `avatar.model.missing_main_textures`
  - `avatar.render.environment_diagnostics`
- 判定基準（最小）:
  - 1) 白飛びの有無（Gameスクショ）
  - 2) `transparentMats/materials` 比率
  - 3) `missingMainTexSpecMats` と `toon/sphere missing` の組み合わせ
- 次に実装して有効な診断:
  - Transparent化理由タグ（`diffuse_alpha` / `edge_alpha` / `texture_alpha`）
  - `material_diagnostics` に理由別カウント追加

## Commands
- `Get-Date -Format "yyMMdd_HHmm"`
- `Get-Date -Format "yyyy-MM-dd HH:mm:ss"`
- `Set-Content docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1748.md`
- `Test-Path` / `rg`（必須項目チェック）

## Tests
- Code test: 未実施（本作業は引継ぎ文書作成のみ）
- Record checks:
  - `Report-Path` 実在
  - `Obsidian-Log` 実在
  - 必須項目（Execution fields / Refs / Record Check / Next Actions）存在確認

## Rationale (Key Points)
- 次セッションでの再現・比較コストを最小化するため、ログ事実と評価軸を分離して固定化。
- 直近の主目的は「原因候補の絞り込み」であり、今回時点で確定できるのは相関まで。
- 不確定要素（候補モード混在、ファクタ混在）を明示し、再観測手順にガードを設けた。

## Rollback
1. `docs/worklog/2026-02-11_pmx-whiteout-handoff-next-session.md` を削除
2. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1748.md` を削除

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. `Candidate Mode: Model` 固定で 8モデルを再採取し、`transparentMats/materials` 比率を比較表化する。
2. 白飛び有無との対応をモデル単位で判定し、反例（Chloe）との差分を材質サンプルで抽出する。
3. Transparent化理由タグと理由別カウントを実装し、次段の原因切り分けを進める。
4. 必要に応じて `SemiTransparentPixelRatioThreshold` のA/B調整を行い、前後比較を記録する。
