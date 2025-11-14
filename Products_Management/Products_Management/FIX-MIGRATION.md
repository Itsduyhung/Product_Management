# 🔧 Hướng dẫn fix Migration - Tạo đầy đủ Tables trên Neon

## ❌ Vấn đề hiện tại:

1. **Connection String**: `appsettings.json` đang trỏ đến localhost, không phải Neon
2. **Migration thiếu**: Chỉ có migration `UpdateOrderCodeToString` (update cột), không có Initial Migration tạo toàn bộ tables
3. **Result**: Neon DB chỉ có 1 table (có thể là `__EFMigrationsHistory`)

## ✅ Giải pháp:

### Bước 1: Tạo Initial Migration (Tạo toàn bộ schema)

Migration hiện tại chỉ update cột, không tạo tables. Cần tạo Initial Migration:

```powershell
cd Products_Management\Products_Management

# Cách 1: Dùng script
.\create-initial-migration.ps1

# Cách 2: Chạy thủ công
dotnet ef migrations add InitialCreate
```

**Lưu ý**: Migration mới sẽ tạo:
- Table `Entities` (Products)
- Table `users`
- Table `Carts`
- Table `CartItems`
- Table `Order`
- Table `OrderItems`
- Table `__EFMigrationsHistory` (EF Core tracking)

### Bước 2: Lấy Neon Connection String

1. Đăng nhập [Neon Console](https://console.neon.tech)
2. Chọn project
3. Vào **Connection Details**
4. Copy connection string (PostgreSQL format)

### Bước 3: Cập nhật và chạy Migration

#### Option A: Dùng Script (Khuyến nghị)

1. Mở `run-migration-simple.ps1`
2. Cập nhật dòng 7:
   ```powershell
   $NeonConnectionString = "Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
   ```
3. Chạy:
   ```powershell
   .\run-migration-simple.ps1
   ```

#### Option B: Chạy thủ công

```powershell
cd Products_Management\Products_Management

dotnet ef database update --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

### Bước 4: Kiểm tra kết quả

Sau khi chạy migration, kiểm tra trên Neon Console:
- Phải có ít nhất 7 tables:
  1. `__EFMigrationsHistory`
  2. `Entities` (hoặc tên table Products)
  3. `users`
  4. `Carts`
  5. `CartItems`
  6. `Order`
  7. `OrderItems`

### Bước 5: Cập nhật appsettings.json (Tùy chọn)

Nếu muốn app tự động dùng Neon DB, cập nhật `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
  }
}
```

**Cảnh báo**: Nếu cập nhật appsettings.json, đảm bảo không commit connection string có password lên git!

## 🚨 Nếu Migration bị lỗi "Table already exists"

Nếu một số tables đã tồn tại trên Neon DB, có 2 cách:

### Cách 1: Xóa và tạo lại (Chỉ dùng cho Dev/Test)

```sql
-- Trên Neon SQL Editor, chạy lệnh này để xóa tất cả tables:
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
```

Sau đó chạy lại migration.

### Cách 2: Kiểm tra và skip (An toàn hơn)

1. Xem tables nào đã tồn tại trên Neon
2. Chỉnh sửa migration `InitialCreate` để skip các tables đã có
3. Hoặc xóa migration cũ và tạo lại

## 📋 Checklist

- [ ] Đã tạo Initial Migration
- [ ] Đã lấy Neon Connection String
- [ ] Đã cập nhật script với Neon Connection String
- [ ] Đã chạy migration script
- [ ] Đã kiểm tra tables trên Neon Console (phải có ít nhất 6 tables)
- [ ] Đã test kết nối database

## 💡 Tips

1. **Backup trước**: Nếu DB đã có data quan trọng, backup trước khi chạy migration
2. **Test local**: Test migration trên local DB trước khi chạy trên Neon
3. **Check logs**: Xem console logs khi chạy migration để biết lỗi (nếu có)
4. **Use script**: Script đã có error handling và validation, dùng script sẽ an toàn hơn

