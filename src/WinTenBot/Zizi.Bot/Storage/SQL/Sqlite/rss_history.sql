CREATE TABLE IF NOT EXISTS rss_history
(
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    chat_id      INTEGER,
    rss_source   TEXT,
    url          TEXT,
    title        TEXT,
    publish_date TEXT,
    author       TEXT,
    created_at   TEXT DEFAULT 'CURRENT_TIMESTAMP'
);
