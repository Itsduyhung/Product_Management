// API Configuration
// Đổi giá trị này để switch giữa local và production

const isDevelopment = process.env.NODE_ENV === 'development';
const USE_LOCAL = true; // Set thành false để dùng production API

// Local API (khi backend chạy local)
const LOCAL_API_BASE = 'http://localhost:5000/api';

// Production API (backend trên Render)
const PROD_API_BASE = 'https://product-management-4.onrender.com/api';

// Chọn API URL dựa trên config
export const API_BASE_URL = USE_LOCAL ? LOCAL_API_BASE : PROD_API_BASE;

// Products API endpoints
export const PRODUCTS_API = `${API_BASE_URL}/products`;
export const CART_API = `${API_BASE_URL}/cart`;
export const ORDERS_API = `${API_BASE_URL}/orders`;
export const AUTH_API = `${API_BASE_URL}/auth`;

console.log(`📍 Using API: ${API_BASE_URL}`);

