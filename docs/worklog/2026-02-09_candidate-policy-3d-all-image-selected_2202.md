# Worklog: Candidate Policy Update (3D All + Selected Images)

- Date: 2026-02-09
- Task: 候補抽出を「3Dモデル全件＋画像選抜」に変更
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: n/a
- Report-Path: docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - docs/reports/2026-02-09_factor-isolation-execution-plan.md
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本タスクはリポジトリ内の記録を優先）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, candidate-policy]

## 変更内容

- `SimpleModelBootstrap.DiscoverModelCandidates()` を更新:
  - 3Dモデル拡張子（`.pmx`, `.pmd`, `.vrm`）は全件採用
  - 画像拡張子（`.png`, `.jpg`, `.jpeg`, `.bmp`）は選抜採用
- 画像選抜ルール:
  - `characters/<name>` 単位でグルーピング
  - ファイル名優先度でソート（`face` > `body/skin` > `hair` > `main/diffuse/albedo/base` > `tex` > その他）
  - 各グループ最大4件
- 補助メソッド追加:
  - `GetCandidateGroupKey()`
  - `GetImageSelectionRank()`

## 実行コマンド

- `Select-String`（差分対象シンボル確認）
- `Get-ChildItem`（assets_user配下の対象ファイル観測）
- PowerShellワンライナーで新ポリシー抽出結果を模擬実行
- `apply_patch`（実装/レポート更新/本worklog追加）

## テスト結果

- Unity再生テスト: 未実施（ユーザー担当）
- 静的検証:
  - `DiscoverModelCandidates` の参照・選抜ロジック確認
  - 3Dモデルが漏れず含まれることを確認
  - 画像が全件ではなく選抜されることを模擬出力で確認

## 判断理由（要点）

- ユーザー要求が「3Dモデルは全部、画像は選抜」であるため、抽出段階で明示的にポリシー分離した。
- 画像を無制限で含めると比較対象が多すぎるため、キャラクター単位で上位のみ採用する実装にした。

## 次アクション

1. UnityでHUDの `Model: rescan` を押し、候補一覧を再生成
2. 実運用で「選抜画像が多すぎる/少なすぎる」場合は `MaxSelectedImagesPerCharacter` を調整
3. 必要ならキーワード優先度をプロジェクト用語に合わせて調整

## ロールバック方針

- `Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs` の候補抽出差分を戻す
- `docs/reports/2026-02-09_factor-isolation-execution-plan.md` のポリシー記述を戻す
- `docs/worklog/2026-02-09_candidate-policy-3d-all-image-selected_2202.md` を削除

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes (n/a)
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
