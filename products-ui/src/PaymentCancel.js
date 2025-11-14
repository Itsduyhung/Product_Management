import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

const PaymentCancel = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { orderCode } = location.state || {};
  const [isAnimating, setIsAnimating] = useState(false);

  useEffect(() => {
    setIsAnimating(true);
  }, []);

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px',
      fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif'
    }}>
      <div style={{
        backgroundColor: '#ffffff',
        borderRadius: '24px',
        boxShadow: '0 20px 60px rgba(0, 0, 0, 0.3)',
        padding: '48px',
        maxWidth: '500px',
        width: '100%',
        textAlign: 'center',
        transform: isAnimating ? 'scale(1)' : 'scale(0.9)',
        opacity: isAnimating ? 1 : 0,
        transition: 'all 0.5s ease-out'
      }}>
        {/* Cancel Icon with Animation */}
        <div style={{
          margin: '0 auto 32px',
          width: '120px',
          height: '120px',
          background: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)',
          borderRadius: '50%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '0 10px 30px rgba(239, 68, 68, 0.4)',
          animation: isAnimating ? 'bounceIn 0.6s ease-out' : 'none'
        }}>
          <svg
            style={{
              width: '64px',
              height: '64px',
              color: '#ffffff',
              strokeWidth: '3'
            }}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </div>

        {/* Title */}
        <h1 style={{
          fontSize: '32px',
          fontWeight: '700',
          color: '#1f2937',
          marginBottom: '12px',
          lineHeight: '1.2'
        }}>
          ⚠️ Thanh toán đã bị hủy
        </h1>
        <p style={{
          fontSize: '16px',
          color: '#6b7280',
          marginBottom: '32px',
          lineHeight: '1.6'
        }}>
          Bạn đã hủy quá trình thanh toán. Đơn hàng của bạn vẫn được lưu trong giỏ hàng và bạn có thể thanh toán lại bất cứ lúc nào.
        </p>

        {/* Order Info Card */}
        {orderCode && (
          <div style={{
            marginBottom: '32px',
            padding: '20px',
            background: 'linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%)',
            borderRadius: '12px',
            border: '1px solid #fecaca'
          }}>
            <p style={{
              fontSize: '13px',
              color: '#991b1b',
              marginBottom: '8px',
              textTransform: 'uppercase',
              letterSpacing: '0.5px',
              fontWeight: '600'
            }}>
              Mã đơn hàng
            </p>
            <p style={{
              fontSize: '20px',
              fontWeight: '700',
              color: '#7f1d1d',
              letterSpacing: '1px'
            }}>
              #{orderCode}
            </p>
          </div>
        )}

        {/* Action Buttons */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          <button
            onClick={() => navigate('/cart')}
            style={{
              width: '100%',
              padding: '16px 24px',
              background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
              color: '#ffffff',
              fontSize: '16px',
              fontWeight: '600',
              borderRadius: '12px',
              border: 'none',
              cursor: 'pointer',
              boxShadow: '0 4px 15px rgba(245, 87, 108, 0.4)',
              transition: 'all 0.3s ease',
              transform: 'translateY(0)',
            }}
            onMouseEnter={(e) => {
              e.target.style.transform = 'translateY(-2px)';
              e.target.style.boxShadow = '0 6px 20px rgba(245, 87, 108, 0.5)';
            }}
            onMouseLeave={(e) => {
              e.target.style.transform = 'translateY(0)';
              e.target.style.boxShadow = '0 4px 15px rgba(245, 87, 108, 0.4)';
            }}
          >
            🛍️ Quay lại giỏ hàng
          </button>
          
          <button
            onClick={() => navigate('/')}
            style={{
              width: '100%',
              padding: '16px 24px',
              background: '#ffffff',
              color: '#4b5563',
              fontSize: '16px',
              fontWeight: '600',
              borderRadius: '12px',
              border: '2px solid #e5e7eb',
              cursor: 'pointer',
              transition: 'all 0.3s ease',
              transform: 'translateY(0)',
            }}
            onMouseEnter={(e) => {
              e.target.style.background = '#f9fafb';
              e.target.style.borderColor = '#d1d5db';
              e.target.style.transform = 'translateY(-2px)';
            }}
            onMouseLeave={(e) => {
              e.target.style.background = '#ffffff';
              e.target.style.borderColor = '#e5e7eb';
              e.target.style.transform = 'translateY(0)';
            }}
          >
            🛒 Tiếp tục mua sắm
          </button>
        </div>
      </div>

      <style>{`
        @keyframes bounceIn {
          0% {
            transform: scale(0);
            opacity: 0;
          }
          50% {
            transform: scale(1.1);
          }
          100% {
            transform: scale(1);
            opacity: 1;
          }
        }
      `}</style>
    </div>
  );
};

export default PaymentCancel;

