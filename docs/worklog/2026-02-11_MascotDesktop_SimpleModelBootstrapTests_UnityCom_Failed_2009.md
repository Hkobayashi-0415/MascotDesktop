# 2026-02-11 MascotDesktop SimpleModelBootstrapTests (Unity.com) Execution Failure

## Metadata
- **Date**: 2026-02-11
- **Project**: MascotDesktop
- **Used-Skills**: None
- **Repo-Refs**: `tools/run_unity_tests.ps1`, `Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` (Updated)
- **Obsidian-Refs**: None
- **Execution-Tool**: Antigravity
- **Execution-Agent**: System
- **Execution-Model**: gemini-2.0-pro-exp-0211
- **Tags**: [test, unity, unity.com, failure, assertions]

## Summary
Execution of `SimpleModelBootstrapTests` using `Unity.com` completed with **2 failures**.
The test file `SimpleModelBootstrapTests.cs` has been updated since the last successful run (file size increased from ~10KB to ~20KB), adding new transparency heuristic tests which are now failing.

## Execution Log

### 1. Test Execution
- Command: `tools/run_unity_tests.ps1 ... Unity.com ...`
- Result: **Failed** (Exit Code 2).
- Log file: `artifacts/test-results/editmode-20260211_200355.log`.
- XML file: `artifacts/test-results/editmode-20260211_200355.xml`.

### 2. Failed Tests
1. **`MaterialLoaderTransparencyHeuristic_UsesSemiTransparentRatioThreshold`**
   - **Location**: `SimpleModelBootstrapTests.cs:261`
   - **Assertion**: `Expected: True, But was: False`
   - **Context**: Testing if 10/100 pixels with alpha 0.59 triggers transparency. It did not.

2. **`MaterialLoaderTransparencyHeuristic_UsesStrongTransparentRatioThreshold`**
   - **Location**: `SimpleModelBootstrapTests.cs:279`
   - **Assertion**: `Expected: True, But was: False`
   - **Context**: Testing if 32/1000 pixels with alpha 0.10 triggers transparency. It did not.

## Analysis
The new tests expect specific thresholds for transparency detection in `LibMMD.Unity3D.MaterialLoader`. The actual implementation seems to have stricter thresholds or different logic than what the tests anticipate.

## Next Actions
- Adjust the test expectations (thresholds) or update the `MaterialLoader` implementation to match the requirements.
