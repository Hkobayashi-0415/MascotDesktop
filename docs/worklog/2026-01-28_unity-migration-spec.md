# Worklog: Unity migration spec (legacy split)
- Repo-Refs: n/a
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-01-28_unity-migration-spec.md
- Obsidian-Log: 未実施:当時未運用

- Date: 2026-01-28
- Task: Mark legacy spec as reference-only and add new Unity migration spec
- Used-Skills: n/a

## Summary
- Marked `spec/latest/spec.md` as legacy (reference-only) for pre-Unity.
- Added a new Unity migration spec document under `workspace/docs/00-overview/migrations/`.
- Linked the new spec from the documentation index.

## Changes
- Added legacy notice to `workspace/spec/latest/spec.md`.
- Created `workspace/docs/00-overview/migrations/unity-migration-spec.md`.
- Updated `workspace/docs/00-overview/documentation-index.md`.

## Commands
- `Get-Content D:\\dev\\MascotDesktop\\workspace\\docs\\00-overview\\documentation-rules.md`
- `Get-ChildItem D:\\dev\\MascotDesktop\\workspace\\docs -Recurse -File | Select-String -Pattern "Unity|migration|migrate|移行|移植" -SimpleMatch`

## Tests
- Not run (documentation changes only).

## Verification
- Verified files exist and contain expected headings/notes.

## Open Issues
- None.

## Rationale (Key Points)
- Avoid mixing legacy (pre-Unity) spec with new Unity migration direction.
- Provide a dedicated, discoverable spec for the new direction.

## Rollback
- Remove the legacy note from `workspace/spec/latest/spec.md`.
- Delete `workspace/docs/00-overview/migrations/unity-migration-spec.md`.
- Remove the link from `workspace/docs/00-overview/documentation-index.md`.

## Next Actions
- Expand Unity migration spec with detailed acceptance criteria and MMD scope.
- Review findings and refs are logged separately in `docs/worklog/2026-02-01_review-unity-migration.md`.

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
