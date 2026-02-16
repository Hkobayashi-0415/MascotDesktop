# Report: PMX runtime switch for libmmd verification
Date: 2026-02-09 0924

Result:
- Copied a PMX model package into Unity runtime asset root.
- Switched default runtime model path from PNG to PMX.
- Attempted Unity batch startup validation from this environment; process launch failed before Unity startup (module not found).

Changed files:
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/NurseTaso.pmx
- Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/readme.txt
- Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/textures/*

Validation:
- Static checks:
  - PMX exists in canonical runtime root (characters/nurse_taso_v1/mmd/NurseTaso.pmx).
  - Default config path now points to PMX.
- Runtime check in this Codex environment:
  - Unity.exe and Unity.com launch failed with process start error (指定されたモジュールが見つかりません).
  - Therefore runtime log assertion (vatar.model.displayed) could not be verified from this environment.

Recommended local validation command:
$ts = Get-Date -Format 'yyyyMMdd_HHmmss'; ='D:\dev\MascotDesktop\Unity_PJ\artifacts\runtime-check'; New-Item -ItemType Directory -Force -Path  | Out-Null; & "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -batchmode -nographics -projectPath "D:\dev\MascotDesktop\Unity_PJ\project" -logFile "\runtime-.log" -quit
"@;

 = @"
# Worklog: PMX runtime switch for libmmd verification
Date: 2026-02-09 0924

Summary:
- Added PMX runtime asset package under Unity canonical asset root.
- Updated default model path to PMX for immediate runtime boot attempt.
- Runtime process launch verification is blocked in this execution environment.

Changes:
- Runtime default path update:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
    - modelRelativePath changed to characters/nurse_taso_v1/mmd/NurseTaso.pmx.
- Runtime asset package added:
  - Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/NurseTaso.pmx
  - Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/readme.txt
  - Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/textures/*

Commands:
- Get-ChildItem -Recurse -File | Where-Object Name -match '(?i)\\.pmx$'
- Copy-Item "MMD/ナースたそ/ナースたそ/*" -> "Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd"
- pply_patch Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Unity.exe -batchmode ... -logFile ... (failed to launch in this environment)
- Unity.com -batchmode ... -logFile ... (failed to launch in this environment)

Test Results:
- Static: PMX package placement and config switch confirmed.
- Runtime execution from Codex environment: blocked by Unity process start error (module not found).

Reasoning:
- To move from loader-ready state to real PMX path verification, runtime needed a PMX under Unity_PJ/data/assets_user and a default path pointing to it.

Next Action:
- Run the recommended local Unity batch command and confirm vatar.model.displayed for PMX in generated runtime log.
- If display succeeds, proceed to desktop window control integration validation.

Rollback:
- Revert Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs path to PNG.
- Remove Unity_PJ/data/assets_user/characters/nurse_taso_v1/.

Execution-Tool: PowerShell
Execution-Agent: Codex
Execution-Model: GPT-5
Used-Skills: worklog-update
Repo-Refs:
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs
- Unity_PJ/data/assets_user/characters/nurse_taso_v1/mmd/NurseTaso.pmx
- docs/worklog/2026-02-09_pmx-runtime-switch_0924_report.md
- D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
Obsidian-Refs:
- D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
Report-Path: docs/worklog/2026-02-09_pmx-runtime-switch_0924_report.md
Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260209_0924.md
Tags: unity, pmx, libmmd, runtime, bootstrap, worklog

Record Check:
- Report-Path exists: True
- Repo-Refs recorded: yes
- Obsidian-Refs recorded: yes
- Obsidian-Log recorded: yes
- Execution-Tool/Execution-Agent/Execution-Model recorded: yes
- Tags recorded: yes
