# 🔧 Fix CORS and 500 Internal Server Error

## ✅ Đã sửa:

### 1. **CORS Configuration**
   - **Trước:** CORS policy dùng `SetIsOriginAllowed` với custom logic có thể gây conflict
   - **Sau:** 
     - **Development:** Cho phép cụ thể `http://localhost:3000`, `http://localhost:3001`
     - **Production:** Cho phép tất cả origins (vì backend trên Render cần accept từ nhiều frontend URLs)
   - **Lưu ý:** CORS middleware đã được đặt đúng vị trí trong pipeline (trước `UseRouting` và `UseAuthentication`)

### 2. **Global Exception Handler**
   - Thêm try-catch global để bắt tất cả exceptions
   - Log chi tiết errors ra console
   - Trả về JSON error response thay vì crash

### 3. **Products Controller Error Handling**
   - Thêm try-catch trong `GetAll()` method
   - Log errors và trả về proper error response

## 🔍 Kiểm tra trên Render:

### Environment Variables cần set trên Render:
1. **ConnectionStrings__DefaultConnection** = Connection string Neon DB
2. **ASPNETCORE_ENVIRONMENT** = `Production` (hoặc để default)
3. **PayOSSettings__ClientId** = PayOS Client ID
4. **PayOSSettings__ApiKey** = PayOS API Key
5. **PayOSSettings__ChecksumKey** = PayOS Checksum Key
6. **Jwt__Key** = JWT secret key
7. **Jwt__Issuer** = JWT issuer
8. **Jwt__Audience** = JWT audience

### Cách set trên Render:
1. Vào Render Dashboard → chọn service
2. Settings → Environment Variables
3. Add từng variable ở trên

### Kiểm tra logs trên Render:
1. Vào Render Dashboard → chọn service
2. Logs tab
3. Xem có errors về:
   - Database connection
   - CORS issues
   - Missing environment variables

## 🧪 Test:

### Test CORS:
```bash
# Test từ localhost:3000
curl -H "Origin: http://localhost:3000" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     https://product-management-4.onrender.com/api/products \
     -v
```

### Test API:
```bash
# Test GET products
curl https://product-management-4.onrender.com/api/products
```

## 🐛 Nếu vẫn gặp lỗi:

### 500 Internal Server Error:
1. **Kiểm tra database connection string trên Render**
   - Đảm bảo environment variable `ConnectionStrings__DefaultConnection` đã được set đúng
   - Format: `Host=ep-xxx.neon.tech;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require;Trust Server Certificate=true`

2. **Kiểm tra database migration**
   - Đảm bảo tables đã được tạo trên Neon DB
   - Chạy lại migration nếu cần: `dotnet ef database update`

3. **Kiểm tra logs trên Render**
   - Xem chi tiết error trong logs
   - Error message sẽ hiển thị nguyên nhân cụ thể

### CORS Error:
1. **Kiểm tra frontend URL**
   - Đảm bảo frontend đang chạy trên `http://localhost:3000`
   - Nếu dùng URL khác, thêm vào CORS policy

2. **Kiểm tra backend environment**
   - Development: Dùng policy `AllowReactApp` (chỉ cho localhost)
   - Production: Dùng policy `AllowAll` (cho tất cả origins)

## 📝 Notes:

- **CORS Policy "AllowAll"** dùng `AllowAnyOrigin()` nên **KHÔNG THỂ** dùng với `AllowCredentials()`
- Nếu cần credentials trong Production, phải specify exact origins thay vì `AllowAnyOrigin()`
- Backend trên Render đang dùng Production environment nên sẽ dùng policy `AllowAll`

