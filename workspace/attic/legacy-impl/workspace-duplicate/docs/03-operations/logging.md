# Logging & Monitoring (baseline)

- Levels: DEBUG/INFO/WARN/ERROR. Default INFO. LLM requests/responses: log metadata only (model, token count, latency), mask content.
- Channels: core, memory, audio, avatar, reminder, import/export, ui. Separate files optional; rotate daily.
- Health: scheduler polls core/memory/shell IPC endpoints; log failures with timestamps.
- Privacy: screenshot exclusion patterns applied; no secrets in logs.
- Traceability: include character_id/version_id, session/request ids, and config snapshot id in structured logs.
