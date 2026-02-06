# Worklog: Unity migration review (difficulty, plans, tradeoffs)

- Date: 2026-02-04
- Task: Review Unity migration difficulty, tiered plans, pros/cons, and current issues for MascotDesktop.
- Used-Skills: code-review
- Repo-Refs: workspace/README.md, workspace/docs/00-overview/migrations/unity-migration-spec.md, workspace/docs/05-dev/dev-status.md, workspace/docs/05-dev/run-poc.md, workspace/docs/PACKAGING.md, workspace/docs/VIEWER_SEAMS.md, workspace/docs/03-operations/logging.md, workspace/docs/02-architecture/interfaces/config-schema.md, workspace/spec/latest/spec.md, workspace/viewer/index.html, workspace/viewer/viewer.js, workspace/apps/avatar/poc/poc_avatar_mmd_viewer.py, workspace/apps/shell/poc/poc_shell.py, workspace/apps/core/poc/poc_core_http.py, docs/worklog/2026-02-01_review-unity-migration.md, docs/worklog/2026-02-02_review-unity-migration-followup.md, docs/worklog/2026-01-28_unity-migration-spec.md, memo.text, refs/CocoroAI_3.5.0Beta/CocoroShell/UnityPlayer.dll, refs/CocoroAI_4.7.4Beta/CocoroAI_4.7.4Beta/CocoroShell/UnityPlayer.dll
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260201.md, D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0149.md
- Report-Path: docs/worklog/2026-02-04_unity-migration-review.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md

## Summary
- Reviewed current HTML/JS viewer + Python core/shell architecture and Unity migration spec.
- Identified migration risks (config schema mismatch, MMD seam fix re-implementation, multi-process to single-window shift).
- Produced tiered migration plans and captured pros/cons and current issues.

## Changes
- Added docs/worklog/2026-02-04_unity-migration-review.md.
- Added D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md.

## Commands
- Get-Content .git\HEAD
- Get-Content .git\config
- Get-ChildItem -Force
- Get-Content README.md
- Get-Content workspace\README.md
- Get-Content workspace\docs\00-overview\migrations\unity-migration-spec.md
- Get-Content workspace\docs\05-dev\dev-status.md
- Get-Content workspace\docs\05-dev\run-poc.md
- Get-Content workspace\docs\PACKAGING.md
- Get-Content workspace\docs\VIEWER_SEAMS.md
- Get-Content workspace\docs\03-operations\logging.md
- Get-Content workspace\docs\02-architecture\interfaces\config-schema.md
- Get-Content workspace\spec\latest\spec.md
- Get-Content workspace\viewer\index.html
- Get-Content workspace\viewer\viewer.js
- Get-Content workspace\apps\avatar\poc\poc_avatar_mmd_viewer.py
- Get-Content workspace\apps\shell\poc\poc_shell.py
- Get-Content workspace\apps\core\poc\poc_core_http.py
- Get-Content docs\worklog\2026-02-01_review-unity-migration.md
- Get-Content docs\worklog\2026-02-02_review-unity-migration-followup.md
- Get-Content docs\worklog\2026-01-28_unity-migration-spec.md
- Get-Content memo.text
- Get-ChildItem -Path refs -Recurse -File -Filter UnityPlayer.dll
- Get-ChildItem -Path D:\Obsidian\Programming -Filter *MascotDesktop*
- Get-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0149.md
- Get-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260201.md
- Select-String -Path workspace\docs\02-architecture\interfaces\config-schema.md -Pattern "topmost|pinned"
- Select-String -Path workspace\apps\core\poc\poc_core_http.py -Pattern "pinned|topmost"
- Select-String -Path workspace\apps\shell\poc\poc_shell.py -Pattern "pinned|topmost"
- Select-String -Path workspace\viewer\viewer.js -Pattern "MMDLoader|MMDAnimationHelper|seamFix|alphaBleed|slot"
- Select-String -Path workspace\viewer\index.html -Pattern "unpkg|three"
- Select-String -Path workspace\docs\05-dev\run-poc.md -Pattern "poc_core_http|poc_shell|poc_avatar_mmd_viewer"
- Select-String -Path workspace\docs\05-dev\dev-status.md -Pattern "MMD|Avatar Mode1"
- Select-String -Path workspace\docs\00-overview\migrations\unity-migration-spec.md -Pattern "Goals|Non-Goals|Target Architecture|Phased Plan|Unity"
- Select-String -Path workspace\docs\PACKAGING.md -Pattern "viewer/|apps/|Python"
- Select-String -Path workspace\docs\VIEWER_SEAMS.md -Pattern "seam|Seam"
- Select-String -Path workspace\docs\03-operations\logging.md -Pattern "request_id|JSON Lines|payload"
- Select-String -Path workspace\spec\latest\spec.md -Pattern "Avatar Viewer|別プロセス|Shell|Core"
- Select-String -Path workspace\spec\latest\spec.md -Pattern "ipc-contract"
- Get-Date -Format "yyMMdd_HHmm"

## Tests
- Test-Path docs\worklog\2026-02-04_unity-migration-review.md
- Test-Path D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md

## Rationale (Key Points)
- Unity migration scope in workspace spec is UI + MMD + minimal core; memory/embedding remain external, which shapes the tiered plan.
- Current viewer is a Three.js MMD implementation with seam-fix and slot controls, implying significant re-implementation in Unity.
- Core/Shell/Avatar are separate processes with HTTP/JSON IPC and request_id logging; a single-window Unity target must preserve these contracts or redesign them explicitly.

## Rollback
- Delete docs/worklog/2026-02-04_unity-migration-review.md.
- Delete D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md.

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded: Yes

## Next Actions
- Decide migration scope (viewer-only vs UI+minimal core vs full unity) and update acceptance criteria.
- Align config schema (topmost vs pinned) to avoid Unity IPC breakage.
- Confirm whether HTTP/JSON IPC remains or a new transport is required.
