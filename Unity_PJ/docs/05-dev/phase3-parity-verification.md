# Phase 3 Parity Verification (Unity Full Cutover)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-24
- Scope: Trace inherited must requirements (`UR-001`..`UR-012`) to Unity runtime implementation and verification points.

## Requirement Trace Matrix

| UR | Requirement | Implementation Evidence | Verification Evidence |
|---|---|---|---|
| UR-001 | Single Runtime | `Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` runtime root bootstrap; `Assets/Scripts/Runtime/Core/CoreOrchestrator.cs` | PlayMode bootstrap smoke (`runtime object exists`) |
| UR-002 | Window Control | `Assets/Scripts/Runtime/Windowing/WindowController.cs` (`frameless`, `topmost`, restore, drag entrypoint) | Manual: confirm frameless window + HUD buttons `Toggle Topmost` / `Drag Window` |
| UR-003 | Resident Operation | `Assets/Scripts/Runtime/Windowing/ResidentController.cs` (`Hide/Show/Exit`) | Manual: HUD `Hide/Show` and `Exit`; hotkeys `F10/F12` |
| UR-004 | Avatar MMD Load | `Assets/Scripts/Runtime/Avatar/ReflectionModelLoaders.cs` + `SimpleModelBootstrap.cs` | Existing model loader path tests + runtime log `avatar.model.displayed` |
| UR-005 | Motion Slot Playback | `Assets/Scripts/Runtime/Avatar/MotionSlotPlayer.cs` | `Assets/Tests/EditMode/MotionSlotPlayerTests.cs` |
| UR-006 | State Event Handling | `Assets/Scripts/Runtime/Avatar/AvatarStateController.cs` + `CoreOrchestrator.cs` | `Assets/Tests/EditMode/AvatarStateControllerTests.cs` |
| UR-007 | Request Correlation | `RuntimeLog.NewRequestId()` use across core/avatar/window/ipc | `Assets/Tests/EditMode/RuntimeLogTests.cs` + runtime JSON lines |
| UR-008 | Error Contract | Structured `error_code` on WARN/ERROR in runtime modules | `Assets/Tests/EditMode/RuntimeLogTests.cs` |
| UR-009 | Logging | `Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs` schema | `Assets/Tests/EditMode/RuntimeLogTests.cs` |
| UR-010 | Asset Policy | `Assets/Scripts/Runtime/Assets/AssetPathResolver.cs` canonical + fallback policy | `Assets/Tests/EditMode/AssetPathResolverTests.cs` |
| UR-011 | Legacy Cutover Safety | Runtime resolves from `Unity_PJ/data/assets_user`/StreamingAssets only | `AssetPathResolverTests` legacy path forbidden case |
| UR-012 | Windows Local Constraint | `WindowController` / `ResidentController` Win-specific branches | Manual Windows run (Editor/Standalone) |

## Manual Runtime Check (Screen Confirmation)

1. Open project:
- `D:\dev\MascotDesktop\Unity_PJ\project`

2. Enter Play Mode (any scene; bootstrap auto-runs):
- Confirm HUD appears at top-left: `MascotDesktop Runtime HUD`

3. Validate visible avatar:
- Confirm MMD model is centered and fully visible (auto-fit to stage height).
- If loader fails, `SimpleModelFallback` is present.

4. Validate control path:
- `State: happy` / `State: sleepy`
- `Motion: wave`
- `Toggle Topmost`
- `Hide/Show`
- Note: `Toggle Topmost` / `Hide/Show` native window effect is validated on Windows Standalone Player. In Unity Editor, validate simulation logs (`window.topmost.simulated`, `window.resident.*.simulated`).

5. Validate logs:
- Console contains JSON with `request_id`, `event_name`, `error_code`.
- `avatar.model.displayed` is emitted when model/image is shown.
- `avatar.model.candidates.discovered` で `canonical_exists` / `streaming_exists` を確認し、候補0時の原因を切り分ける。

## Batch Test Commands

```powershell
$ts = Get-Date -Format 'yyyyMMdd_HHmmss'
$outDir = "D:\dev\MascotDesktop\Unity_PJ\artifacts\test-results"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

& "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe" `
  -batchmode -nographics `
  -projectPath "D:\dev\MascotDesktop\Unity_PJ\project" `
  -runTests -testPlatform EditMode `
  -testResults "$outDir\editmode-phase3-$ts.xml" `
  -logFile "$outDir\editmode-phase3-$ts.log" `
  -quit
```

## Notes

- In this Codex runtime environment, external process start intermittently fails (`Unity.exe` launch failure), so final pass/fail must be confirmed on local machine where Unity can launch.
