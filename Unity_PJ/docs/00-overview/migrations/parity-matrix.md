# Parity Matrix (Legacy -> Unity)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Map inherited legacy requirements to Unity implementation requirements.

## Mapping Table

| Legacy Concern | Legacy Source | Unity Requirement | Decision | Note |
|---|---|---|---|---|
| Single-window desktop UX | legacy spec/window docs | UR-001, UR-002 | Inherited | Unity runtime replaces legacy process split |
| Topmost / position persistence | legacy window control docs | UR-002 | Inherited | behavior preserved, implementation changed |
| Resident tray behavior | legacy resident mode docs | UR-003 | Inherited | Unity-native operation path required |
| MMD model rendering | legacy avatar/viewer docs | UR-004 | Inherited | MMD-first remains mandatory |
| Slot-based motion playback | legacy manifest/motion docs | UR-005 | Inherited | fallback rules preserved |
| Avatar state transitions | legacy avatar state docs | UR-006 | Inherited | event bus implementation can differ |
| request_id correlation | legacy logging/IPC docs | UR-007 | Inherited | required for traceability |
| Error code discipline | legacy error code docs | UR-008 | Inherited | catalog can be extended |
| JSONL-style structured logs | legacy logging docs | UR-009 | Inherited | metadata-only sensitive payload policy |
| Asset placement and git policy | legacy asset/path docs | UR-010 | Inherited | canonical path moved to Unity_PJ |
| Physical separation of legacy env | migration reset policy | UR-011 | Added/Required | explicit cutover gate |
| Windows local target | legacy spec overview | UR-012 | Inherited | no cross-platform target in this phase |

## Delta Summary
- Architecture changed: multi-process legacy runtime -> Unity primary runtime.
- Requirement intent preserved for window/avatar/logging/error/asset policy.
- Cutover safety formalized as first-class requirement (UR-011).

## Review Checklist
- [x] Every legacy must concern maps to at least one Unity requirement.
- [x] Implementation deltas are explicitly documented.
- [x] Cutover safety is represented in requirements.
