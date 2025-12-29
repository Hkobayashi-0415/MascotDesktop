# Dev Status (2025-12-30)

## 現状OK
- Core/Shell PoC: HTTP (health/config/chat) + フレームレス/ドラッグ/Topmost/位置保存は稼働
- ログ基盤: JSONL, component/feature単位, request_id相関あり
- DTO/IPC: 基本セット（config/chat/health）あり、error_code方式で統一

## 現状NG / リスク
- 開発パスが OneDrive + 日本語パスで、ASCII前提と矛盾（Tk/pywebview/三次元表示で不安定化リスク）
- Avatar Mode1 (MMD) 未着手 → 最優先の縦切りが必要
- PoCエントリが apps/* 配下に散在していた（今回 apps/*/poc へ整理済み）

## 次の縦切り前提（MMD PoC）
- ASCIIパス側にコピーして実行（例: C:\dev\MascotDesktop\workspace）
- MMDアセットは data/templates/assets/mmd_mode1 を参照し、実体は data/assets_user/ 以下に配置（Git除外）
- Avatar Viewer は別プロセスで立ち上げ、Coreからの簡易IPC（AvatarLoad/SetState）を受ける形で最小表示を優先
