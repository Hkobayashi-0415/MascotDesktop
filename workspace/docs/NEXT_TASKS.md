# NEXT_TASKS

## P0 (Most urgent)
- [ ] Manifest slot demo (repeatable)
  - DoD: Manifest placed -> run -> slot playback is reproducible by a third party.
  - DoD: Missing slot returns `error_code` + log + on-screen error.
- [ ] Smoke test for repeated switching (50-200 iterations)
  - DoD: Finishes with exit code 0 or logs a clear failure reason.
- [ ] `run.ps1 -Mode single` option
  - DoD: Single console mode works and logs are readable.

## P1
- [ ] Expand manifest guidance (slots/priority/fallback examples)
- [ ] Seam diagnostics UI simplification (no reload required)

## P2
- [ ] ASCII path migration guidance (stronger doc cues)
- [ ] Packaging investigation (pyinstaller) for local-only distribution
