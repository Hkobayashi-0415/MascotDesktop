# Worklog: Rule priority fix (code-review, refs, git fallback)
- Obsidian-Log: 未実施:当時未運用

- Date: 2026-02-02
- Task: Fix rule priority for review skill, reference fields, and git fallback
- Used-Skills: worklog-update
- Repo-Refs: docs/worklog/README.md, docs/worklog/_template.md, docs/worklog/2026-02-02_review-unity-migration-followup.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260202.md
- Report-Path: docs/worklog/2026-02-02_rule-priority-fix.md

## Summary
- Clarified that reviews must use `code-review`, removed `Repo-Refs/Obsidian-Refs`, and enforced repo/Obsidian ref separation.

## Changes
- Updated `docs/worklog/README.md` to require `Repo-Refs`/`Obsidian-Refs`, remove `Repo-Refs/Obsidian-Refs`, and mandate `code-review` for reviews.
- Updated `docs/worklog/2026-02-02_review-unity-migration-followup.md` to remove `Repo-Refs/Obsidian-Refs` and align record check.
- Updated `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260202.md` notes.

## Commands
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\README.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\_template.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\2026-02-02_review-unity-migration-followup.md`
- `Get-Content D:\\Obsidian\\Programming\\MascotDesktop_phaseNA_log_260202.md`

## Tests
- n/a (docs-only change).

## Rationale (Key Points)
- Prevents drift between declared rules and logged fields.

## Rollback
- Revert changes to README and updated logs.

## Record Check
- This worklog includes `Report-Path` pointing to its own existing file.

## Next Actions
- None.

