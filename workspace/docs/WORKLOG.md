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
