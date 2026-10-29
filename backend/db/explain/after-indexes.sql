SET client_encoding = 'UTF8';

\echo '=== A1: sort by created_at desc, limit 20 ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT p0.id, p0.sku, p0.name, p0.price, p0.is_active, p0.created_at_utc, p0.updated_at_utc, p0.category_id, c.name
FROM (SELECT id, sku, name, price, is_active, created_at_utc, updated_at_utc, category_id
      FROM products ORDER BY created_at_utc DESC, id DESC LIMIT 20 OFFSET 0) AS p0
INNER JOIN categories AS c ON p0.category_id = c.id
ORDER BY p0.created_at_utc DESC, p0.id DESC;

\echo '=== A2: is_active=true, sort by price desc, offset 40 ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT p0.id, p0.sku, p0.name, p0.price, p0.is_active, p0.created_at_utc, p0.updated_at_utc, p0.category_id, c.name
FROM (SELECT id, sku, name, price, is_active, created_at_utc, updated_at_utc, category_id
      FROM products WHERE is_active = true
      ORDER BY price DESC, id DESC LIMIT 20 OFFSET 40) AS p0
INNER JOIN categories AS c ON p0.category_id = c.id
ORDER BY p0.price DESC, p0.id DESC;

\echo '=== A3: category + is_active, sort by price, page ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT p0.id, p0.sku, p0.name, p0.price, p0.is_active, p0.created_at_utc, p0.updated_at_utc, p0.category_id, c.name
FROM (SELECT id, sku, name, price, is_active, created_at_utc, updated_at_utc, category_id
      FROM products
      WHERE category_id = (SELECT id FROM categories WHERE name = 'Электроника') AND is_active = true
      ORDER BY price DESC, id DESC LIMIT 20 OFFSET 0) AS p0
INNER JOIN categories AS c ON p0.category_id = c.id
ORDER BY p0.price DESC, p0.id DESC;

\echo '=== A4: search word, page ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT p0.id, p0.sku, p0.name, p0.price, p0.is_active, p0.created_at_utc, p0.updated_at_utc, p0.category_id, c.name
FROM (SELECT id, sku, name, price, is_active, created_at_utc, updated_at_utc, category_id
      FROM products
      WHERE (name ILIKE '%ноутбук pro%' ESCAPE '\' OR sku ILIKE '%ноутбук pro%' ESCAPE '\')
      ORDER BY created_at_utc DESC, id DESC LIMIT 20 OFFSET 0) AS p0
INNER JOIN categories AS c ON p0.category_id = c.id
ORDER BY p0.created_at_utc DESC, p0.id DESC;

\echo '=== A4b: search word count ==='
EXPLAIN (ANALYZE) SELECT count(*) FROM products
WHERE (name ILIKE '%ноутбук pro%' ESCAPE '\' OR sku ILIKE '%ноутбук pro%' ESCAPE '\');

\echo '=== A5: sku prefix search count ==='
EXPLAIN (ANALYZE) SELECT count(*) FROM products
WHERE (name ILIKE '%SKU-00005%' ESCAPE '\' OR sku ILIKE '%SKU-00005%' ESCAPE '\');

\echo '=== A6: sort by name desc, limit 20 ==='
EXPLAIN (ANALYZE) SELECT id FROM products ORDER BY name DESC, id DESC LIMIT 20;
