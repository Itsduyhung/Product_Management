import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import QRCodeDisplay from './QRCodeDisplay';
import { fetchCart, placeOrder, removeFromCart } from './api';

const Cart = () => {
  const navigate = useNavigate();
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);
  const [removingProductId, setRemovingProductId] = useState(null); // Track which product is being removed
  const [showQRCode, setShowQRCode] = useState(false);
  const [paymentUrl, setPaymentUrl] = useState('');
  const [orderCode, setOrderCode] = useState('');
  const [error, setError] = useState('');

  const loadCart = async () => {
    setLoading(true);
    try {
      const data = await fetchCart();
      console.log('✅ Cart loaded:', data);
      console.log('✅ Cart loaded type:', typeof data);
      console.log('✅ Cart loaded is array?', Array.isArray(data));
      console.log('✅ Cart loaded length:', Array.isArray(data) ? data.length : (data?.items?.length || 0));
      
      // Log chi tiết các items
      if (Array.isArray(data)) {
        console.log('✅ Cart items:', data.map(item => ({
          id: item.id,
          productId: item.productId,
          productName: item.productName,
          quantity: item.quantity
        })));
      } else if (data?.items) {
        console.log('✅ Cart items:', data.items.map(item => ({
          id: item.id,
          productId: item.productId,
          productName: item.productName,
          quantity: item.quantity
        })));
      }
      
      // Backend trả về array trực tiếp, không phải object với items property
      // Nếu data là array, wrap nó vào object với items property
      if (Array.isArray(data)) {
        setCart({ items: data });
      } else if (data && data.items && Array.isArray(data.items)) {
        setCart(data);
      } else {
        // Nếu không có items, set empty array
        setCart({ items: [] });
      }
      setError('');
      
      // Return data để có thể check ngay lập tức
      return Array.isArray(data) ? data : (data?.items || []);
    } catch (error) {
      console.error('Error loading cart:', error);
      setError('Không thể tải giỏ hàng. Vui lòng đăng nhập lại.');
      setCart({ items: [] });
      return [];
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCart();
  }, []);

  const handleRemoveItem = async (productId) => {
    if (!productId) {
      console.error('❌ No productId provided!');
      setError('Không tìm thấy ID sản phẩm để xóa.');
      return;
    }

    try {
      console.log('🗑️ ========== REMOVE ITEM START ==========');
      console.log('🗑️ ProductId to remove:', productId);
      console.log('🗑️ ProductId type:', typeof productId);
      console.log('🗑️ Current cart before remove:', cart);
      console.log('🗑️ Current cart items before remove:', cart?.items);
      
      setError('');
      setRemovingProductId(productId); // Set which product is being removed
      
      console.log('🗑️ About to call removeFromCart API...');
      console.log('🗑️ removeFromCart function exists?', typeof removeFromCart);
      console.log('🗑️ removeFromCart value:', removeFromCart);
      console.log('🗑️ removeFromCart is function?', typeof removeFromCart === 'function');
      
      // Kiểm tra xem removeFromCart có phải là function không
      if (typeof removeFromCart !== 'function') {
        console.error('❌ removeFromCart is not a function! Type:', typeof removeFromCart);
        console.error('❌ removeFromCart value:', removeFromCart);
        throw new Error(`removeFromCart is not a function. Type: ${typeof removeFromCart}`);
      }
      
      let response;
      try {
        console.log('🗑️ Calling removeFromCart with productId:', productId);
        console.log('🗑️ Before calling removeFromCart...');
        const removePromise = removeFromCart(productId);
        console.log('🗑️ removeFromCart called, waiting for promise...');
        console.log('🗑️ Promise:', removePromise);
        response = await removePromise;
        console.log('✅ Remove API response received:', response);
        console.log('✅ Product removed successfully from backend');
      } catch (apiError) {
        console.error('❌ API call failed:', apiError);
        console.error('❌ API call error type:', typeof apiError);
        console.error('❌ API call error name:', apiError?.name);
        console.error('❌ API call error message:', apiError?.message);
        throw apiError; // Re-throw để catch block bên ngoài xử lý
      }
      
      // Reload cart after removing item
      console.log('🔄 Reloading cart...');
      const reloadedCartItems = await loadCart();
      console.log('✅ Cart reloaded successfully');
      console.log('✅ Reloaded cart items:', reloadedCartItems);
      
      // Kiểm tra xem item đã bị remove chưa (dùng data từ loadCart thay vì state)
      // Chỉ so sánh với productId, không dùng item.id (CartItem Id)
      const removedItemStillExists = reloadedCartItems.some(item => 
        item.productId === productId
      );
      
      if (removedItemStillExists) {
        console.warn('⚠️ Item với productId', productId, 'vẫn còn trong cart sau khi remove!');
        console.warn('⚠️ Reloaded cart items:', reloadedCartItems.map(item => ({
          id: item.id,
          productId: item.productId,
          productName: item.productName
        })));
        console.warn('⚠️ Có thể backend không remove đúng hoặc có nhiều items với cùng productId');
      } else {
        console.log('✅ Item với productId', productId, 'đã được remove khỏi cart');
        console.log('✅ UI sẽ tự động update khi state cart được set');
      }
      
      console.log('🗑️ ========== REMOVE ITEM END ==========');
    } catch (error) {
      console.error('❌ ========== REMOVE ITEM ERROR ==========');
      console.error('❌ Error removing item from cart:', error);
      console.error('❌ Error response:', error.response);
      console.error('❌ Error status:', error.response?.status);
      console.error('❌ Error data:', error.response?.data);
      console.error('❌ ========== END ERROR ==========');
      setError(`Không thể xóa sản phẩm khỏi giỏ hàng. ${error.response?.data?.message || error.message || 'Vui lòng thử lại.'}`);
    } finally {
      setRemovingProductId(null); // Clear removing state
    }
  };

  const handleOrderClick = async () => {
    console.log('🖱️ Button clicked - handleOrderClick called!');
    console.log('🖱️ Current cart state:', cart);
    console.log('🖱️ Current cart items:', cart?.items);
    
    setError('');
    
    // Refresh cart trước khi đặt hàng
    console.log('🔄 Starting cart refresh...');
    setLoading(true);
    try {
      const freshCartData = await fetchCart();
      console.log('✅ Fresh cart before order:', freshCartData);
      
      // Handle array response
      let freshCart;
      if (Array.isArray(freshCartData)) {
        freshCart = { items: freshCartData };
      } else if (freshCartData && freshCartData.items) {
        freshCart = freshCartData;
      } else {
        freshCart = { items: [] };
      }
      
      if (!freshCart || !freshCart.items || freshCart.items.length === 0) {
        console.error('❌ Cart is empty after refresh!');
        setError('Giỏ hàng trống! Vui lòng thêm sản phẩm vào giỏ hàng.');
        setLoading(false);
        setCart(freshCart);
        return;
      }
      
      console.log('✅ Cart refreshed successfully, items count:', freshCart.items.length);
      setCart(freshCart);
      setLoading(false);

      // Đặt hàng
      console.log('🔄 About to call placeOrder API...');
      console.log('🔄 Checking authentication token...');
      const token = localStorage.getItem('token');
      if (!token) {
        console.error('❌ No authentication token found!');
        setError('Vui lòng đăng nhập lại.');
        return;
      }
      console.log('✅ Token found:', token.substring(0, 20) + '...');
      console.log('🔄 Calling placeOrder API now...');
      let response;
      try {
        console.log('🔄 Waiting for placeOrder API response...');
        console.log('🔄 This may take up to 60 seconds if backend is slow...');
        response = await placeOrder();
        console.log('✅ placeOrder returned successfully');
        console.log('✅ Response object:', response);
        console.log('✅ Response type:', typeof response);
        console.log('✅ Response is array?', Array.isArray(response));
        
        if (response) {
          console.log('✅ Response keys:', Object.keys(response));
          
          // Log tất cả properties của response
          for (const key in response) {
            console.log(`  - ${key}:`, response[key], typeof response[key]);
            if (typeof response[key] === 'object' && response[key] !== null) {
              console.log(`    Keys of ${key}:`, Object.keys(response[key]));
            }
          }
        }
      } catch (apiError) {
        console.error('❌ Error in placeOrder call:', apiError);
        throw apiError; // Re-throw để catch block bên ngoài xử lý
      }
      
      // Backend OrderController trả về:
      // { message: "Order placed successfully!", data: OrderResponse }
      // Và placeOrder() trả về response.data (là object trên)
      // Vậy cần: response.data.paymentUrl hoặc response.data.PaymentUrl
      let paymentUrlFromResponse = null;
      let orderCodeFromResponse = null;
      
      if (response) {
        console.log('🔍 Analyzing response structure...');
        
        // Case 1: response = { message: "...", data: { paymentUrl, orderCode, ... } }
        if (response.data) {
          console.log('✅ Found response.data:', response.data);
          console.log('✅ response.data keys:', Object.keys(response.data));
          
          if (response.data.paymentUrl) {
            paymentUrlFromResponse = response.data.paymentUrl;
            orderCodeFromResponse = response.data.orderCode;
            console.log('✅ Found paymentUrl in response.data (camelCase)');
          }
          // Case 2: PascalCase
          else if (response.data.PaymentUrl) {
            paymentUrlFromResponse = response.data.PaymentUrl;
            orderCodeFromResponse = response.data.OrderCode;
            console.log('✅ Found PaymentUrl in response.data (PascalCase)');
          }
          else {
            console.warn('⚠️ response.data exists but no paymentUrl found');
            console.warn('response.data content:', JSON.stringify(response.data, null, 2));
          }
        }
        // Case 3: response chính là OrderResponse trực tiếp (camelCase)
        else if (response.paymentUrl) {
          paymentUrlFromResponse = response.paymentUrl;
          orderCodeFromResponse = response.orderCode;
          console.log('✅ Found paymentUrl directly in response (camelCase)');
        }
        // Case 4: PascalCase trực tiếp
        else if (response.PaymentUrl) {
          paymentUrlFromResponse = response.PaymentUrl;
          orderCodeFromResponse = response.OrderCode;
          console.log('✅ Found PaymentUrl directly in response (PascalCase)');
        }
        else {
          console.error('❌ No paymentUrl found in any structure');
          console.error('Full response:', JSON.stringify(response, null, 2));
        }
      } else {
        console.error('❌ Response is null or undefined');
      }
      
      console.log('✅ Final Extracted Payment URL:', paymentUrlFromResponse);
      console.log('✅ Final Extracted Order Code:', orderCodeFromResponse);
      
      if (paymentUrlFromResponse) {
        console.log('✅ Setting payment URL and showing QR code');
        setPaymentUrl(paymentUrlFromResponse);
        setOrderCode(orderCodeFromResponse || '');
        setShowQRCode(true);
        console.log('✅ QR code should be displayed now');
      } else {
        console.error('❌ No payment URL found - cannot show QR code');
        console.error('Response structure:', JSON.stringify(response, null, 2));
        setError(`Không thể tạo link thanh toán. Vui lòng kiểm tra backend logs trên Render. Response keys: ${Object.keys(response || {}).join(', ')}`);
      }
    } catch (error) {
      console.error('❌ ========== Error creating order ==========');
      console.error('❌ Error object:', error);
      console.error('❌ Error name:', error?.name);
      console.error('❌ Error message:', error?.message);
      console.error('❌ Error code:', error?.code);
      console.error('❌ Error response:', error?.response);
      console.error('❌ Error response status:', error?.response?.status);
      console.error('❌ Error response data:', error?.response?.data);
      console.error('❌ Error config:', error?.config);
      console.error('❌ Is timeout?', error?.code === 'ECONNABORTED' || error?.message?.includes('timeout'));
      console.error('❌ ========== End error ==========');
      
      let errorMessage = 'Có lỗi xảy ra khi đặt hàng.';
      
      // Network/timeout errors
      if (!error.response) {
        if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
          errorMessage = 'Request timeout. Backend có thể đang chậm hoặc không phản hồi. Vui lòng thử lại sau.';
        } else if (error.message?.includes('Network Error') || error.message?.includes('Failed to fetch')) {
          errorMessage = 'Không thể kết nối đến backend. Vui lòng kiểm tra kết nối mạng hoặc backend có đang chạy không.';
        } else {
          errorMessage = `Lỗi kết nối: ${error.message}`;
        }
      }
      // HTTP errors
      else if (error.response?.data?.message) {
        errorMessage = error.response.data.message;
      } else if (error.response?.data?.error) {
        errorMessage = error.response.data.error;
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      // Nếu là lỗi PayOS hoặc payment link
      if (errorMessage.toLowerCase().includes('payos') || errorMessage.toLowerCase().includes('payment')) {
        errorMessage += ' Vui lòng kiểm tra cấu hình PayOS trong backend.';
      }
      
      setError(errorMessage);
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div style={{ padding: '20px', textAlign: 'center' }}>
        <p>Loading...</p>
      </div>
    );
  }

  if (showQRCode) {
    console.log('🎯 Rendering QRCodeDisplay component');
    console.log('🎯 Payment URL passed:', paymentUrl);
    console.log('🎯 Order Code passed:', orderCode);
    return <QRCodeDisplay paymentUrl={paymentUrl} orderCode={orderCode} />;
  }

  const calculateTotal = () => {
    if (!cart?.items || !Array.isArray(cart.items)) return 0;
    return cart.items.reduce((sum, item) => {
      const itemPrice = item.price || 0;
      const itemQuantity = item.quantity || 0;
      return sum + (itemPrice * itemQuantity);
    }, 0);
  };

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1>🛒 Giỏ hàng</h1>
        <button 
          onClick={() => navigate('/')}
          style={{
            padding: '8px 16px',
            background: '#6b7280',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            fontWeight: '600'
          }}
        >
          ← Quay lại
        </button>
      </div>

      {error && (
        <div style={{ 
          padding: '12px', 
          background: '#fee', 
          color: '#c00', 
          marginBottom: '20px',
          borderRadius: '8px',
          border: '1px solid #fcc'
        }}>
          {error}
        </div>
      )}

      {!cart || !cart.items || cart.items.length === 0 ? (
        <div style={{ 
          textAlign: 'center', 
          padding: '60px 20px',
          background: '#f9fafb',
          borderRadius: '12px',
          border: '2px dashed #d1d5db'
        }}>
          <p style={{ fontSize: '18px', color: '#6b7280', marginBottom: '20px' }}>
            Giỏ hàng của bạn đang trống
          </p>
          <button 
            onClick={() => navigate('/')}
            style={{
              padding: '12px 24px',
              background: '#3b82f6',
              color: 'white',
              border: 'none',
              borderRadius: '8px',
              cursor: 'pointer',
              fontWeight: '600',
              fontSize: '16px'
            }}
          >
            Tiếp tục mua sắm
          </button>
        </div>
      ) : (
        <div>
          <div style={{ 
            background: 'white', 
            borderRadius: '12px', 
            padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            marginBottom: '20px'
          }}>
            {cart.items.map((item, index) => (
              <div 
                key={item.id || index}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  padding: '16px',
                  borderBottom: index < cart.items.length - 1 ? '1px solid #e5e7eb' : 'none',
                  gap: '20px'
                }}
              >
                <div style={{
                  width: '80px',
                  height: '80px',
                  background: '#f3f4f6',
                  borderRadius: '8px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: '#9ca3af',
                  fontSize: '24px'
                }}>
                  🛍️
                </div>
                <div style={{ flex: 1 }}>
                  <h3 style={{ margin: '0 0 8px 0', fontSize: '18px', color: '#111827' }}>
                    {item.productName || item.name || 'Sản phẩm'}
                  </h3>
                  <p style={{ margin: '0', color: '#6b7280', fontSize: '14px' }}>
                    Giá đơn vị: {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.price)}
                  </p>
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '12px' }}>
                  <div style={{ textAlign: 'right' }}>
                    <p style={{ margin: '0 0 8px 0', color: '#6b7280', fontSize: '14px' }}>
                      Số lượng: <strong>{item.quantity}</strong>
                    </p>
                    <p style={{ margin: '0', fontSize: '18px', fontWeight: '600', color: '#111827' }}>
                      {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(item.price * (item.quantity || 1))}
                    </p>
                  </div>
                  <button
                    onClick={() => {
                      console.log('🖱️ Remove button clicked for item:', item);
                      console.log('🖱️ Item productId:', item.productId);
                      console.log('🖱️ Item id (CartItem Id - NOT USED):', item.id);
                      
                      // Backend API yêu cầu productId, không phải CartItem Id
                      if (!item.productId) {
                        console.error('❌ No productId found in item!', item);
                        setError('Không tìm thấy Product ID. Item: ' + JSON.stringify(item));
                        return;
                      }
                      
                      console.log('🖱️ ProductId to remove:', item.productId);
                      handleRemoveItem(item.productId);
                    }}
                    disabled={removingProductId === item.productId || loading || !item.productId}
                    style={{
                      padding: '8px 16px',
                      background: (removingProductId === item.productId || loading || !item.productId) ? '#9ca3af' : '#ef4444',
                      color: 'white',
                      border: 'none',
                      borderRadius: '6px',
                      cursor: (removingProductId === item.productId || loading || !item.productId) ? 'not-allowed' : 'pointer',
                      fontSize: '14px',
                      fontWeight: '500',
                      transition: 'background 0.2s',
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      opacity: (removingProductId === item.productId || loading || !item.productId) ? 0.6 : 1
                    }}
                    onMouseEnter={(e) => {
                      if (removingProductId !== item.productId && !loading && item.productId) {
                        e.target.style.background = '#dc2626';
                      }
                    }}
                    onMouseLeave={(e) => {
                      if (removingProductId !== item.productId && !loading && item.productId) {
                        e.target.style.background = '#ef4444';
                      }
                    }}
                    title={(removingProductId === item.productId || loading) ? "Đang xóa..." : (!item.productId ? "Không có Product ID" : "Xóa sản phẩm khỏi giỏ hàng")}
                  >
                    {(removingProductId === item.productId || loading) ? '⏳ Đang xóa...' : '🗑️ Xóa'}
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div style={{ 
            background: 'white', 
            borderRadius: '12px', 
            padding: '20px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
          }}>
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center',
              marginBottom: '20px',
              paddingBottom: '20px',
              borderBottom: '2px solid #e5e7eb'
            }}>
              <span style={{ fontSize: '20px', fontWeight: '600', color: '#111827' }}>
                Tổng tiền:
              </span>
              <span style={{ fontSize: '24px', fontWeight: 'bold', color: '#3b82f6' }}>
                {new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(calculateTotal())}
              </span>
            </div>
            <button 
              onClick={(e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('🖱️ Button onClick event fired!');
                handleOrderClick();
              }}
              disabled={loading}
              style={{
                width: '100%',
                padding: '16px',
                background: loading 
                  ? 'linear-gradient(135deg, #9ca3af 0%, #6b7280 100%)' 
                  : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                color: 'white',
                border: 'none',
                borderRadius: '8px',
                fontSize: '18px',
                fontWeight: '600',
                cursor: loading ? 'not-allowed' : 'pointer',
                transition: 'all 0.3s ease',
                opacity: loading ? 0.6 : 1
              }}
              onMouseOver={(e) => !loading && (e.target.style.transform = 'translateY(-2px)')}
              onMouseOut={(e) => !loading && (e.target.style.transform = 'translateY(0)')}
            >
              {loading ? '⏳ Đang xử lý...' : 'Đặt hàng và thanh toán'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default Cart;