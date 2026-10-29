SET client_encoding = 'UTF8';

BEGIN;

TRUNCATE product_price_history, products, categories;

INSERT INTO categories (id, name, created_at_utc)
SELECT gen_random_uuid(), n, TIMESTAMP WITH TIME ZONE '2025-01-01T00:00:00Z'
FROM unnest(ARRAY[
    'Электроника', 'Книги', 'Мебель', 'Инструменты', 'Продукты',
    'Одежда', 'Спорт', 'Сад', 'Игрушки', 'Автотовары'
]) AS n;

SELECT setseed(0.42);

INSERT INTO products (id, sku, name, description, category_id, price, is_active, created_at_utc)
WITH cat AS (
    SELECT id, row_number() OVER (ORDER BY name) AS rn FROM categories
)
SELECT
    gen_random_uuid(),
    'SKU-' || lpad(gs::text, 7, '0'),
    CASE gs % 8
        WHEN 0 THEN 'Ноутбук Pro ' || gs
        WHEN 1 THEN 'Гаджет X ' || gs
        WHEN 2 THEN 'Стул деревянный ' || gs
        WHEN 3 THEN 'Книга "PostgreSQL" ' || gs
        WHEN 4 THEN 'Дрель ударная ' || gs
        WHEN 5 THEN 'Кроссовки беговые ' || gs
        WHEN 6 THEN 'Товар 100% натуральный ' || gs
        ELSE 'Мяч футбольный ' || gs
    END,
    CASE WHEN gs % 3 = 0 THEN 'Описание товара ' || gs ELSE NULL END,
    cat.id,
    round((random() * 500)::numeric, 2),
    gs % 15 <> 0,
    TIMESTAMP WITH TIME ZONE '2025-01-01T00:00:00Z'
        + gs * INTERVAL '1 minute' + (random() * 60) * INTERVAL '1 second'
FROM generate_series(1, 50000) AS gs
JOIN cat ON cat.rn = 1 + (gs % 10);

SELECT setseed(0.99);
INSERT INTO product_price_history (id, product_id, old_price, new_price, changed_at_utc)
WITH numbered AS (
    SELECT p.id, p.created_at_utc, p.price,
           row_number() OVER (ORDER BY p.sku) AS rn
    FROM products p
),
targets AS (
    SELECT id, created_at_utc, price,
           1 + (floor(random() * 3)::int) AS changes
    FROM numbered
    WHERE rn % 5 <> 4
)
SELECT
    gen_random_uuid(),
    t.id,
    t.price,
    round((random() * 500)::numeric, 2),
    t.created_at_utc + gs * INTERVAL '1 day'
FROM targets t
CROSS JOIN LATERAL generate_series(1, t.changes) AS gs;

COMMIT;

ANALYZE categories;
ANALYZE products;
ANALYZE product_price_history;

SELECT count(*) AS products FROM products;
SELECT count(*) AS history_rows FROM product_price_history;
