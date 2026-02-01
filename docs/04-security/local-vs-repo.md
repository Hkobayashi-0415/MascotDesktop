# ローカルとリポジトリの境界

## レイヤ概要
- **レイヤ1 (GitHub管理対象)**: `workspace/` 配下のソース・スクリプト・テンプレ・ドキュメント。`data/templates/**` はここに含まれる。
- **レイヤ2 (ローカル専用・workspace内)**: Git除外の運用データ。例: `data/assets_user/**`, `logs/**`, `secrets/**`, `data/db/**`, `attic/**`, 実行生成物。誤コミット防止のため `.gitignore` で除外。
- **レイヤ3 (workspace外)**: `refs/` は sibling ディレクトリで参照専用かつ Git管理外。CocoroAI 等はブラックボックス観察のみ（逆アセンブル/改変禁止）。

## 方針
- 実アセット（MMD/PNG/音声）はすべて `data/assets_user/` に置き、Gitに含めない。テンプレやプレースホルダは `data/templates/assets/` に置きGit管理する。
- 秘密情報（APIキー/設定）やログは `secrets/` `logs/` に置きGit除外。必要ならサンプル/テンプレを templates/config に置く。
- 参照物（`refs/`）は常に読み取り専用として扱い、スクリプトで read-only 属性を付与する運用を推奨。

## 運用チェックポイント
- `git status` で `refs/` や `data/assets_user/` 配下が追跡されていないことを確認。
- テンプレ資産 (`data/templates/assets/**`, `data/templates/db/**`) が存在することを確認。
- 実行時の作業ディレクトリは必ず `workspace/` を基点とし、ASCII パスを推奨（非ASCIIは WARN のみ）。***
