-- SQL Script để đánh dấu migrations đã được apply trên Neon DB
-- Chạy script này trên Neon SQL Editor nếu tables đã tồn tại nhưng migrations chưa được đánh dấu

-- Đảm bảo bảng migrations history tồn tại
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Đánh dấu migrations đã được apply (chỉ insert nếu chưa tồn tại)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
    ('20231025_UpdateOrderCodeToString', '9.0.9'),
    ('20251113212223_InitialCreate', '9.0.9')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Kiểm tra kết quả
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";

