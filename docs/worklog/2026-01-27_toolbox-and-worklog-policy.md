# Worklog: Toolbox and Worklog Policy

- Date: 2026-01-27
- Task: Define toolbox usage and worklog fields
- Used-Skills: n/a
- External-Refs: n/a

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
- Start using the template for upcoming tasks; record `Used-Skills` and `External-Refs`.

