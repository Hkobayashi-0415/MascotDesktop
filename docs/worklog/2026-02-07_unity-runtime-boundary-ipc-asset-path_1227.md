# Worklog: unity-runtime-boundary-ipc-asset-path (MascotDesktop)

- Date: 2026-02-07
- Task: Unity runtime境界とIPC/transport方針を確定し、asset path読み取りテスト設計を作成
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs: Unity_PJ/spec/latest/spec.md, Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md, Unity_PJ/docs/05-dev/asset-path-read-test-design.md, Unity_PJ/docs/NEXT_TASKS.md, Unity_PJ/docs/00-overview/documentation-index.md, Unity_PJ/docs/02-architecture/assets/asset-layout.md, docs/02-architecture/interfaces/ipc-contract.md, docs/02-architecture/assets/asset-handling.md, docs/PATHS.md, Unity_PJ/docs/00-overview/migrations/parity-matrix.md, Unity_PJ/docs/05-dev/migration-from-legacy.md, apps/core/poc/poc_core_http.py, apps/shell/poc/poc_shell.py
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-07_unity-runtime-boundary-ipc-asset-path_1227_report.md
- Obsidian-Log: 未実施: 現セッションでは D:\Obsidian\Programming への新規書き込みを実施していないため
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
Unity migration向けに runtime境界と IPC/transport 方針をドキュメントとして凍結し、Unity側 asset path 読み取りテストの設計を追加した。関連タスク状態と参照インデックスも更新した。

## Changes
- `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md` を追加
- `Unity_PJ/docs/05-dev/asset-path-read-test-design.md` を追加
- `Unity_PJ/spec/latest/spec.md` に方針凍結セクションと参照を追加
- `Unity_PJ/docs/NEXT_TASKS.md` のフェーズ進捗を更新
- `Unity_PJ/docs/00-overview/documentation-index.md` に新規ドキュメントを追加
- `docs/worklog/2026-02-07_unity-runtime-boundary-ipc-asset-path_1227_report.md` を追加

## Commands
- Get-Content -Path ".git/HEAD"
- Get-Content -Path ".git/config"
- Get-ChildItem -Path "docs" -Recurse -File | Select-Object FullName
- Get-ChildItem -Path "Unity_PJ" -Recurse -File | Select-Object FullName
- Get-Content -Path "docs/02-architecture/interfaces/ipc-contract.md"
- Get-Content -Path "docs/02-architecture/assets/asset-handling.md"
- Get-Content -Path "docs/VIEWER_SEAMS.md"
- Get-Content -Path "docs/PATHS.md"
- Get-Content -Path "Unity_PJ/spec/latest/spec.md"
- Get-Content -Path "Unity_PJ/docs/NEXT_TASKS.md"
- Get-Content -Path "Unity_PJ/docs/00-overview/documentation-index.md"
- apply_patch: Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md
- apply_patch: Unity_PJ/docs/05-dev/asset-path-read-test-design.md
- apply_patch: Unity_PJ/spec/latest/spec.md
- apply_patch: Unity_PJ/docs/NEXT_TASKS.md
- apply_patch: Unity_PJ/docs/00-overview/documentation-index.md
- Get-Date -Format "yyyy-MM-dd HHmm"; Get-Date -Format "yyMMdd_HHmm"

## Tests
- Test-Path -Path "Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md"
- Test-Path -Path "Unity_PJ/docs/05-dev/asset-path-read-test-design.md"
- Select-String -Path "Unity_PJ/spec/latest/spec.md" -Pattern "runtime-boundary-and-ipc.md"
- Select-String -Path "Unity_PJ/spec/latest/spec.md" -Pattern "asset-path-read-test-design.md"
- Select-String -Path "Unity_PJ/docs/00-overview/documentation-index.md" -Pattern "runtime-boundary-and-ipc.md"
- Select-String -Path "Unity_PJ/docs/00-overview/documentation-index.md" -Pattern "asset-path-read-test-design.md"
- Unity runtime tests: Not Run (design doc only)

## Rationale (Key Points)
- Unity migrationのSoTは `Unity_PJ/spec/latest/spec.md` であり、transport未確定状態を解消する必要があった。
- 既存PoCが loopback HTTP + `request_id` で整合しており、移行期の最小リスク方針として採用可能。
- asset path は cutover安全性に直結するため、実装前に EditMode/PlayMode の分離テスト設計を先行した。

## Rollback
- 以下ファイルを差し戻す:
  - `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md`
  - `Unity_PJ/docs/05-dev/asset-path-read-test-design.md`
  - `Unity_PJ/spec/latest/spec.md`
  - `Unity_PJ/docs/NEXT_TASKS.md`
  - `Unity_PJ/docs/00-overview/documentation-index.md`
  - `docs/worklog/2026-02-07_unity-runtime-boundary-ipc-asset-path_1227_report.md`
  - `docs/worklog/2026-02-07_unity-runtime-boundary-ipc-asset-path_1227.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Unity側に `AssetPathResolver` / `AssetReadService` を実装し、AP-001..AP-106 をテストコード化する。
- Unity Test Runner の batch 実行結果（XML）を `Unity_PJ/artifacts/test-results/` に保存し、cutover gateへ接続する。
