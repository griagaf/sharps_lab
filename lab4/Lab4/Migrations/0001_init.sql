CREATE TABLE semesters (
    id         serial      PRIMARY KEY,
    seed       integer     NOT NULL,
    started_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE teacher_rules (
    semester_id integer NOT NULL REFERENCES semesters (id) ON DELETE CASCADE,
    subject     text    NOT NULL,
    kind        text    NOT NULL,   -- Random, CopyYesterday, XorYesterday
    subject_a   text,
    subject_b   text,
    PRIMARY KEY (semester_id, subject),
    CHECK (
           (kind = 'Random'        AND subject_a IS NULL     AND subject_b IS NULL)
        OR (kind = 'CopyYesterday' AND subject_a IS NOT NULL AND subject_b IS NULL)
        OR (kind = 'XorYesterday'  AND subject_a IS NOT NULL AND subject_b IS NOT NULL AND subject_a <> subject_b)
    )
);

CREATE TABLE days (
    semester_id    integer   NOT NULL REFERENCES semesters (id) ON DELETE CASCADE,
    day            integer   NOT NULL,
    total_pleasure integer   NOT NULL,
    is_expelled    boolean   NOT NULL,
    go_to_lesson   boolean[] NOT NULL,  -- решение студента по каждому предмету
    asked          boolean[] NOT NULL,  -- спросили ли по каждому предмету
    random_draws   integer   NOT NULL,  -- сколько чисел выдал рандом к концу дня
    PRIMARY KEY (semester_id, day)
);
