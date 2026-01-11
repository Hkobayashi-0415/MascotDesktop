# 2025-12-30 add MMD PoC and path guard

## 変更点
- PoCエントリを apps/*/poc に整理（core/shell）。
- ASCIIパス警告を Core/Shell 起動時に追加。
- Avatar Mode1 (MMD) Viewer PoC を別プロセスとして追加（HTTP /avatar/health, /avatar/load）。
- MMDアセットテンプレ `data/templates/assets/mmd_mode1/README.md` を追加。
- DTOサンプルに AvatarLoadModelRequest / AvatarPlayMotionRequest / AvatarHealthCheck を追加。
- 仕様書に Mode1 方針とアセット規約を追記。

## 影響範囲
- 実行パスが非ASCIIの場合、WARNが出る（動作は継続）。
- PoC実行パス変更: `python apps/core/poc/poc_core_http.py`, `python apps/shell/poc/poc_shell.py`, `python apps/avatar/poc/poc_avatar_mmd_viewer.py`
- MMD Viewer は表示のみのプレースホルダ。実レンダリングは後続タスク。

## TODO
- MMD Loader (Three.js) の実装とローカル依存バンドル。
- Shell から Avatar Viewer への IPC 統合（AvatarLoad/SetState）。
- ASCIIパス移行を自動/半自動化（スクリプト）。
