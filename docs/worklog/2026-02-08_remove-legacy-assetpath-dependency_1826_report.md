# Report: Remove legacy asset-path runtime dependency
Date: 2026-02-08 1826

Result:
- Removed runtime option property `LegacyAssetsRoot` from `AssetPathResolverOptions`.
- Removed bootstrap-time assignment of `Old_PJ/workspace/data/assets_user` from `SimpleModelBootstrap`.
- Kept policy enforcement by rejecting legacy/workspace-like relative paths.
- Updated EditMode tests to match the new dependency-free path policy input.

Changed files:
- `Unity_PJ/project/Assets/Scripts/Runtime/Assets/AssetPathResolver.cs`
- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
- `Unity_PJ/project/Assets/Tests/EditMode/AssetPathResolverTests.cs`

Validation:
- Static check: no remaining `LegacyAssetsRoot` reference under `Unity_PJ/project/Assets`.
- Runtime check: Unity batch execution from this Codex runtime could not start (`Unity.com`/`cmd.exe`: module not found).
- Local Unity execution is required for final runtime verification.

Recommended local check:
1. Run EditMode tests for `MascotDesktop.Tests.EditMode.AssetPathResolverTests`.
2. Confirm log XML has `result="Passed"` and all 6 tests pass.
3. Confirm runtime display flow still logs `avatar.model.displayed` on success.
