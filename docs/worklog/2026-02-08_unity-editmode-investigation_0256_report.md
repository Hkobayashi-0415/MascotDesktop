# Report: Unity EditMode test investigation
Date: 2026-02-08 0256

Result:
- Test assembly definition is already configured for EditMode tests (TestAssemblies reference, Editor platform).
- editmode.xml still not generated; editmode.log shows normal startup/shutdown with no TestRunner output.

Notes:
- Likely issue is tests not being discovered/run in batchmode despite -runTests; recommend explicit class-level filter or Editor Test Runner export.

Next:
- Run batchmode with filters for specific test classes and separate result files.
- If still missing, run via Editor Test Runner and export XML.
