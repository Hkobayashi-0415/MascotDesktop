# Secrets and Config (paths only, no values)

- refs/character_chat_prototype/character_chat_prototype/settings.py : contains SECRET_KEY and GEMINI_API_KEY (values MUST NOT be committed). Treat as secret; keep under refs (git外) and never copy values.
- Any future app settings with API keys: place outside git (`secrets/`), load via env or secure store.
- db.sqlite3 or other runtime DBs: do not commit; keep under `data/db/` (gitignore).

## Revision History

### v1.0 (~2025-12-30)
- 初版作成
- ルートレベル `docs/04-security/secrets-and-config.md` と workspace 版に同一内容で存在
- 2026-01-11: workspace 版が正式版として確定
