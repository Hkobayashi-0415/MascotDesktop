# U8 Ops Full Closeout (2026-02-25 17:45)

## Executed Checks
```powershell
./tools/run_u8_ops_checks.ps1 -Profile Daily -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/run_u8_ops_checks.ps1 -Profile Gate  -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/run_u8_ops_checks.ps1 -Profile Custom -LogPattern "runtime-20260225-*.jsonl" -ArtifactDir "Unity_PJ/artifacts/manual-check"
./tools/check_unity_legacy_docs_sync.ps1 -RequireSameDay -ArtifactPath "Unity_PJ/artifacts/manual-check/u8_docs_sync_strict_pass_20260225.json"
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -DryRun
./tools/unregister_u8_ops_checks_task.ps1 -DryRun
./tools/register_u8_ops_checks_task.ps1 -StartTime "09:00" -Force
```

## Results
| Check | Result | Evidence |
|---|---|---|
| Daily profile | Pass | `u8_ops_checks_run_daily_20260225_174307.json` |
| Gate profile | Pass | `u8_ops_checks_run_gate_20260225_174307.json` |
| Custom profile | Pass | `u8_ops_checks_run_custom_20260225_174453.json` |
| Strict same-day docs sync | Pass | `u8_docs_sync_strict_pass_20260225.json` |
| Scheduler register dry-run | Pass | console output |
| Scheduler unregister dry-run | Pass | console output |
| Scheduler register actual | Blocked-Environment | `schtasks.exe` 起動時モジュール不足 |

## Notes
- `run_u8_ops_checks` は artifact名に `<profile>` を含めるよう更新し、同秒実行の衝突を解消。
- この環境では `schtasks.exe` 実行不可のため、Task Scheduler 実登録は通常端末で実施する。

