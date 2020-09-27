CREATE TABLE IF NOT EXISTS word_filter
(
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    word        VARCHAR(100),
    deep_filter BOOLEAN DEFAULT (False),
    is_global   BOOLEAN DEFAULT (False),
    from_id     VARCHAR(15),
    chat_id     VARCHAR(20),
    created_at  DATETIME
);
