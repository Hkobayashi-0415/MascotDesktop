# Worklog: Candidate Mode Separation (Model/Image)

- Date: 2026-02-10
- Task: `Model: next/prev` の候補混在解消（モデル専用巡回 + 画像分離モード + 安定ソート）
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
  - Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs
  - D:/dev/00_repository_templates/ai_playbook/skills/bug-investigation/SKILL.md
  - D:/dev/00_repository_templates/ai_playbook/skills/worklog-update/SKILL.md
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-10_candidate-mode-separation_0144.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260210_0144.md
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, candidate-routing]

## Summary

`next/prev` がモデル本体と画像を同一候補列で巡回していたため、体感ランダム表示になっていた。候補抽出 API を「モデル」と「画像」に分離し、HUD にモード切替を追加して、既定をモデル巡回に固定した。

## Changes

- `SimpleModelBootstrap`:
  - `DiscoverModelCandidates()` をモデル専用化（`.pmx/.pmd/.vrm`）
  - `DiscoverImageCandidates()` を新設
  - `BuildModelCandidatesFromRelativePaths()` をモデル専用化
  - `BuildImageCandidatesFromRelativePaths()` を新設
  - 両候補で `Distinct + OrderBy(StringComparer.OrdinalIgnoreCase)` を適用し安定ソート化
- `RuntimeDebugHud`:
  - `CandidateMode`（Model / Image）を追加
  - モード切替ボタン `Mode: models` / `Mode: images` を追加
  - `SwitchModel` をアクティブモード配列のみ巡回するよう変更
  - 候補数表示を `Model Candidates` / `Image Candidates` に分離
- `SimpleModelBootstrapTests`:
  - モデル候補が画像を含まないことを検証へ更新
  - モデル候補の sorted/distinct を検証追加
  - 画像候補は `BuildImageCandidatesFromRelativePaths` で上限4件選択を検証

## Commands

- `Get-Content .git/HEAD; Get-Content .git/config`
- `Get-ChildItem -Path Unity_PJ -Recurse -File | Where-Object { $_.Name -match ... }`
- `Select-String -Path Unity_PJ/project/Assets/Scripts/Runtime/**/*.cs,...`
- `apply_patch` (3 files)
- `./tools/run_unity.ps1 -ExtraArgs @('-runTests','-testPlatform','EditMode','-testFilter','MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests','-testResults',...) -LogFile ...`

## Tests

- Unity EditMode batch test: **Failed**
  - Error: `Unity.exe` 起動時 `指定されたモジュールが見つかりません`
  - 結果: 実行環境依存でテストランナー未起動（テスト結果XML未生成）
- Static verification:
  - `RuntimeDebugHud` がモデル/画像候補を別配列で扱うことを確認
  - `SwitchModel` がアクティブモード候補のみ読むことを確認
  - テストコードが新仕様（モデル候補と画像候補分離）に一致していることを確認

## Rationale (Key Points)

- 根本原因は候補列生成で model+image を連結していた点。
- `next/prev` の仕様を保ったまま挙動を明確化するため、候補列を2系統化して UI モードで選択する設計を採用。
- 候補順の揺れを防ぐため、両系統とも最終的に大小比較ソートを強制。

## Rollback

1. `SimpleModelBootstrap.cs` の `DiscoverImageCandidates` / `BuildImageCandidatesFromRelativePaths` 追加差分を戻す。
2. `RuntimeDebugHud.cs` の `CandidateMode` 追加差分を戻す。
3. `SimpleModelBootstrapTests.cs` を旧仕様テストへ戻す。

## Next Actions

1. Unity Editor 起動可能環境で `SimpleModelBootstrapTests` を実行する。
2. PlayMode で Model モード時に `avatar.model.loader_selected` が `Pmx/Vrm` のみになることを確認する。
3. Image モードでのみ画像候補を巡回することを確認する。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
