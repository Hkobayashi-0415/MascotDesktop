# Release Completion Plan (Legacy P0-P6 Reference)

- Status: done（R4 Done: R1/R2 Conditional Pass をリリース完了扱いとして確定）
- Owner/Agent: antigravity
- Last Updated: 2026-02-23
- Scope: U6完了後のリリース完了判定

## 1. 背景
本計画は `docs/NEXT_TASKS.md` の「旧計画との対比（Pre-Unity vs Unity）」を根拠に、
旧P0-P6の主題を現在のUnity計画へ再対応付けしてリリース判定ゲートを定義する。

## 2. 旧計画との対応（リリース観点）
| 旧計画 | 旧主題 | 現行対応 | リリース観点の判定対象 |
|---|---|---|---|
| P0-P2 | 起動導線、manifest、packaging準備 | U0 + 運用ドキュメント | 配布導線と起動導線の最終確認 |
| P3-P4 | Launcher/Resident UX | U1 + Runtime/Resident導線 | 常駐/操作導線が運用可能 |
| P5 | キャラ切替・運用改善 | U4 | 運用手順に沿って切替と確認が可能 |
| P6 | Core統合（LLM/TTS/STT） | U5 + U6 | 回帰品質ゲートで失敗検知/結果記録が成立 |

## 3. リリース完了ゲート
### Gate R1: 配布/起動導線
- 参照: `docs/PACKAGING.md`, `docs/05-dev/QUICKSTART.md`
- 判定条件:
  - 配布手順が現行構成に矛盾しない。
  - 起動導線の手順が `docs/05-dev` 側と一致している。
- **判定結果: Conditional Pass**（2026-02-22）
  - Unity側起動導線は `QUICKSTART.md` で整合済み。
  - 旧PoC配布（`PACKAGING.md`）は PyInstaller 前提で未更新。Unity移行後は非スコープ。
  - 根拠: `docs/worklog/2026-02-22_rls_s1_gate_execution.md`

### Gate R2: Runtime/Resident運用
- 参照: `docs/RESIDENT_MODE.md`, `docs/05-dev/unity-runtime-manual-check.md`
- 判定条件:
  - 常駐/Runtime確認手順が最新運用手順として利用可能。
  - 主要運用手順の参照先が切れていない。
- **判定結果: Conditional Pass**（2026-02-22）
  - Runtime HUD 導線は `unity-runtime-manual-check.md` / `QUICKSTART.md` で整合済み。
  - 参照切れなし（全参照先ファイル存在確認済み）。
  - 旧PoC常駐（`RESIDENT_MODE.md`）は Python/tray_host 前提で未更新。
  - 根拠: `docs/worklog/2026-02-22_rls_s1_gate_execution.md`

### R1/R2 Conditional Pass のリリース完了扱い基準（Unity Scope）
- Unityスコープ受入条件:
  - R1: `docs/05-dev/QUICKSTART.md` の起動導線で運用可能。
  - R2: `docs/05-dev/unity-runtime-manual-check.md` の Runtime/Resident 導線で運用可能。
- 旧PoC文書未更新の扱い:
  - `docs/PACKAGING.md` / `docs/RESIDENT_MODE.md` は legacy 参照（read-only）として扱い、Unityスコープ判定では non-blocking。
  - Unityスコープ手順との矛盾が確認された場合のみ blocking に昇格。
- 残課題の管理方針:
  - Unityリリース機能に直接影響しない残課題は次フェーズ管理（worklog + 次アクション）へ移管し、R4を停止しない。
  - Gate失敗、artifact欠損、重大不具合のようにUnityリリース機能へ直接影響する証跡がある場合のみ R4 を停止する。
- 判定:
  - 上記を満たす場合、R1/R2 の Conditional Pass を「リリース完了扱い（Unity Scope）」として確定する。

### Gate R3: Core統合品質
- 参照: `docs/05-dev/u5-llm-tts-stt-operations.md`, `docs/05-dev/u6-regression-gate-operations.md`
- 判定条件:
  - U5統合手順とU6回帰ゲート手順が併用可能。
  - 4スイート結果を `worklog` / `dev-status` へ同期する運用が成立。
- **判定結果: Pass**（2026-02-22 18:02-18:07 の直接実行を根拠）
  - Unity.exe 直接実行で 4スイート全て Passed（STT 4/4, TTS 3/3, LLM 5/5, Loopback 5/5）。
  - artifact 全件生成確認済み。
  - `run_unity_tests.ps1` 品質確認（事実）:
    - 2026-02-22 20:54-20:58: artifact 待機ログ後に check passed、4スイート exit 0（`script_retest_*.txt`）。
    - 2026-02-22 21:31-21:32: Unity.exe/Unity.com とも起動前失敗、4スイート exit 1（`review_run_*_20260222_1.txt`）。
    - 2026-02-22 22:23-22:24: `run_unity_tests.ps1 -RequireArtifacts` で4スイート全て Passed（Loopback 5/5, STT 4/4, TTS 3/3, LLM 5/5）、artifact（xml/log）全件生成。
    - 2026-02-23 00:07-00:08: `run_unity_tests.ps1 -RequireArtifacts` 再実行でも4スイート全て Passed（STT 4/4=`editmode-20260223_000743.xml`, TTS 3/3=`editmode-20260223_000753.xml`, LLM 5/5=`editmode-20260223_000803.xml`, Loopback 5/5=`editmode-20260223_000813.xml`）、artifact（xml/log）全件生成。
    - 2026-02-22 17:43-17:47 実行分（`174300` / `174535` / `174556` / `174722`）は当時記録が missing/exit1 だが、現存 artifact XML は全件 Passed。
  - `run_unity_tests.ps1` 品質確認（仮説）:
    - 当時の exit 1 は artifact 判定タイミング要因と環境依存の起動前失敗が重なっていた可能性がある。単一原因は未確定。
  - 根拠: `docs/worklog/2026-02-22_unity_recovery_r3_pass.md`, `docs/worklog/2026-02-22_deepfix_rls_docsync.md`, `docs/worklog/2026-02-22_rls_docsync_script_success_sync.md`

### Gate R4: 最終判定記録
- 参照: `docs/worklog/`
- 判定条件:
  - 最終バッチ回帰結果が `worklog` に記録されている。
  - `NEXT_TASKS` と `dev-status` が同一状態に同期されている。
- **判定結果: Done**（2026-02-22）
  - R1/R2 Conditional Pass は上記 Unity Scope 基準でリリース完了扱いとして確定。
  - R3 は 22:23-22:24 と 2026-02-23 00:07-00:08 の再実行成功（Loopback 5/5, STT 4/4, TTS 3/3, LLM 5/5、artifact全件生成）で Pass 維持。
  - 根拠: `docs/worklog/2026-02-22_rls_docsync_script_success_sync.md`, `docs/worklog/2026-02-22_r4_closure_fullquality.md`

### RLS-R4 タスク最終状態
| ID | state | blocker | completion condition |
|---|---|---|---|
| RLS-R4-01 | Done | - | R1/R2 Conditional Pass を「リリース完了扱い（Unity Scope）」として確定し、R4=Done を3文書で一致させる。 |
| RLS-R4-02 | Done | - | 最終判定履歴（根拠/残課題/ロールバック方針）を `worklog` に確定し、Report-Path/Obsidian-Log/Record Check を充足する。 |

## 4. 実施順序
1. U6完了状態を反映（運用手順・記録テンプレートを固定）。
2. Gate R1/R2 の文書整合を確認。
3. ユーザー実行で最終バッチ回帰（4スイート）を実施。
4. Gate R3/R4 を確認してリリース完了判定を記録。

## 5. 完了条件（Release DoD）
- Gate R1-R4 は判定済み。R4 は R1/R2 Conditional Pass のリリース完了扱い確定により Done。
- 最終バッチ回帰の結果（成功/失敗、artifact有無、原因）が `worklog` に記録されている。→ ✅ `docs/worklog/2026-02-22_unity_recovery_r3_pass.md` に記録済み
- `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` の Gate 状態と RLS タスク状態が一致している。→ ✅ 一致確認済み

## 参照
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/PACKAGING.md`
- `docs/RESIDENT_MODE.md`
- `docs/05-dev/QUICKSTART.md`
- `docs/05-dev/unity-runtime-manual-check.md`
- `docs/05-dev/u5-llm-tts-stt-operations.md`
- `docs/05-dev/u6-regression-gate-operations.md`
