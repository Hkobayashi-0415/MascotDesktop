# Asset Path Read Test Design (Unity)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-07
- Scope: Test design for Unity-side asset path resolution and read behavior.

## Objective

Validate that Unity runtime resolves and reads avatar assets from canonical roots with deterministic fallback, while enforcing cutover and safety rules.

## Target Contract

- Canonical root: `Unity_PJ/data/assets_user/`
- Transition rule: temporary migration validation may reference legacy path, but final cutover forbids runtime read from legacy root.
- Path rules:
  - Relative paths only (no absolute path acceptance).
  - Normalize path separators.
  - Reject traversal (`..`) outside allowed roots.
  - Non-ASCII path is warning + continue (unless explicit hard-fail policy is introduced).
- Fallback order:
  1. `data/assets_user/...`
  2. `project/Assets/StreamingAssets/...` (template/test fixtures)
  3. Placeholder/fallback asset path

## Test Layers

### Layer 1: EditMode (Pure Path Resolution)
- Purpose: deterministic validation of path normalization and policy checks.
- Test target class (planned): `AssetPathResolver`.
- Input: base roots + relative asset path + policy flags.
- Output: resolved path + source tier + warning/error code.

### Layer 2: PlayMode (Actual File Read)
- Purpose: validate runtime read behavior and fallback in Unity execution context.
- Test target class (planned): `AssetReadService`.
- Input: resolved candidate list and expected extension policy.
- Output: loaded/not loaded result, fallback tier, emitted error code/log metadata.

## Test Cases

| ID | Layer | Scenario | Input | Expected |
|---|---|---|---|---|
| AP-001 | EditMode | Canonical relative path joins correctly | `characters/demo/pngtuber_mode3/states/01_normal.png` | Resolved under `Unity_PJ/data/assets_user/...` |
| AP-002 | EditMode | Backslash and slash mixed | `characters\\demo/pngtuber_mode3\\states/01_normal.png` | Normalized and resolved to same canonical path |
| AP-003 | EditMode | Absolute path is rejected | `D:\\tmp\\avatar.png` | Validation error (`ASSET.PATH.ABSOLUTE_FORBIDDEN`) |
| AP-004 | EditMode | Traversal attempt is rejected | `../secrets/token.txt` | Validation error (`ASSET.PATH.TRAVERSAL_FORBIDDEN`) |
| AP-005 | EditMode | Non-ASCII segment warning | `characters/サンプル/state.png` | Warning emitted, resolution continues |
| AP-006 | EditMode | Legacy root forbidden after cutover flag | path under `Old_PJ/workspace/data/assets_user` | Validation error (`ASSET.PATH.LEGACY_FORBIDDEN`) |
| AP-101 | PlayMode | File exists in assets_user | existing png under canonical root | Load success, source=`assets_user` |
| AP-102 | PlayMode | Missing in assets_user but exists in StreamingAssets | missing canonical + existing streaming fixture | Load success, source=`streaming_assets` |
| AP-103 | PlayMode | Missing in both roots | nonexistent file | Fallback placeholder selected, `assets.placeholder_used` log |
| AP-104 | PlayMode | Extension not allowed | `.exe` or unsupported extension | Validation error (`ASSET.READ.UNSUPPORTED_EXTENSION`) |
| AP-105 | PlayMode | Corrupted file read | broken image fixture | Load failure with explicit error code, no crash |
| AP-106 | PlayMode | Request correlation propagation | request with `request_id` | `request_id` present in read/fallback/error logs |

## Fixtures and Layout

- Keep fixtures lightweight and text/image-minimal.
- Use ASCII-safe fixture directories for baseline tests.
- Fixture roots:
  - `Unity_PJ/data/assets_user/characters/test_ascii/...`
  - `Unity_PJ/project/Assets/StreamingAssets/characters/test_fallback/...`

## Execution Plan (Unity Test Runner)

1. Implement resolver tests in EditMode.
2. Implement read/fallback tests in PlayMode.
3. Run batch tests and store XML reports under `Unity_PJ/artifacts/test-results/`.

Example commands:

```powershell
"<UnityEditorPath>\\Unity.exe" -batchmode -projectPath "D:\\dev\\MascotDesktop\\Unity_PJ\\project" -runTests -testPlatform EditMode -testResults "D:\\dev\\MascotDesktop\\Unity_PJ\\artifacts\\test-results\\editmode.xml" -quit
"<UnityEditorPath>\\Unity.exe" -batchmode -projectPath "D:\\dev\\MascotDesktop\\Unity_PJ\\project" -runTests -testPlatform PlayMode -testResults "D:\\dev\\MascotDesktop\\Unity_PJ\\artifacts\\test-results\\playmode.xml" -quit
```

## Pass Criteria

- All AP-001..AP-006 policy tests pass with deterministic error/warn behavior.
- AP-101..AP-106 read/fallback tests pass without unhandled exception.
- Logs include `request_id`, source tier, and error code without sensitive payload body.

