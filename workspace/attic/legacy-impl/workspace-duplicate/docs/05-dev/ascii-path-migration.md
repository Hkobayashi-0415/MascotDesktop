# ASCII Path Migration (optional)

## リスク
- OneDrive/日本語パスはツールやPyInstaller/LLMで不具合を起こす可能性あり。

## 推奨先
- `C:\dev\CocoroMascotDesktop\` に丸ごとコピーし、以後はASCII側で開発。

## 手順
1. `C:\Users\...\デスクトップ\MascotDesktop` を `C:\dev\CocoroMascotDesktop` にコピー（元は残す）。
2. 新側の `workspace/` で git 運用（必要なら `git remote` 設定を移植）。
3. refs/ もコピー（read-only運用を維持）。scripts/setup/make_refs_readonly.ps1 を新側でも実行可。
4. 古いパスを参照する設定/ショートカットを更新。

## 注意
- 実行はユーザー判断。元の環境は保持し、破壊的操作はしない。
