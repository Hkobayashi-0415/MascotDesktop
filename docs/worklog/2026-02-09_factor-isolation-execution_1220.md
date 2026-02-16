# Worklog: Factor Isolation Execution Setup

- Date: 2026-02-09
- Task: 1要因ずつ全MMDに適用して評価できる実行基盤を追加
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Report-Path: docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本リポジトリのworklog/reportを優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, test/factor-isolation, unity/runtime]

## 変更内容
- `SimpleModelBootstrap` に以下を追加:
  - モデル候補検出 `DiscoverModelCandidates()`
  - 現モデル再読込 `ReloadModel()`
  - 相対パス指定再読込 `LoadModelByRelativePath()`
  - アクティブモデルの登録/クリア機構
- `RuntimeDebugHud` に以下を追加:
  - モデル候補数・インデックス表示
  - `Model: prev/next/reload/rescan` ボタン

## 実行コマンド
- `Get-Content` で対象コード確認
- `Get-ChildItem` で assets_user 内モデル列挙
- `apply_patch` で2ファイル編集

## テスト結果
- Unity実行テスト: 未実施（ユーザー担当）
- 静的確認: 追加メソッド・ボタン文字列・参照先を確認

## 判断理由
- 1要因×全モデル方式には、Play中のモデル切替が必須。
- 手動でconfigを書き換えるより、誤差が少なく比較が安定する。

## 次アクション
1. Factor1適用状態で `amane_kanata` と `nurse_taso` をHUDで往復確認
2. 結果を表に記録
3. Factor1を戻してFactor2へ進行

## ロールバック方針
- `SimpleModelBootstrap.cs` と `RuntimeDebugHud.cs` の今回差分を戻す。

## Record Check
- Report-Path exists: True
- Repo-Refs recorded: Yes
- Obsidian-Refs recorded (or n/a): Yes (n/a)
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags recorded: Yes
