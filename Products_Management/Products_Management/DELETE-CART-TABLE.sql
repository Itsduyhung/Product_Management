-- Script xóa table "Cart" cụ thể và kiểm tra trong TẤT CẢ schemas
-- Chạy script này trên Neon SQL Editor

-- 1. Kiểm tra table "Cart" trong schema public
SELECT table_schema, table_name 
FROM information_schema.tables 
WHERE table_name = 'Cart' 
ORDER BY table_schema;

-- 2. Xóa table "Cart" trong schema public (nếu có)
DROP TABLE IF EXISTS public."Cart" CASCADE;

-- 3. Xóa table "Cart" trong TẤT CẢ schemas (để chắc chắn)
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (
        SELECT table_schema, table_name 
        FROM information_schema.tables 
        WHERE table_name = 'Cart'
    ) 
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.table_schema) || '.' || quote_ident(r.table_name) || ' CASCADE';
        RAISE NOTICE 'Dropped table: %.%', r.table_schema, r.table_name;
    END LOOP;
END $$;

-- 4. Xóa tất cả tables (để chắc chắn)
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') 
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
        RAISE NOTICE 'Dropped table: %', r.tablename;
    END LOOP;
END $$;

-- 5. Kiểm tra lại xem còn table "Cart" không
SELECT table_schema, table_name 
FROM information_schema.tables 
WHERE table_name = 'Cart';

-- 6. Kiểm tra tất cả tables còn lại
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;

