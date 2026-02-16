# Worklog: unity-cli-env-diagnosis

- Date: 2026-02-09
- Task: Unity.exe 起動失敗の原因調査
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation
- Repo-Refs: tools/run_unity.ps1
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-09_unity-cli-env-diagnosis.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260209_1058.md
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
Unity.exe が「指定されたモジュールが見つかりません」で起動できない件を調査。Editor ルートに必要 DLL が存在しないため、インストール欠落/破損が有力と判断。

## Changes
- 変更なし（調査のみ）

## Commands
- `Get-Item "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe" | Select-Object FullName,Length,LastWriteTime`
- `Get-Item "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" | Select-Object FullName,Length,LastWriteTime`
- `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" ...`
- `Get-ItemProperty "HKLM:\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x86" ...`
- `Get-ChildItem "C:\Windows\System32" -Filter "vcruntime140*.dll" ...`
- `Get-ChildItem "C:\Windows\SysWOW64" -Filter "vcruntime140*.dll" ...`
- `"C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe" -version`（失敗再現）
- `Get-ChildItem "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor" -Filter "UnityPlayer.dll" -Recurse ...`
- `Test-Path "...\UnityPlayer.dll"; Test-Path "...\UnityCrashHandler64.exe"; Test-Path "...\mono-2.0-bdwgc.dll"`
- `Test-Path "C:\Windows\System32\ucrtbase.dll"; Test-Path "C:\Windows\System32\api-ms-win-crt-runtime-l1-1-0.dll"`
- `Get-ChildItem "C:\Windows\System32\downlevel" -Filter "api-ms-win-crt-runtime-l1-1-0.dll" ...`

## Tests
- 未実施（診断のみ）

## Rationale (Key Points)
- Unity.exe 実行時に OS から「指定されたモジュールが見つかりません」が返る
- VC++ ランタイムは x64/x86 ともに Installed=1
- Editor ルートに UnityPlayer.dll / UnityCrashHandler64.exe / mono-2.0-bdwgc.dll が存在しない
- UCRT の api-ms-win-crt-runtime-l1-1-0.dll が System32 ではなく downlevel に存在

## Rollback
- 変更なしのため不要

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Unity Hub で 6000.3.7f1 を修復/再インストール
- OS の UCRT/VC++ 再配布の整合性を確認
- 再度 `tools/run_unity.ps1 -ExecuteMethod MascotDesktop.Editor.Automation.Ping` を実行
