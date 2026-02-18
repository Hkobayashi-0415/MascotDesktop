# Runtime Review and Refactor Proposal (2026-02-19)

## Scope
- Review target:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`
- Goal:
  - Build a refactor plan for stronger error handling and richer log collection.

## Debugger/Execution Result
- Command:
  - `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- Result:
  - Unity process startup failed at `tools/run_unity_tests.ps1:69`.
  - Error: `指定されたモジュールが見つかりません。`
  - Artifacts missing:
    - `Unity_PJ/artifacts/test-results/editmode-20260219_003936.log`
    - `Unity_PJ/artifacts/test-results/editmode-20260219_003936.xml`

## Findings (ordered by severity)

### 1) High: PMX/VRM load failures lose diagnostic depth before logging
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:149`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:206`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:594`
- Detail:
  - Loader layer catches broad `Exception` and returns only base message (`Fail(..., ex.GetBaseException().Message)`).
  - Bootstrap logs failure without exception payload, so exception type/stack/source method are not preserved.
- Operational impact:
  - Hard to distinguish reflection API mismatch, loader missing type, invocation fault, and IO decode failure.

### 2) High: Loader path has no request-scoped log continuity
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:27`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:61`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:107`
- Detail:
  - Loader APIs do not take `requestId`, and do not emit `RuntimeLog` entries.
  - Failures are reported upstream, but fine-grained loader steps cannot be correlated in one trace.
- Operational impact:
  - Root cause analysis requires cross-file guesswork instead of single request timeline.

### 3) High: Reflection fallback loader discovery is over-broad and side-effect prone
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:157`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:186`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs:195`
- Detail:
  - Scans all loaded assemblies and invokes any public static method with one `string` parameter on PMX-related types.
  - This can execute unintended methods and produces unstable behavior/perf depending on assembly set.
- Operational impact:
  - Non-deterministic loader behavior and difficult reproducibility across environments.

### 4) Medium: RuntimeLog writes synchronously to disk on caller thread
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:152`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:162`
- Detail:
  - Every log call does `File.AppendAllText` inside lock.
  - High-frequency diagnostics (model/material scans) can generate large log volume on frame-critical paths.
- Operational impact:
  - Potential frame hitch and IO contention under heavy diagnostics.

### 5) Medium: RuntimeLog lacks retention/rotation and level gating
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:156`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:158`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:99`
- Detail:
  - Single `runtime.jsonl` grows unbounded.
  - No runtime-configurable minimum level/category filter.
  - All info-level logs are always emitted to console and file.
- Operational impact:
  - Log volume growth, noisy telemetry, and reduced signal-to-noise in incidents.

### 6) Medium: Silent catch blocks hide diagnostic failures
- Evidence:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:943`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs:958`
- Detail:
  - Path-structure diagnostic enumeration failures are swallowed silently (`catch (Exception)`).
  - Counters become `-1` without recording why.
- Operational impact:
  - Diagnostic trustworthiness drops when IO/permission issues happen.

### 7) Medium: Mixed logging systems reduce observability consistency
- Evidence:
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs:369`
  - `Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs:362`
  - `Unity_PJ/project/Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs:23`
- Detail:
  - LibMMD path uses `Debug.Log*` while runtime path uses structured `RuntimeLog`.
  - Cross-layer events are not correlated by shared schema/request_id.
- Operational impact:
  - Troubleshooting requires manual stitching of heterogeneous logs.

### 8) Low: Test coverage does not guard runtime failure classification flows
- Evidence:
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:13`
  - `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs:494`
- Detail:
  - Existing tests are mostly helper/heuristic focused.
  - Missing direct tests for loader error taxonomy, request_id propagation, and fallback reason consistency.

## Refactor Proposal (heavy-duty error handling + logging)

### A. Error model unification
1. Introduce unified error envelope for loader pipeline.
- Add `RuntimeErrorDetail`:
  - `ErrorCode`, `Message`, `ExceptionType`, `ExceptionMessage`, `Stage`, `Cause`, `Path`, `SourceTier`.
- Extend:
  - `ModelLoadAttemptResult` and `ImageLoadAttemptResult` to carry detail object.
- Keep existing `ErrorCode/Message` fields for backward compatibility during migration.

2. Preserve exception chain at source.
- In `ReflectionModelLoaders`, do not flatten to only base message.
- Capture:
  - `ex.GetType().FullName`
  - `ex.Message`
  - optional `ex.InnerException?.GetType().FullName`

### B. Request-scoped tracing from bootstrap to low-level loaders
1. Add `requestId` parameter to loader APIs.
- `TryLoadImageTexture(string absolutePath, string requestId = null)`
- `TryLoadVrm(string absolutePath, string requestId = null)`
- `TryLoadPmx(string absolutePath, string requestId = null)`

2. Emit structured stage logs.
- Events:
  - `avatar.loader.stage.begin`
  - `avatar.loader.stage.end`
  - `avatar.loader.stage.fail`
- Stages:
  - classify -> resolve -> instantiate -> reflection invoke -> post-normalize.

### C. Reflection fallback hardening
1. Replace broad discovery with explicit contract.
- Preferred:
  - Known type + known method signature only.
- Optional fallback:
  - Whitelist assembly prefixes and method name patterns.

2. Cache resolved reflection handles.
- Cache `Type` and `MethodInfo` once to avoid repeated assembly scans.

### D. RuntimeLog backend hardening
1. Add async buffered writer.
- Use in-memory queue + background flush coroutine/thread.
- Batch write every N ms or max queue size.

2. Add rotation and retention.
- Rotate by size/day:
  - `runtime-YYYYMMDD-HH.jsonl`.
- Keep last N files or max total size.

3. Add log policy controls.
- Runtime-configurable:
  - `min_level`
  - component/event include/exclude
  - sample rate for verbose diagnostics.

### E. LibMMD logging bridge
1. Add adapter boundary.
- Wrap important LibMMD failures/warnings into `RuntimeLog` with request_id when called from runtime pipeline.
- Keep original `Debug.Log*` if needed for legacy tooling, but mirror critical events.

### F. Test expansion
1. Add focused tests for error classification.
- Missing file, API mismatch, invoke exception, null root, decode fail.

2. Add request correlation tests.
- Ensure same `request_id` appears from bootstrap to loader fail event.

3. Add logging policy tests.
- Verify `min_level` suppresses noisy info logs.
- Verify rotation policy creates new file when threshold exceeded.

## Proposed Phase Plan
1. Phase 1 (safe foundation)
- Add error detail DTOs and requestId plumbing through loader APIs.
- Add stage logs without behavior changes.

2. Phase 2 (logging infra)
- Introduce async writer + rotation + level gating in `RuntimeLog`.
- Add compatibility fallback to current sync write behind feature flag.

3. Phase 3 (reflection hardening)
- Narrow fallback loader discovery and add reflection cache.
- Add strict error code mapping per stage.

4. Phase 4 (test and rollout)
- Add EditMode tests for new error/log contracts.
- Add runtime verification checklist and incident playbook update.

## Acceptance Criteria
- Same failure produces stable `error_code`, `stage`, and `exception_type`.
- One request can be traced end-to-end by `request_id`.
- Log file growth is bounded by rotation policy.
- High-volume model diagnostics do not block main thread with sync file IO.

