# Worklog: PMXテクスチャ問題包括調査

- Date: 2026-02-09
- Task: PMXモデルテクスチャの白っぽい表示問題の包括調査・対策立案
- Execution-Tool: Antigravity
- Execution-Agent: Gemini CLI Agent
- Execution-Model: gemini-2.5-pro
- Used-Skills: web-search
- Repo-Refs: `Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs`, `Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs`, `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-09_pmx-texture-comprehensive-review_1830_report.md
- Obsidian-Log: 未実施:調査段階のため
- Tags: [agent/gemini-cli, model/gemini-2.5-pro, tool/antigravity]

## Summary
PMXモデルの顔・服・肌が白っぽく表示される問題について包括的に調査。テクスチャディレクトリの重複、TGAローダーの動作、カラースペース設定など複数の観点から分析を実施。

## 発見した問題点

### 1. テクスチャディレクトリ重複（確定）
- `mmd/texture/` と `texture/` の2箇所にテクスチャが存在
- システムは `mmd/texture/` を優先して読み込む
- PNGフォールバック用のファイルは `texture/` にあるため機能しない

### 2. TGAファイルの最初のピクセル
- 全てのTGAファイルで最初のピクセルがグレー(R=0.5, G=0.5, B=0.5)
- 32-bit/24-bit 両方で同じ現象
- これはTGAファイル自体の内容か、ローダーの問題か要確認

### 3. カラースペース設定（未確認）
- Unity Project Settings のカラースペース（Linear/Gamma）未確認
- シェーダーのsRGB設定も要確認

## WEB調査結果
- TGAのBGRA順序問題はコードで対応済み
- カラースペース(Linear vs Gamma)の不一致が原因の可能性あり
- シェーダーのEmission設定も確認推奨

## Commands
```powershell
# Python TGA→PNG変換
python -c "from PIL import Image; ..."

# TGAバックアップ
Move-Item -Path "...TEX_PPT_Face.tga" -Destination "...TEX_PPT_Face.tga.bak"
```

## Tests
- Play Mode実行で視覚確認
- ログ出力の分析

## Rationale (Key Points)
1. ログ分析により `mmd/texture/` から読み込まれていることを特定
2. WEB調査でカラースペース問題の可能性を発見
3. 全TGAの最初のピクセルがグレー = テクスチャデータ自体の確認が必要

## Rollback
- バックアップファイル（`.bak`）から元に戻す
- シェーダー変更は git reset で巻き戻し可能

## Record Check
- Report-Path exists: False（作成予定）
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): Yes（未実施理由記載）
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
1. Unity Color Space設定の確認
2. `mmd/texture/` のファイルを整理（削除またはバックアップ）
3. TGAファイルの原データ確認（画像ビューアで開く）
4. カラースペース修正後の再テスト
5. 必要に応じて全TGAをPNGに変換
