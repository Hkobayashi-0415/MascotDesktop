# Migration From Legacy Workspace

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Practical migration checklist from legacy workspace to Unity_PJ.

## Preconditions
- Legacy source exists: `Old_PJ/workspace/`
- New target exists: `Unity_PJ/`
- Asset target exists: `Unity_PJ/data/assets_user/`

## Step 1: Requirement Baseline
- Confirm `Unity_PJ/spec/latest/spec.md` is approved.
- Freeze legacy feature development.

## Step 2: Asset Migration (Recommended)
Preferred (if `robocopy` is available):
```powershell
robocopy "Old_PJ\\workspace\\data\\assets_user" "Unity_PJ\\data\\assets_user" /E /XO /R:1 /W:1
```

Fallback (PowerShell only):
```powershell
Copy-Item -Path "Old_PJ\\workspace\\data\\assets_user\\*" -Destination "Unity_PJ\\data\\assets_user" -Recurse -Force
```

## Step 3: Verification
```powershell
Get-ChildItem "Old_PJ\\workspace\\data\\assets_user" -Recurse -File | Measure-Object
Get-ChildItem "Unity_PJ\\data\\assets_user" -Recurse -File | Measure-Object
```

## Step 4: Runtime Cutover Gate
- Unity runtime path config points only to `Unity_PJ/data/assets_user/`.
- No runtime dependency on `Old_PJ/workspace/`.

## Step 5: Physical Separation
- Legacy environment should live under `Old_PJ/workspace/`.
- If any legacy root remains at repository top-level, move it into `Old_PJ/`.
