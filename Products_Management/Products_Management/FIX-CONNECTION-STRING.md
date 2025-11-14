# 🔧 Fix Connection String Issue

## ❌ Vấn đề:

Migration đang kết nối tới **localhost PostgreSQL** thay vì **Neon DB**!

File `appsettings.Development.json` hiện có:
```json
"DefaultConnection": "Host=localhost;Database=PRN_ASS1;Username=postgres;Password=2402"
```

## ✅ Giải pháp:

### Cách 1: Update Connection String trong `appsettings.Development.json`

1. Mở file `appsettings.Development.json`
2. Thay connection string bằng Neon connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
  }
}
```

### Cách 2: Dùng `appsettings.json` (không phải Development)

Nếu bạn đã update connection string trong `appsettings.json` nhưng migration vẫn dùng `appsettings.Development.json`, hãy:

1. Kiểm tra connection string trong `appsettings.json`
2. Chạy migration với environment không phải Development:
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"
dotnet ef database update --project Products_Management.csproj
```

### Cách 3: Chỉ định connection string trực tiếp

```powershell
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"
dotnet ef database update --project Products_Management.csproj --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## 🔍 Kiểm tra:

1. Kiểm tra connection string đang được dùng:
```powershell
# Xem connection string từ appsettings.json
Get-Content Products_Management.csproj\appsettings.json | Select-String "ConnectionStrings"
```

2. Xóa table "Cart" trên Neon DB (nếu có):
   - Chạy script `DELETE-CART-TABLE.sql` trên Neon SQL Editor

3. Xóa table "Cart" trên localhost DB (nếu cần):
   ```sql
   DROP TABLE IF EXISTS "Cart" CASCADE;
   ```

## 💡 Lưu ý:

- Migration trong môi trường Development sẽ đọc `appsettings.Development.json`
- Migration trong môi trường Production sẽ đọc `appsettings.json`
- Nếu không set environment, mặc định là Development

