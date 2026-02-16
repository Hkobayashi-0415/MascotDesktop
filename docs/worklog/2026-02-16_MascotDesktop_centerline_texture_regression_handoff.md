# Worklog: MascotDesktop_centerline_texture_regression_handoff

- Date: 2026-02-16
- Task: 次セッション向け引継ぎ資料の作成（中央線/欠損回帰調査の現状固定）
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md, docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md, docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md, docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md, docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md, docs/worklog/2026-02-15_MascotDesktop_revert_opaque_missing_spec_white_fallback.md, docs/worklog/2026-02-15_MascotDesktop_character_variant_mmd_pkg_migration.md, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs.bak_current, Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialOutline.shader
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_handoff.md
- Obsidian-Log: 未実施: 引継ぎ資料作成のみ
- Tags: [agent/codex, model/gpt-5, tool/codex, handoff, centerline, texture]

## Purpose
- 次セッションで、状態を増やさずに「復元済みチェックポイント」から調査を再開できるようにする。
- 追加の提案プロンプトはこのドキュメントに含めない（運用指示に従う）。

## Current Fixed Checkpoint
- 基準: `docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md:210`
- `MaterialLoader.cs` は `MaterialLoader.cs.bak_current` と一致状態へ復元済み（前セッションで確認）。
- `TextureLoader.cs` は `Texture ret = null` を維持し、2/16追加の sibling PNG fallback helper は含まない。
- outline 8 shader は CRLF 正規化済み。

## Latest Known Runtime Symptoms
1. 欠損・中央線継続時ログ（ユーザー報告）
- `avatar.model.material_diagnostics`:
  - `transparentMats=13`
  - `transparentByEdgeAlphaMats=12`
  - `toonMissingMats=0`
  - `toonStatus=loaded_fallback_white`
- 観測: 線あり、欠損あり

2. 以前の欠損再発ログ（参考）
- `toonMissingMats=16` / `toonMissingSpecMats=16` のケースも観測済み。
- 状態遷移があるため、同一条件での再採取が必須。

## Required Reading Order (Next Session)
1. `AGENTS.md`
2. `docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_fix.md`
3. `docs/worklog/2026-02-16_MascotDesktop_warning_cleanup_findobject_rsp_lineendings.md`
4. `docs/worklog/2026-02-16_MascotDesktop_fix_review_findings_1_to_4.md`
5. `docs/worklog/2026-02-15_MascotDesktop_centerline_review_and_repo_audit.md`
6. `docs/worklog/2026-02-15_MascotDesktop_shadow_policy_for_fallback_toon_models.md`
7. `docs/worklog/2026-02-15_MascotDesktop_revert_opaque_missing_spec_white_fallback.md`
8. `docs/worklog/2026-02-15_MascotDesktop_character_variant_mmd_pkg_migration.md`

## Execution Guardrails For Next Session
- `AGENTS.md` の `Plan -> Approval -> Execute` を厳守する。
- 承認前にコマンド実行・ファイル編集を行わない。
- 変更は1要因ずつ、最小差分で行い、毎回ロールバック可能にする。
- 根拠はこのリポジトリ内に限定する。

## Recommended Next Actions
1. チェックポイント状態のまま実機で再表示し、`avatar.model.material_diagnostics` を再採取する。
2. 同一モデル/同一条件で、前回ログとの差分（`transparentMats`, `transparentByEdgeAlphaMats`, `toonMissing*`, `samples`）を比較する。
3. 差分が取れた後、`MaterialLoader` / outline shader / shadow policy の順で影響範囲を狭く切り分ける。

## Rollback
1. 本引継ぎDocを削除:
- `Remove-Item docs/worklog/2026-02-16_MascotDesktop_centerline_texture_regression_handoff.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
