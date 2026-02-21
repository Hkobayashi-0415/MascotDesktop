# U6 Regression Gate Operations

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-21
- Scope: U6（回帰品質ゲート運用）

## 根拠
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/05-dev/unity-test-result-collection-template.md`
- `tools/run_unity_tests.ps1`

## 1. 運用方針（U6）
- 主要4スイート（STT/TTS/LLM/Loopback）は `-RequireArtifacts` を必須で実行する。
- 起動前失敗時は exit code だけでなく、artifact（xml/log）有無を併記して判定する。
- 実行結果は `NEXT_TASKS` / `dev-status` / `worklog` の3点同期を基本とする。

## 2. 実行コマンド（標準）
```powershell
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorSttIntegrationTests" -RequireArtifacts
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorTtsIntegrationTests" -RequireArtifacts
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.CoreOrchestratorLlmIntegrationTests" -RequireArtifacts
./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.LoopbackHttpClientTests" -RequireArtifacts
```

## 3. 判定ルール
- Pass: テスト実行が成功し、対象runの xml/log artifact が生成されている。
- Fail(起動前): Unity.exe / Unity.com の起動に失敗し、artifact が未生成。
- Fail(実行後): artifact は生成されたが xml上の failed > 0。
- 要再実行: 環境復旧後に同一条件で再実行し、同じ形式で記録する。

## 4. 記録テンプレート（U6）
```markdown
| run_at | suite | pass_fail | exit_code | cause | artifact_xml | artifact_log |
|---|---|---|---|---|---|---|
| 2026-02-21 21:32 | CoreOrchestratorSttIntegrationTests | Failed (起動前) | 1 | Unity.exe / Unity.com とも `指定されたモジュールが見つかりません` | missing (`...xml`) | missing (`...log`) |
```

## 5. 失敗時復旧フロー
1. `docs/05-dev/unity-test-environment-recovery.md` の手順で環境復旧を試行する。
2. 復旧後、4スイートを同一条件（`-RequireArtifacts`）で再実行する。
3. `docs/worklog` に失敗内容、復旧内容、artifact 有無を追記する。
4. `docs/05-dev/dev-status.md` に再発/解消の状態を同期する。

## 6. U6完了条件
- `-RequireArtifacts` 実装が反映済み。
- 4スイートの未生成検知結果が記録済み。
- 標準運用手順と記録テンプレートが `docs/05-dev` に文書化済み。

## 参照
- `tools/run_unity_tests.ps1`
- `docs/05-dev/unity-test-environment-recovery.md`
- `docs/05-dev/unity-test-result-collection-template.md`
- `docs/worklog/2026-02-21_u6_t1_kickoff.md`
