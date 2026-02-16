# Unity Full Migration Reset Plan

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Freeze legacy plan and restart requirement structuring for Unity full migration.

## Decisions
- Decision 1: `Unity_PJ/` is the active implementation root.
- Decision 2: Legacy `Old_PJ/workspace/` is frozen and used for reference/verification only.
- Decision 3: Concept and requirements are inherited, then rewritten under Unity constraints.

## Phases
1. Requirement reset
- Rebuild source-of-truth spec in `Unity_PJ/spec/latest/spec.md`.
- Define migration gates and parity criteria.

2. Data and asset transition
- Prepare `Unity_PJ/data/assets_user/` as canonical local asset root.
- Migrate required MMD/motion data from legacy path.

3. Runtime migration
- Build Unity runtime and remove dependency on legacy runtime.

4. Physical separation
- Legacy environment resides under `Old_PJ/workspace/`.
- Verify no runtime dependency on `Old_PJ/workspace/`.

## Freeze Rules
- No new feature implementation in `Old_PJ/workspace/`.
- Bug fixes in `Old_PJ/workspace/` are allowed only if required for migration validation.
- All new planning/spec decisions are documented under `Unity_PJ/`.

## Exit Criteria
- Unity requirements are approved and baselined.
- Migration checklist is complete.
- Legacy separation dry-run passes.
