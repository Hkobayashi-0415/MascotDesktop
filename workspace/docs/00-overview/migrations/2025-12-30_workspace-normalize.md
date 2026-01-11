# Workspace normalization (2025-12-30)

## 背景
- Gitルート `workspace/` の配下に旧構造 `workspace/workspace` が残存し、二重化による混乱リスクがあった。

## 対応
- 旧 `workspace/workspace` を `attic/legacy-impl/workspace-duplicate` へ退避（履歴参照用）。
- 正のソースを `workspace/` 直下に統一（apps/docs/spec/data/scripts/tests 等）。
- refs はリポジトリ外（または .gitignore）運用を維持。

## 今後
- ASCIIパス側（例: C:\dev\MascotDesktop\workspace）にコピーして開発することを推奨。
- 退避ディレクトリは参照のみ。再利用が不要になったら削除を検討。*** End Patch
