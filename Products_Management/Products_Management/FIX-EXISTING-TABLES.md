# Fix lỗi "relation already exists" khi migration

## ❌ Vấn đề:

Table "Cart" (hoặc các tables khác) đã tồn tại trên Neon DB, nhưng migration chưa được đánh dấu là đã apply.

Lỗi: `42P07: relation "Cart" already exists`

## ✅ Giải pháp:

### Cách 1: Mark migrations đã apply (Giữ nguyên data - Khuyến nghị)

Nếu tables đã tồn tại và bạn muốn giữ nguyên data:

1. **Vào Neon Console**: https://console.neon.tech
2. **Chọn project** → **SQL Editor**
3. **Copy và chạy** script từ file `mark-migrations-applied.sql`:
   ```sql
   CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
       "MigrationId" character varying(150) NOT NULL,
       "ProductVersion" character varying(32) NOT NULL,
       CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
   );

   INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
   VALUES 
       ('20231025_UpdateOrderCodeToString', '9.0.9'),
       ('20251113212223_InitialCreate', '9.0.9')
   ON CONFLICT ("MigrationId") DO NOTHING;
   ```

4. **Kiểm tra** migrations đã được đánh dấu:
   ```sql
   SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
   ```

5. **Sau đó chạy migration lại** (sẽ không tạo lại tables vì đã được đánh dấu):
   ```powershell
   $env:PATH += ";C:\Users\duyhu\.dotnet\tools"
   dotnet ef database update --project Products_Management.csproj
   ```

### Cách 2: Xóa tất cả tables và chạy lại (MẤT DATA - Chỉ dùng nếu DB chưa có data quan trọng)

Nếu DB chưa có data quan trọng và bạn muốn reset hoàn toàn:

1. **Vào Neon Console** → **SQL Editor**
2. **Copy và chạy** script từ file `RESET-DB.sql`:
   ```sql
   DROP SCHEMA public CASCADE;
   CREATE SCHEMA public;
   GRANT ALL ON SCHEMA public TO postgres;
   GRANT ALL ON SCHEMA public TO public;
   ```

3. **Sau đó chạy migration**:
   ```powershell
   $env:PATH += ";C:\Users\duyhu\.dotnet\tools"
   dotnet ef database update --project Products_Management.csproj
   ```

### Cách 3: Dùng script PowerShell tự động

Chạy script sẽ hướng dẫn bạn:
```powershell
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"
.\fix-existing-tables.ps1
```

## 📊 Kiểm tra kết quả:

Sau khi fix, kiểm tra trên Neon Console:
```sql
-- Xem tất cả tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public'
ORDER BY table_name;

-- Xem migrations đã apply
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

Phải thấy các tables:
- `__EFMigrationsHistory`
- `Cart`
- `CartItem`
- `Order`
- `OrderItem`
- `products`
- `users`

## 💡 Khuyến nghị:

**Dùng Cách 1** (mark migrations as applied) vì:
- Giữ nguyên data hiện có
- An toàn hơn
- Không mất công setup lại

Chỉ dùng Cách 2 nếu:
- DB chưa có data quan trọng
- Bạn muốn reset hoàn toàn để test

