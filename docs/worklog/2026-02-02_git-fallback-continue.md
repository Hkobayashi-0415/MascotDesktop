# Worklog: Git fallback continue rule
- Obsidian-Log: 未実施:当時未運用

- Date: 2026-02-02
- Task: Clarify git-fallback to continue work instead of stopping
- Used-Skills: worklog-update
- Repo-Refs: docs/worklog/README.md
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-02_git-fallback-continue.md

## Summary
- Updated README to explicitly continue work after .git-based identification when git is unavailable.

## Changes
- Updated `docs/worklog/README.md` to state "do not stop" and continue after `.git/HEAD` + `.git/config`.

## Commands
- `Get-Content D:\\dev\\MascotDesktop\\docs\\worklog\\README.md`

## Tests
- n/a (docs-only change).

## Rationale (Key Points)
- Prevent unnecessary stop when git CLI is unavailable.

## Rollback
- Revert the README wording.

## Record Check
- This worklog includes `Report-Path` pointing to its own existing file.

## Next Actions
- None.

