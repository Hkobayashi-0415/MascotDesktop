# Prototype Mapping (refs → workspace)

| Source (refs) | 種別 | 再利用方式 | Target (workspace) | 実行アクション | 注意点 |
|---|---|---|---|---|---|
| refs/character_chat_prototype/character_management/models.py | code/schema | DDL_ONLY | data/refs/prototype/character_management_models.py | 参照コピー済み、DDL起こし | avatar_mode/state/loop等は後付け拡張 |
| refs/character_chat_prototype/character_management/migrations/0001_initial.py | schema | DDL_ONLY | data/refs/prototype/character_management_0001_initial.py | 参照コピー済み、DDL起こし | unique制約を正規DDLへ統合 |
| refs/character_chat_prototype/chat/models.py | code/schema | DDL_ONLY | data/refs/prototype/chat_models.py | 参照コピー済み、memory_itemsにマップ | embedding/summary列は正規DDLで追加 |
| refs/character_chat_prototype/chat/migrations/0001_initial.py | schema | DDL_ONLY | data/refs/prototype/chat_0001_initial.py | 参照コピー済み、DDL起こし | role/content/timestamp中心 |
| refs/character_chat_prototype/accounts/models.py | code/schema | SPEC_ONLY | data/refs/prototype/accounts_models.py | 参照コピー済み、設計参照のみ | 認証は採用せず、Affinity部だけDDLへ反映 |
| refs/character_chat_prototype/accounts/migrations/0001_initial.py | schema | SPEC_ONLY | data/refs/prototype/accounts_0001_initial.py | 参照コピー済み | UserCharacterAffinity相当の列をDDLへ反映 |
| refs/character_chat_prototype/chat/static/chat/js/chat_main.js | ui | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | 振る舞い要約 | デスクトップUIで再実装 |
| refs/character_chat_prototype/chat/static/chat/js/character_media.js | ui | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | メディア連動要約 | AvatarRenderer設計へ反映 |
| refs/character_chat_prototype/chat/templates/chat/chat.html | ui | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | テンプレ構造要約 | Webテンプレは置換 |
| refs/character_chat_prototype/static/character_chat_prototype/css/ | ui/asset | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | スタイル参考 | 直接取り込まない |
| refs/character_chat_prototype/templates/character_chat_prototype/base.html | ui | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | レイアウト要約 | Web前提排除 |
| refs/character_chat_prototype/character_display_implementation.txt | design | SPEC_ONLY | docs/02-architecture/ui/ (要約予定) | メディア切替要約 | AvatarRendererに反映 |
| refs/character_chat_prototype/project_files_part*.txt | design | SPEC_ONLY | docs/00-overview/legacy/ (または要約) | 参照資料として保存 | 秘密情報なし確認 |
| refs/character_chat_prototype/character_chat_prototype/settings.py | config | REFERENCE | docs/04-security/secrets-and-config.md (パスのみ記載) | 値は書かない | SECRET_KEY/GEMINI_API_KEY有り（マスク） |
| refs/character_chat_prototype/db.sqlite3 | db | REFERENCE | （取り込まない） | 存在報告のみ | Git除外・実データ不使用 |
| refs/character_chat_prototype/venv/ | env | REFERENCE | （取り込まない） | 存在報告のみ | Git除外 |
| refs/character_chat_prototype/staticfiles/* | asset/build | REFERENCE | （取り込まない） | ビルド成果物無視 | Git除外 |
| refs/CocoroAI_* | binary/config | REFERENCE | docs/04-security/refs-policy.md | ブラックボックス参照のみ | 逆アセンブル禁止 |
| refs/public_files-main | ui/design | SPEC_ONLY | docs/02-architecture/ui/public_files-notes.md, docs/02-architecture/ui/window-controller.md, docs/02-architecture/avatar/avatar-renderer.md | 要約/受入条件化/状態遷移表作成 | コード移植せず参照のみ。依存(PyQt等)は技術選定と切離し |
