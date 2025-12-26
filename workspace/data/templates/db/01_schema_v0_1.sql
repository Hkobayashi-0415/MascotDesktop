-- SQLite DDL v0.1 (spec baseline). Prototype-derived columns included; TODO marks spec-only additions.

PRAGMA foreign_keys = ON;

CREATE TABLE characters (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    slug TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    summary TEXT,
    persona TEXT,               -- TODO: structured persona JSON?
    style_guidelines TEXT,
    safety_notes TEXT,
    default_voice_profile_id INTEGER,
    default_prompt_version_id INTEGER,
    default_memory_id TEXT,
    default_wake_word TEXT,
    default_avatar_mode TEXT CHECK(default_avatar_mode IN ('3d','video','pngtuber')),
    default_avatar_state TEXT,
    default_image_category TEXT,
    is_published INTEGER NOT NULL DEFAULT 0,
    is_readonly INTEGER NOT NULL DEFAULT 0,
    created_by TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE character_versions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    version_tag TEXT NOT NULL,
    changelog TEXT,
    status TEXT NOT NULL CHECK(status IN ('draft','published')),
    created_by TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE
);

CREATE TABLE character_prompts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_version_id INTEGER NOT NULL,
    system TEXT,
    developer TEXT,
    character TEXT,
    examples TEXT, -- JSON
    memory_hook_template TEXT,
    tool_instructions TEXT,
    max_tokens INTEGER,
    reasoning_effort TEXT,
    vision_model TEXT,
    tts_profile_override_id INTEGER,
    FOREIGN KEY(character_version_id) REFERENCES character_versions(id) ON DELETE CASCADE
);

CREATE TABLE character_voice_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    tts_type TEXT NOT NULL CHECK(tts_type IN ('voicevox','style-bert-vits2','aivis','other')),
    params TEXT,            -- JSON blob
    locale TEXT,
    sample_url TEXT,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE
);

CREATE TABLE character_audio_clips (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    category TEXT NOT NULL CHECK(category IN ('greeting','ack','happy','angry','confused','reminder','custom')),
    trigger TEXT,                -- event/state
    priority INTEGER DEFAULT 0,
    file_path TEXT NOT NULL,
    volume REAL DEFAULT 1.0,
    allow_interrupt INTEGER DEFAULT 1,
    is_default INTEGER DEFAULT 0,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE
);

CREATE TABLE character_assets (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    avatar_mode TEXT NOT NULL CHECK(avatar_mode IN ('3d','video','pngtuber')),
    state TEXT NOT NULL,         -- idle/talk/happy/...  (Mode3 default: 01_normal/02_smile/03_oko/04_sleep/05_on)
    category TEXT,               -- optional subtype (notify/talk/etc). Keep flexible; validate at app-level if custom.
    file_path TEXT NOT NULL,
    loop INTEGER DEFAULT 0,
    sort_order INTEGER DEFAULT 0,
    is_default INTEGER DEFAULT 0,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE
);

CREATE TABLE tags (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE character_tags (
    character_id INTEGER NOT NULL,
    tag_id INTEGER NOT NULL,
    PRIMARY KEY(character_id, tag_id),
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE,
    FOREIGN KEY(tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

CREATE TABLE memory_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER NOT NULL,
    user_id TEXT,                -- ローカル単一ユーザ前提ならNULL許容。将来 multi-user なら local_profile_id等で拡張。
    role TEXT NOT NULL,
    content TEXT NOT NULL,
    embedding_vector BLOB,       -- 方式方針: A) このカラムにfloat32配列を格納 (PoC) / B) 分離テーブル(memory_embeddings)
    summary_id INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE
);

CREATE TABLE reminders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id TEXT,
    character_id INTEGER,
    content TEXT NOT NULL,
    due_at DATETIME NOT NULL,
    status TEXT DEFAULT 'pending' CHECK(status IN ('pending','done','canceled')),
    channel TEXT DEFAULT 'ui' CHECK(channel IN ('ui','voice')),
    last_notified_at DATETIME,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE SET NULL
);

CREATE TABLE speaker_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id TEXT,
    embedding_vector BLOB,
    threshold REAL,
    last_seen_at DATETIME
);

CREATE TABLE audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    character_id INTEGER,
    version_id INTEGER,
    actor TEXT,
    action TEXT,
    payload_hash TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE SET NULL,
    FOREIGN KEY(version_id) REFERENCES character_versions(id) ON DELETE SET NULL
);

CREATE TABLE user_character_affinity (
    user_id TEXT NOT NULL,
    character_id INTEGER NOT NULL,
    affinity_value INTEGER DEFAULT 0,
    is_default INTEGER DEFAULT 0,
    selected_voice_profile_id INTEGER,
    PRIMARY KEY(user_id, character_id),
    FOREIGN KEY(character_id) REFERENCES characters(id) ON DELETE CASCADE,
    FOREIGN KEY(selected_voice_profile_id) REFERENCES character_voice_profiles(id) ON DELETE SET NULL
);

-- Embedding storage options:
--   A) Keep memory_items.embedding_vector (PoC default).
--   B) Split table memory_embeddings(memory_item_id, dim, embedding_vector BLOB). Choose before production.

-- Index candidates (apply as needed):
CREATE INDEX IF NOT EXISTS idx_characters_slug ON characters(slug);
CREATE INDEX IF NOT EXISTS idx_versions_character ON character_versions(character_id);
CREATE INDEX IF NOT EXISTS idx_prompts_version ON character_prompts(character_version_id);
CREATE INDEX IF NOT EXISTS idx_assets_character_state ON character_assets(character_id, state);
CREATE INDEX IF NOT EXISTS idx_clips_character_category ON character_audio_clips(character_id, category);
CREATE INDEX IF NOT EXISTS idx_tags_name ON tags(name);
CREATE INDEX IF NOT EXISTS idx_character_tags_tag ON character_tags(tag_id);
CREATE INDEX IF NOT EXISTS idx_memory_character_created ON memory_items(character_id, created_at);
CREATE INDEX IF NOT EXISTS idx_reminders_due ON reminders(due_at);
CREATE INDEX IF NOT EXISTS idx_audit_created ON audit_logs(created_at);

-- TODOs:
-- - Enforce published prompt immutability (app logic / triggers).
-- - Define app-level mapping for state names (idle/talk/notify vs Mode3 codes) and validate before insert.
-- - Choose embedding storage option before production and update schema accordingly.
