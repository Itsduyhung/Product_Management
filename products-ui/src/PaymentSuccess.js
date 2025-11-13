import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

const PaymentSuccess = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { orderCode, totalAmount } = location.state || {};

  const formatAmount = (amount) => {
    if (!amount) return '';
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-green-50 to-emerald-100 p-4">
      <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-md w-full text-center">
        <div className="mb-6">
          <div className="mx-auto w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mb-4">
            <svg
              className="w-12 h-12 text-green-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
          <h2 className="text-3xl font-bold text-gray-800 mb-2">
            Thanh toán thành công!
          </h2>
          <p className="text-gray-600">
            Cảm ơn bạn đã mua sắm tại cửa hàng của chúng tôi
          </p>
        </div>

        {orderCode && (
          <div className="mb-4 p-4 bg-gray-50 rounded-lg">
            <p className="text-sm text-gray-600 mb-1">Mã đơn hàng</p>
            <p className="text-lg font-semibold text-gray-800">{orderCode}</p>
          </div>
        )}

        {totalAmount && (
          <div className="mb-6 p-4 bg-indigo-50 rounded-lg">
            <p className="text-sm text-gray-600 mb-1">Tổng tiền đã thanh toán</p>
            <p className="text-2xl font-bold text-indigo-600">
              {formatAmount(totalAmount)}
            </p>
          </div>
        )}

        <div className="space-y-3">
          <button
            onClick={() => navigate('/')}
            className="w-full py-3 px-6 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Tiếp tục mua sắm
          </button>
          <button
            onClick={() => navigate('/cart')}
            className="w-full py-3 px-6 bg-gray-200 text-gray-700 font-semibold rounded-lg hover:bg-gray-300 transition-colors"
          >
            Xem giỏ hàng
          </button>
        </div>
      </div>
    </div>
  );
};

export default PaymentSuccess;

