# Worklog: U8 Ops Fail — <TITLE> (<YYYY-MM-DD>)

<!-- 使い方:
  - このファイルをコピーして docs/worklog/<YYYY-MM-DD>_u8_ops_fail_<title>.md に保存する。
  - <...> プレースホルダを実際の値に置き換える。
  - AGENTS.md §6 の必須項目をすべて埋めること。
-->

- Date: <YYYY-MM-DD>
- Task: U8運用チェック Fail 記録 — <失敗の概要>
- Execution-Tool: <claude-code|codex|manual>
- Execution-Agent: <claude-sonnet-4-6|codex|human>
- Execution-Model: <claude-sonnet-4-6|gpt-5|n/a>
- Used-Skills: worklog-update
- Repo-Refs:
  - `docs/05-dev/u8-operations-automation.md`
  - `<失敗 artifact のパス>`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/<YYYY-MM-DD>_u8_ops_fail_<title>.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_<YYMMDD_HHMM>.md
- Tags: [agent/<agent>, model/<model>, tool/<tool>, u8, operations, fail]

## Summary

- FailType: <runtime | docs | freshness>
- Profile: <Daily | Gate | Custom>
- 失敗発生日時: <YYYY-MM-DD HH:MM UTC>
- ArtifactPath: `<Unity_PJ/artifacts/manual-check/u8_ops_checks_run_<profile>_<timestamp>.json>`
- 概要: <1〜2行で失敗の内容を記述>

## Fail Detail

<!-- runtime Fail の場合 -->
- max_consecutive: <値>（閾値: 3）
- max_sequence（対象タイムスタンプ）: <開始> 〜 <終了>
- 関連ログファイル: `<runtime-YYYYMMDD-*.jsonl>`

<!-- docs sync Fail の場合 -->
- violations:
  - <ファイル名>: <Last Updated 不一致 / 参照リンク欠落>

<!-- freshness Fail の場合 -->
- elapsed_hours: <値>
- threshold_hours: <値>
- latest_artifact: `<ファイル名>`
- latest_run_at_utc: <値>

## Remediation

<!-- docs/05-dev/u8-operations-automation.md の「失敗時一次対応」フローに従う -->

1. **確認**: artifact の violations / max_consecutive を確認する。
2. **対処**:
   - runtime Fail: Player/Play Mode を再起動し、`avatar.model.displayed` 出力を確認する。継続Failの場合は以下に記録する。
   - docs sync Fail: 対象文書の `Last Updated` と Unity参照リンクを揃える。
   - freshness Fail: `./tools/run_u8_ops_checks.ps1 -Profile Daily` を実行して鮮度を更新する。
3. **再確認**: 復旧後に再実行し、Pass artifact を採取する。
4. **完了**: 本ファイルに復旧完了と再実行結果を追記する。

## Commands

```powershell
# 状況再確認（artifact 参照）
Get-Content '<artifact_path>' | ConvertFrom-Json | Format-List

# 復旧後の再実行
./tools/run_u8_ops_checks.ps1 -Profile <Daily|Gate|Custom> `
  -ArtifactDir "Unity_PJ/artifacts/manual-check"

# 鮮度確認
./tools/check_u8_ops_freshness.ps1 `
  -ArtifactPath "Unity_PJ/artifacts/manual-check/freshness_check_<timestamp>.json"
```

## Tests

- 復旧後再実行:
  - runtime monitor: <Pass | Fail>
  - docs sync: <Pass | Fail>
  - artifact: `<u8_ops_checks_run_<profile>_<timestamp>.json>`

## Rationale (Key Points)

- <なぜ Fail したか（原因の要点）>
- <対処の判断根拠>

## Rollback

- 実施なし（チェックスクリプトの変更がない場合）
- 文書変更がある場合は対象ファイルを逆適用し、本ファイルにロールバック理由を追記する。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): <Yes | n/a>
- Obsidian-Log recorded (path or reason): <Yes | 理由>
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions

1. <復旧後の次アクション>
2. 再発防止策があれば `docs/05-dev/u8-operations-automation.md` の「失敗時一次対応」を更新する。
