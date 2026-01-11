# Manifest Guide（モーション管理ガイド）

slot ベースのモーション再生を実現する manifest.json の仕様と運用方法。

## 目次
- [ディレクトリ構成](#ディレクトリ構成)
- [slot 解決の優先順位](#slot-解決の優先順位)
- [manifest.json 構造](#manifestjson-構造)
- [エラーコード一覧](#エラーコード一覧)
- [よくある失敗と対処](#よくある失敗と対処)

---

## ディレクトリ構成

```
data/assets_user/characters/<slug>/
├── mmd/
│   ├── model.pmx           # MMDモデル
│   ├── manifest.json       # モーション定義（ここを探索）
│   └── idle.vmd            # fallback モーション（manifest不在時に使用）
└── texture/
    └── *.png               # テクスチャファイル
```

**ポイント:**
- manifest.json は **model.pmx と同じディレクトリ** に配置
- モーション(vmd)のパスは **manifest.json からの相対パス** で記述
- texture/ は mmd/ と並列に配置（Viewer が自動解決）

---

## slot 解決の優先順位

`/avatar/play` で slot を指定したとき、以下の順で解決される:

| 順位 | 条件 | 動作 |
|------|------|------|
| 1 | manifest.json 存在 & slot 定義あり | slot の variants から weight で選択 |
| 2 | manifest.json 存在 & slot 未定義 | `MOTION_NOT_FOUND` エラー |
| 3 | manifest.json 不在 & slot="idle" | `idle.vmd` を model 隣接から探索 |
| 4 | manifest.json 不在 & slot≠"idle" | `MANIFEST_NOT_FOUND` エラー |
| 5 | idle.vmd も不在 | `MOTION_NOT_FOUND` エラー |

**idle は特別扱い**: manifest がなくても `idle.vmd` があれば自動適用される。

---

## manifest.json 構造

```json
{
  "version": "0.1.0",
  "avatar": {
    "model_path": "data/assets_user/characters/<slug>/mmd/model.pmx"
  },
  "motions": {
    "slots": {
      "idle": {
        "variants": [
          {
            "path": "../motions/idle.vmd",     // manifest からの相対パス
            "weight": 1,                        // 選択確率の重み
            "loop": true,                       // ループ再生
            "time_scale": 1.0,                  // 再生速度
            "root_lock": true,                  // ルート位置固定
            "crossfade_sec": 0.35,              // クロスフェード秒数
            "physics": false                    // 物理演算
          }
        ]
      },
      "speaking": {
        "variants": [
          { "path": "../motions/speaking_01.vmd", "weight": 1, "loop": true }
        ]
      }
    }
  }
}
```

### variants のパラメータ

| パラメータ | 型 | デフォルト | 説明 |
|------------|-----|-----------|------|
| `path` | string | 必須 | vmd ファイルへの相対パス |
| `weight` | number | 1 | 選択確率の重み（複数 variant 時） |
| `loop` | bool | true | ループ再生するか |
| `time_scale` | number | 1.0 | 再生速度（2.0=2倍速） |
| `root_lock` | bool | false | ルートボーン位置を固定 |
| `crossfade_sec` | number | 0.35 | モーション切替時のクロスフェード秒数 |
| `physics` | bool | false | 物理演算を有効化 |

---

## エラーコード一覧

| コード | 意味 | 対処 |
|--------|------|------|
| `MODEL_NOT_FOUND` | model_path が存在しない | パス確認、ファイル配置 |
| `MANIFEST_NOT_FOUND` | manifest.json が見つからない | model と同階層に manifest.json を配置 |
| `MOTION_NOT_FOUND` | slot に対応する vmd がない | manifest に slot を追加するか idle.vmd を配置 |
| `AVATAR.LOAD.MISSING_MODEL` | model_path が未指定 | リクエストに model_path を含める |
| `AVATAR.PLAY.MISSING_PARAM` | slot も motion_path も未指定 | どちらかを指定 |
| `AVATAR.PLAY.NO_MODEL` | モデル未ロードで play | 先に /avatar/load を呼ぶ |

---

## よくある失敗と対処

### 1. 非ASCIIパス警告
```
WARNING: Non-ASCII path detected (C:\Users\sugar\OneDrive\デスクトップ\...)
```
**対処**: ASCII パス（例: `C:\dev\MascotDesktop\workspace`）にリポジトリをコピー

### 2. ファイル未配置
```json
{"error_code": "MODEL_NOT_FOUND", "message": "model file not found"}
```
**対処**: `data/assets_user/characters/<slug>/mmd/model.pmx` が存在するか確認

### 3. slot 未定義
```json
{"error_code": "MOTION_NOT_FOUND", "message": "motion not found for slot 'speaking'"}
```
**対処**: manifest.json に `speaking` slot を追加するか、定義済み slot を使用

### 4. 相対パス誤り
```json
{"error_code": "MOTION_NOT_FOUND"}
```
**対処**: manifest.json からの相対パスを確認（例: `../motions/idle.vmd`）

---

## 関連ドキュメント
- [MOTIONS_DEMO.md](MOTIONS_DEMO.md) — 3分で再現チュートリアル
- [VIEWER_SEAMS.md](VIEWER_SEAMS.md) — 縫い目診断ガイド（存在する場合）
