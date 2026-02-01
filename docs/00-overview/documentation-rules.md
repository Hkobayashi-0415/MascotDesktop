# Documentation Rules (Multi-Agent)

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-01-01
- Scope: Where to write, how to name, and how to update docs.

## Purpose
Keep docs consistent, discoverable, and safe for multi-agent work.

## Source Of Truth
- Product spec: `spec/latest/spec.md`
- Spec diffs: `spec/diffs/YYYY-MM-DD_<topic>.md`
- Architecture details: `docs/02-architecture/**`
- Ops/runbooks: `docs/05-dev/**`
- Security/policies: `docs/04-security/**`
- Worklog: `docs/WORKLOG.md`
- Backlog: `docs/NEXT_TASKS.md`

## Canonical Root
- All authoritative docs live under `workspace/`.
- Root-level docs outside `workspace/` are legacy and must not be updated.
- If duplicate content exists, update the `workspace/` copy only.

## Placement Rules
- Put new docs under the correct subfolder first.
- If a doc is an entrypoint (quick how-to, index, or policy), it may live in `docs/`.
- When adding a top-level doc in `docs/`, add a link from `README.md` and `docs/00-overview/documentation-index.md`.
- Never edit or add files under `refs/`.

## Naming Rules
- Use ASCII file names.
- Prefer `kebab-case` for new doc files (e.g., `viewer-seams.md`).
- Existing uppercase files remain for compatibility; do not rename without a migration note and index update.

## Formatting Rules
- Use Markdown headings with a single H1 at the top.
- Keep sections short; prefer bullet lists.
- Clearly label assumptions, TODO, and known limitations.
- Do not paste secrets or full payloads.
- Use this header block when creating a new doc:
  - Status: draft|active|deprecated
  - Owner/Agent: short id
  - Last Updated: YYYY-MM-DD
  - Scope: 1-2 lines

## Multi-Agent Collaboration
- Append-only for logs and worklogs; do not rewrite past entries.
- Every worklog entry must include:
  - Date (YYYY-MM-DD)
  - Summary (1-3 bullets)
  - Verification (what was checked)
  - Open issues (if any)
  - Author/Agent (short id)
- If a change is unverified, mark it explicitly as "UNVERIFIED".

## Update Triggers
- If an API/DTO changes, update:
  - `docs/02-architecture/interfaces/ipc-contract.md`
  - `docs/WORKLOG.md` (entry)
  - `spec/latest/spec.md` and `spec/diffs/` (if behavior changes)
- If a run command or path changes, update:
  - `README.md`
  - `docs/05-dev/run-poc.md`
  - `docs/00-overview/documentation-index.md`

## Safety
- Never commit binary assets (pmx/vmd/texture/audio).
- Keep `data/assets_user/**` git-ignored.
- Use relative paths; avoid absolute paths in docs unless showing an example.

---

## Document Collision Prevention (Multi-Agent)

### Before Creating New Documents
エージェントまたは開発者が新規ドキュメントを作成する前に、**必ず**以下を確認してください：

1. **既存ファイル検索**
   - ファイル名で検索: `find_by_name -Pattern "keyword" -SearchDirectory "workspace/docs"`
   - 内容で検索: `grep_search -Query "keyword" -SearchPath "workspace/docs"`

2. **重複判定の基準**
   - トピックが50%以上重複している場合は、既存ファイルを更新する
   - 既存ファイルが古い場合は、改訂履歴を追加して更新
   - 新しい視点を追加する場合は、既存ファイルにセクション追加を検討

3. **類似ファイル名の確認**
   - ケバブケース、スネークケース、大文字小文字の違いに注意
   - 例: `run-poc.md` と `run_poc.md` と `RUN_POC.md` は別ファイル扱い

### Revision History Format

既存ドキュメントを大幅に更新した場合、以下の形式で改訂履歴を追加してください：

```markdown
## Revision History

### v2.0 (2026-01-11, agent:codex)
- 大幅に拡張：〇〇機能の詳細を追加
- 〇〇セクションを新規追加

### v1.0 (~2025-12-30)
- 初版作成
- 基本方針のみ定義
```

### Prohibited Actions (Strict)

以下の行為は**絶対に禁止**です：

1. **ルートレベル `MascotDesktop/docs/` への新規作成・更新**
   - すべてのドキュメントは `workspace/docs/` に配置してください
   - ルートレベル `docs/` は legacy 扱い（参照のみ）
   
2. **`refs/` への書き込み**
   - `refs/` は read-only 参照資料です
   
3. **二重管理になるファイル作成**
   - 既存ドキュメントと同じトピックの新規ファイルは作成しないでください

### Agent Decision Flow

**新規ドキュメント作成時**:

1. **既存ファイル検索を実行**
2. **トピック重複度を判定**
   - 50%以上重複 → 既存ファイルを更新
   - 50%未満 → 新規作成可能か検討
3. **セクション追加で対応可能か確認**
4. **新規作成の場合**
   - 適切な配置先を決定（`workspace/docs/` 配下）
   - `documentation-index.md` を更新
   - 必要に応じて `README.md` からリンク追加

**既存ドキュメント更新時**:

1. **軽微な修正**（typo、リンク修正等）: 改訂履歴不要
2. **セクション追加**（新機能追加等）: 改訂履歴に簡潔に記録
3. **大幅な書き換え**（全体の30%以上変更）: 改訂履歴に詳細を記録
