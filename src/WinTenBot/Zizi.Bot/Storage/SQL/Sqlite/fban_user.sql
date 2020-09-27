CREATE TABLE IF NOT EXISTS fban_user (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id     INTEGER,
    reason_ban  TEXT,
    banned_by   VARCHAR,
    banned_from VARCHAR,
    created_at  VARCHAR
);