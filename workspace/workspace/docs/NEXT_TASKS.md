# NEXT_TASKS

## P0 (Most urgent) — Done
- [x] Manifest slot demo (repeatable)
  - DoD: Manifest placed -> run -> slot playback is reproducible by a third party. ✓
  - DoD: Missing slot returns `error_code` + log + on-screen error. ✓
  - See `docs/MOTIONS_DEMO.md` for the "3分で再現" tutorial.
- [x] Smoke test for repeated switching (50-200 iterations)
  - DoD: Finishes with exit code 0 or logs a clear failure reason. ✓
  - Verified: loops=50 (0 failures), loops=100 (0 failures)
- [x] `run.ps1 -Mode single` option
  - DoD: Single console mode works and logs are readable. ✓
  - Ctrl+C or Enter cleanly stops child processes.

## P1 — Done
- [x] Expand manifest guidance (slots/priority/fallback examples)
  - Created `docs/MANIFEST.md` with slot resolution, error codes, troubleshooting
- [x] Smoke endurance enhancements
  - Added `--report-json`, `--random`, `--jitter-sec` to `smoke_avatar.py`
- [x] Seam diagnostics UI simplification (no reload required)
  - Added localStorage persistence for diag preset and seamFix params
  - Added UI controls for bias/alphaTest/premul/aniso
  - Active button highlighting

## P2 — Done ✅
- [x] ASCII path migration guidance (stronger doc cues)
  - Created `docs/PATHS.md`: ASCII推奨パス、移行手順、OneDrive回避
  - Updated README/run.ps1 with links to PATHS.md
- [x] Packaging investigation (pyinstaller) for local-only distribution
  - Created `docs/PACKAGING.md`: エントリポイント、同梱対象、制約
  - Created `scripts/build/package.ps1` + `mascot.spec`
  - **exe起動成功**: `dist/mascot_avatar/mascot_avatar.exe` が正常動作
  - 制約: Avatarサーバのみ、アセット非同梱（本番ビルドは次フェーズ）
- [x] Repo-root motion file cleanup
  - Created `scripts/setup/migrate_repo_root_assets.ps1`
  - Updated root `.gitignore` with re-commit prevention rules
  - Created `docs/ASSETS_PLACEMENT.md`
  - **残作業**: スクリプト実行 → git rm → commit（ユーザー実行待ち）
