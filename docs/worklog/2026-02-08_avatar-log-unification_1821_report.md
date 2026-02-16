# Report: Avatar runtime log unification
Date: 2026-02-08 1821

Result:
- Unified failure-path logging in `SimpleModelBootstrap` so runtime triage consistently includes `request_id`, `error_code`, `path`, and `source_tier`.
- Added explicit bootstrap failure log when project roots cannot be resolved.
- Updated placeholder fallback log to carry the attempted asset path and source tier.

Changed file:
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`

Validation:
- Static code review confirmed all modified failure/fallback branches now pass path/source tier values.
- Unity batch test execution from this Codex runtime failed to start (`Unity.com`: module not found), so runtime verification must be done locally.

Recommended local check:
- Launch play mode and confirm logs include:
  - `avatar.model.resolve_failed`
  - `avatar.model.fallback_used`
  - `avatar.model.placeholder_displayed`
  with non-empty `error_code`, `path`, and `source_tier`.
