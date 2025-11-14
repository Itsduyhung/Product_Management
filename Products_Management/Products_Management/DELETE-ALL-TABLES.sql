-- Script xóa TẤT CẢ tables và data trên Neon DB
-- Chạy script này trên Neon SQL Editor để xóa hoàn toàn

-- Tắt foreign key checks (PostgreSQL không cần, nhưng để chắc chắn)
-- Xóa tất cả tables trong schema public
DO $$ 
DECLARE
    r RECORD;
BEGIN
    -- Xóa tất cả tables
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') 
    LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END $$;

-- Xóa tất cả sequences (nếu có)
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT sequence_name FROM information_schema.sequences WHERE sequence_schema = 'public') 
    LOOP
        EXECUTE 'DROP SEQUENCE IF EXISTS public.' || quote_ident(r.sequence_name) || ' CASCADE';
    END LOOP;
END $$;

-- Xóa tất cả views (nếu có)
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT table_name FROM information_schema.views WHERE table_schema = 'public') 
    LOOP
        EXECUTE 'DROP VIEW IF EXISTS public.' || quote_ident(r.table_name) || ' CASCADE';
    END LOOP;
END $$;

-- Reset schema hoàn toàn
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;

-- Kiểm tra kết quả (phải không còn table nào)
SELECT COUNT(*) as remaining_tables
FROM information_schema.tables 
WHERE table_schema = 'public';
-- Kết quả phải là 0

