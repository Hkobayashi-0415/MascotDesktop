# Error Code and Logging Schema

- Status: active
- Owner/Agent: codex
- Last Updated: 2026-02-08
- Scope: Final schema for runtime structured logs and stable error-code naming.

## Purpose

Define one stable logging contract for Unity runtime so troubleshooting is deterministic across asset-path, model-load, and fallback flows.

## Log Entry Schema

All runtime logs are JSON object lines (`runtime.jsonl`) and Unity console JSON messages.

Required keys:

| Key | Type | Rule |
|---|---|---|
| `timestamp` | string | UTC ISO-8601 (`DateTimeOffset.UtcNow.ToString("o")`) |
| `level` | string | `INFO` / `WARN` / `ERROR` |
| `component` | string | non-empty; fallback `runtime` |
| `event_name` | string | non-empty; fallback `runtime.event` |
| `request_id` | string | non-empty; fallback auto-generated `req-<32hex>` |
| `error_code` | string | always present (may be empty only for INFO) |
| `message` | string | always present (empty allowed) |
| `path` | string | always present (empty allowed) |
| `source_tier` | string | always present (empty allowed) |
| `exception_type` | string | always present (empty when no exception) |
| `exception_message` | string | always present (empty when no exception) |

Normalization rules:
- Missing `level` -> `INFO`
- Missing `component` -> `runtime`
- Missing `event_name` -> `runtime.event`
- Missing `request_id` -> auto `req-<32hex>`
- Missing `error_code`:
  - `WARN` -> `RUNTIME.WARN.UNSPECIFIED`
  - `ERROR` -> `RUNTIME.ERROR.UNSPECIFIED`
  - `INFO` -> `""`

## Naming Rules

### `event_name`
- Format: lowercase dotted identifier (`domain.subject.action`)
- Example: `avatar.model.displayed`, `assets.path.resolve_failed`

### `error_code`
- Format: uppercase dotted identifier (`DOMAIN.CATEGORY.DETAIL`)
- Example: `ASSET.PATH.TRAVERSAL_FORBIDDEN`, `AVATAR.VRM.LOADER_NOT_FOUND`

## Source Tier Vocabulary

Current values in runtime:
- `assets_user`
- `streaming_assets`
- `config_relative`
- `resolved_asset`
- `project`
- `placeholder`

## Event Registry (Current Runtime)

### assets
- `assets.path.non_ascii`
- `assets.path.resolved`
- `assets.path.resolve_failed`

### avatar
- `avatar.paths.resolved`
- `avatar.paths.resolve_failed`
- `avatar.model.bootstrap_failed`
- `avatar.model.resolve_failed`
- `avatar.model.loader_selected`
- `avatar.model.unsupported_extension`
- `avatar.model.texture_decode_failed`
- `avatar.model.displayed`
- `avatar.model.display_failed`
- `avatar.model.vrm_load_failed`
- `avatar.model.pmx_load_failed`
- `avatar.model.fallback_used`
- `avatar.model.placeholder_displayed`

## Error Code Registry (Current Runtime)

### path policy / resolution
- `ASSET.PATH.EMPTY`
- `ASSET.PATH.ABSOLUTE_FORBIDDEN`
- `ASSET.PATH.TRAVERSAL_FORBIDDEN`
- `ASSET.PATH.LEGACY_FORBIDDEN`
- `ASSET.PATH.NON_ASCII_WARN`
- `ASSET.PATH.NOT_FOUND`
- `ASSET.PATH.RESOLVE_FAILED`

### asset read / format
- `ASSET.READ.FILE_NOT_FOUND`
- `ASSET.READ.DECODE_FAILED`
- `ASSET.READ.EXTENSION_MISSING`
- `ASSET.READ.UNSUPPORTED_EXTENSION`
- `ASSET.PLACEHOLDER.USED`

### avatar runtime
- `AVATAR.PATHS.ROOT_EMPTY`
- `AVATAR.PATHS.RESOLVE_FAILED`
- `AVATAR.MODEL.UNSUPPORTED_OR_READ_FAILED`
- `AVATAR.VRM.API_MISMATCH`
- `AVATAR.VRM.ROOT_NOT_FOUND`
- `AVATAR.VRM.LOAD_FAILED`
- `AVATAR.VRM.LOADER_NOT_FOUND`
- `AVATAR.PMX.LOAD_FAILED`
- `AVATAR.PMX.LOADER_NOT_FOUND`

### runtime fallback defaults
- `RUNTIME.WARN.UNSPECIFIED`
- `RUNTIME.ERROR.UNSPECIFIED`

## Validation

The following must hold in test/runtime checks:
- All emitted logs include `request_id`, `error_code`, `path`, `source_tier` keys.
- `WARN` and `ERROR` never emit empty `error_code`.
- Error-code naming follows uppercase dotted format.
