-- Script kiểm tra tables hiện có trên Neon DB
-- Chạy trên Neon SQL Editor để xem tables nào đang tồn tại

-- Xem tất cả tables trong schema public
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;

-- Xem migrations đã được đánh dấu
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";

-- Đếm số tables
SELECT COUNT(*) as total_tables
FROM information_schema.tables 
WHERE table_schema = 'public';

