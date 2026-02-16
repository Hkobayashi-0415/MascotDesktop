# Worklog: 2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317

- Date: 2026-02-11
- Task: SimpleModelBootstrapTests 成功結果と PMX runtime 診断ログ（F0固定）の結果整理
- Execution-Tool: Antigravity
- Execution-Agent: System
- Execution-Model: gemini-2.0-pro-exp-0211
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md, docs/worklog/2026-02-11_pmx-whiteout-texture-diagnostics-fix.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260211_1420.md
- Tags: [agent/system, model/gemini-2.0-pro-exp-0211, tool/antigravity, pmx, simplemodelbootstrap, runtime-validation]

## Summary
`SimpleModelBootstrapTests` は成功（Exit Code 0）し、runtime 側でも `missingMainTexUnknownMats=0` が維持され、unknown ノイズ低減の狙いは達成された。白飛びは一部モデル（特に AZKi / Nurse 系）で継続し、`missingMainTexResolveMats=0` のまま発生しているため、主因は texture resolve ではなく shader/material 経路側に絞られる。

## Source Evidence
- Test success evidence: `artifacts/test-results/editmode-20260211_031241.log`（provided）
- Runtime diagnostics evidence: 2026-02-10 18:24 UTC 〜 2026-02-11 05:18 UTC の `avatar.model.render_diagnostics` / `avatar.model.missing_main_textures` / screenshots（provided）

## Findings
1. Unknown ノイズは解消
- 観測対象モデルで `missingMainTexUnknownMats=0` が維持された。
- `renderers=1` と `renderers>1` の両ケースで unknown は増えていない。

2. Resolve 経路は主因ではない
- 主要観測モデルで `missingMainTexResolveMats=0` が継続。
- 白飛びが残るモデルでも resolve カウントは増えていない。

3. Spec 未指定群が白飛び群と重なる
- Nurse 系（`NurseTaso.pmx`, `NurseTaso_src_mmd/NurseTaso.pmx`）で `missingMainTexSpecMats=7`。
- Momone Nene 系（`桃鈴ねね_BEA.pmx`, `桃鈴ねね_STD.pmx`）で `missingMainTexSpecMats=1`。
- AZKi は `missingMainTex*=0` でも白飛びが目視で発生し、shader/material 側疑いが強い。

4. 非ASCIIパス警告は既知で解決継続
- `assets.path.non_ascii` WARN は出るが、直後に `assets.path.resolved` が成功しており、ロード失敗原因ではない。

## Model Snapshot (from provided logs/screenshots)
- 天音かなた: `missingMainTexMats=1, spec=1, resolve=0, unknown=0`（表示は概ね正常）
- AZKi: `missingMainTexMats=0, spec=0, resolve=0, unknown=0`（白飛びあり）
- 桃鈴ねね_BEA: `missingMainTexMats=1, spec=1, resolve=0, unknown=0`（白飛びあり）
- 桃鈴ねね_STD: `missingMainTexMats=1, spec=1, resolve=0, unknown=0`（白飛びあり）
- NurseTaso: `missingMainTexMats=7, spec=7, resolve=0, unknown=0`（白飛びあり）
- NurseTaso_src: `missingMainTexMats=7, spec=7, resolve=0, unknown=0`（白飛びあり）
- SakamataChloe: `missingMainTexMats=0, spec=0, resolve=0, unknown=0`（表示正常）

## Commands
- (provided) `tools/run_unity_tests.ps1` with filter `SimpleModelBootstrapTests`
- (recorder) `Get-Content docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md`
- (recorder) `Set-Content docs/worklog/2026-02-11_MascotDesktop_SimpleModelBootstrapTests_success_0317.md`

## Tests
- EditMode: `SimpleModelBootstrapTests`
- Result: Pass (Exit Code 0)
- Note: pass結果は提供ログに基づく記録。Codex 環境での再実行ではなく、実行者ログの転記整理。

## Rationale (Key Points)
- `unknown=0` かつ `resolve=0` で白飛び継続モデルが存在するため、分類上は B/C 分岐（Shader/Material or PMX spec）に進むのが妥当。
- Nurse 系は `spec` 高値が安定しており、白飛びと同時発生しているため優先調査対象。
- AZKi は texture欠損なしで白飛びが出るため、shader係数の過剰寄与/トーンマップ寄与の再検証対象。

## Next Actions
1. Shader A/B 比較（現在経路 vs specular抑制強化）を runtime toggle で実施。
2. Nurse 系 2モデルで材質単位に `_Color` / `_SpecularColor` / `_Shininess` / toon 適用状態を採取。
3. AZKi を基準モデルとして `missingMainTex*=0` 条件の白飛び再現手順を固定化。
4. 8モデルの同条件スクリーンショットに model index と request_id を紐付けて再比較。

## Rollback
- 本ファイルは記録更新のみのためコードロールバック不要。
- 実装ロールバックが必要な場合は `docs/worklog/2026-02-11_pmx-whiteout-texture-diagnostics-fix.md` 記載の逆順手順に従う。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
