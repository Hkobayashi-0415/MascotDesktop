# WORKLOG

## 2025-12-31 G0 (setup)
- git が実行環境で認識されずブランチ作成不可のため、以降は作業ディレクトリに直接反映する前提で進行する。
- 目的: MMD Viewer/Motion MVP に向けたゲート進行の下準備。
- ログ方針（avatar系）: `logs/avatar/*.log` に load/play/state/例外を出す。request_id を必須とし、error時は error_code を付与。
- 現状メモ: /viewer は three.js CDN + MMDLoader で pmx を表示中。モーション未再生。起動は core/shell/avatar を手動起動。非ASCIIパス警告あり。

## 2025-12-31 G1 (viewer静的化 + state下地)
- viewer を静的ファイル化（viewer/index.html, viewer.js, styles.css）。three.js CDN + MMDLoader + MMDAnimationHelper で pmx を読み込む。
- /viewer は workspace/viewer 配下を配信、/viewer/state を追加（model_path/current_motion_path 返却）。
- /avatar/load は state を更新し idle.vmd を自動検出（motions/idle.vmd など）。/avatar/play は state に motion/slot をセットする簡易実装。
- inline VIEWER_HTML を廃止し static 配信に一本化。非ASCIIパス警告は維持。

## 2025-12-31 Ops (起動導線・manifest・Docs)
- README に最短起動（bootstrap→run）と縫い目注意の導線を追加。QUICKSTART/NEXT_TASKS を用意。
- `manifest.sample.json` で slot/variants テンプレを配置済み（バイナリなしで運用方針を明確化）。
- 起動スクリプトの下地を整備予定（bootstrap.ps1 / run.ps1）。非ASCIIパス警告は継続。

## 2025-12-31 Ops-2 (bootstrap/run 実装)
- `scripts/setup/bootstrap.ps1` を追加し、venv作成と requirements インストールを自動化。非ASCIIパスを警告。
- `scripts/run.ps1` を追加し、core/avatars を起動→health待機→viewerをブラウザで開くフローをワンアクション化。PIDs を表示。
- git 未使用環境のため手動反映前提。バイナリ（pmx/vmd/texture）は引き続き git へ含めない。

## 2026-01-01 Ops-3 (PS5.1互換・導線整理)
- `bootstrap.ps1` / `run.ps1` を PowerShell 5.1 互換に調整し、python/py 検出・venv作成・health待ち・viewer自動オープンを安定化。非ASCIIパス警告は継続。
- `run.ps1` はデフォルトsplitで別コンソール起動し、PIDs表示。Docsに最短起動導線を追記し、QUICKSTART/NEXT_TASKS を追加。縫い目は素材側課題として `VIEWER_SEAMS.md` へ誘導。
- `data/templates/motions/manifest.sample.json` を配置済み（バイナリ非同梱）。refs は無変更。

## 2026-01-01 P0-1 (manifest demo)
- Added `docs/MOTIONS_DEMO.md` with a reproducible slot-based demo workflow.
- Added `data/templates/motions/manifest.demo.json` as a concrete template.
- No binary assets were added; `data/assets_user` remains git-ignored.

## 2026-01-02 P0-1 (manifest demo doc refresh)
- Updated `docs/MOTIONS_DEMO.md` to include workspace-root step and corrected copy command paths.
- Added `docs/NEXT_TASKS.md` to track P0/P1/P2 backlog.
