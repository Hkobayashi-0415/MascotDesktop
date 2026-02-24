# Worklog: U7 テスト実施（4スイート）・SimpleModelBootstrapTests バグ修正

- Date: 2026-02-24
- Branch: feature/U7
- Execution-Tool: Claude Code
- Execution-Agent: claude-code
- Execution-Model: claude-sonnet-4-6
- Used-Skills: なし（直接実行）
- Repo-Refs: `tools/run_unity_tests.ps1`, `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs`
- Obsidian-Refs: なし
- Obsidian-Log: `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260224_2210.md`
- Report-Path: `docs/worklog/2026-02-24_test_execution_u7_four_suites.md`
- Tags: test, editmode, bugfix, u7, SimpleModelBootstrapTests, artifact

---

## 目的

U7 ブランチの変更（SimpleModelBootstrap リファクタリング・WindowController 修正等）に対して、
指定4スイートの EditMode テストを実行し、Pass/Fail を確認する。

## 実行コマンド

```powershell
cd D:\dev\MascotDesktop
$env:UNITY_EXE="C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
$env:UNITY_COM="C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"

./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.AssetCatalogServiceTests" -RequireArtifacts
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.WindowNativeGatewayTests" -RequireArtifacts
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.SimpleModelBootstrapTests" -RequireArtifacts  # 初回失敗→修正→再実行
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeLogTests" -RequireArtifacts
```

## テスト結果

| スイート | 結果 | Pass | Fail | artifact (xml) |
|---|---|---|---|---|
| AssetCatalogServiceTests | **Passed** | 4/4 | 0 | `editmode-20260224_220045.xml` |
| WindowNativeGatewayTests | **Passed** | 1/1 | 0 | `editmode-20260224_220544.xml` |
| SimpleModelBootstrapTests（初回） | **Failed** | 35/36 | 1 | `editmode-20260224_220843.xml` |
| SimpleModelBootstrapTests（修正後） | **Passed** | 36/36 | 0 | `editmode-20260224_221052.xml` |
| RuntimeLogTests | **Passed** | 9/9 | 0 | `editmode-20260224_221136.xml` |

**最終: 全スイート Passed（50/50）**

---

## バグ詳細: BuildRelativeAssetPathsFromRoots_MergesDistinctRelativePathsAcrossRoots

### 症状

```
System.IO.DirectoryNotFoundException: Could not find a part of the path
"C:\Users\sugar\AppData\Local\Temp\MascotDesktop_SceneCandidates_xxx\B\characters\demo\mmd\avatar.pmx"
```

`SimpleModelBootstrapTests.cs:88` で `File.WriteAllText` が失敗。

### 原因

テストセットアップで `rootB` のディレクトリ作成が不完全だった。

```csharp
// 修正前（rootB/mmd を作成していなかった）
Directory.CreateDirectory(Path.Combine(rootA, "characters", "demo", "mmd"));
Directory.CreateDirectory(Path.Combine(rootB, "characters", "demo", "images"));  // mmd なし
File.WriteAllText(Path.Combine(rootB, "characters", "demo", "mmd", "avatar.pmx"), "b"); // ← 失敗

// 修正後
Directory.CreateDirectory(Path.Combine(rootA, "characters", "demo", "mmd"));
Directory.CreateDirectory(Path.Combine(rootB, "characters", "demo", "mmd"));    // 追加
Directory.CreateDirectory(Path.Combine(rootB, "characters", "demo", "images"));
File.WriteAllText(Path.Combine(rootB, "characters", "demo", "mmd", "avatar.pmx"), "b"); // OK
```

### 変更ファイル

- `Unity_PJ/project/Assets/Tests/EditMode/SimpleModelBootstrapTests.cs` L85-86（1行追加）

---

## artifact 確認

```
Unity_PJ/artifacts/test-results/editmode-20260224_220045.xml  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_220045.log  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_220544.xml  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_220544.log  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_220843.xml  ✓ (初回 35/36 Failed)
Unity_PJ/artifacts/test-results/editmode-20260224_220843.log  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_221052.xml  ✓ (修正後 36/36 Passed)
Unity_PJ/artifacts/test-results/editmode-20260224_221052.log  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_221136.xml  ✓
Unity_PJ/artifacts/test-results/editmode-20260224_221136.log  ✓
```

---

## 次アクション

- [ ] テスト修正・artifact をコミット（ユーザー指示後）
- [ ] feature/U7 PR #5 への反映（push後）

## ロールバック方針

- テストコード変更（1行追加）のみ。プロダクションコードへの影響なし。
- 戻す場合は `Directory.CreateDirectory(Path.Combine(rootB, "characters", "demo", "mmd"));` の1行削除のみ。

---

## Record Check

- [x] Report-Path: `docs/worklog/2026-02-24_test_execution_u7_four_suites.md` — 存在確認済み
- [x] Repo-Refs: `tools/run_unity_tests.ps1`, `SimpleModelBootstrapTests.cs` を参照
- [x] Obsidian-Refs: なし（今回は外部参照なし）
- [x] Obsidian-Log: `D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260224_2210.md`
- [x] Execution-Tool: Claude Code
- [x] Execution-Agent: claude-code
- [x] Execution-Model: claude-sonnet-4-6
- [x] Tags: test, editmode, bugfix, u7, SimpleModelBootstrapTests, artifact
