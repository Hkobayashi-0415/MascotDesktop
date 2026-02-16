# Worklog: Render Investigation (Not Brightness-Only)

- Date: 2026-02-09
- Task: 明度以外の要因のレビューと課題調査
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Report-Path: docs/reports/2026-02-09_render-investigation-not-brightness-only.md
- Repo-Refs:
  - Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/ProjectSettings/ProjectSettings.asset
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本リポジトリ内ログを優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, review/render, unity/runtime]

## Summary
- 課題は明度だけではなく、シェーダー計算順序・スフィアUV計算・透過判定・色空間の複合要因。

## Commands
- `Get-Content` / `Select-String` で関連箇所を確認。

## Tests
- Unity実行テストは未実施（ユーザー担当）。
- 静的レビューのみ実施。

## Rationale
- ログ上は `avatar.model.displayed` でロード成功しており、問題は描画パイプライン側。

## Next Actions
1. シェーダーの早期 saturate 除去。
2. sphere UV の法線ソース再設計。
3. 透過判定閾値見直し。

## Rollback
- 本調査はコード変更なし（ドキュメント追加のみ）。

## Record Check
- Report-Path exists: True
- Repo-Refs recorded: Yes
- Obsidian-Refs recorded (or n/a): Yes (n/a)
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags recorded: Yes
