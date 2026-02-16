# Worklog: PMX Texture Loading Fix - 24-bit TGA BGR Conversion

- Date: 2026-02-09
- Task: PMXモデルのテクスチャ読み込み問題を修正（24-bit TGAのBGR→RGB変換）
- Execution-Tool: Antigravity
- Execution-Agent: antigravity
- Execution-Model: claude-sonnet-4-20250514
- Used-Skills: n/a
- Repo-Refs: Assets/LibMmd/Unity3D/Utils.cs, Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs, Assets/LibMmd/Unity3D/TextureLoader.cs
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-09_pmx-texture-24bit-tga-fix_1704_report.md
- Obsidian-Log: 未実施:PMX Loader統合作業の一部として後で記録予定
- Tags: [agent/antigravity, model/claude-sonnet-4-20250514, tool/antigravity]

## Summary

PMXモデル「天音かなた」のテクスチャが白く表示される問題を調査・修正しました。
主な原因は以下の2点：
1. 相対パス解決の問題（前回修正済み）
2. 24-bit TGAファイルのBGR→RGB変換が行われていなかった（本セッションで修正）

## Changes

### `Assets/LibMmd/Unity3D/Utils.cs`
- `Bgr24ToColors()` メソッドを追加
- 24-bit TGAファイル（BGR順）をUnity Color配列（RGB順）に変換

### `Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs`
- `LoadTGAImage()` メソッドを修正
- 24-bit TGA（RGB24フォーマット）も `SetPixels` で処理するように変更
- BGR→RGB変換を適用

## Commands

なし（Unityエディタで動作確認）

## Tests

- Playモードでモデル表示を確認
- 髪（青）、靴（黒）など一部のテクスチャは正しく表示
- 顔・服はまだ白っぽい（シェーダー/ライティングの問題の可能性）

## Rationale (Key Points)

- TGAファイルはピクセルデータをBGR順で保存
- 32-bit TGA: `Bgra32ToColors()` で変換済み
- 24-bit TGA: `LoadRawTextureData()` で直接読み込んでいたため、BGR順のまま
- 修正により24-bit TGAも正しくRGB変換

## Rollback

- `Utils.cs` から `Bgr24ToColors()` メソッドを削除
- `TargaImage.cs` の `LoadTGAImage()` から24-bit TGA用の条件分岐を削除

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): n/a
- Obsidian-Log recorded (path or reason): No (後で記録予定)
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions

1. シェーダー（MeshPmdMaterialSurface.cginc）の調査
2. ライティング設定の確認
3. 白っぽく表示される根本原因の特定と修正

---

## Update: 3Dモデル集約と再検証準備（2026-02-09）

### 実施内容

- リポジトリ実体を再走査し、`Unity_PJ/data/assets_user/characters` 配下に不足していたPMXソースをコピー集約。
- 追加コピー元:
  - `MMD/AZKi_4th (1)/AZKi_4th`
  - `MMD/SakamataChloe (1)/SakamataChloe`
  - `MMD/ナースたそ/ナースたそ`
  - `MMD/桃鈴ねね公式mmd_ver1.0/桃鈴ねね公式mmd_ver1.0`
  - `refs/assets_inbox/天音かなた公式mmd_ver1.0/天音かなた公式mmd_ver1.0`

### 集約後のPMX実体（`Unity_PJ/data/assets_user/characters`）

1. `amane_kanata_official_v1/mmd/PMX/天音かなた.pmx`
2. `amane_kanata_v1/mmd/天音かなた.pmx`
3. `azki_v1/mmd/AZKi_4th_src/AZKi_4th.pmx`
4. `momone_nene_official_v1/mmd/PMX/桃鈴ねね_BEA.pmx`
5. `momone_nene_official_v1/mmd/PMX/桃鈴ねね_STD.pmx`
6. `nurse_taso_v1/mmd/NurseTaso_src_mmd/NurseTaso.pmx`
7. `nurse_taso_v1/mmd/NurseTaso.pmx`
8. `SakamataChloe_v1/mmd/SakamataChloe_src/SakamataChloe.pmx`

- PMX件数確認結果: `8`

### 現在のテスト認識

- Runtime HUDの`Model: next / prev / reload / rescan`で候補切替を手動検証中。
- 「天音かなたPMXは正常表示と言い切れない」状態を継続課題として扱う。
- 画像候補が混在するため、3D確認の連続性が低下しやすい（3Dのみを順に確認しにくい）。

### 次の検証計画（直近）

1. 8件すべてのPMXで`loader_selected=Pmx`と`model.displayed=pmx`を取得。
2. 各PMXでF0〜F3を比較し、白飛び/黒つぶれ/材質破綻をスクリーンショットで記録。
3. 天音かなたの異常表示を「テクスチャ解決」「材質設定」「照明係数」の観点で切り分け。
4. `rescan`後の候補件数と実体件数（PMX=8）の整合性を再確認。
