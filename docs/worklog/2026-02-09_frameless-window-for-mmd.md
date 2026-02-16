# Worklog: frameless-window-for-mmd

- Date: 2026-02-09
- Task: Implement frameless window support for desktop MMD display readiness
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: n/a
- Repo-Refs: Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs, Unity_PJ/docs/05-dev/phase3-parity-verification.md, Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-09_frameless-window-for-mmd.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260209_1328.md
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
Added Windows frameless window style application at startup and updated UR-002 verification evidence. Default PMX path already points to 天音かなた.pmx.

## Changes
- Updated `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- Updated `Unity_PJ/docs/05-dev/phase3-parity-verification.md`

## Commands
- `Get-ChildItem Unity_PJ/data/assets_user -Recurse -Filter *.pmx`
- `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- `Get-Content Unity_PJ/docs/05-dev/phase3-parity-verification.md`

## Tests
- Not run here (Unity.exe fails to start in this environment). Suggested:
  - `tools/run_unity_tests.ps1 -EditMode`
  - Manual runtime check for frameless window + PMX load

## Rationale (Key Points)
- UR-002 requires frameless window; implementation added using Win32 window styles
- Default PMX path already configured to `characters/amane_kanata_v1/mmd/天音かなた.pmx`

## Rollback
- Revert changes in `Unity_PJ/project/Assets/Scripts/Runtime/Windowing/WindowController.cs`
- Revert changes in `Unity_PJ/docs/05-dev/phase3-parity-verification.md`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Run Unity EditMode tests and runtime manual check on a machine where Unity.exe launches
