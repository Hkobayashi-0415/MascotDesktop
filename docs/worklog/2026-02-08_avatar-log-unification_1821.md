# Worklog: Avatar log unification
Date: 2026-02-08 1821

Summary:
- Standardized avatar runtime failure logging so diagnostics always include `error_code`, `request_id`, `path`, and `source_tier`.
- Added bootstrap-stage error logging before fallback primitive creation.
- Propagated resolved/attempted asset path into fallback placeholder logs.

Changes:
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - Added explicit `avatar.model.bootstrap_failed` error event.
  - Normalized resolve/load fallback error code handling.
  - Extended `CreateFallbackPrimitive` to receive `path/source_tier/error_code`.
  - Added `SafeSourceTier(...)` helper.

Commands:
- Read:
  - `Get-Content Unity_PJ\\project\\Assets\\Scripts\\Runtime\\Avatar\\SimpleModelBootstrap.cs`
  - `Get-Content Unity_PJ\\project\\Assets\\Scripts\\Runtime\\Diagnostics\\RuntimeLog.cs`
- Test (attempted):
  - `Unity.com -batchmode -runTests ...` (failed to start in this runtime: module not found)

Tests:
- Unity batch execution could not run from this Codex runtime due host process start error.
- Local Unity verification required.

Reasoning:
- Existing logs were partially missing path/source context in fallback and bootstrap-failure scenarios.
- Structured log parity on all failure paths is required for reliable root cause tracing.

Next Action:
- Local run with both success and forced-failure paths to verify event payload completeness.
- Then proceed to IPC/transport contract documentation.

Rollback:
- Revert changes in `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`.

Execution-Tool: PowerShell
Execution-Agent: Codex
Execution-Model: GPT-5
Used-Skills: worklog-update
Repo-Refs:
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs
- docs/worklog/2026-02-08_avatar-log-unification_1821_report.md
- D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
Obsidian-Refs:
- D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
Report-Path: docs/worklog/2026-02-08_avatar-log-unification_1821_report.md
Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260208_1821.md
Tags: unity, runtime, logging, diagnostics, avatar, worklog

Record Check:
- Report-Path exists: True
- Repo-Refs recorded: yes
- Obsidian-Refs recorded: yes
- Obsidian-Log recorded: yes
- Execution-Tool/Execution-Agent/Execution-Model recorded: yes
- Tags recorded: yes
