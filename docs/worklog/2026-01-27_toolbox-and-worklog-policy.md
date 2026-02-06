# Worklog: Toolbox and Worklog Policy
- Repo-Refs: n/a
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-01-27_toolbox-and-worklog-policy.md
- Obsidian-Log: 未実施:当時未運用

- Date: 2026-01-27
- Task: Define toolbox usage and worklog fields
- Used-Skills: n/a

## Summary
- Documented allowed external toolboxes and clarified required worklog fields.

## Changes
- Updated `AGENTS.md` to allow referencing `D:\dev\00_repository_templates` and `D:\Obsidian\Programming` with constraints.
- Added `docs/worklog/` with a README and a worklog template.

## Commands
- `Get-Content .\\AGENTS.md`
- `Get-ChildItem .\\.git -Force`
- `Get-Content .\\.git\\HEAD`
- `Get-Content .\\.git\\config`

## Tests
- n/a (docs-only change)

## Rationale (Key Points)
- Keep product reasoning and minimum reproducibility inside `MascotDesktop` while allowing external references for workflow and learning.

## Rollback
- Revert changes to `AGENTS.md` and delete `docs/worklog/` additions.

## Next Actions
- Start using the template for upcoming tasks; record `Used-Skills` and `Repo-Refs/Obsidian-Refs`.


## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
