# 🔧 Fix Deploy Errors on Render

## ❌ Lỗi gặp phải:

```
error CS0246: The type or namespace name 'Net' could not be found
error CS0234: The type or namespace name 'JwtBearer' does not exist
error CS0234: The type or namespace name 'IdentityModel' does not exist
```

## ✅ Đã sửa:

### 1. **Thêm Missing Packages**
   - ✅ `Microsoft.IdentityModel.Tokens` (version 8.3.2)
   - ✅ `System.Net.Http` (version 4.3.4)

### 2. **Loại bỏ PayOS SDK Dependency**
   - ❌ Xóa `using Net.payOS;` và `using Net.payOS.Types;` từ `OrderController.cs`
   - ✅ Sửa `PayOSWebhookRequest.cs` để không dùng `WebhookType` từ PayOS SDK
   - ✅ Project đã tự implement PayOS integration trong `PayOSService.cs` nên không cần SDK

### 3. **Files đã thay đổi:**
   - `Products_Management.csproj` - Thêm packages
   - `Controller/OrderController.cs` - Xóa using không cần thiết
   - `DTOs/Request/PayOSWebhookRequest.cs` - Loại bỏ dependency PayOS SDK

## 📦 Packages hiện có trong project:

```xml
- CloudinaryDotNet (1.27.7)
- FluentValidation.AspNetCore (11.3.1)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.14)
- Microsoft.EntityFrameworkCore.* (9.0.9)
- Microsoft.IdentityModel.Tokens (8.3.2) ⬅️ THÊM MỚI
- Npgsql.EntityFrameworkCore.PostgreSQL (9.0.4)
- payOS (1.0.9) ⬅️ KHÔNG CÒN DÙNG, CÓ THỂ XÓA
- Swashbuckle.AspNetCore (6.6.2)
- System.IdentityModel.Tokens.Jwt (8.14.0)
- System.Net.Http (4.3.4) ⬅️ THÊM MỚI
```

## 🧪 Test build local:

```bash
cd Products_Management/Products_Management
dotnet clean
dotnet restore
dotnet build
```

Nếu build thành công, có thể deploy lên Render.

## 🚀 Deploy lại:

1. Commit và push code:
   ```bash
   git add .
   git commit -m "Fix missing packages and remove PayOS SDK dependency"
   git push
   ```

2. Render sẽ tự động build lại

3. Kiểm tra logs trên Render để đảm bảo build thành công

## 💡 Lưu ý:

- **PayOS Package:** Package `payOS` vẫn còn trong `.csproj` nhưng không được sử dụng. Có thể xóa nó để giảm dependencies:
  ```xml
  <!-- Có thể xóa dòng này -->
  <PackageReference Include="payOS" Version="1.0.9" />
  ```

- **Microsoft.IdentityModel.Tokens:** Cần thiết cho JWT authentication

- **System.Net.Http:** Cần thiết cho HTTP client trong PayOSService

