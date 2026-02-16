# Unity Runtime Boundary and IPC/Transport Policy

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Freeze runtime boundary and out-of-process transport policy for Unity full migration.

## Decision Summary

| Area | Decision | Freeze Scope |
|---|---|---|
| Runtime boundary | Unity is the primary runtime. UI/window, avatar rendering, and core orchestration run in Unity process. | Phase 1-3 |
| Out-of-process boundary | Cross-process communication is limited to optional local companion services (launcher/tray/tools). | Phase 1-3 |
| Transport | Loopback HTTP (`127.0.0.1`) + JSON DTO is adopted for out-of-process IPC in migration phase. | Phase 1-2 (default), Phase 3 (until re-evaluation) |
| Correlation | `request_id` is mandatory in `X-Request-Id` header and body field. | All phases |
| Named pipe | Not adopted in current migration baseline. Re-evaluate only when explicit security/performance trigger is observed. | Deferred |

## Runtime Boundary (Unity Primary)

### Unity In-Process Responsibilities
- `UI/Window`: frameless window, drag, topmost, resident show/hide/exit flow.
- `Avatar Runtime`: MMD model load, motion slot playback, avatar state transitions.
- `Core Orchestrator`: chat/avatar/state/reminder flow and runtime coordination.
- `Observability`: structured logs, error-code mapping, request correlation propagation.

### External Boundary Responsibilities
- Optional companion process (if needed): tray integration helper, launcher lifecycle, diagnostics/test harness.
- No business logic ownership in companion process; business flow remains Unity-side.

## IPC/Transport Policy (Frozen)

### Transport and Encoding
- Transport: HTTP over loopback only (`http://127.0.0.1:<port>`).
- Payload: JSON DTO with `dto_version`.
- Header/body contract: `X-Request-Id` header equals body `request_id`.

### Endpoint Namespace Policy
- Use service-style endpoint groups compatible with existing draft contract:
  - `/health`
  - `/v1/chat/send`
  - `/v1/config/get`
  - `/v1/config/set`
- Additional endpoints should follow the same versioned namespace rule (`/v1/...`).

### Failure and Retry Rules
- Error payload uses stable fields (`error_code`, `message`, `retryable`, `status`).
- Idempotent config updates should deduplicate by `request_id` when retried.
- Timeouts and transport failures must map to explicit IPC error codes.

### Security and Exposure Rules
- Bind server to loopback only. No LAN/WAN exposure.
- Do not log full payload body; log metadata (keys, sizes, codes, duration).
- Treat transport as local-only integration channel, not remote API surface.

## Why HTTP (Current Phase)

- Existing PoC path already uses loopback HTTP and request correlation:
  - `apps/core/poc/poc_core_http.py`
  - `apps/shell/poc/poc_shell.py`
- Existing draft IPC contract is JSON-first and correlation-aware:
  - `docs/02-architecture/interfaces/ipc-contract.md`
- Unity migration spec requires request correlation, error discipline, and Windows-local operation:
  - `Unity_PJ/spec/latest/spec.md` (`UR-007`, `UR-008`, `UR-012`)

## Re-evaluation Triggers (Named Pipe or Alternate Transport)

Re-open transport decision only if at least one condition is met:
- Loopback HTTP cannot satisfy required latency/reliability for target flow.
- Security review requires stricter endpoint isolation than loopback binding provides.
- Operational evidence shows repeated HTTP-specific instability not fixable in app layer.

When re-evaluated, keep DTO contract and `request_id` propagation unchanged.

## Acceptance Checklist

- [x] Unity in-process vs out-of-process responsibilities are explicitly separated.
- [x] Out-of-process transport is fixed to one baseline for migration execution.
- [x] Correlation/logging/error handling constraints are aligned with spec requirements.
- [x] Transport re-evaluation conditions are explicit and bounded.
