# Hướng dẫn chạy Migration lên Neon Database

## 🚀 Cách 1: Sử dụng Script Đơn Giản (Khuyến nghị)

1. Mở file `run-migration-simple.ps1`
2. Tìm dòng này ở đầu file:
   ```powershell
   $NeonConnectionString = "Host=YOUR_HOST.neon.tech;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;"
   ```
3. Thay thế bằng connection string thật từ Neon:
   ```powershell
   $NeonConnectionString = "Host=ep-cool-darkness-123456.us-east-2.aws.neon.tech;Database=neondb;Username=duyhung;Password=abc123xyz;SSL Mode=Require;"
   ```
4. Chạy script:
   ```powershell
   .\run-migration-simple.ps1
   ```

## 🎯 Cách 2: Sử dụng Script Tương Tác

Script này sẽ hỏi bạn muốn:
- Sử dụng connection string từ `appsettings.json`
- Hoặc nhập connection string thủ công

```powershell
.\run-migration.ps1
```

Hoặc truyền connection string trực tiếp:
```powershell
.\run-migration.ps1 -ConnectionString "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## 📋 Cách 3: Chạy thủ công với dotnet CLI

### Bước 1: Cài đặt dotnet ef tool (nếu chưa có)
```powershell
dotnet tool install --global dotnet-ef
```

### Bước 2: Chạy migration với connection string Neon
```powershell
cd Products_Management\Products_Management

dotnet ef database update --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## 📊 Kiểm tra migrations

### Xem danh sách migrations:
```powershell
dotnet ef migrations list --connection "YOUR_NEON_CONNECTION_STRING"
```

### Kiểm tra migration status:
```powershell
dotnet ef migrations list --connection "YOUR_NEON_CONNECTION_STRING" --context ApplicationDbContext
```

## 🔧 Cấu hình appsettings.json

Nếu muốn app tự động chạy migration khi start, cập nhật `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
  }
}
```

Sau đó chỉ cần chạy:
```powershell
dotnet run
```

App sẽ tự động apply migrations chưa được chạy.

## 📝 Lấy Neon Connection String

1. Đăng nhập vào [Neon Console](https://console.neon.tech)
2. Chọn project của bạn
3. Vào tab **Connection Details**
4. Copy connection string (format PostgreSQL)
5. Đảm bảo có `SSL Mode=Require;` ở cuối

## ⚠️ Lưu ý

1. **SSL Mode**: Neon yêu cầu SSL, nên luôn thêm `SSL Mode=Require;` vào connection string
2. **Backup**: Trước khi chạy migration trên production, nên backup database
3. **Testing**: Test migration trên staging trước khi chạy production
4. **Rollback**: Nếu cần rollback, dùng:
   ```powershell
   dotnet ef database update PreviousMigrationName --connection "YOUR_CONNECTION_STRING"
   ```

## 🆘 Troubleshooting

### Lỗi: "dotnet ef command not found"
```powershell
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### Lỗi: "Connection timeout"
- Kiểm tra connection string
- Kiểm tra firewall/network
- Đảm bảo Neon project đang active

### Lỗi: "SSL connection required"
- Thêm `SSL Mode=Require;` vào cuối connection string

### Lỗi: "Table already exists"
- Có thể migration đã được chạy trước đó
- Kiểm tra bằng `dotnet ef migrations list`

