# Worklog: Unity_PJ cutover and physical separation

- Date: 2026-02-07
- Task: Finalize Unity spec + parity matrix, physically separate legacy workspace
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: phase-planning, worklog-update
- Repo-Refs: Unity_PJ/spec/latest/spec.md; Unity_PJ/docs/00-overview/migrations/parity-matrix.md; Unity_PJ/docs/00-overview/migrations/unity-full-migration-reset.md; Unity_PJ/docs/00-overview/documentation-rules.md; Unity_PJ/docs/00-overview/documentation-index.md; Unity_PJ/docs/05-dev/migration-from-legacy.md; Unity_PJ/docs/02-architecture/assets/asset-layout.md; Unity_PJ/docs/NEXT_TASKS.md; README.md; Old_PJ/workspace
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-07_unity-pj-cutover.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260207_0213.md
- Tags: [agent/codex, model/gpt-5, tool/codex, unity, migration, cutover]

## Summary
- Finalized Unity full-migration spec and parity matrix.
- Physically separated legacy workspace to `Old_PJ/workspace` and removed root `workspace` directory.
- Updated Unity_PJ docs and root README to reflect new legacy location.

## Changes
- Updated `Unity_PJ/spec/latest/spec.md`
- Added `Unity_PJ/docs/00-overview/migrations/parity-matrix.md`
- Updated `Unity_PJ/docs/00-overview/migrations/unity-full-migration-reset.md`
- Updated `Unity_PJ/docs/00-overview/documentation-rules.md`
- Updated `Unity_PJ/docs/00-overview/documentation-index.md`
- Updated `Unity_PJ/docs/05-dev/migration-from-legacy.md`
- Updated `Unity_PJ/docs/02-architecture/assets/asset-layout.md`
- Updated `Unity_PJ/docs/NEXT_TASKS.md`
- Updated `README.md`
- Moved legacy `workspace` to `Old_PJ/workspace` and removed root `workspace`

## Commands
- `Get-Date -Format "yyMMdd_HHmm"`
- `Test-Path "workspace"; Test-Path "Old_PJ/workspace"; Test-Path "Unity_PJ/spec/latest/spec.md"`
- `Get-Content README.md -TotalCount 20`
- `Get-Content Unity_PJ/README.md -TotalCount 40`
- `Get-ChildItem -Path "Unity_PJ" -Recurse -File | Select-String -Pattern "workspace" | Where-Object { $_.Line -notmatch "Old_PJ/workspace" } | Select-Object Path,LineNumber,Line`
- `Get-ChildItem -Path "workspace" -Recurse -Force | ForEach-Object { $_.Attributes = 'Normal' }`
- `Remove-Item -Path "workspace" -Recurse -Force`
- `Set-Content ...` (Unity_PJ docs and README updates)

## Tests
- `Test-Path "workspace"` => False
- `Test-Path "Old_PJ/workspace"` => True
- `Test-Path "Unity_PJ/spec/latest/spec.md"` => True
- Verified Unity_PJ docs only reference legacy under `Old_PJ/workspace`.

## Rationale (Key Points)
- Physical separation prevents accidental coupling with legacy runtime.
- Old_PJ path normalization makes all migration references explicit.
- Parity matrix anchors inherited requirements to Unity spec.

## Rollback
- Restore `Old_PJ/workspace` back to root `workspace` if separation must be reversed.
- Revert Unity_PJ docs and README references to old paths.

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Define Unity runtime boundaries and IPC transport strategy.
- Validate asset path policy and fallback behavior in Unity runtime.
