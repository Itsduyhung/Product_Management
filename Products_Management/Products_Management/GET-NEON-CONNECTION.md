# Hướng dẫn lấy Neon Connection String

## Bước 1: Lấy Connection String từ Neon Console

1. **Đăng nhập** vào [Neon Console](https://console.neon.tech)
2. **Chọn project** của bạn
3. Vào tab **Connection Details** hoặc **Dashboard**
4. Tìm phần **Connection string** (PostgreSQL format)
5. **Copy connection string**

Connection string sẽ có dạng như sau:
```
postgresql://username:password@ep-xxx-xxx.us-east-2.aws.neon.tech/neondb?sslmode=require
```

Hoặc dạng khác:
```
Host=ep-xxx-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=username;Password=password;SSL Mode=Require;
```

## Bước 2: Convert sang format cho EF Core (nếu cần)

Nếu Neon cung cấp dạng PostgreSQL URL, convert sang format cho EF Core:

**Format PostgreSQL URL:**
```
postgresql://username:password@ep-xxx.us-east-2.aws.neon.tech/neondb?sslmode=require
```

**Convert sang EF Core format:**
```
Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=username;Password=password;SSL Mode=Require;
```

## Bước 3: Chạy Migration

### Cách A: Thay trực tiếp trong lệnh

```powershell
# Fix PATH
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"

# Thay YOUR_CONNECTION_STRING bằng connection string thật
dotnet ef database update --project Products_Management.csproj --connection "YOUR_CONNECTION_STRING"
```

**Ví dụ:**
```powershell
dotnet ef database update --project Products_Management.csproj --connection "Host=ep-cool-darkness-123456.us-east-2.aws.neon.tech;Database=neondb;Username=duyhung;Password=abc123xyz;SSL Mode=Require;"
```

### Cách B: Cập nhật trong script

1. Mở file `run-migration-simple.ps1`
2. Tìm dòng 9:
   ```powershell
   $NeonConnectionString = "Host=YOUR_HOST.neon.tech;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;"
   ```
3. Thay bằng connection string thật:
   ```powershell
   $NeonConnectionString = "Host=ep-cool-darkness-123456.us-east-2.aws.neon.tech;Database=neondb;Username=duyhung;Password=abc123xyz;SSL Mode=Require;"
   ```
4. Chạy script:
   ```powershell
   $env:PATH += ";C:\Users\duyhu\.dotnet\tools"
   .\run-migration-simple.ps1
   ```

## ⚠️ Lưu ý:

1. **KHÔNG commit connection string có password lên git!**
2. **SSL Mode**: Phải có `SSL Mode=Require;` ở cuối
3. **Host format**: Thường là `ep-xxx-xxx.us-east-2.aws.neon.tech` (không phải `ep-xxx.neon.tech`)

## 🔒 Bảo mật:

Nếu deploy production, nên dùng Environment Variable thay vì hardcode trong file:
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
dotnet ef database update --project Products_Management.csproj
```

