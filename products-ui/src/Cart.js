import React, { useEffect, useState } from 'react';
import QRCodeDisplay from './QRCodeDisplay';
import { fetchCart, placeOrder } from './api';

const Cart = () => {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showQRCode, setShowQRCode] = useState(false);
  const [paymentUrl, setPaymentUrl] = useState('');
  const [orderCode, setOrderCode] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const loadCart = async () => {
      try {
        const data = await fetchCart();
        setCart(data);
      } catch (error) {
        console.error('Error loading cart:', error);
        setError('Không thể tải giỏ hàng. Vui lòng đăng nhập lại.');
      } finally {
        setLoading(false);
      }
    };

    loadCart();
  }, []);

  const handleOrderClick = async () => {
    if (!cart || !cart.items || cart.items.length === 0) {
      setError('Giỏ hàng trống!');
      return;
    }

    setError('');

    try {
      const response = await placeOrder();
      console.log('Order created:', response);
      
      if (response.data && response.data.paymentUrl) {
        setPaymentUrl(response.data.paymentUrl);
        setOrderCode(response.data.orderCode || '');
        setShowQRCode(true);
      } else {
        setError('Không thể tạo link thanh toán. Vui lòng thử lại.');
      }
    } catch (error) {
      console.error('Error creating order:', error);
      setError(
        error.response?.data?.message || 
        'Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.'
      );
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
    return <QRCodeDisplay paymentUrl={paymentUrl} orderCode={orderCode} />;
  }

  return (
    <div style={{ padding: '20px' }}>
      <h1>Cart</h1>
      {error && (
        <div style={{ padding: '10px', background: '#fee', color: '#c00', marginBottom: '10px' }}>
          {error}
        </div>
      )}
      {!cart || !cart.items || cart.items.length === 0 ? (
        <p>Giỏ hàng trống</p>
      ) : (
        <div>
          <pre>{JSON.stringify(cart, null, 2)}</pre>
          <button onClick={handleOrderClick} style={{ marginTop: '10px', padding: '10px 20px' }}>
            Đặt hàng và thanh toán
          </button>
        </div>
      )}
    </div>
  );
};

export default Cart;