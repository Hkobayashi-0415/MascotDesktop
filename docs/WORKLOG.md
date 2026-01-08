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
- Added `data/templates/motions/manifest.demo.json` as a concrete template。
- No binary assets were added; `data/assets_user` remains git-ignored.

## 2026-01-02 P0-1 (manifest demo doc refresh)
- Updated `docs/MOTIONS_DEMO.md` to include workspace-root step and corrected copy command paths.
- Added `docs/NEXT_TASKS.md` to track P0/P1/P2 backlog.

## 2026-01-06 P0 (manifest demo + smoke + single-console)
- Rewrote `docs/MOTIONS_DEMO.md` as "3分で再現" quick tutorial; added manifest路ルール table and error code list.
- Added link from `README.md` to `docs/MOTIONS_DEMO.md` for discoverability.
- Created `scripts/dev/smoke_avatar.py`: HTTP-based slot switching smoke test (loops, delay, exit code).
- Improved `run.ps1 -Mode single`: try-finally cleanup ensures child processes stop on Ctrl+C or Enter.
- Updated `NEXT_TASKS.md` P0 items to Done and added P2 note for repo-root motion cleanup.
- **Verified**: smoke_avatar.py 5 loops × idle slot → 0 failures. Fixed Python syntax error (global declaration).

## 2026-01-06 P0 Endurance Test (DoD補強)
- **loops=50**: `smoke_avatar.py --model "data/assets_user/characters/kanata_official_v1/mmd/model.pmx" --slots idle --loops 50 --delay-sec 0.3`
  - Result: **50 plays, 0 failures** ✅
- **loops=100**: `smoke_avatar.py --model "..." --slots idle --loops 100 --delay-sec 0.3`
  - Result: **100 plays, 0 failures** ✅
- 注: speaking slot は manifest 未定義のためテスト対象外（idle のみ）

## 2026-01-06 P1 (manifest guide + smoke enhance + viewer UI)
- Created `docs/MANIFEST.md`: slot resolution priority, manifest structure, error codes, troubleshooting guide.
- Added link from `README.md` to `docs/MANIFEST.md`.
- Enhanced `smoke_avatar.py`: added `--report-json`, `--random`, `--jitter-sec` options; detailed failure reporting.
- Improved Viewer UI:
  - localStorage persistence for diag preset and seamFix params (`mmdviewer_diag`, `mmdviewer_seamfix`).
  - Added UI controls for bias/alphaTest/premul/aniso in `index.html`.
  - Active button highlighting for diag presets.
  - Updated `styles.css` for new UI elements.

## 2026-01-07 P1 Verification (smoke強化 検証)
- **--report-json テスト (20 loops)**: 
  - Command: `smoke_avatar.py --model "..." --slots idle --loops 20 --report-json logs/smoke_report.json`
  - Result: **20 plays, 0 failures** ✅, レポート出力成功
- **--random + --jitter-sec テスト (10 loops)**:
  - Command: `smoke_avatar.py --model "..." --slots idle --loops 10 --random --jitter-sec 0.2`
  - Result: **10 plays, 0 failures** ✅
- P1 完全検証完了

## 2026-01-07 P2 仕様確認サマリ
### 参照した spec
- `workspace/spec/latest/spec.md` (6839 bytes)
- `spec/latest/spec.md` (5567 bytes, 旧版)

### P2 関連の仕様要点
| セクション | 内容 |
|------------|------|
| §1 背景・目的 | 「日本語パス非対応リスクを回避（ASCII前提）」 |
| §7 アバター仕様 | 「アセットは ASCII パスの assets_user 側に配置」 |
| §11 データ配置 | 「Gitルート: workspace。refsは外・read-only」 |

### 現状の衝突（README方針「rootにはworkspace/refs以外置かない」と矛盾）
| 問題 | 対象 |
|------|------|
| repo-root に .vmd/.fbx | `きゅーぴっど。モーションデータ.vmd/fbx` |
| repo-root にモーションフォルダ | `Eyedart_Breath_motion_v1.1`, `MMO用待機モーションセット`, `天音かなた公式mmd_ver1.0` |
| .gitignore 未対応 | root への .vmd/.fbx 再混入防止ルールなし |

### P2 解決方針
1. **退避スクリプト**: 対象を `../refs/assets_inbox/` へ移動（git外）
2. **git rm**: Git管理から削除
3. **.gitignore 更新**: `/*.vmd`, `/*.fbx`, モーションフォルダ名を追加
4. **docs 整備**: PATHS.md, PACKAGING.md, ASSETS_PLACEMENT.md を新設

## 2026-01-07 P2 Implementation
### P2-1: ASCIIパス移行ガイド強化
- Created `docs/PATHS.md`: ASCII推奨パス例、OneDrive回避、移行手順、警告の意味
- Updated `README.md`: PATHS.mdへのリンク追加（強く推奨に変更）
- Updated `run.ps1`: 非ASCII警告にPATHS.mdリンク追加

### P2-2: Packaging調査（PyInstaller）
- Created `docs/PACKAGING.md`: エントリポイント、同梱/除外対象、ビルド手順、制約
- Created `scripts/build/package.ps1`: venv内pyinstallerインストール、specビルド
- Created `mascot.spec`: Avatar PoC one-folder ビルド設定
- Updated `workspace/.gitignore`: `!scripts/build/` 追加

### P2-3: Repo-root motion cleanup
- Created `scripts/setup/migrate_repo_root_assets.ps1`: 退避スクリプト（Copy/Move対応）
- Updated `(root)/.gitignore`: 再混入防止ルール追加（`/*.vmd`, `/*.fbx`, モーションフォルダ）
- Created `docs/ASSETS_PLACEMENT.md`: 配置ルール、構成、移行手順

### 残作業（ユーザー実行待ち）
- [ ] `migrate_repo_root_assets.ps1` 実行でファイル退避
- [ ] `git rm` で対象ファイルをGit管理から削除
- [ ] `git commit` でクリーンアップ完了

## 2026-01-07 P2 Verification
### 退避スクリプト検証
- `migrate_repo_root_assets.ps1` 実行成功
- Found: 1 (`Eyedart_Breath_motion_v1.1`)、Migrated: 1
- 他のファイル（.vmd/.fbx等）は既に削除済み or 別の場所

### PyInstallerビルド検証
- `package.ps1` 実行成功 ✅
- pyinstaller 6.17.0 インストール・ビルド完了
- **出力**: `dist/mascot_avatar/mascot_avatar.exe`
- 非ASCIIパスでも正常動作（警告のみ）
- スクリプト修正: ErrorActionPreference を一時的に SilentlyContinue に変更
- spec修正: `hiddenimports` に `logging.handlers`, `tkinter` を追加
- **exe起動成功**: Avatar Mode1 (MMD) Viewer - PoC ウィンドウが正常表示 ✅

### ビルド成果物の制約（明文化）
| 項目 | 説明 |
|------|------|
| Avatarサーバのみ | Coreサーバは別モジュール、含まれない |
| ブラウザ自動オープンなし | 手動で `http://127.0.0.1:8770/viewer` を開く |
| アセット非同梱 | `data/assets_user/` はユーザーが配置 |
| モデル自動ロードなし | `/avatar/load` API を手動で呼ぶ必要あり |

→ 本番ビルド（Core統合、自動起動）は次フェーズの課題

## 2026-01-07 P2 Complete ✅
- P2-1: ASCIIパス移行ガイド（PATHS.md）完了
- P2-2: Packaging調査（package.ps1 + mascot.spec）完了、exe起動確認済み
- P2-3: Repo-root cleanup（退避スクリプト + .gitignore更新）完了、git rm待ち

## 2026-01-08 P3 Implementation
### P3-0: Repo Root 最終クリーンアップ
- repo-rootの全アセット（5アイテム）を `refs/assets_inbox/` へ移動
  - きゅーぴっど。モーションデータ.vmd/fbx
  - Eyedart_Breath_motion_v1.1/
  - MMO用待機モーションセット/
  - 天音かなた公式mmd_ver1.0/
- README規約「NEW_ROOTにはworkspaceとrefsのみ」と実体が一致

### P3-1: Packaging UX 改善（Avatar exe）
- `poc_avatar_mmd_viewer.py` にCLIオプション追加:
  - `--open-viewer` (デフォルトTrue): ブラウザ自動オープン
  - `--no-browser`: ブラウザを開かない
  - `--model <path>`: PMXファイルパス指定
  - `--slug <slug>`: キャラクタースラッグ指定
  - `--port <port>`: HTTPサーバポート指定
- アセット未配置時のガイド表示（docs/ASSETS_PLACEMENT.md誘導）
- `mascot.spec` に argparse, webbrowser を hiddenimports 追加

### P3-2: 統合ランチャーの器
- `apps/shell/launcher.py` 新規作成
  - 将来Core+Avatar統合起動のための構造
  - 現時点ではAvatar単体起動
  - 停止時の子プロセス確実終了（CTRL_BREAK_EVENT）
  - logs/へのログ集約

### ドキュメント更新
- `docs/PACKAGING.md`: CLIオプション、ビルド失敗時典型原因を追記

## 2026-01-08 P3 Verification ✅
### ビルド検証
- `package.ps1` 実行成功（OneDrive競合で一度リトライ）
- `dist/mascot_avatar/mascot_avatar.exe` 生成
- exe起動成功、ブラウザ自動オープン確認
- 非ASCIIパス警告表示（動作継続）

### DoD チェック結果
| 項目 | 結果 |
|------|------|
| P3-0: README規約と実体一致 | ✅ |
| P3-1: exe起動でブラウザ自動オープン | ✅ |
| P3-1: アセット未配置時ガイド表示 | ✅ |
| P3-2: launcher経由で起動・停止確実 | ✅ |

## 2026-01-08 P3 Complete ✅
- ブランチ: `feature/p3-packaging-ux`
- 次フェーズ候補（P4）: Core統合、キャラ切替、状態遷移、音声連携

## 2026-01-08 P4 Implementation: 常駐体験の実現
### P4-0: トレイ常駐ホスト
- `apps/shell/tray_host.py` 新規作成
  - pystray でトレイアイコン
  - pywebview で WebView2 ベース Viewer 埋め込み（枠なし）
  - 単一インスタンス制御（lockファイル）
  - 子プロセス確実終了（CTRL_BREAK_EVENT）

### P4-1: Viewer埋め込み
- `poc_avatar_mmd_viewer.py` 修正:
  - `--open-viewer` デフォルトを False に変更
  - `--headless` モード追加（Tkinter UI なし、HTTP サーバのみ）
- pywebview で viewer URL を内部ロード

### P4-2: コンソール窓を出さない
- `tray_host.spec` 新規作成（console=False）
- `mascot.spec` も console=False に変更
- 子プロセス起動時 STARTF_USESHOWWINDOW で非表示

### 新規ファイル
- `apps/shell/tray_host.py` — トレイ常駐ホスト
- `tray_host.spec` — PyInstaller spec（windowed）
- `requirements.txt` — 依存関係定義
- `docs/RESIDENT_MODE.md` — 常駐モードガイド
