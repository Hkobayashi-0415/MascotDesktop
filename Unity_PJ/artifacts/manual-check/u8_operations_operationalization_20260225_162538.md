# U8 Operationalization Run (2026-02-25 16:25:38)

## Command
```powershell
./tools/run_u8_ops_checks.ps1 `
  -LogPattern "runtime-20260225-*.jsonl" `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"
```

## Result
- runtime monitor: Pass
- docs sync: Pass
- overall: Pass

## Artifacts
- `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_summary_20260225_162538.json`
- `Unity_PJ/artifacts/manual-check/u8_docs_sync_summary_20260225_162538.json`
- `Unity_PJ/artifacts/manual-check/u8_ops_checks_run_20260225_162538.json`
- `Unity_PJ/artifacts/manual-check/u8_runtime_monitor_default_20260225.json`

## Note: Legacy Naming (Pre-Profile)
- この実行は `run_u8_ops_checks.ps1` に profile 機能が追加される前のものであり、artifact 名に profile トークンが含まれていない。
- 現行命名（profile 付き）の等価 artifact: `u8_ops_checks_run_custom_20260225_174453.json`

