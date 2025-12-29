# Refs Policy (read-only, black-box)

- refs/CocoroAI_3.5.0Beta and refs/CocoroAI_4.7.4Beta: black-box observation only. No reverse engineering (no decompile/unwrap/DRM bypass/obfuscation removal). Use config/logs/behavioral observations only.
- refs/character_chat_prototype: user-owned; code/design/schema reuse allowed. Still avoid committing secrets.
- refs/public_files-main: reference-only. Code is readable but do not edit/copy into workspace; treat as behavioral/UI reference (no reverse engineering needed, but keep it read-only).
- Keep refs/ outside git. Optionally run `scripts/setup/make_refs_readonly.ps1` to set +R attributes.
