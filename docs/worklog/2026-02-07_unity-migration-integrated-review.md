# Worklog: Unity migration integrated review (difficulty, plans, tradeoffs)

- Date: 2026-02-07
- Task: Integrated Unity migration review with tiered plans and current issues
- Used-Skills: code-review, phase-planning, worklog-update
- Repo-Refs: workspace/docs/00-overview/migrations/unity-migration-spec.md; workspace/spec/latest/spec.md; workspace/docs/05-dev/run-poc.md; workspace/docs/03-operations/logging.md; workspace/docs/02-architecture/interfaces/config-schema.md; workspace/docs/02-architecture/interfaces/error-codes.md; workspace/docs/02-architecture/assets/asset-handling.md; workspace/docs/PATHS.md; workspace/docs/ASSETS_PLACEMENT.md; workspace/docs/PACKAGING.md; workspace/docs/RESIDENT_MODE.md; workspace/docs/VIEWER_SEAMS.md; workspace/docs/MANIFEST.md; workspace/docs/MOTIONS_DEMO.md; workspace/docs/04-security/local-vs-repo.md; docs/02-architecture/interfaces/ipc-contract.md; docs/02-architecture/ui/window-controller.md; docs/02-architecture/avatar/avatar-renderer.md; docs/worklog/2026-01-28_unity-migration-spec.md; docs/worklog/2026-02-01_review-unity-migration.md; docs/worklog/2026-02-02_review-unity-migration-followup.md; docs/worklog/2026-02-04_unity-migration-review.md
- Obsidian-Refs: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260201.md; D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0149.md; D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md
- Report-Path: docs/worklog/2026-02-07_unity-migration-integrated-review.md
- Obsidian-Log: D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260207_0044.md
- Execution-Tool: shell_command (PowerShell)
- Execution-Agent: codex
- Execution-Model: gpt-5
- Tags: review, unity, migration, plan, integrated

## Summary
- Unity移行に関する既存worklogとworkspace docsを統合し、難易度別プラン・メリデメ・課題を整理した。
- Legacy docs にのみ存在する契約/受入条件があり、Unity移行の前提仕様が不完全である点を指摘した。

## Report: Unity移行レビュー（統合改訂版）

### 根拠（主要）
- `workspace/docs/00-overview/migrations/unity-migration-spec.md`
- `workspace/spec/latest/spec.md`
- `workspace/docs/05-dev/run-poc.md`
- `workspace/docs/03-operations/logging.md`
- `workspace/docs/02-architecture/interfaces/config-schema.md`
- `workspace/docs/02-architecture/interfaces/error-codes.md`
- `workspace/docs/02-architecture/assets/asset-handling.md`
- `workspace/docs/PATHS.md`
- `workspace/docs/ASSETS_PLACEMENT.md`
- `workspace/docs/PACKAGING.md`
- `workspace/docs/RESIDENT_MODE.md`
- `workspace/docs/VIEWER_SEAMS.md`
- `workspace/docs/MANIFEST.md`
- `workspace/docs/MOTIONS_DEMO.md`
- `workspace/docs/04-security/local-vs-repo.md`
- `docs/02-architecture/interfaces/ipc-contract.md` (legacy)
- `docs/02-architecture/ui/window-controller.md` (legacy)
- `docs/02-architecture/avatar/avatar-renderer.md` (legacy)
- `docs/worklog/2026-01-28_unity-migration-spec.md`
- `docs/worklog/2026-02-01_review-unity-migration.md`
- `docs/worklog/2026-02-02_review-unity-migration-followup.md`
- `docs/worklog/2026-02-04_unity-migration-review.md`

### Findings (重要度順)
High
- 現行PoCは Core/Shell/Avatar の別プロセス + HTTP/JSON 連携で、request_id を前提とした横断ログ運用がある。Unity移行specは単一ウィンドウ＋Minimal Core を想定しており、プロセス境界の再設計が不可避。ログ/相関IDの運用を崩すとデバッグ不能になる。`workspace/docs/05-dev/run-poc.md`, `workspace/docs/03-operations/logging.md`, `workspace/docs/00-overview/migrations/unity-migration-spec.md`
- config schema の `topmost` と Unity spec の `pinned`/`fit` が不一致。さらに Window Controller 受入条件（クリック透過、安全解除、最小/最大サイズ等）は legacy docs にのみ記載。Unity側のUI要件に反映しないと動作仕様が分裂する。`workspace/docs/02-architecture/interfaces/config-schema.md`, `workspace/docs/00-overview/migrations/unity-migration-spec.md`, `docs/02-architecture/ui/window-controller.md`
- Viewer は HTML/JS + three.js MMDLoader で構成され、seam診断/slot/manifest等の機能がある。Unity移行では同等機能の再実装が必要で、難易度が上がる。`workspace/docs/WORKLOG.md`, `workspace/docs/VIEWER_SEAMS.md`, `workspace/docs/MANIFEST.md`, `workspace/docs/MOTIONS_DEMO.md`
- IPC contract / Window controller / Avatar renderer の正規ドキュメントが workspace 側に存在せず、legacy docs のみに残っている。Unity移行の設計根拠が断片化しており、仕様凍結ができない。`workspace/docs/00-overview/documentation-rules.md`, `docs/02-architecture/interfaces/ipc-contract.md`, `docs/02-architecture/ui/window-controller.md`, `docs/02-architecture/avatar/avatar-renderer.md`

Medium
- Asset handling/placement と ASCII path 前提を Unity に反映しないと、MMD読み込みやビルド失敗が再発する。`workspace/docs/02-architecture/assets/asset-handling.md`, `workspace/docs/ASSETS_PLACEMENT.md`, `workspace/docs/PATHS.md`
- Packaging/Resident mode は PyInstaller+pywebview を前提としている。Unity移行では配布/常駐の設計とログ導線を作り直す必要がある。`workspace/docs/PACKAGING.md`, `workspace/docs/RESIDENT_MODE.md`
- Error code と JSONL logging の運用は PoC で確立。Unity側も `request_id` と error_code の契約を維持する必要がある。`workspace/docs/02-architecture/interfaces/error-codes.md`, `workspace/docs/03-operations/logging.md`

Low
- CocoroAI は blackbox 観察のみで、Unity実装の根拠にはできない。比較は参考レベルに留めるべき。`workspace/docs/04-security/local-vs-repo.md`, `docs/worklog/2026-02-01_review-unity-migration.md`

### 難易度別プラン概要
Tier 1 (Low): Unity Viewer Only
Scope: HTML/JS viewer を Unity 置換。Core/Shell は Python/HTTP のまま維持する。
Key tasks: MMDロードと最小再生、manifest slot 解決、seam対策、`request_id` を含むログ連携、assets_user の相対パス解決を実装する。
Exit criteria: `run-poc` の受入条件に沿って表示/操作が成立し、`MOTIONS_DEMO` の slot 再生が再現できる。`workspace/docs/05-dev/run-poc.md`, `workspace/docs/MOTIONS_DEMO.md`
Risks: Unity側のMMD実装コストと品質差（seam/alpha問題）。`workspace/docs/VIEWER_SEAMS.md`

Tier 2 (Medium): Unity UI + Avatar + Minimal Core
Scope: Unity移行specの通り、単一ウィンドウでUI+Avatar+Minimal Core を持ち、Memory/Embeddingは外部のまま残す。
Key tasks: window controller 受入条件の移植、config schema の統一、UIイベントのログ/相関ID維持、既存Coreとの境界確定。`workspace/docs/00-overview/migrations/unity-migration-spec.md`, `workspace/docs/02-architecture/interfaces/config-schema.md`, `docs/02-architecture/ui/window-controller.md`
Exit criteria: 位置/サイズ/Topmostが `data/user/config.json` と一致し、`request_id` を使った追跡が成立する。`workspace/docs/03-operations/logging.md`
Risks: IPC契約が未確定で、Unityと外部プロセスの接続方式が不明確。`docs/02-architecture/interfaces/ipc-contract.md`

Tier 3 (High): Full Unity (Core統合)
Scope: LLM/TTS/Memory/Reminder 等の Core 機能も Unity に統合する。
Key tasks: Core機能再実装、DB/埋め込み/監査ログの再設計、運用ツールとテスト基盤の作り直し。`workspace/spec/latest/spec.md`
Exit criteria: specの Must を Unity 単体で満たし、現行PoC相当の運用が可能。
Risks: 工数と回帰が最大。段階移行せずに全面移行すると不確実性が高い。

### メリット
- 単一ウィンドウ/単一ランタイムでUXが簡潔化する。
- 3D表示の一体化により、描画や入力制御を Unity 側に集約できる可能性がある。
- 配布形態を Unity build に統一できれば、運用導線が整理される。

### デメリット
- ViewerのMMD/slot/seam対策を再実装する必要があり、初期コストが高い。
- Python運用資産（runスクリプト、PyInstaller、pywebview常駐）が再設計対象になる。
- IPC/ログ/設定契約が崩れるとデバッグと運用が悪化する。

### 現時点の課題
- 移行スコープの合意（Viewer-only / UI+Minimal Core / Full）。
- IPC transport の確定（HTTPかnamed pipeか）。
- `topmost` と `pinned` の定義統一、`fit` や clickthrough の扱い整理。
- legacy docs にしか無い契約（IPC/Window/Avatar）の workspace 側への移設。
- MMD最小要件（idleのみ/モーション対応範囲）の明文化。`workspace/docs/00-overview/migrations/unity-migration-spec.md`
- ASCIIパスと assets_user の前提を Unity 側でも保証する方針化。`workspace/docs/PATHS.md`

### CocoroAI比較の前提
- CocoroAI は blackbox 観察のみと明記されており、Unity実装の仕様は参照レベルに留める。`workspace/docs/04-security/local-vs-repo.md`

### Obsidian/既存レポート統合メモ
- 2026-02-01/2026-02-04 の Obsidian ログは、記録運用とスコープ未確定（viewer-only vs UI+minimal core）を再確認する内容で、新規の技術要件は追加されていない。

## Commands
- Get-Content workspace/docs/00-overview/documentation-rules.md
- Get-Content workspace/docs/00-overview/documentation-index.md
- Get-Content workspace/docs/00-overview/migrations/unity-migration-spec.md
- Get-Content workspace/spec/latest/spec.md
- Get-Content workspace/docs/05-dev/run-poc.md
- Get-Content workspace/docs/03-operations/logging.md
- Get-Content workspace/docs/02-architecture/interfaces/config-schema.md
- Get-Content workspace/docs/02-architecture/interfaces/error-codes.md
- Get-Content workspace/docs/02-architecture/assets/asset-handling.md
- Get-Content workspace/docs/PATHS.md
- Get-Content workspace/docs/ASSETS_PLACEMENT.md
- Get-Content workspace/docs/PACKAGING.md
- Get-Content workspace/docs/RESIDENT_MODE.md
- Get-Content workspace/docs/VIEWER_SEAMS.md
- Get-Content workspace/docs/MANIFEST.md
- Get-Content workspace/docs/MOTIONS_DEMO.md
- Get-Content workspace/docs/04-security/local-vs-repo.md
- Get-Content docs/02-architecture/interfaces/ipc-contract.md
- Get-Content docs/02-architecture/ui/window-controller.md
- Get-Content docs/02-architecture/avatar/avatar-renderer.md
- Get-Content docs/worklog/2026-01-28_unity-migration-spec.md
- Get-Content docs/worklog/2026-02-01_review-unity-migration.md
- Get-Content docs/worklog/2026-02-02_review-unity-migration-followup.md
- Get-Content docs/worklog/2026-02-04_unity-migration-review.md
- Get-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260201.md
- Get-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0149.md
- Get-Content D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260204_0205.md
- Get-Date -Format "yyMMdd_HHmm"

## Tests
- Not run (review only).

## Rationale (Key Points)
- Unity移行は spec上の非Goalが明確で、Minimal Core に限定する設計が前提。
- Viewer機能とログ/IPC契約が現行PoCの品質を支えており、再実装の影響が大きい。
- legacy docs への依存が残るため、仕様の正規化が先に必要。

## Rollback
- Delete docs/worklog/2026-02-07_unity-migration-integrated-review.md.
- Delete D:\Obsidian\Programming\MascotDesktop_phaseNA_log_260207_0044.md.

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated: Yes
- Obsidian-Log recorded: Yes
- Execution-Tool/Execution-Agent/Execution-Model present: Yes
- Tags present: Yes

## Next Actions
- Decide migration scope and freeze IPC/config contracts before Unity implementation starts.
- Migrate legacy IPC/Window/Avatar docs into workspace/docs or mark them deprecated with replacements.
