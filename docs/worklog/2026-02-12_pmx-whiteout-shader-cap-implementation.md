# Worklog: pmx-whiteout-shader-cap-implementation

- Date: 2026-02-12
- Task: Shader側補正（specular/edge寄与の上限制御）の最小差分実装
- Execution-Tool: Codex
- Execution-Agent: codex
- Execution-Model: gpt-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs: AGENTS.md, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc, Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc, Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs, Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs, Unity_PJ/docs/05-dev/pmx-validation-procedure-and-record.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_obsidian_log_template.md, D:\dev\00_repository_templates\ai_playbook\skills\worklog-update\SKILL.md
- Report-Path: docs/worklog/2026-02-12_pmx-whiteout-shader-cap-implementation.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1809.md
- Tags: [agent/codex, model/gpt-5, tool/codex, pmx, whiteout, shader, specular, edge]

## Summary
MMD共通シェーダに specular/edge 寄与の上限制御を追加し、`MaterialLoader` からマテリアルごとにキャップ値を設定する最小差分を実装した。高 shininess 材質と低 edge alpha 材質でのみ強めに制御が効くようにし、既存の材質経路を崩さない方針を採った。

## Changes
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
  - `_MascotSpecularContributionCap` を追加。
  - specular加算係数 `0.65` を可変化し、cap値で上限制御。
- `Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
  - `_MascotEdgeContributionCap` を追加。
  - outline alpha を `min(_OutlineColor.a * _Opacity, cap)` で上限制御。
- `Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
  - 追加プロパティ名:
    - `_MascotSpecularContributionCap`
    - `_MascotEdgeContributionCap`
  - 動的キャップ決定ヘルパー追加:
    - `ResolveSpecularContributionCap(MmdMaterial)`
    - `ResolveEdgeContributionCap(MmdMaterial)`
  - 設定値:
    - specular: default `0.45`, high shininess(`>=32`) `0.32`
    - edge alpha: default `0.75`, low alpha(`<0.9999`) `0.55`
- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
  - `MaterialLoaderShaderCaps_UsesStrongerSpecularClampForHighShininess`
  - `MaterialLoaderShaderCaps_UsesStrongerEdgeClampWhenEdgeAlphaIsLow`
  - 反射ヘルパー:
    - `InvokeMaterialLoaderSpecularContributionCap`
    - `InvokeMaterialLoaderEdgeContributionCap`

## Commands
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialSurface.cginc`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Resources/Shaders/MeshPmdMaterialVertFrag.cginc`
- `Get-Content Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs`
- `Get-Content Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- `Set-Content docs/worklog/2026-02-12_pmx-whiteout-shader-cap-implementation.md`
- `Set-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1809.md`

## Tests
- 実行コマンド:
  - `./tools/run_unity_tests.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com" -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests"`
- 結果:
  - この環境では Unity.com 起動時に `指定されたモジュールが見つかりません` で失敗。
  - `editmode-20260212_180923.log/.xml` は未生成。
- 補足:
  - ユーザー環境の Unity.com 実行で再確認が必要。

## Rationale (Key Points)
- 既存比較で `shader_lighting_candidate` が白飛び系モデルに集中しており、specular/edge寄与を shader 側で制御するのが主線。
- 直接的な定数書き換えではなく、マテリアル条件（shininess / edge alpha）で軽く分岐させ、AZKi側の見え方劣化リスクを抑える。
- 共通 `.cginc` 変更 + `MaterialLoader` 注入で、MMD系シェーダ全種へ最小差分で反映できる。

## Next Actions
1. ユーザー環境で `SimpleModelBootstrapTests` を Unity.com 実行し、追加テストを含めて pass を確認する。
2. F0/F3・3モデル（kanata/AZKi/nene_BEA）を再採取し、`Record Sheet B/D` を補正前後で比較する。
3. AZKiの質感劣化が顕著なら、specular/edge cap値を段階的に緩和して再評価する。

## Rollback
1. `MeshPmdMaterialSurface.cginc` の `_MascotSpecularContributionCap` 関連行を削除し、specular係数を `0.65` 固定へ戻す。
2. `MeshPmdMaterialVertFrag.cginc` の `_MascotEdgeContributionCap` 関連行を削除し、outline alpha を `_OutlineColor.a * _Opacity` へ戻す。
3. `MaterialLoader.cs` の cap定数・cap設定・`Resolve*Cap` ヘルパーを削除する。
4. `SimpleModelBootstrapTests.cs` の追加テスト2件と反射ヘルパー2件を削除する。
5. `docs/worklog/2026-02-12_pmx-whiteout-shader-cap-implementation.md` を削除する。
6. `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260212_1809.md` を削除する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution-Tool / Execution-Agent / Execution-Model recorded: Yes
- Tags include agent/model/tool: Yes
