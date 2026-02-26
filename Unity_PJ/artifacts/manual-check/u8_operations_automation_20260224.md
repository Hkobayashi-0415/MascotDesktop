# U8 Operations Automation Execution (2026-02-24)

## Scope
- Check 1: `ui.hud.bootstrap_missing` 連続監視
- Check 2: Unity導線/legacy文書 同期チェック

## Commands
```powershell
./tools/check_runtime_bootstrap_missing.ps1 `
  -LogDir "Unity_PJ/artifacts/manual-check/u8-runtime-log-samples" `
  -LogPattern "runtime-20260224-pass.jsonl" `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_pass.json"

./tools/check_runtime_bootstrap_missing.ps1 `
  -LogDir "Unity_PJ/artifacts/manual-check/u8-runtime-log-samples" `
  -LogPattern "runtime-20260224-fail.jsonl" `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_fail.json"

./tools/check_unity_legacy_docs_sync.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_pass.json"

./tools/check_unity_legacy_docs_sync.ps1 `
  -RequireSameDay `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_fail.json"
```

## Results
| Check | Result | Notes |
|---|---|---|
| bootstrap monitor (pass sample) | Pass | `max_consecutive=2`, `threshold=3` |
| bootstrap monitor (fail sample) | Fail (expected) | `max_consecutive=3`, `threshold=3` |
| docs sync (default) | Pass | Unity latest=2026-02-24, legacy=2026-02-25 |
| docs sync (`-RequireSameDay`) | Fail (expected) | legacy date mismatch（same-day strict） |

## Artifacts
- `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_pass.json`
- `Unity_PJ/artifacts/manual-check/u8_runtime_bootstrap_monitor_fail.json`
- `Unity_PJ/artifacts/manual-check/u8_docs_sync_pass.json`
- `Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_fail.json`
- `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-pass.jsonl`
- `Unity_PJ/artifacts/manual-check/u8-runtime-log-samples/runtime-20260224-fail.jsonl`

