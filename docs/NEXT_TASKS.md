# NEXT_TASKS

## P0 (Most urgent) — Done
- [x] Manifest slot demo (repeatable)
  - DoD: Manifest placed -> run -> slot playback is reproducible by a third party. ✓
  - DoD: Missing slot returns `error_code` + log + on-screen error. ✓
  - See `docs/MOTIONS_DEMO.md` for the "3分で再現" tutorial.
- [x] Smoke test for repeated switching (50-200 iterations)
  - DoD: Finishes with exit code 0 or logs a clear failure reason. ✓
  - Verified: loops=50 (0 failures), loops=100 (0 failures)
- [x] `run.ps1 -Mode single` option
  - DoD: Single console mode works and logs are readable. ✓
  - Ctrl+C or Enter cleanly stops child processes.

## P1 — Done
- [x] Expand manifest guidance (slots/priority/fallback examples)
  - Created `docs/MANIFEST.md` with slot resolution, error codes, troubleshooting
- [x] Smoke endurance enhancements
  - Added `--report-json`, `--random`, `--jitter-sec` to `smoke_avatar.py`
- [x] Seam diagnostics UI simplification (no reload required)
  - Added localStorage persistence for diag preset and seamFix params
  - Added UI controls for bias/alphaTest/premul/aniso
  - Active button highlighting

## P2 — Done ✅
- [x] ASCII path migration guidance (stronger doc cues)
  - Created `docs/PATHS.md`: ASCII推奨パス、移行手順、OneDrive回避
  - Updated README/run.ps1 with links to PATHS.md
- [x] Packaging investigation (pyinstaller) for local-only distribution
  - Created `docs/PACKAGING.md`: エントリポイント、同梱対象、制約
  - Created `scripts/build/package.ps1` + `mascot.spec`
  - **exe起動成功**: `dist/mascot_avatar/mascot_avatar.exe` が正常動作
  - 制約: Avatarサーバのみ、アセット非同梱
- [x] Repo-root motion file cleanup
  - Created `scripts/setup/migrate_repo_root_assets.ps1`
  - Updated root `.gitignore` with re-commit prevention rules
  - Created `docs/ASSETS_PLACEMENT.md`

## P3 — Done ✅
- [x] P3-0: Repo Root 最終クリーンアップ
  - repo-rootの全アセットを `refs/assets_inbox/` へ移動
  - README規約と実体が完全一致
- [x] P3-1: Packaging UX 改善（Avatar exe）
  - CLIオプション追加: `--open-viewer`, `--no-browser`, `--model`, `--slug`, `--port`
  - 起動時ブラウザ自動オープン（デフォルト）
  - アセット未配置時ガイド表示
- [x] P3-2: 統合ランチャーの器
  - `apps/shell/launcher.py` 新規作成
  - 停止時の子プロセス確実終了
  - logs/へのログ集約
- ドキュメント更新: PACKAGING.md（CLIオプション、ビルド失敗時典型原因）

## P4 — Done ✅ (常駐体験)
- [x] P4-0: トレイ常駐ホスト
  - `apps/shell/tray_host.py` — pystray + pywebview
  - トレイメニュー: Show/Hide, Reload, Open Logs, Exit
  - 単一インスタンス制御、子プロセス確実終了
- [x] P4-1: Viewer埋め込み
  - `--open-viewer` デフォルト OFF
  - `--headless` モード追加
  - pywebview で WebView2 埋め込み
- [x] P4-2: コンソール窓を出さない
  - `tray_host.spec` (console=False)
  - 子プロセス STARTF_USESHOWWINDOW
- ドキュメント: `docs/RESIDENT_MODE.md`

## P5 — In Progress (キャラクター管理)
- [x] P5-0: Character Registry (read-only)
  - `scripts/setup/list_characters.py` で slug 一覧・モード検出・必須ファイル確認
  - `--json` オプションでJSON出力対応
  - 標準ライブラリのみ使用、新規依存なし
- [ ] P5-1: Tray menu でキャラクター切替UI
- [ ] P5-2: ホットリロード（実行中のキャラクター変更）

## P6 — Future (候補)
- [ ] Core統合（LLM/TTS/STT連携）
- [ ] 状態遷移（sleep/smile等）
- [ ] 音声クリップ/TTS連携
- [ ] P4-3: マスコット用ウインドウ（透過・クリック透過）

## 既知の課題（モデル関連）
> **注意**: 以下はPMXモデル自体の問題であり、viewer実装では解決困難。PMXEditorでの編集が必要。

- [ ] **SakamataChloe_v1: 顔に表情/マスクレイヤー表示**
  - 症状: 顔部分に半透明の表情パーツ（頬赤み等）がデフォルトで表示
  - 原因: PMXファイル内で表情モーフがデフォルトONで保存されている
  - 対策: PMXEditorで表情モーフを無効化して再保存
- [ ] **amane_kanata: モデル品質問題**
  - 既存の課題（詳細は別途）
