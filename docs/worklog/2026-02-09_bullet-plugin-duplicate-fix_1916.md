# Worklog: Bullet Plugin Duplicate Fix

- Date: 2026-02-09
- Task: libbulletc 重複plugin警告と DllNotFoundException による PMXロード失敗の修正
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Report-Path: docs/reports/2026-02-09_bullet-plugin-duplicate-fix.md
- Repo-Refs:
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x64/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x86/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x64/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x86/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/ARM/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本リポジトリ内worklog/reportへ記録）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, fix/bullet-plugin, unity/libmmd]

## 変更内容
- libbulletc.dll.meta に PluginImporter を明示追加し、EditorでロードされるDLLを Native/x64 側に限定。
- UWP.disabled 配下 DLL は Editor互換を明示的に無効化。
- MmdGameObject で Bullet初期化失敗時に PhysicsMode=None へフォールバックするよう修正。
- _physicsReactor.Reset() 呼び出しを null/モード条件付きへ変更。

## 実行コマンド
- Get-Content, Select-String による該当行・meta内容確認
- pply_patch による6ファイル修正

## テスト結果
- Unity実行テストはユーザー担当のため未実施。
- 静的検証として、metaの PluginImporter と nabled 設定行、および MmdGameObject の例外フォールバック行を確認。

## 判断理由（要点）
- 重複警告は PluginImporter の Editor互換重複が直接原因。
- DllNotFound が再発しても PMX表示まで巻き込んで失敗しないよう、防御的フォールバックを追加。

## 次アクション
1. Unity再生で重複警告の消失確認
2. PMXがフォールバックカプセルでなく描画されることを確認
3. 必要なら UWP ターゲット用の importer 設定を別途有効化（今回の修正はUWP.disabledを全面無効）

## ロールバック方針
- 以下を修正前へ戻す。
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x64/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/x86/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x64/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/x86/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Plugins/BulletUnity/Native/UWP.disabled/ARM/libbulletc.dll.meta
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs

## Record Check
- Report-Path exists: True
- Repo-Refs recorded: Yes
- Obsidian-Refs recorded (or n/a): Yes (n/a)
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags recorded: Yes
