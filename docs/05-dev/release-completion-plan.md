# Release Completion Plan (Legacy P0-P6 Reference)

- Status: draft
- Owner/Agent: codex
- Last Updated: 2026-02-21
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

### Gate R2: Runtime/Resident運用
- 参照: `docs/RESIDENT_MODE.md`, `docs/05-dev/unity-runtime-manual-check.md`
- 判定条件:
  - 常駐/Runtime確認手順が最新運用手順として利用可能。
  - 主要運用手順の参照先が切れていない。

### Gate R3: Core統合品質
- 参照: `docs/05-dev/u5-llm-tts-stt-operations.md`, `docs/05-dev/u6-regression-gate-operations.md`
- 判定条件:
  - U5統合手順とU6回帰ゲート手順が併用可能。
  - 4スイート結果を `worklog` / `dev-status` へ同期する運用が成立。

### Gate R4: 最終判定記録
- 参照: `docs/worklog/`
- 判定条件:
  - 最終バッチ回帰結果が `worklog` に記録されている。
  - `NEXT_TASKS` と `dev-status` が同一状態に同期されている。

## 4. 実施順序
1. U6完了状態を反映（運用手順・記録テンプレートを固定）。
2. Gate R1/R2 の文書整合を確認。
3. ユーザー実行で最終バッチ回帰（4スイート）を実施。
4. Gate R3/R4 を確認してリリース完了判定を記録。

## 5. 完了条件（Release DoD）
- Gate R1-R4 がすべて満たされている。
- 最終バッチ回帰の結果（成功/失敗、artifact有無、原因）が `worklog` に記録されている。
- `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` がリリース完了状態で一致している。

## 参照
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/PACKAGING.md`
- `docs/RESIDENT_MODE.md`
- `docs/05-dev/QUICKSTART.md`
- `docs/05-dev/unity-runtime-manual-check.md`
- `docs/05-dev/u5-llm-tts-stt-operations.md`
- `docs/05-dev/u6-regression-gate-operations.md`
