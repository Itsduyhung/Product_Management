import axios from 'axios';
import { API_BASE_URL } from './config';

// Get auth token from localStorage
const getAuthToken = () => {
  return localStorage.getItem('token');
};

// Create axios instance with default config
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 60000, // 60 seconds timeout
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
      console.log('🔐 Adding Authorization header to request:', config.url);
    } else {
      console.warn('⚠️ No token found for request:', config.url);
    }
    console.log('📤 Request config:', {
      method: config.method,
      url: config.url,
      baseURL: config.baseURL,
      headers: config.headers
    });
    return config;
  },
  (error) => {
    console.error('❌ Request interceptor error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor để log responses
api.interceptors.response.use(
  (response) => {
    console.log('✅ Response interceptor triggered:', {
      status: response.status,
      statusText: response.statusText,
      url: response.config?.url || response.config?.baseURL + response.config?.url,
      method: response.config?.method,
      data: response.data,
      dataType: typeof response.data,
      dataKeys: response.data ? Object.keys(response.data) : null
    });
    
    // Log chi tiết nếu là placeOrder response
    if (response.config?.url?.includes('/order/place')) {
      console.log('✅ PlaceOrder response details:');
      console.log('  - Full response:', response);
      console.log('  - Response.data:', response.data);
      if (response.data?.data) {
        console.log('  - Response.data.data:', response.data.data);
        console.log('  - PaymentUrl:', response.data.data.PaymentUrl || response.data.data.paymentUrl);
        console.log('  - OrderCode:', response.data.data.OrderCode || response.data.data.orderCode);
      }
    }
    
    return response;
  },
  (error) => {
    console.error('❌ Response interceptor ERROR triggered:', {
      message: error.message,
      name: error.name,
      code: error.code,
      status: error.response?.status,
      statusText: error.response?.statusText,
      url: error.config?.url || (error.config?.baseURL + error.config?.url),
      method: error.config?.method,
      data: error.response?.data,
      timeout: error.code === 'ECONNABORTED',
      networkError: !error.response && error.message
    });
    
    // Log chi tiết hơn nếu là network error
    if (!error.response) {
      console.error('❌ Network error - no response received:');
      console.error('  - This could be timeout, connection refused, or CORS issue');
      console.error('  - Error message:', error.message);
      console.error('  - Error code:', error.code);
    }
    
    return Promise.reject(error);
  }
);

export const fetchCart = async () => {
  try {
    const response = await api.get('/cart');
    return response.data;
  } catch (error) {
    console.error('Error fetching cart:', error);
    throw error;
  }
};

export const placeOrder = async () => {
  try {
    console.log('📤 ========== START placeOrder API call ==========');
    console.log('📤 API Base URL:', API_BASE_URL);
    console.log('📤 Endpoint: POST', API_BASE_URL + '/order/place');
    console.log('📤 Token:', localStorage.getItem('token') ? 'Present' : 'Missing');
    console.log('📤 Current time:', new Date().toISOString());
    console.log('📤 Axios timeout config:', api.defaults.timeout);
    
    const startTime = Date.now();
    console.log('📤 Sending request...');
    
    // Thêm timeout check để đảm bảo request không bị pending quá lâu
    const timeoutPromise = new Promise((_, reject) => {
      setTimeout(() => {
        reject(new Error('Request timeout after 60 seconds'));
      }, 60000); // 60 seconds timeout
    });
    
    // Race giữa API call và timeout
    const response = await Promise.race([
      api.post('/order/place'),
      timeoutPromise
    ]);
    
    const endTime = Date.now();
    const duration = endTime - startTime;
    console.log(`📥 Request completed in ${duration}ms`);
    console.log('📥 ========== placeOrder API Response Received ==========');
    console.log('📥 Full response:', response);
    console.log('📥 Response.status:', response.status);
    console.log('📥 Response.statusText:', response.statusText);
    console.log('📥 Response.headers:', response.headers);
    console.log('📥 Response.data:', response.data);
    console.log('📥 Response.data type:', typeof response.data);
    console.log('📥 Response.data keys:', response.data ? Object.keys(response.data) : 'null');
    
    // Log chi tiết response.data
    if (response.data) {
      console.log('📥 Response.data structure:');
      console.log('  - message:', response.data.message);
      console.log('  - data:', response.data.data);
      if (response.data.data) {
        console.log('  - data.PaymentUrl:', response.data.data.PaymentUrl);
        console.log('  - data.paymentUrl:', response.data.data.paymentUrl);
        console.log('  - data.OrderCode:', response.data.data.OrderCode);
        console.log('  - data.orderCode:', response.data.data.orderCode);
      }
    }
    
    console.log('📥 ========== END placeOrder API call ==========');
    
    return response.data;
  } catch (error) {
    const errorType = error.code || error.message;
    console.error('❌ ========== placeOrder API ERROR ==========');
    console.error('❌ Error type:', errorType);
    console.error('❌ Error name:', error.name);
    console.error('❌ Error message:', error.message);
    console.error('❌ Error code:', error.code);
    console.error('❌ Is timeout?', error.code === 'ECONNABORTED' || error.message?.includes('timeout'));
    console.error('❌ Error response:', error.response);
    console.error('❌ Error response status:', error.response?.status);
    console.error('❌ Error response data:', error.response?.data);
    console.error('❌ Error config:', error.config);
    console.error('❌ Error config URL:', error.config?.url);
    console.error('❌ Error config method:', error.config?.method);
    console.error('❌ Error stack:', error.stack);
    console.error('❌ ========== END placeOrder API ERROR ==========');
    throw error;
  }
};

export const getPaymentStatus = async (orderCode) => {
  try {
    const response = await api.get(`/payment/status/${orderCode}`);
    return response.data;
  } catch (error) {
    console.error('Error getting payment status:', error);
    throw error;
  }
};

export const addToCart = async (productId, quantity = 1) => {
  try {
    const response = await api.post('/cart/add', {
      productId,
      quantity
    });
    return response.data;
  } catch (error) {
    console.error('Error adding to cart:', error);
    throw error;
  }
};

export const removeFromCart = async (productId) => {
  console.log('📤 ========== API: removeFromCart START ==========');
  console.log('📤 API: ProductId:', productId);
  console.log('📤 API: ProductId type:', typeof productId);
  console.log('📤 API: API_BASE_URL:', API_BASE_URL);
  console.log('📤 API: Endpoint:', `${API_BASE_URL}/cart/remove/${productId}`);
  console.log('📤 API: Full URL will be:', api.defaults.baseURL + `/cart/remove/${productId}`);
  console.log('📤 API: api instance:', api);
  console.log('📤 API: api.defaults:', api.defaults);
  
  try {
    console.log('📤 API: About to call api.delete()...');
    const startTime = Date.now();
    const response = await api.delete(`/cart/remove/${productId}`);
    const endTime = Date.now();
    console.log(`📥 API: Request completed in ${endTime - startTime}ms`);
    console.log('📥 API: Remove response:', response);
    console.log('📥 API: Response status:', response.status);
    console.log('📥 API: Response data:', response.data);
    console.log('📥 API: Response config URL:', response.config?.url);
    console.log('📥 ========== API: removeFromCart SUCCESS ==========');
    return response.data;
  } catch (error) {
    console.error('❌ ========== API: removeFromCart ERROR ==========');
    console.error('❌ API: Error removing from cart:', error);
    console.error('❌ API: Error name:', error.name);
    console.error('❌ API: Error message:', error.message);
    console.error('❌ API: Error code:', error.code);
    console.error('❌ API: Error response:', error.response);
    console.error('❌ API: Error response status:', error.response?.status);
    console.error('❌ API: Error response data:', error.response?.data);
    console.error('❌ API: Error config:', error.config);
    console.error('❌ API: Error config URL:', error.config?.url);
    console.error('❌ API: Error config method:', error.config?.method);
    console.error('❌ ========== API: removeFromCart ERROR END ==========');
    throw error;
  }
};

// Auth APIs - không cần token
export const login = async (email, password) => {
  try {
    const response = await axios.post(`${API_BASE_URL}/Auth/login`, {
      email,
      password
    });
    return response.data;
  } catch (error) {
    console.error('Error logging in:', error);
    throw error;
  }
};

export const register = async (username, email, password) => {
  try {
    const response = await axios.post(`${API_BASE_URL}/Auth/register`, {
      username,
      email,
      password
    });
    return response.data;
  } catch (error) {
    console.error('Error registering:', error);
    throw error;
  }
};