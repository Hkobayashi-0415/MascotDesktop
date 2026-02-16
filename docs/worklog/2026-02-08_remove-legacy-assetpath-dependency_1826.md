# Worklog: Remove legacy asset-path runtime dependency
Date: 2026-02-08 1826

Summary:
- Implemented Phase 2 hardening to remove runtime dependency on legacy asset root wiring.
- Preserved path policy by keeping explicit rejection of workspace-like legacy paths.
- Updated EditMode tests to reflect the dependency-free policy input.

Changes:
- `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs`
  - Removed `LegacyAssetsRoot` option property.
  - Generalized legacy-like path detection to `workspace/data/assets_user` pattern.
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - Removed `LegacyAssetsRoot = Path.Combine(repoRoot, "Old_PJ", ...)` assignment.
- `Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs`
  - Removed unused legacy temp-root setup.
  - Updated forbidden-path test input to `workspace/data/assets_user/...`.
  - Removed `LegacyAssetsRoot` option assignment in resolver creation.

Commands:
- Discovery:
  - `rg -n "Old_PJ|workspace/data/assets_user|LegacyAssetsRoot" Unity_PJ/project/Assets -g "*.cs"`
  - `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs`
  - `Get-Content Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Get-Content Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs`
- Validation attempt:
  - `Unity.com -batchmode -nographics -runTests ...` (failed to start in this runtime)
  - `cmd /c Unity.exe -batchmode -nographics -runTests ...` (failed to start in this runtime)
- Static verification:
  - `rg -n "LegacyAssetsRoot" Unity_PJ/project/Assets -g "*.cs"` (no match)

Test Results:
- Unity batch test execution could not run from this Codex runtime due process start error (module not found).
- Static validation passed (no stale symbol reference).
- Local Unity execution is required to complete runtime verification.

Reasoning:
- A runtime option still referenced a legacy folder structure although it was not functionally required.
- Removing the option wiring prevents accidental coupling to `Old_PJ` while preserving explicit path policy checks.

Next Action:
- Run `AssetPathResolverTests` locally in Unity batch or Test Runner.
- If passed, continue with "Execute path policy and fallback validation" in `Unity_PJ/docs/NEXT_TASKS.md`.

Rollback:
- Revert the 3 changed files listed above.

Execution-Tool: PowerShell
Execution-Agent: Codex
Execution-Model: GPT-5
Used-Skills: worklog-update
Repo-Refs:
- Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
- Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs
- Unity_PJ/docs/NEXT_TASKS.md
- docs/worklog/2026-02-08_remove-legacy-assetpath-dependency_1826_report.md
- D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
Obsidian-Refs:
- D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
Report-Path: docs/worklog/2026-02-08_remove-legacy-assetpath-dependency_1826_report.md
Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260208_1826.md
Tags: unity, assets, path-policy, legacy-cutover, runtime, tests, worklog

Record Check:
- Report-Path exists: True
- Repo-Refs recorded: yes
- Obsidian-Refs recorded: yes
- Obsidian-Log recorded: yes
- Execution-Tool/Execution-Agent/Execution-Model recorded: yes
- Tags recorded: yes
