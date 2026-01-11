# Motions Demo (Manifest + Slot) — 3分で再現

slot ベースのモーション再生を **3分で再現** する手順です。バイナリアセットは git 非同梱のため、ユーザー自身で配置してください。

## 前提
- `scripts/run.ps1` でサーバ起動済み（`http://127.0.0.1:8770`）
- モデル/モーション/manifest を `data/assets_user/` 配下に配置済み

## manifest 読み込みルール

| 項目 | ルール |
|------|--------|
| **探索パス** | モデル (`.pmx`) と同じディレクトリ内の `manifest.json` |
| **相対パス基準** | manifest ファイルのあるディレクトリ |
| **fallback** | manifest がない or slot が解決できない場合は `idle.vmd` を探索 |
| **エラー** | どちらも見つからない場合は `MOTION_NOT_FOUND` を返却 |

**例:**
```
data/assets_user/characters/miku/mmd/
  ├─ model.pmx          # モデルファイル
  ├─ manifest.json      # ← ここに配置
  └─ idle.vmd           # fallback モーション
```

## Quick Demo (PowerShell)

```powershell
# 1) workspace ルートへ移動
Set-Location C:\dev\MascotDesktop\workspace

# 2) テンプレートをコピー（<slug> は自分のキャラ名に置き換え）
Copy-Item .\data\templates\motions\manifest.demo.json `
          .\data\assets_user\characters\<slug>\mmd\manifest.json

# 3) manifest.json を編集し path を実際のファイルに合わせる
# (vmd は manifest.json からの相対パス)

# 4) モデルをロード
Invoke-RestMethod http://127.0.0.1:8770/avatar/load -Method Post `
  -Body '{"dto_version":"0.1.0","request_id":"req-load","model_path":"data/assets_user/characters/<slug>/mmd/model.pmx"}' `
  -ContentType 'application/json'

# 5) slot を指定して再生
Invoke-RestMethod http://127.0.0.1:8770/avatar/play -Method Post `
  -Body '{"dto_version":"0.1.0","request_id":"req-play","slot":"idle"}' `
  -ContentType 'application/json'

# 6) 状態を確認
Invoke-RestMethod http://127.0.0.1:8770/viewer/state
```

## 期待結果
- `/viewer/state` に `motion.motion_path` と `motion.slot` が返却される
- ブラウザ Viewer に `Loaded: <model> + <motion>` が表示される

## fallback（manifest なし）
manifest.json がない場合:
1. `idle.vmd` を model 隣接ディレクトリから探索
2. 見つかれば自動適用
3. 見つからなければ `error_code: MOTION_NOT_FOUND`

## エラーコード一覧
| コード | 意味 |
|--------|------|
| `MANIFEST_NOT_FOUND` | manifest が見つからない/読み取れない |
| `MOTION_NOT_FOUND` | slot からモーションが解決できず fallback も失敗 |
| `MODEL_NOT_FOUND` | model_path が存在しない/アクセス不可 |

## 注意
- `.pmx/.vmd/texture` はコミットしないでください（`data/assets_user` は gitignored）
- 非 ASCII パスでは警告が出ます。`C:\dev\MascotDesktop\workspace` のような ASCII パスを推奨
