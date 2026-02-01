# アセット配置ガイド（ASSETS_PLACEMENT.md）

MascotDesktop でのアセット（PMX/VMD/テクスチャ等）の正しい配置場所と理由。

## 目次
- [配置ルール](#配置ルール)
- [ディレクトリ構成](#ディレクトリ構成)
- [なぜ repo-root に置かないのか](#なぜ-repo-root-に置かないのか)
- [既存ファイルの移行](#既存ファイルの移行)

---

## 配置ルール

| アセット種別 | 配置場所 | Git管理 |
|--------------|----------|---------|
| ユーザーモデル（PMX/テクスチャ） | `workspace/data/assets_user/characters/<slug>/mmd/` | ❌ 除外 |
| ユーザーモーション（VMD） | `workspace/data/assets_user/characters/<slug>/motions/` | ❌ 除外 |
| テンプレート（manifest等） | `workspace/data/templates/motions/` | ✅ 含む |
| 参照アーティファクト | `refs/` (workspace外) | ❌ 外 |

---

## ディレクトリ構成

```
MascotDesktop/                     ← リポジトリルート
├── workspace/                     ← Gitルート
│   ├── data/
│   │   ├── assets_user/           ← ユーザーアセット（gitignored）
│   │   │   └── characters/
│   │   │       └── <slug>/
│   │   │           ├── mmd/
│   │   │           │   ├── model.pmx
│   │   │           │   ├── manifest.json
│   │   │           │   └── *.png (texture)
│   │   │           └── motions/
│   │   │               └── *.vmd
│   │   └── templates/             ← テンプレート（Git管理）
│   │       └── motions/
│   │           └── manifest.sample.json
│   └── ...
├── refs/                          ← 参照アーティファクト（Git外）
│   └── assets_inbox/              ← 誤コミット退避場所
└── .gitignore                     ← ルートの除外設定
```

---

## なぜ repo-root に置かないのか

### READMEの方針
> Do not place files at `<NEW_ROOT>/` except `workspace/` and `refs/`.

### 理由

| 問題 | 説明 |
|------|------|
| Git肥大化 | バイナリファイルは履歴が大きくなる |
| クローン遅延 | 不要なファイルがダウンロードされる |
| パス衝突 | 非ASCIIファイル名は処理系によって問題 |
| 管理混乱 | どこに何があるか分からなくなる |

---

## 既存ファイルの移行

repo-root に誤って配置されたファイルがある場合:

### 1. 退避スクリプトを実行

```powershell
cd workspace
powershell -ExecutionPolicy Bypass -File scripts/setup/migrate_repo_root_assets.ps1
```

これにより以下のファイル/フォルダが `../refs/assets_inbox/` へコピーされます:
- `きゅーぴっど。モーションデータ.vmd`
- `きゅーぴっど。モーションデータ.fbx`
- `Eyedart_Breath_motion_v1.1/`
- `MMO用待機モーションセット/`
- `天音かなた公式mmd_ver1.0/`

### 2. Git から削除

```powershell
cd ..  # MascotDesktop ルートへ
git rm -r "きゅーぴっど。モーションデータ.vmd"
git rm -r "きゅーぴっど。モーションデータ.fbx"
git rm -r "Eyedart_Breath_motion_v1.1"
git rm -r "MMO用待機モーションセット"
git rm -r "天音かなた公式mmd_ver1.0"
git commit -m "Remove motion files from repo root"
```

### 3. 使用する場合

退避したファイルを使いたい場合は:
```powershell
Copy-Item -Path "refs\assets_inbox\<folder>" -Destination "workspace\data\assets_user\characters\<slug>\mmd" -Recurse
```

---

## 関連ドキュメント
- [PATHS.md](PATHS.md) — ASCIIパス移行ガイド
- [MANIFEST.md](MANIFEST.md) — モーション管理ガイド
- [PACKAGING.md](PACKAGING.md) — パッケージングガイド
