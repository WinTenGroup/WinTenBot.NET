CREATE TABLE IF NOT EXISTS warn_member_history (
    id                   INTEGER  PRIMARY KEY AUTOINCREMENT,
    from_id              INTEGER,
    first_name           TEXT,
    last_name            TEXT,
    step_count           INTEGER,
    reason_warn          TEXT,
    last_warn_message_id BIGINT   DEFAULT ( -1),
    warner_user_id       INTEGER,
    warner_first_name    TEXT,
    warner_last_name     TEXT,
    chat_id              BIGINT,
    created_at           DATETIME,
    updated_at           DATETIME
);