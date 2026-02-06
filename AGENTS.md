# AGENTS.md — Project Guardrails (No Cross-Project Mixing)

## 0) Project Identity (MUST)
- Hard rule: このタスクでは当該リポジトリ内の情報のみを根拠にする。
  <!-- 他PJ・他タスク・他チャット履歴の内容を混ぜない（混同の温床）。 -->
- リポジトリ同定が曖昧なら停止し、以下を提示して確認する：
  - `git rev-parse --show-toplevel`
  - `git remote -v`
  - `git status`
  - git が使えない場合は `.git/HEAD` と `.git/config` で同定し、同定できれば続行する（不明な場合のみ停止）。

### External Toolboxes (Allowed, With Limits)
- 開発支援として、以下の外部リポジトリ/ノートを参照してよい（skills/テンプレ/手順/経験の検索用途）。
  - `D:\dev\00_repository_templates`（skills・テンプレ・補助ツール）
  - `D:\Obsidian\Programming`（実装時の経験値DB。参考資料）
- ただし、このリポジトリの仕様・実装判断の「根拠」は `MascotDesktop` 内の記述で完結させる（外部参照だけに依存しない）。
- 外部参照を行った場合は、worklog に `Repo-Refs` / `Obsidian-Refs` として識別可能な形（パス/タイトル等）で記録する。
<!-- 目的: コーディングエージェントが確実に理解・実行できる形を維持する。 -->

## 0.1) General Execution Rules (MUST)
- 作業前に該当ドキュメントを確認してから着手する。
- 作業記録は都度記録する。
- 実装系レビューは 機能ごと／フェーズごと に実施 /レビューは 明示的に指示されたときにも実行。
- 指示内容を理解し、適切なスキルを使用する。
- タスク中に追加スキルが必要な場合は、妥当性を評価して使用し、記録に残す。
- 同一目的の作業は 1回のPLAN/承認で完結させ、範囲内は再承認を求めない。

## 0.2) Skill Operation (MUST)
- スキルはタスク内容に応じて選定する（特定スキル名の強制はしない）。
- 追加スキルが必要になった場合は妥当性を評価して使用し、記録に残す。

## 0.3) Chat Title Rule (MUST)
- 新規チャット開始時、最初の返答に `Suggested-Title: <PJ>_<ShortTitle>` を必ず出力する。
- PJ が不明な場合は確認し、PJ が確定するまで作業を開始しない。
- 以降の応答も必要なら `Project: <PJ>` を1行目に記載する。

## 1) Workflow Gate: Plan → Approve → Execute (理由・根拠必須)
### 1.1 Plan（必ず最初）
コマンド実行・ファイル編集の前に、必ず PLAN を提示する（理由と根拠を含む）：
- 目的 / スコープ / 非スコープ
- 現状の根拠（参照ファイル/行番号/観測ログ/再現手順）
- 変更対象ファイル一覧（パス）
- 変更内容の要約（差分の意図）
- 実行予定コマンド一覧（目的付き）
- テスト計画（必須）
- ロールバック手順
- リスク・代替案

### 1.2 Approval（停止条件）
- ユーザーが次のいずれかを返すまで、実行・変更適用をしない：
  - `APPROVE`
  - `GO`
  - `実行して`
  - `実行`
  - `実行してください`
  - `進めて`
  - `はい`

### 1.3 Execute（小さく刻む）
- 意味のある変更単位ごとに：
  - 差分提示（git diff 等）
  - テスト実行（§3）
  - worklog 追記（§6）

## 2) Dependencies / Install policy (A)
- 依存追加は常に事前承認。
- ドキュメントに沿った手順を優先する。
- ホストOSへのグローバルインストールは避け、隔離環境（devcontainer / Docker / repo内venv等）を優先する。

## 3) Testing (A)
- 変更ごとに必ずテストを実行。
- 失敗したら停止し、原因特定→修正→再実行。

## 4) Git policy (B)
- デフォルトはコミットしない。
- ユーザーが明示的に「コミットして」と指示した場合のみコミットする。
- コミット時は小さく・戻せる単位にする。

## 5) Logging (追加要件)
- 実装にはログ整備を併走させる（保守性・障害特定の強化）。
- 機密情報・トークン・個人情報はログ出力しない。

## 6) Work history (追加要件)
- 作業履歴は `docs/worklog/` に残す（このリポジトリ内で再現・検証できる状態にするため）。
- `docs/worklog/` に残す必須項目：変更内容、実行コマンド、テスト結果、判断理由（要点）、次アクション、ロールバック方針、`Execution-Tool`、`Execution-Agent`、`Execution-Model`、`Used-Skills`、`Repo-Refs`、`Obsidian-Refs`、`Tags`。
- `docs/worklog/` に残す必須項目に `Report-Path` を含める。
- `docs/worklog/` には `Obsidian-Log`（作成したObsidianログのパス、または未実施理由）を必ず記載する。
- `Record Check` セクションで、`Report-Path` の実在確認・`Repo-Refs`/`Obsidian-Refs` の有無・`Obsidian-Log` 記載・`Execution-Tool`/`Execution-Agent`/`Execution-Model`・`Tags` を確認する。
- 詳細手順・長文の経験談・補足は `D:\Obsidian\Programming` に蓄積してよい（ただし参照情報として扱い、worklog には識別子を残す）。
  - Obsidianログは `D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md` を使用し、背景/経緯/実行プラン/TODO と `agent`/`model`/`tool` を必ず含める。
  - 同日上書きを避けるため `MascotDesktop_phaseNA_log_YYMMDD_HHMM.md` で新規作成する。
