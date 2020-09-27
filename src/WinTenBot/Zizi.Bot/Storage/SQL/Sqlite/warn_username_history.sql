CREATE TABLE IF NOT EXISTS warn_username_history
(
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    from_id              INTEGER,
    first_name           VARCHAR,
    last_name            VARCHAR,
    step_count           INTEGER,
    chat_id              INTEGER,
    last_warn_message_id Integer DEFAULT - 1,
    created_at           DATETIME,
    updated_at           DATETIME
);
