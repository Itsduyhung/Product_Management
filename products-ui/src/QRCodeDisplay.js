import React, { useEffect, useState } from 'react';
import { QRCodeCanvas } from 'qrcode.react';
import { getPaymentStatus } from './api';
import { useNavigate } from 'react-router-dom';

const QRCodeDisplay = ({ paymentUrl, orderCode }) => {
  const [paymentStatus, setPaymentStatus] = useState('Pending');
  const [isPolling, setIsPolling] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    if (!orderCode || !isPolling) return;

    const pollInterval = setInterval(async () => {
      try {
        const response = await getPaymentStatus(orderCode);
        const status = response.status;

        setPaymentStatus(status);

        if (status === 'Paid' || status === 'Processing') {
          setIsPolling(false);
          clearInterval(pollInterval);
          setTimeout(() => {
            navigate('/payment-success', { 
              state: { orderCode } 
            });
          }, 1500);
        } else if (status === 'Cancelled') {
          setIsPolling(false);
          clearInterval(pollInterval);
          navigate('/payment-cancel', { 
            state: { orderCode } 
          });
        }
      } catch (error) {
        console.error('Error polling payment status:', error);
      }
    }, 3000); // Poll every 3 seconds

    return () => clearInterval(pollInterval);
  }, [orderCode, isPolling, navigate]);

  return (
    <div style={{ padding: '20px', textAlign: 'center' }}>
      <h2>Quét mã QR để thanh toán</h2>
      {paymentUrl ? (
        <div style={{ margin: '20px 0' }}>
          <QRCodeCanvas value={paymentUrl} size={256} />
        </div>
      ) : (
        <p>Đang tải mã QR...</p>
      )}
      
      {orderCode && (
        <div style={{ marginTop: '20px' }}>
          <p>Trạng thái: {paymentStatus}</p>
          {paymentStatus === 'Pending' && (
            <p style={{ color: '#666', fontSize: '14px' }}>Đang chờ thanh toán...</p>
          )}
        </div>
      )}

      {paymentUrl && (
        <div style={{ marginTop: '20px' }}>
          <a 
            href={paymentUrl} 
            target="_blank" 
            rel="noopener noreferrer"
            style={{ color: '#0066cc', textDecoration: 'underline' }}
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