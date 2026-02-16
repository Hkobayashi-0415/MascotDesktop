# Worklog: Factor Isolation Execution (Runtime Controls)

- Date: 2026-02-09
- Task: 1要因ずつ全MMDで比較できる実行方式をコードに反映
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: n/a
- Report-Path: docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs
  - docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本タスクはリポジトリ内 worklog/report を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, test/factor-isolation]

## 変更内容

- `SimpleModelBootstrap`:
  - 要因プリセット（F0-F3）を追加
  - `GetRenderFactorNames()` / `GetRenderFactorIndex()` / `SetRenderFactorIndex()` を追加
  - 要因変更時にライト設定を更新し、モデルを再読込する制御を追加
  - モデル表示時に `AlbedoMultiplier` を適用する処理を追加
  - モデルクリアを「登録済みアクティブモデル優先破棄」に変更
- `RuntimeDebugHud`:
  - `Render Factor` 表示を追加
  - `Factor: prev` / `Factor: next` ボタンを追加
  - モデル切替UIと組み合わせて、要因固定のまま全モデル巡回を可能化
- `docs/reports/2026-02-09_factor-isolation-execution-plan.md`:
  - 実装済み操作系と推奨実行順（factor固定→全モデル→次factor）を更新

## 実行コマンド

- `Get-Content`（対象C# / report / worklog確認）
- `Get-ChildItem docs/reports`（既存レポート確認）
- `Get-Date -Format "yyyy-MM-dd_HHmm"`（worklog命名）
- `Get-Content .git/HEAD` / `Get-Content .git/config`（リポジトリ同定）
- `apply_patch`（3ファイル更新 + 1ファイル新規作成）

## テスト結果

- Unity再生テスト: 未実施（ユーザー担当）
- 静的確認:
  - 追加メソッド参照関係を確認
  - HUDボタンと `SimpleModelBootstrap` 公開APIの接続を確認
  - 要因変更時の再読込フローをコード上で確認

## 判断理由（要点）

- 要因を1つずつ切り替えつつ全MMDに同条件適用するには、
  - Play中モデル切替
  - Play中要因切替
  - 非累積適用
  が必須。
- 要因変更時に再読込することで、前要因の影響が残らない形にした。

## 次アクション

1. Unityで `F0 -> F1 -> F2 -> F3` の順に切替
2. 各factorで全モデルを `Model: prev/next` で巡回してスクリーンショット採取
3. reportの評価表を埋めて、採用factorを決定

## ロールバック方針

- 次の3ファイルを本変更前に戻す:
  - `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs`
  - `Unity_PJ/project/Assets/Scripts/Runtime/UI/RuntimeDebugHud.cs`
  - `docs/reports/2026-02-09_factor-isolation-execution-plan.md`
- 追加ファイル:
  - `docs/worklog/2026-02-09_factor-isolation-execution_2144.md` を削除

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes (n/a)
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
