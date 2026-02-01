# Documentation Index

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-01-01
- Scope: Canonical entry points and doc map.

## Start Here
- `README.md`
- `docs/05-dev/QUICKSTART.md`
- `docs/05-dev/run-poc.md`
- `docs/00-overview/documentation-rules.md`

## Guides (Assets, Motions, Viewer)
- `docs/ASSETS_PLACEMENT.md`
- `docs/MANIFEST.md`
- `docs/MOTIONS_DEMO.md`
- `docs/VIEWER_SEAMS.md`
- `docs/RESIDENT_MODE.md`
- `docs/PATHS.md`
- `docs/PACKAGING.md`

## Specs and Architecture
- `spec/latest/spec.md`
- `spec/diffs/`
- `docs/02-architecture/`
- `docs/03-operations/logging.md`
- `docs/04-security/`

## Tracking
- `docs/WORKLOG.md`
- `docs/NEXT_TASKS.md`
- `docs/05-dev/dev-status.md`
- `docs/00-overview/reviews/`
- `docs/00-overview/migrations/`
- `docs/00-overview/migrations/unity-migration-spec.md`

## Legacy and Archive
- `docs/00-overview/legacy/`
- `docs/05-dev/ascii-path-migration.md` (legacy, use `docs/PATHS.md`)
- `attic/legacy-impl/` (read-only, do not update)

---

## For Agents (MUST READ)

> 🤖 エージェントが新規ドキュメントを作成する前に必読

- **Documentation Rules**: [`documentation-rules.md`](documentation-rules.md) ← **必ず読んでください**
  - 重複防止フロー、改訂履歴の付け方、禁止事項を記載
  - ルートレベル `MascotDesktop/docs/` への作成・更新は**絶対禁止**

### Document Revision History

`workspace/docs/` 内の主要ドキュメントは改訂履歴を持ちます。大幅な更新時は「## Revision History」セクションを追加してください。

### Legacy Documents Notice

ルートレベルの `MascotDesktop/docs/` は **legacy** です。参照・更新は行わず、必ず `workspace/docs/` を使用してください。詳細は [`docs/DEPRECATED.md`](../../../docs/DEPRECATED.md)（予定）を参照。
