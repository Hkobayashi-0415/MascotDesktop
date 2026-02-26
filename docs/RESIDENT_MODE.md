# 常駐モード ガイド（RESIDENT_MODE.md）

- Status: legacy-reference
- Last Updated: 2026-02-25
- Scope: 旧PoC常駐（tray host）向けの参照情報。現行 Unity Runtime の常駐操作は対象外。

> [!IMPORTANT]
> この文書は旧PoC手順の参照です。現行 Unity Runtime では `docs/05-dev/QUICKSTART.md` と
> `docs/05-dev/unity-runtime-manual-check.md` の手順を優先してください。

MascotDesktopをトレイ常駐で運用する方法。

## Unity Scope（Current）

- `Hide/Show` は現仕様で最小化/復帰（`SW_MINIMIZE` / `SW_RESTORE`）。
- `Toggle Topmost` / `Hide/Show` / `Drag Window` は Windows Standalone Player でネイティブ効果を確認する。
- 受入時は runtime ログで以下イベントを確認する:
  - `window.resident.hidden`
  - `window.resident.restored`
  - `window.topmost.changed`
- 詳細は `docs/05-dev/QUICKSTART.md` / `docs/05-dev/unity-runtime-manual-check.md` を参照。

## 概要

常駐モードでは：
- **トレイアイコン**からアバターを制御
- **アバター窓1枚**だけが表示される（ブラウザ/コンソール不要）
- 起動時に自動でアバターを表示

---

## 起動方法

### 開発時（Pythonから）
```powershell
cd workspace
python apps/shell/tray_host.py
```

### ビルド後（exe）
```powershell
cd dist\mascot_tray
.\mascot_tray.exe
```

---

## トレイメニュー

| 項目 | 説明 |
|------|------|
| Show Avatar | アバターウインドウを表示 |
| Hide Avatar | アバターウインドウを非表示 |
| Reload | モデルを再読み込み |
| Open Logs | ログフォルダを開く |
| Exit | 完全終了 |

---

## デバッグ時のブラウザ起動

常駐モードではブラウザは開きません。デバッグ時にブラウザを使用する場合：

```powershell
# Avatar単体起動（ブラウザ自動オープン）
python apps/avatar/poc/poc_avatar_mmd_viewer.py --open-viewer

# または手動で開く
start http://127.0.0.1:8770/viewer
```

---

## ログ

ログは `logs/` に出力されます：
- `logs/tray_host.log` — トレイホストのログ
- `logs/avatar_tray.log` — Avatarサーバのログ

---

## 既知の制約

| 項目 | 説明 |
|------|------|
| 単一インスタンス | 多重起動はできません |
| WebView2必須 | Windows 10/11で自動インストール済み |
| 非ASCIIパス | 警告が出ますが動作継続 |

---

## 関連ドキュメント
- [PACKAGING.md](PACKAGING.md) — ビルド手順
- [ASSETS_PLACEMENT.md](ASSETS_PLACEMENT.md) — アセット配置
- [docs/05-dev/QUICKSTART.md](05-dev/QUICKSTART.md) — 現行 Unity Runtime 起動導線
- [docs/05-dev/unity-runtime-manual-check.md](05-dev/unity-runtime-manual-check.md) — 現行 Unity Runtime 手動確認
