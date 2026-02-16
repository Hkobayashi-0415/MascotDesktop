# Worklog: Unity EditMode log assertion fix
Date: 2026-02-08 1228

Summary:
- Fixed EditMode test design mismatch where expected failure paths emitted `RuntimeLog.Error` and caused unhandled log test failures.
- Added explicit log expectations in tests and prepared non-ASCII fixture input.
- Validation re-run was attempted but blocked by runtime process launch errors in this Codex environment.

Changes:
- Updated `Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs`
  - Added `LogAssert.Expect` for failure-path test cases.
  - Added fixture file creation for non-ASCII path case.
  - Added helper method `ExpectErrorCode`.

Commands:
- Get-Content `Unity_PJ\project\Assets\Tests\EditMode\AssetPathResolverTests.cs`
- Get-Content `Unity_PJ\project\Assets\Scripts\Runtime\Assets\AssetPathResolver.cs`
- Get-Content `Unity_PJ\project\Assets\Scripts\Runtime\Diagnostics\RuntimeLog.cs`
- `Unity.exe ... -runTests ...` (failed to launch: module not found)
- `Unity.com ... -runTests ...` (failed to launch: module not found)
- `cmd.exe /c "Unity.com ..."` (failed to launch: module not found)

Tests:
- Not executed successfully from this runtime due host process start failures.
- Target validation commands prepared for local terminal execution:
  1. `Unity.com ... -testFilter "MascotDesktop.Tests.EditMode.AssetPathResolverTests"`
  2. `Unity.com ... -testFilter "MascotDesktop.Tests.EditMode.ModelFormatRouterTests"`

Reasoning:
- Runtime behavior intentionally logs error JSON for invalid paths.
- Tests that assert failure outcomes must also assert expected error logs, otherwise Unity Test Runner reports unhandled errors.

Next Action:
- Execute the two filtered EditMode test commands from local PowerShell.
- Verify `Unity_PJ\artifacts\test-results\editmode-assetpath.xml` and `Unity_PJ\artifacts\test-results\editmode-modelrouter.xml`.

Rollback:
- Revert `Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs` to previous content.

Execution-Tool: PowerShell
Execution-Agent: Codex
Execution-Model: GPT-5
Used-Skills: bug-investigation, worklog-update
Repo-Refs:
- Unity_PJ\project\Assets\Tests\EditMode\AssetPathResolverTests.cs
- Unity_PJ\project\Assets\Scripts\Runtime\Assets\AssetPathResolver.cs
- Unity_PJ\project\Assets\Scripts\Runtime\Diagnostics\RuntimeLog.cs
- docs\worklog\2026-02-08_unity-editmode-logassert-fix_1228_report.md
- D:\dev\00_repository_templates\ai_playbook\skills\bug-investigation\SKILL.md
- D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md
Obsidian-Refs:
- D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md
Report-Path: docs/worklog/2026-02-08_unity-editmode-logassert-fix_1228_report.md
Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260208_1228.md
Tags: unity, tests, editmode, nunit, worklog

Record Check:
- Report-Path exists: True
- Obsidian-Log exists: True
- Repo-Refs recorded: yes
- Obsidian-Refs recorded: yes
- Obsidian-Log recorded: yes
- Execution-Tool/Agent/Model recorded: yes
- Tags recorded: yes
