-- Script xóa table "Cart" trên LOCALHOST PostgreSQL
-- Chạy script này trên localhost nếu cần

-- Kết nối tới localhost PostgreSQL và chạy:
-- psql -U postgres -d PRN_ASS1 -f DELETE-CART-LOCALHOST.sql

-- Hoặc chạy trực tiếp:
DROP TABLE IF EXISTS "CartItem" CASCADE;
DROP TABLE IF EXISTS "Cart" CASCADE;
DROP TABLE IF EXISTS "OrderItem" CASCADE;
DROP TABLE IF EXISTS "Order" CASCADE;
DROP TABLE IF EXISTS "products" CASCADE;
DROP TABLE IF EXISTS "users" CASCADE;
DROP TABLE IF EXISTS "__EFMigrationsHistory" CASCADE;

-- Kiểm tra xem đã xóa chưa
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;

