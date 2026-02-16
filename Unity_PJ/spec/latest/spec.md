# Unity Full Migration Specification

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Source of truth for full Unity migration implementation.

## 1. Objective
Deliver a Windows-local mascot desktop application with Unity as the primary runtime.
Legacy Python/HTML implementation is frozen and used only for parity reference.

## 2. Legacy Inheritance Policy
- Inherit concept, behavior, and requirement intent from legacy docs.
- Re-define execution architecture, runtime boundaries, and packaging for Unity.
- Legacy root path `Old_PJ/workspace/` is reference-only.

## 3. In Scope
- Unity UI/window runtime.
- Unity avatar renderer (MMD-first).
- Unity-side core orchestration for chat/avatar/state/reminder flow.
- Logging, error code, and correlation-id discipline.
- Local asset management under `Unity_PJ/data/assets_user/`.

## 4. Out of Scope
- New feature development on legacy runtime.
- Binary asset commit policy changes.
- External cloud operations beyond local runtime requirements.

## 5. Functional Requirements (Must)

### UR-001 Single Runtime
System shall run primary UX in Unity without dependency on legacy process startup.

### UR-002 Window Control
System shall support frameless window, drag move, position/size restore, topmost toggle.

### UR-003 Resident Operation
System shall support resident operation with hide/show/exit control path.

### UR-004 Avatar MMD Load
System shall load MMD model assets and resolve required textures from local paths.

### UR-005 Motion Slot Playback
System shall play motion by slot and apply fallback rules when slot is missing.

### UR-006 State Event Handling
System shall apply avatar state transitions from runtime events.

### UR-007 Request Correlation
System shall propagate `request_id` across runtime actions and logs.

### UR-008 Error Contract
System shall emit explicit error codes and stable failure metadata.

### UR-009 Logging
System shall output structured logs with metadata-only payload handling.

### UR-010 Asset Policy
System shall resolve assets from project-relative paths and avoid binary commits.

### UR-011 Legacy Cutover Safety
System shall allow physical separation of legacy environment without runtime break.

### UR-012 Windows Local Constraint
System shall operate on Windows local environment as the target platform.

## 6. Non-Functional Requirements (Must)
- Startup flow is reproducible with documented steps.
- Troubleshooting paths are documented for runtime and asset errors.
- Runtime avoids hard dependency on non-ASCII path behavior.

## 7. Data and Asset Requirements
- Canonical path: `Unity_PJ/data/assets_user/characters/<slug>/...`
- During transition only, temporary validation can reference `Old_PJ/workspace/data/assets_user/`.
- Final cutover forbids runtime read from legacy asset path.

## 8. Acceptance Gates

### Gate A: Requirement Baseline
- This spec is approved.
- Parity matrix is created and reviewed.

### Gate B: Runtime Path Independence
- Runtime config and code reference only Unity_PJ paths for assets.

### Gate C: Physical Separation Readiness
- Legacy directory move does not invalidate Unity docs/runtime assumptions.

## 9. Cutover Definition
Cutover is complete when all conditions are true:
1. Unity runtime has no required read path under `Old_PJ/workspace/`.
2. Required MMD/motion assets are available under Unity_PJ asset root.
3. Legacy environment can be moved without breaking active development flow.

## 10. Traceability References
- `Unity_PJ/docs/00-overview/migrations/parity-matrix.md`
- `Unity_PJ/docs/05-dev/migration-from-legacy.md`
- `Unity_PJ/docs/02-architecture/assets/asset-layout.md`
- `Unity_PJ/docs/02-architecture/runtime-boundary-and-ipc.md`
- `Unity_PJ/docs/05-dev/asset-path-read-test-design.md`

## 11. Runtime Boundary and Transport Freeze (2026-02-07)
- Out-of-process IPC transport is fixed to loopback HTTP (`127.0.0.1`) + JSON DTO for migration baseline.
- `request_id` correlation is mandatory in both HTTP header (`X-Request-Id`) and DTO body.
- Unity runtime owns primary UX/business flow; external companion process is optional and integration-only.
- Named pipe is deferred unless explicit re-evaluation triggers are met (security/performance/operational evidence).
