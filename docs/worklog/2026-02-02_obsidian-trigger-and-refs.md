# Worklog: Obsidian trigger and reference clarity
- Obsidian-Log: 未実施:当時未運用

- Date: 2026-02-02
- Task: Make Obsidian logging triggers and reference separation explicit
- Used-Skills: worklog-update
- Repo-Refs: docs/worklog/README.md, docs/worklog/_template.md
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-02_obsidian-trigger-and-refs.md

## Summary
- Added explicit Obsidian logging triggers and separated Repo-Refs/Obsidian-Refs in the template.

## Changes
- Updated `docs/worklog/README.md` with Obsidian logging triggers and exception handling.
- Updated `docs/worklog/_template.md` to split `Repo-Refs` and `Obsidian-Refs`.
- Updated existing worklogs to follow the new reference format.

## Commands
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\README.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\_template.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\2026-02-01_review-unity-migration.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\2026-02-01_record-verification-and-git-fallback.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\2026-02-02_report-path-backfill.md`
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\2026-02-02_report-path-self-test.md`

## Tests
- n/a (docs-only change).

## Rationale (Key Points)
- Stabilize Obsidian logging and make external references auditable.

## Rollback
- Revert changes in `docs/worklog/README.md` and `docs/worklog/_template.md`.
- Revert reference field edits in updated worklogs.
- Delete `docs/worklog/2026-02-02_obsidian-trigger-and-refs.md`.

## Record Check
- This worklog includes `Report-Path` pointing to its own existing file.

## Next Actions
- None.

