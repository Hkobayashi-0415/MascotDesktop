# MascotDesktop - Unity_PJ

- Status: active
- Owner: Unity migration team
- Last Updated: 2026-02-07
- Scope: Full Unity migration workspace

## Purpose
`Unity_PJ/` is the active development root for full Unity migration.
Legacy implementation remains in `Old_PJ/workspace/` as frozen reference.

## Core Policy
- Full migration target: Unity UI + Avatar + Core integration.
- Inherit concept/spec/requirements from legacy docs, then re-define under `Unity_PJ/`.
- `Old_PJ/workspace/` is read-only reference for migration.

## Directory Layout
- `Unity_PJ/spec/latest/spec.md`: source-of-truth requirements for Unity migration.
- `Unity_PJ/docs/`: migration design and execution docs.
- `Unity_PJ/project/`: Unity project root (Assets/Packages/ProjectSettings).
- `Unity_PJ/data/assets_user/`: local MMD/motion assets (gitignored).

## Legacy
- Legacy reference root: `Old_PJ/workspace/`
- Legacy runbook: `Old_PJ/workspace/docs/05-dev/run-poc.md`
- Legacy spec: `Old_PJ/workspace/spec/latest/spec.md`

## First Steps
1. Finalize requirements in `Unity_PJ/spec/latest/spec.md`.
2. Execute migration checklist in `Unity_PJ/docs/05-dev/migration-from-legacy.md`.
3. Move or mirror required MMD/motion data into `Unity_PJ/data/assets_user/`.


