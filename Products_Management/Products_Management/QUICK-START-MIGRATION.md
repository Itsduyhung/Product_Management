# 🚀 Quick Start - Chạy Migration lên Neon DB

## ⚠️ Lỗi "dotnet ef command not found"

Nếu gặp lỗi này, đây là các cách fix:

### Cách 1: Cài đặt Package trực tiếp vào Project (Đơn giản nhất)

Vì project đã có `Microsoft.EntityFrameworkCore.Tools` package, bạn có thể dùng Package Manager Console trong Visual Studio:

1. Mở Visual Studio
2. Tools → NuGet Package Manager → Package Manager Console
3. Đảm bảo project được chọn trong dropdown
4. Chạy lệnh:
   ```powershell
   Add-Migration InitialCreate
   Update-Database -Connection "YOUR_NEON_CONNECTION_STRING"
   ```

### Cách 2: Dùng script đã tạo (Tự động fix mọi thứ)

Chạy script tổng hợp:
```powershell
cd Products_Management\Products_Management
.\setup-and-migrate.ps1
```

Script này sẽ:
- Tự động cài đặt dotnet ef tool (nếu thiếu)
- Tạo Initial Migration
- Apply migrations lên Neon DB

### Cách 3: Cài đặt thủ công với version cụ thể

```powershell
# Cài đặt version 8.0 (tương thích với .NET 9)
dotnet tool install --global dotnet-ef --version 8.0.0

# Hoặc cài đặt local vào project
cd Products_Management\Products_Management
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.0
```

### Cách 4: Dùng Visual Studio (Nếu có)

1. Mở project trong Visual Studio
2. Mở **Package Manager Console** (Tools → NuGet → Package Manager Console)
3. Chạy:
   ```powershell
   Add-Migration InitialCreate
   Update-Database -Connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
   ```

## 📋 Quy trình đầy đủ

### Bước 1: Lấy Neon Connection String

1. Vào [Neon Console](https://console.neon.tech)
2. Chọn project
3. Vào **Connection Details**
4. Copy connection string (PostgreSQL format)

### Bước 2: Tạo Initial Migration

Bạn đã ở đúng thư mục `Products_Management\Products_Management`, chỉ cần:

```powershell
# Cách A: Dùng script
.\setup-and-migrate.ps1

# Cách B: Thủ công (sau khi cài đặt dotnet ef)
dotnet ef migrations add InitialCreate
```

### Bước 3: Apply Migration lên Neon

```powershell
# Cách A: Dùng script (khuyến nghị)
.\run-migration-simple.ps1

# Cách B: Thủ công
dotnet ef database update --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## ✅ Kiểm tra kết quả

Sau khi chạy migration, kiểm tra trên Neon Console phải có các tables:
- `__EFMigrationsHistory`
- `Entities` (Products)
- `users`
- `Carts`
- `CartItems`
- `Order`
- `OrderItems`

## 💡 Tips

1. **Bạn đã ở đúng thư mục**: Không cần `cd` nữa, đang ở `Products_Management\Products_Management`
2. **Connection String**: Phải có `SSL Mode=Require;` ở cuối
3. **Initial Migration**: Migration hiện tại chỉ update cột, cần tạo Initial Migration để tạo toàn bộ tables

