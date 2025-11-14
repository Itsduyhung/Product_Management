# Hướng dẫn chạy Migration lên Neon Database

## Cách 1: Chạy Migration với Connection String Neon (Khuyến nghị)

Nếu bạn muốn chạy migration thủ công với connection string Neon:

```bash
cd Products_Management/Products_Management

# Thay YOUR_NEON_CONNECTION_STRING bằng connection string thật từ Neon
dotnet ef database update --connection "YOUR_NEON_CONNECTION_STRING"
```

**Ví dụ connection string Neon:**
```
Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=user;Password=password;SSL Mode=Require;
```

## Cách 2: Cập nhật appsettings.json với Neon Connection String

1. Mở file `appsettings.json` hoặc `appsettings.Production.json`
2. Cập nhật `ConnectionStrings:DefaultConnection` với connection string từ Neon:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=user;Password=password;SSL Mode=Require;"
  }
}
```

3. Sau đó chạy migration:
```bash
cd Products_Management/Products_Management
dotnet ef database update
```

## Cách 3: Sử dụng Environment Variable (Production)

Thiết lập environment variable:
```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=ep-xxx...;Database=neondb;..."

# Linux/Mac
export ConnectionStrings__DefaultConnection="Host=ep-xxx...;Database=neondb;..."
```

Sau đó chạy:
```bash
dotnet ef database update
```

## Lưu ý:

1. **Tự động Migration**: Code đã được cập nhật để tự động chạy migration khi app start. Điều này có nghĩa là:
   - Khi bạn chạy `dotnet run`, nó sẽ tự động check và apply migrations chưa được chạy
   - Nếu Neon DB chưa có migrations, nó sẽ tự động tạo schema

2. **Kiểm tra migrations đã được apply**:
   ```bash
   dotnet ef migrations list --connection "YOUR_NEON_CONNECTION_STRING"
   ```

3. **Tạo migration mới** (nếu có thay đổi model):
   ```bash
   dotnet ef migrations add MigrationName --connection "YOUR_NEON_CONNECTION_STRING"
   ```

4. **Rollback migration** (nếu cần):
   ```bash
   dotnet ef database update PreviousMigrationName --connection "YOUR_NEON_CONNECTION_STRING"
   ```

## Để lấy Neon Connection String:

1. Đăng nhập vào Neon Console: https://console.neon.tech
2. Chọn project của bạn
3. Vào tab "Connection Details"
4. Copy connection string và paste vào code

