-- Сколько чисел рандом выдал на выбор правил: с этого места он продолжится, если не прошло ни одного дня.
ALTER TABLE semesters ADD COLUMN random_draws integer NOT NULL DEFAULT 0;
