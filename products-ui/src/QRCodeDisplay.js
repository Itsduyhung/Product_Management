import React, { useEffect, useState, useRef } from 'react';
import { QRCodeCanvas } from 'qrcode.react';
import { getPaymentStatus } from './api';
import { useNavigate } from 'react-router-dom';

const QRCodeDisplay = ({ paymentUrl, orderCode }) => {
  const [paymentStatus, setPaymentStatus] = useState('Pending');
  const [isPolling, setIsPolling] = useState(true);
  const navigate = useNavigate();
  const pollIntervalRef = useRef(null);

  useEffect(() => {
    console.log('🔄 QRCodeDisplay mounted');
    console.log('📍 Payment URL:', paymentUrl);
    console.log('📍 Order Code:', orderCode);
    console.log('📍 Payment URL type:', typeof paymentUrl);
    console.log('📍 Payment URL length:', paymentUrl?.length);
    
    if (!paymentUrl) {
      console.warn('⚠️ No payment URL provided to QRCodeDisplay!');
    }
  }, [paymentUrl, orderCode]);

  useEffect(() => {
    if (!orderCode || !isPolling) return;

    console.log('🔄 Starting payment status polling for order:', orderCode);
    
    const pollStatus = async () => {
      try {
        console.log('📡 Polling payment status for order:', orderCode);
        const response = await getPaymentStatus(orderCode);
        const status = response?.status || 'Pending';

        console.log('📊 Payment status received:', status);
        setPaymentStatus(status);

        // Check various status formats from backend
        const normalizedStatus = status?.toLowerCase() || '';
        
        if (normalizedStatus === 'paid' || normalizedStatus === 'processing') {
          console.log('✅ Payment successful! Stopping polling and redirecting...');
          setIsPolling(false);
          if (pollIntervalRef.current) {
            clearInterval(pollIntervalRef.current);
            pollIntervalRef.current = null;
          }
          setTimeout(() => {
            navigate('/payment-success', { 
              state: { orderCode, status } 
            });
          }, 1500);
        } else if (normalizedStatus === 'cancelled' || normalizedStatus === 'canceled') {
          console.log('❌ Payment cancelled! Stopping polling and redirecting...');
          setIsPolling(false);
          if (pollIntervalRef.current) {
            clearInterval(pollIntervalRef.current);
            pollIntervalRef.current = null;
          }
          navigate('/payment-cancel', { 
            state: { orderCode, status } 
          });
        }
      } catch (error) {
        console.error('❌ Error polling payment status:', error);
        // Continue polling even if there's an error
        // The error might be temporary (network issue, etc.)
      }
    };

    // Poll immediately on mount
    pollStatus();

    // Then poll every 3 seconds
    pollIntervalRef.current = setInterval(pollStatus, 3000);

    return () => {
      console.log('🛑 Stopping payment status polling');
      if (pollIntervalRef.current) {
        clearInterval(pollIntervalRef.current);
        pollIntervalRef.current = null;
      }
    };
  }, [orderCode, isPolling, navigate]);

  console.log('🎨 QRCodeDisplay render - paymentUrl:', paymentUrl, 'orderCode:', orderCode);

  // Normalize payment URL - trim whitespace, ensure it's a valid string, and ensure proper encoding
  const normalizedPaymentUrl = paymentUrl ? String(paymentUrl).trim() : '';
  
  // Log normalized URL for debugging
  useEffect(() => {
    if (normalizedPaymentUrl) {
      console.log('✅ Normalized Payment URL for QR:', normalizedPaymentUrl);
      console.log('✅ URL length:', normalizedPaymentUrl.length);
      console.log('✅ URL starts with http:', normalizedPaymentUrl.startsWith('http'));
      console.log('✅ URL encoding check - original:', paymentUrl);
      console.log('✅ URL encoding check - normalized:', normalizedPaymentUrl);
      
      // Verify URL is identical to what will be used in link
      const linkUrl = normalizedPaymentUrl;
      console.log('✅ Verifying QR URL matches link URL:', linkUrl === normalizedPaymentUrl);
    }
  }, [normalizedPaymentUrl, paymentUrl]);

  return (
    <div style={{ padding: '20px', textAlign: 'center', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Quét mã QR để thanh toán</h2>
      {normalizedPaymentUrl ? (
        <div style={{ margin: '20px 0', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <div style={{ 
            padding: '25px', 
            background: '#ffffff', 
            borderRadius: '12px',
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
            display: 'inline-block',
            border: '2px solid #f0f0f0'
          }}>
            <QRCodeCanvas 
              value={normalizedPaymentUrl} 
              size={320}
              level="H"
              includeMargin={true}
              marginSize={4}
              fgColor="#000000"
              bgColor="#FFFFFF"
            />
          </div>
          <p style={{ marginTop: '20px', fontSize: '12px', color: '#666', wordBreak: 'break-all', padding: '0 10px' }}>
            {normalizedPaymentUrl.length > 70 
              ? `${normalizedPaymentUrl.substring(0, 70)}...` 
              : normalizedPaymentUrl}
          </p>
          <p style={{ marginTop: '10px', fontSize: '11px', color: '#999' }}>
            (Mã QR này chứa cùng link như nút bên dưới)
          </p>
        </div>
      ) : (
        <div style={{ padding: '40px', background: '#f3f4f6', borderRadius: '8px' }}>
          <p style={{ color: '#ef4444', fontWeight: '600' }}>⚠️ Không có Payment URL!</p>
          <p style={{ color: '#6b7280', fontSize: '14px', marginTop: '10px' }}>
            Vui lòng kiểm tra console logs hoặc thử lại.
          </p>
        </div>
      )}
      
      {orderCode && (
        <div style={{ marginTop: '20px', padding: '15px', background: '#f9fafb', borderRadius: '8px' }}>
          <p style={{ fontWeight: '600', marginBottom: '5px' }}>
            Trạng thái thanh toán: <span style={{ color: paymentStatus === 'Processing' || paymentStatus === 'Paid' ? '#10b981' : paymentStatus === 'Cancelled' ? '#ef4444' : '#f59e0b' }}>{paymentStatus}</span>
          </p>
          {paymentStatus === 'Pending' && (
            <p style={{ color: '#666', fontSize: '14px', marginTop: '5px' }}>
              ⏳ Đang chờ thanh toán... (Tự động cập nhật mỗi 3 giây)
            </p>
          )}
          {isPolling && (
            <p style={{ color: '#6b7280', fontSize: '12px', marginTop: '5px' }}>
              Đang theo dõi trạng thái thanh toán...
            </p>
          )}
        </div>
      )}

      {normalizedPaymentUrl && (
        <div style={{ marginTop: '20px' }}>
          <a 
            href={normalizedPaymentUrl} 
            target="_blank" 
            rel="noopener noreferrer"
            style={{ 
              color: '#0066cc', 
              textDecoration: 'underline',
              fontSize: '14px',
              fontWeight: '500'
            }}
          >
            Hoặc mở link thanh toán trong trình duyệt
          </a>
        </div>
      )}

      <div style={{ marginTop: '20px' }}>
        <button 
          onClick={() => navigate('/cart')}
          style={{ padding: '10px 20px', marginRight: '10px' }}
        >
          Quay lại giỏ hàng
        </button>
      </div>
    </div>
  );
};

export default QRCodeDisplay;