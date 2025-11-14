# Fix PATH cho dotnet-ef tool

## Vấn đề:

Tool `dotnet-ef` đã được cài đặt nhưng không tìm thấy vì chưa có trong PATH.

## Giải pháp nhanh:

### Cách 1: Dùng script tự động (Khuyến nghị)

```powershell
cd Products_Management\Products_Management
.\fix-path-and-migrate.ps1
```

Script này sẽ:
- Tự động add PATH cho session hiện tại
- Lưu PATH vĩnh viễn (cho lần sau)
- Tạo Initial Migration
- Apply migrations lên Neon DB

### Cách 2: Fix PATH thủ công

#### Trong PowerShell hiện tại (tạm thời):
```powershell
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"
```

Sau đó thử lại:
```powershell
dotnet ef --version
dotnet ef migrations add InitialCreate
```

#### Lưu PATH vĩnh viễn:
```powershell
# Thêm vào PATH vĩnh viễn
setx PATH "$env:PATH;C:\Users\duyhu\.dotnet\tools"
```

**Lưu ý**: Sau khi chạy `setx`, cần đóng và mở lại terminal để PATH có hiệu lực.

### Cách 3: Dùng dotnet-ef trực tiếp (không cần PATH)

Thay vì `dotnet ef`, dùng `dotnet-ef` trực tiếp:

```powershell
# Thêm PATH cho session hiện tại
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"

# Hoặc dùng đường dẫn đầy đủ
C:\Users\duyhu\.dotnet\tools\dotnet-ef.exe migrations add InitialCreate
```

### Cách 4: Dùng Visual Studio Package Manager Console

1. Mở Visual Studio
2. Tools → NuGet Package Manager → Package Manager Console
3. Chạy:
   ```powershell
   Add-Migration InitialCreate
   Update-Database -Connection "YOUR_NEON_CONNECTION_STRING"
   ```

## Quy trình đầy đủ sau khi fix PATH:

### Bước 1: Fix PATH (chỉ cần làm 1 lần)

```powershell
# Trong PowerShell hiện tại
$env:PATH += ";C:\Users\duyhu\.dotnet\tools"

# Lưu vĩnh viễn (cho lần sau)
setx PATH "$env:PATH;C:\Users\duyhu\.dotnet\tools"
```

### Bước 2: Tạo Initial Migration

```powershell
cd Products_Management\Products_Management
dotnet ef migrations add InitialCreate
```

### Bước 3: Apply lên Neon DB

```powershell
# Cách A: Dùng script
.\run-migration-simple.ps1

# Cách B: Thủ công
dotnet ef database update --connection "Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;"
```

## Kiểm tra:

Sau khi fix PATH, kiểm tra:

```powershell
dotnet ef --version
```

Nếu hiển thị version, nghĩa là đã fix xong!

