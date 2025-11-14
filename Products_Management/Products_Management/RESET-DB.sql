-- SQL Script để XÓA TẤT CẢ tables và data (CHỈ DÙNG NẾU DB CHƯA CÓ DATA QUAN TRỌNG)
-- Chạy script này trên Neon SQL Editor nếu muốn reset DB hoàn toàn

-- Xóa toàn bộ schema và tạo lại
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;

-- Sau khi chạy script này, quay lại và chạy:
-- dotnet ef database update --project Products_Management.csproj

