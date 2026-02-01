# パッケージング ガイド（PACKAGING.md）

ローカル配布用のスタンドアロン実行環境を PyInstaller で作成する方法。

## 目次
- [概要](#概要)
- [エントリポイント](#エントリポイント)
- [同梱対象](#同梱対象)
- [除外対象](#除外対象)
- [ビルド手順](#ビルド手順)
- [実行方法](#実行方法)
- [制約と注意事項](#制約と注意事項)

---

## 概要

| 項目 | 値 |
|------|-----|
| ビルドツール | PyInstaller |
| ビルドモード | one-folder（推奨） |
| 対象 OS | Windows のみ |
| Python | 3.10+ |

one-folder モードは依存DLLを含む `dist/<app>/` フォルダを生成します。

---

## エントリポイント

現在の PoC では2つのサーバが存在します:

| サーバ | モジュール | ポート |
|--------|------------|--------|
| Core | `apps.core.http_server` | 8769 |
| Avatar | `apps.avatar.poc.poc_avatar_mmd_viewer` | 8770 |

> [!NOTE]
> 本番ビルドでは統合起動スクリプトを検討予定。現状は個別起動。

---

## 同梱対象

| 対象 | 理由 |
|------|------|
| `viewer/` | 静的ファイル（HTML/JS/CSS） |
| `data/templates/` | manifest テンプレート等 |
| `apps/` | Python ソースコード |

---

## 除外対象（ビルドに含めない）

| 対象 | 理由 |
|------|------|
| `secrets/` | API キー等（gitignored） |
| `data/assets_user/` | ユーザーアセット（gitignored） |
| `data/db/` | ランタイム DB（gitignored） |
| `logs/` | ログファイル（gitignored） |
| `.venv/` | 仮想環境（ビルド時のみ使用） |
| `refs/` | 参照アーティファクト（git外） |

---

## ビルド手順

### 1. 準備

```powershell
cd C:\dev\MascotDesktop\workspace
powershell -ExecutionPolicy Bypass -File scripts/setup/bootstrap.ps1
```

### 2. ビルド実行

```powershell
powershell -ExecutionPolicy Bypass -File scripts/build/package.ps1
```

### 3. 出力確認

```
workspace/
└── dist/
    └── mascot_avatar/
        ├── mascot_avatar.exe
        ├── viewer/
        ├── data/templates/
        └── (依存DLL群)
```

---

## 実行方法

```powershell
cd dist\mascot_avatar

# 基本起動（ブラウザ自動オープン + デフォルトモデル）
.\mascot_avatar.exe

# ブラウザを開かない
.\mascot_avatar.exe --no-browser

# 特定モデルを指定
.\mascot_avatar.exe --model data/assets_user/characters/my_char/mmd/model.pmx

# slugで指定（data/assets_user/characters/<slug>/mmd/model.pmx）
.\mascot_avatar.exe --slug kanata_official_v1

# ポート変更
.\mascot_avatar.exe --port 8780
```

### CLIオプション

| オプション | デフォルト | 説明 |
|------------|------------|------|
| `--open-viewer` | True | ブラウザで viewer を自動オープン |
| `--no-browser` | - | ブラウザを開かない |
| `--model <path>` | - | PMXファイルパス（workspace相対） |
| `--slug <slug>` | - | キャラクタースラッグ |
| `--port <port>` | 8770 | HTTPサーバポート |

---

## 制約と注意事項

| 制約 | 説明 |
|------|------|
| **ASCIIパス必須** | ビルド/実行パスにASCII以外があると失敗する可能性あり |
| **アセット非同梱** | PMX/VMD/texture は `data/assets_user/` にユーザーが配置 |
| **ネイティブ依存** | TTS/STT 連携時は別途 DLL が必要な場合あり |
| **署名なし** | Windows Defender 警告が出る可能性あり |

---

## ビルド失敗時の典型原因

| 症状 | 原因 | 対処 |
|------|------|------|
| `ModuleNotFoundError: tkinter` | tkinterが除外されている | `mascot.spec` の `excludes` から `tkinter` を削除 |
| `ModuleNotFoundError: logging.handlers` | hiddenimports不足 | `mascot.spec` の `hiddenimports` に追加 |
| 非ASCII文字関連エラー | パスに日本語等が含まれる | ASCIIパスへ移動（docs/PATHS.md参照） |
| pyinstaller not found | venvにインストールされていない | `pip install pyinstaller` を実行 |
| `failed to execute script` | ランタイム依存不足 | `warn-mascot.txt` を確認 |

---

## 関連ドキュメント
- [PATHS.md](PATHS.md) — ASCIIパス移行ガイド
- [ASSETS_PLACEMENT.md](ASSETS_PLACEMENT.md) — アセット配置ガイド
- [README.md](../README.md) — Quick Start

