# Worklog: unity-cli-env-deep-diagnosis

- Date: 2026-02-09
- Task: Unity.exe / OS 基本コマンド起動失敗の包括調査
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation
- Repo-Refs: tools/run_unity.ps1
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-09_unity-cli-env-deep-diagnosis.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260209_1112.md
- Tags: [agent/codex, model/gpt-5, tool/codex]

## Summary
OS 基本コマンド（cmd.exe/where.exe）も起動不可であることを確認。System32 に必要な API Set DLL が欠落しており、環境固有の OS 不整合が原因と判断。

## Changes
- 変更なし

## Commands
- `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" | Select-Object ProductName,DisplayVersion,CurrentBuild,CurrentBuildNumber,UBR,ReleaseId,EditionID`
- `Get-Item env:Path`
- `Get-Item "C:\Windows\System32\cmd.exe" ...`
- `Get-Item "C:\Windows\System32\where.exe" ...`
- `Get-ChildItem "C:\Windows\System32" -Filter "api-ms-win-crt-runtime-l1-1-0.dll" ...`
- `Get-ChildItem "C:\Windows\SysWOW64" -Filter "api-ms-win-crt-runtime-l1-1-0.dll" ...`
- `Get-ChildItem "C:\Windows\System32\downlevel" -Filter "api-ms-win-crt-runtime-l1-1-0.dll" ...`
- `Get-ChildItem "C:\Windows\System32" -Filter "api-ms-win-core-libraryloader-l1-2-0.dll" ...`
- `"C:\Windows\System32\cmd.exe" /c ver`（失敗再現）
- `"C:\Windows\System32\where.exe" cmd`（失敗再現）
- `Get-ChildItem "C:\Windows\System32" -Filter "kernel32.dll" ...`
- `Get-ChildItem "C:\Windows\System32" -Filter "ucrtbase.dll" ...`

## Tests
- 未実施

## Rationale (Key Points)
- OS バージョン: Windows 10 Pro 24H2 (Build 26100.7623)
- System32 に api-ms-win-crt-runtime-l1-1-0.dll が存在せず、downlevel のみに存在
- cmd.exe / where.exe が同一エラーで起動不可

## Rollback
- 変更なし

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- Windows Update 完了
- OS インプレース修復
- Unity CLI Ping 再試行
