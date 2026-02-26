# Worklog: AGENTS Persona README Exclusion Fix（2026-02-26）

- Date: 2026-02-26
- Task: `character_persona/README.md` を personaカウント対象外にするため AGENTS.md を修正。
- Execution-Tool: Codex
- Execution-Agent: codex-gpt5
- Execution-Model: gpt-5
- Used-Skills: n/a
- Repo-Refs:
  - `AGENTS.md`
  - `character_persona/README.md`
- Obsidian-Refs:
  - `D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md`
- Report-Path: docs/worklog/2026-02-26_agents_persona_readme_exclusion.md
- Obsidian-Log: D:/Obsidian/Programming/MascotDesktop_phaseNA_log_260226_1748.md
- Tags: [agent/codex-gpt5, model/gpt-5, tool/codex, agents, persona, readme]

## Summary

`character_persona/README.md` が存在しても persona適用可となるよう、`AGENTS.md` の Persona Operation を更新した。`README.md` を除外した Markdown カウントが1であることを確認し、運用可能状態にした。

## Changes

1. `AGENTS.md`
   - `character_persona/` の persona適用条件を「`README.md` 除外」に修正。
2. `character_persona/README.md`
   - `README.md` は personaカウント対象外であることを明記。

## Commands

```powershell
Select-String -Path AGENTS.md -Pattern "README.md|character_persona/|Persona Operation"
$files=Get-ChildItem character_persona -File -Filter *.md | Where-Object { $_.Name -ne 'README.md' }
"persona_md_count={0}; files={1}" -f $files.Count,($files.Name -join ',')
Get-Content character_persona/README.md
```

## Tests

| テスト | コマンド | exit code | 結果 | artifact |
|---|---|---|---|---|
| T1: AGENTS 反映確認 | `Select-String ... AGENTS.md` | 0 | PASS（README除外ルール確認） | stdout |
| T2: personaカウント確認 | `Get-ChildItem ... | Where-Object Name -ne README.md` | 0 | PASS（`persona_md_count=1`） | stdout |
| T3: README 文言確認 | `Get-Content character_persona/README.md` | 0 | PASS（除外ルール追記済み） | stdout |

## Rationale (Key Points)

- 実運用上 `README.md` を残したまま persona を使いたいため、判定ルール側で除外するのが最小変更。
- 複数 persona 本体を防ぐ制約は維持し、品質境界（報告系非persona）も維持した。

## Rollback
- `AGENTS.md` の 0.4 節該当行を逆適用すれば元に戻せる。
- `character_persona/README.md` の追記行を削除すれば元に戻せる。
- Obsidianログは削除しない。
- ロールバック理由（何を・なぜ戻したか）を本ファイルへ追記する。
- Obsidianログには `Rolled back` / `Superseded` 注記を追記する。

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## Next Actions
- `character_persona/` 配下は `README.md` + persona本体1ファイルの構成を維持する。
