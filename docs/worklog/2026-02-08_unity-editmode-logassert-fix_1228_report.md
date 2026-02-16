# Report: Unity EditMode AssetPathResolver test stabilization
Date: 2026-02-08 1228

Result:
- Updated `AssetPathResolverTests` to explicitly expect error logs in failure-path tests.
- Added non-ASCII canonical test fixture file so `ResolveRelative_AddsWarningCode_ForNonAsciiSegment` no longer depends on a not-found path.
- Could not execute Unity CLI from this Codex runtime due process start failure (`module not found`) for `Unity.exe`, `Unity.com`, and `cmd.exe`.

Implemented changes:
- Added `LogAssert.Expect` helpers for:
  - `ASSET.PATH.ABSOLUTE_FORBIDDEN`
  - `ASSET.PATH.TRAVERSAL_FORBIDDEN`
  - `ASSET.PATH.LEGACY_FORBIDDEN`
- Added setup fixture file:
  - `characters/ナース/state.png`

Blocked validation:
- Test re-run from this runtime was blocked by host process execution constraints.

Next:
- Run the two EditMode filtered commands from local terminal:
  1. `MascotDesktop.Tests.EditMode.AssetPathResolverTests`
  2. `MascotDesktop.Tests.EditMode.ModelFormatRouterTests`
- Confirm XML results under `Unity_PJ/artifacts/test-results/`.
