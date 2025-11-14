# ✅ Checklist - Kiểm tra Frontend call API đúng chưa

## 📋 Đã cấu hình:

### 1. **Config file** (`src/config.js`)
   - ✅ `USE_LOCAL = false` - Đang dùng production API
   - ✅ `PROD_API_BASE = 'https://product-management-4.onrender.com/api'`

### 2. **API Endpoints** (từ backend):
   - ✅ Products: `/api/products` (GET, POST, PUT, DELETE)
   - ✅ Auth: `/api/Auth/login` và `/api/Auth/register` (POST)
   - ✅ Cart: `/api/Cart` (GET), `/api/Cart/add` (POST)
   - ✅ Orders: `/api/Order/place` (POST), `/api/Order/my-orders` (GET)
   - ✅ Payment: `/api/Payment/webhook` (POST), `/api/Payment/status/{orderCode}` (GET)

### 3. **Frontend files sử dụng config:**
   - ✅ `App.js` - Dùng `PRODUCTS_API` từ config
   - ✅ `api.js` - Dùng `API_BASE_URL` từ config
   - ✅ `Login.js` và `Register.js` - Sử dụng functions từ `api.js`

## 🧪 Test API trực tiếp:

### 1. Test Products API:
```
URL: https://product-management-4.onrender.com/api/products
Method: GET
Expected: Array of products hoặc empty array []
```

### 2. Test Auth API:
```
URL: https://product-management-4.onrender.com/api/Auth/register
Method: POST
Body: {
  "username": "test",
  "email": "test@test.com",
  "password": "123456"
}
```

### 3. Test Swagger UI:
```
URL: https://product-management-4.onrender.com/swagger
Expected: Swagger UI hiển thị tất cả API endpoints
```

## 🚀 Cách test frontend:

1. **Mở terminal và chạy:**
   ```bash
   cd products-ui
   npm start
   ```

2. **Mở browser console (F12)**:
   - Kiểm tra có log: `📍 Using API: https://product-management-4.onrender.com/api`
   - Xem Network tab để thấy requests được gửi đến Render

3. **Test các chức năng:**
   - ✅ Load products list
   - ✅ Register/Login
   - ✅ Add to cart
   - ✅ Place order

## ⚠️ Nếu gặp lỗi:

### CORS Error:
- Backend đã config CORS cho production
- Nếu vẫn lỗi, kiểm tra logs trên Render

### 500 Internal Server Error:
- Kiểm tra database connection trên Render
- Kiểm tra environment variables đã set đúng chưa
- Xem logs trên Render Dashboard

### 404 Not Found:
- Kiểm tra route trong backend controller
- Đảm bảo endpoint path đúng (case-sensitive)

## 📝 Notes:

- Frontend đang chạy local nhưng call API từ Render
- Tất cả API calls đều đi qua `config.js` để dễ switch
- Console sẽ log API base URL khi app start

