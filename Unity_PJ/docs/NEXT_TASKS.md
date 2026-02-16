# NEXT_TASKS (Unity_PJ)

## Phase 0: Requirement Reset (Done)
- [x] Create Unity_PJ root and document baseline.
- [x] Freeze legacy workspace as reference-only.
- [x] Finalize requirement parity matrix (legacy -> Unity).

## Phase 1: Architecture Freeze
- [x] Define Unity runtime boundaries (UI, avatar, core services).
- [x] Define transport and correlation-id strategy.
- [x] Finalize error-code and logging schema.
- [x] Add runtime logging baseline (request_id/error_code/path/source_tier).

## Phase 2: Asset Cutover
- [x] Copy legacy MMD/motion assets to `Unity_PJ/data/assets_user`.
- [x] Design Unity-side asset path read tests.
- [x] Implement Unity-side asset path read tests (EditMode: AP-001..AP-006 baseline).
- [x] Execute path policy and fallback validation.
- [x] Remove runtime dependency on legacy asset path.

## Phase 3: Full Cutover
- [x] Implement Unity-first runtime for core features.
- [x] Add minimal model display bootstrap (image plane + placeholder fallback).
- [x] Add PMX/VRM loader bridge (reflection-based; uses installed loader packages when available).
- [x] Verify parity against inherited must requirements.
- [x] Physically separate legacy workspace into `Old_PJ/workspace/`.

## Automation / CLI Enablement
- [x] Add Codex WSL execution hint for stable approvals (`.vscode/settings.json`).
- [x] Add Unity CLI wrapper (`tools/run_unity.ps1`).
- [x] Add Unity test wrapper (`tools/run_unity_tests.ps1`).
- [x] Add Unity Editor automation entrypoints (`Assets/Editor/Automation.cs`).
- [x] Run local Unity batch with `-executeMethod MascotDesktop.Editor.Automation.Ping` and confirm log output.
