# パス設定ガイド（PATHS.md）

MascotDesktop は **ASCIIパス** での運用を前提としています。非ASCIIパス（日本語、OneDrive等）では警告が出ます。

## 目次
- [なぜASCIIパスが必要か](#なぜasciiパスが必要か)
- [推奨パス例](#推奨パス例)
- [非ASCII警告の意味](#非ascii警告の意味)
- [移行手順](#移行手順)
- [OneDrive の回避](#onedrive-の回避)

---

## なぜASCIIパスが必要か

| 問題 | 影響 |
|------|------|
| Python ライブラリの一部が非ASCII未対応 | ファイル読み込み失敗 |
| three.js/MMDLoader のパス処理 | テクスチャ読み込み失敗 |
| PyInstaller ビルド | パス解決エラー |
| ログ出力の文字化け | デバッグ困難 |

仕様書 §1 でも「日本語パス非対応リスクを回避（ASCII前提）」と明記されています。

---

## 推奨パス例

```
✅ 推奨
C:\dev\MascotDesktop\workspace
D:\projects\MascotDesktop\workspace

❌ 非推奨
C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop\workspace
C:\Users\山田\Documents\プロジェクト\MascotDesktop\workspace
```

**ポイント:**
- ドライブ直下または浅い階層
- フォルダ名は英数字のみ
- OneDrive 配下は避ける

---

## 非ASCII警告の意味

起動時に以下の警告が出る場合があります:

```
WARNING: Non-ASCII path detected (C:\Users\sugar\OneDrive\デスクトップ\...)
Recommend copying repo to ASCII path (e.g., C:\dev\MascotDesktop\workspace)
```

**この警告が出ても動作は継続します** が、以下の問題が発生する可能性があります:
- テクスチャ読み込み失敗
- モーション再生失敗
- PyInstaller ビルド失敗
- ログの文字化け

---

## 移行手順

### 方法1: フォルダコピー（推奨）

```powershell
# 1. 推奨パスにフォルダを作成
New-Item -ItemType Directory -Path "C:\dev" -Force

# 2. リポジトリ全体をコピー
Copy-Item -Path "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop" -Destination "C:\dev\MascotDesktop" -Recurse

# 3. 新しいパスで起動
cd C:\dev\MascotDesktop\workspace
powershell -ExecutionPolicy Bypass -File scripts/run.ps1
```

### 方法2: シンボリックリンク（上級者向け）

```powershell
# 管理者権限で実行
New-Item -ItemType SymbolicLink -Path "C:\dev\MascotDesktop" -Target "C:\Users\sugar\OneDrive\デスクトップ\MascotDesktop"
```

> [!WARNING]  
> シンボリックリンクは OneDrive との相性問題が発生することがあります。問題が起きた場合はコピー方式を使用してください。

---

## OneDrive の回避

OneDrive 配下の問題:
- 同期による遅延
- パスに日本語が含まれることが多い
- `.venv` や `node_modules` の同期競合

**推奨**: OneDrive 外のローカルドライブに配置

```
C:\dev\MascotDesktop\workspace   ← 開発用
C:\Users\...\OneDrive\...\       ← バックアップ用（必要に応じて手動コピー）
```

---

## 関連ドキュメント
- [README.md](../README.md) — Quick Start
- [ASSETS_PLACEMENT.md](ASSETS_PLACEMENT.md) — アセット配置ガイド
- [MANIFEST.md](MANIFEST.md) — モーション管理ガイド
