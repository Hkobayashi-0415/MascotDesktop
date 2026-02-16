# Documentation Rules (Unity_PJ)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Document placement and update policy for Unity migration phase.

## Source of Truth
- Product requirements: `Unity_PJ/spec/latest/spec.md`
- Migration planning: `Unity_PJ/docs/00-overview/migrations/`
- Architecture decisions: `Unity_PJ/docs/02-architecture/`
- Execution tasks: `Unity_PJ/docs/NEXT_TASKS.md`

## Rules
- Add all new Unity migration docs under `Unity_PJ/docs/`.
- Do not add new feature specs under legacy `Old_PJ/workspace/docs/`.
- Keep filenames ASCII and kebab-case.
- Update `Unity_PJ/docs/00-overview/documentation-index.md` when adding entrypoint docs.

## Safety
- Do not commit PMX/VMD/texture/audio binaries.
- Keep `Unity_PJ/data/assets_user/` local-only.
- Record every significant migration decision in `docs/worklog/`.
