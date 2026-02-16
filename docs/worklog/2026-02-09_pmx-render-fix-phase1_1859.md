# Worklog: PMX Render Fix Phase 1

- Date: 2026-02-09
- Task: PMX描画品質の主要因修正（テクスチャ縮小・シェーダーdebug残り・ログ過多）
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, code-review, worklog-update
- Report-Path: docs/reports/2026-02-09_pmx-render-fix-phase1.md
- Repo-Refs:
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs
  - Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Obsidian-Refs: n/a
- Obsidian-Log: 未実施（本リポジトリ内worklog/reportに集約して記録）
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, fix/pmx-render, unity/libmmd]

## 変更内容
- DefaultMaxTextureSize を 1024 -> 4096 に引き上げ。
- テクスチャ縮小時にアスペクト比を保持する計算へ変更。
- TGA読込でのPNG優先フォールバックを既定OFF化。
- PMDマテリアルシェーダーのtoon/sphere無効化を解除し、通常寄りの合成へ復帰。
- Texture/Material/TGA読込の詳細デバッグログを既定OFF化。
- 透明判定の全ピクセル走査結果をテクスチャ単位でキャッシュ。

## 実行コマンド
- Get-Content / Select-String による対象箇所の行確認
- Unity.exe -batchmode -runTests ...（EditMode）実行試行
- apply_patch による5ファイル修正

## テスト結果
- Unity EditMode テスト: 実行不可（環境要因）
  - 実行コマンド: C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe -batchmode -nographics -projectPath D:\dev\MascotDesktop\Unity_PJ\project -runTests -testPlatform EditMode ...
  - 結果: Program 'Unity.exe' failed to run ... 指定されたモジュールが見つかりません
- 代替として、変更行の静的整合性確認を実施。

## 判断理由（要点）
- 1024固定上限とdebugシェーダー残りが、現象（粗さ/白っぽさ）に対する直接因子。
- 24-bit TGA変換経路は既に成立していたため、今回は周辺要因（縮小・合成・ログ・コスト）を修正。

## 次アクション
1. Unity実行環境で同一モデルのBefore/After比較（スクリーンショット + ログ）
2. 必要なら DefaultMaxTextureSize をランタイム設定値へ移行
3. 透過判定キャッシュのヒット率確認（プロファイル）

## ロールバック方針
- 以下5ファイルを本修正前状態へ戻す。
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MmdGameObject.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/TargaImage.cs
  - Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc

## Record Check
- Report-Path exists: True
- Repo-Refs recorded: Yes
- Obsidian-Refs recorded (or n/a): Yes (n/a)
- Obsidian-Log recorded: Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags recorded: Yes
