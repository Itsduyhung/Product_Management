# Quick Fix - Apply Migration lên Neon

## ✅ Initial Migration đã được tạo!

Migration `20251113212223_InitialCreate` đã được tạo thành công với đầy đủ tables.

## 🚀 Cách apply migration lên Neon DB:

### Bước 1: Cập nhật Neon Connection String

1. Mở file `run-migration-simple.ps1`
2. Tìm dòng 9:
   ```powershell
   $NeonConnectionString = "Host=YOUR_HOST.neon.tech;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;"
   ```
3. Thay bằng connection string thật từ Neon Console

### Bước 2: Apply Migration

**Cách A: Dùng script (sau khi cập nhật connection string)**
```powershell
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"
.\run-migration-simple.ps1
```

**Cách B: Chạy thủ công (khuyến nghị nếu script lỗi)**
```powershell
# Fix PATH
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"

# Apply migration với Neon connection string
dotnet ef database update --project Products_Management.csproj --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## ⚠️ Lưu ý:

1. **Lệnh đúng**: `dotnet ef database update` (không phải `dotnet ef update database`)
2. **Connection String**: Phải có `SSL Mode=Require;` ở cuối
3. **PATH**: Nhớ add PATH trước khi chạy: `$env:PATH += ";C:\Users\duyhu\.dotnet\tools"`

## 📊 Sau khi chạy migration thành công:

Kiểm tra trên Neon Console phải có các tables:
- `__EFMigrationsHistory`
- `Cart`
- `CartItem`
- `Order`
- `OrderItem`
- `products`
- `users`

