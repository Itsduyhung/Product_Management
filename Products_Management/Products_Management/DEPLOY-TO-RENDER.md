# 🚀 Hướng dẫn Deploy Backend lên Render

## 📋 Bước 1: Chuẩn bị Repository

1. **Push code lên GitHub/GitLab:**
   ```bash
   git add .
   git commit -m "Fix CORS and add error handling"
   git push origin main
   ```

## 📋 Bước 2: Tạo Service trên Render

1. Vào https://dashboard.render.com
2. Click **"New +"** → **"Web Service"**
3. Connect repository (GitHub/GitLab)
4. Chọn repository của bạn

## 📋 Bước 3: Cấu hình Service

### Build & Start Commands:
- **Build Command:** `dotnet publish -c Release -o ./publish`
- **Start Command:** `cd publish && dotnet Products_Management.dll`

### Hoặc đơn giản hơn:
- **Build Command:** (để trống hoặc `dotnet build`)
- **Start Command:** `dotnet run --project Products_Management.csproj`

## 📋 Bước 4: Set Environment Variables

Trong **Environment** tab, thêm các biến sau:

### Database:
```
ConnectionStrings__DefaultConnection=Host=ep-xxx.neon.tech;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require;Trust Server Certificate=true
```

### JWT:
```
Jwt__Key=Product_Management_2025_This_Is_A_Secure_Key_With_At_Least_32_Chars!!
Jwt__Issuer=Phan_Duy_Hung
Jwt__Audience=product_users
Jwt__ExpireMinutes=60
```

### PayOS:
```
PayOSSettings__ClientId=9300a98d-fe52-4a4f-a82b-6748960d729b
PayOSSettings__ApiKey=3c3ffa58-85c9-4704-b51a-b50b9db5340c
PayOSSettings__ChecksumKey=308630c2d6c5ddaa5189af07066b12d30c56b94c693b12733b43a21994aabc1b
```

### Cloudinary:
```
CloudinarySettings__CloudName=dj7rauxsj
CloudinarySettings__ApiKey=763236743457461
CloudinarySettings__ApiSecret=nQZ0ABJEnQzJAb7zgdidk29hcn4
```

### Environment:
```
ASPNETCORE_ENVIRONMENT=Production
```

## 📋 Bước 5: Deploy

1. Click **"Create Web Service"**
2. Render sẽ tự động build và deploy
3. Đợi deploy xong (có thể mất 5-10 phút)

## 📋 Bước 6: Lấy URL

Sau khi deploy xong, bạn sẽ có URL dạng:
```
https://product-management-4.onrender.com
```

## 📋 Bước 7: Update Frontend

1. Mở `products-ui/src/config.js`
2. Đổi `USE_LOCAL = false` để dùng production API
3. Hoặc set `PROD_API_BASE` thành URL Render của bạn

## 🐛 Troubleshooting

### Nếu gặp lỗi build:
- Kiểm tra .NET SDK version trên Render
- Thêm file `.netversion` trong root với nội dung: `8.0`

### Nếu gặp lỗi runtime:
- Xem logs trên Render Dashboard
- Kiểm tra environment variables đã set đúng chưa
- Kiểm tra database connection string

### Nếu CORS vẫn lỗi:
- Đảm bảo CORS policy trong `Program.cs` đã cho phép frontend URL
- Kiểm tra frontend đang call đúng API URL

## ✅ Checklist sau khi deploy:

- [ ] Build thành công
- [ ] Service đang running
- [ ] Test API endpoint: `https://your-app.onrender.com/api/products`
- [ ] Kiểm tra logs không có errors
- [ ] Frontend có thể call API thành công

