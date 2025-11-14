import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./App.css";
import Loading from "./loading"; // Import Loading component
import { addToCart, fetchCart } from "./api";
import { PRODUCTS_API } from "./config";

const API_URL = PRODUCTS_API;

function App() {
  const navigate = useNavigate();
  const [products, setProducts] = useState([]);
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    price: "",
    image: null,
  });
  const [isEditing, setIsEditing] = useState(false);
  const [selectedId, setSelectedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [isAddingToCart, setIsAddingToCart] = useState(false);
  const [quantity, setQuantity] = useState(1);
  const [cartItemCount, setCartItemCount] = useState(0);
  const [fieldErrors, setFieldErrors] = useState({
    name: "",
    description: "",
    price: "",
    image: ""
  });

  useEffect(() => {
    setLoading(true);
    axios
      .get(API_URL)
      .then((res) => {
        console.log('✅ Products loaded:', res.data);
        setProducts(Array.isArray(res.data) ? res.data : []);
        setLoading(false);
      })
      .catch((err) => {
        console.error("❌ Error fetching products:", err);
        setError("Failed to load products. Please try again.");
        setLoading(false);
      });
  }, []);

  // Fetch cart count when authenticated
  useEffect(() => {
    const loadCartCount = async () => {
      const token = localStorage.getItem('token');
      if (!token) {
        setCartItemCount(0);
        return;
      }

      try {
        const cartData = await fetchCart();
        // Backend trả về array trực tiếp
        let items = [];
        if (Array.isArray(cartData)) {
          items = cartData;
        } else if (cartData?.items && Array.isArray(cartData.items)) {
          items = cartData.items;
        }
        const count = items.reduce((sum, item) => sum + (item.quantity || 0), 0);
        setCartItemCount(count);
      } catch (error) {
        console.error('Error fetching cart count:', error);
        setCartItemCount(0);
      }
    };

    loadCartCount();
    // Refresh cart count every 5 seconds
    const interval = setInterval(loadCartCount, 5000);
    return () => clearInterval(interval);
  }, []);

  const handleView = (id) => {
    setError("");
    setFieldErrors({ name: "", description: "", price: "", image: "" });
    setQuantity(1);
    axios.get(`${API_URL}/${id}`).then((res) => {
      setFormData(res.data);
      setSelectedId(id);
      setIsEditing(false);
      setShowForm(true);
    }).catch((err) => {
      setError("Error loading product details: " + err.message);
    });
  };

  // Validate individual field
  const validateField = (fieldName, value, imageFile = null) => {
    let error = "";

    // Helper function to safely convert value to string
    const toString = (val) => {
      if (val == null) return "";
      if (typeof val === "number") return String(val);
      if (typeof val === "string") return val;
      return String(val);
    };

    // Helper function to safely trim string
    const safeTrim = (val) => {
      const str = toString(val);
      return str.trim();
    };

    switch (fieldName) {
      case "name":
        const nameStr = safeTrim(value);
        if (!nameStr) {
          error = "Tên sản phẩm không được để trống";
        } else if (nameStr.length < 3) {
          error = "Tên sản phẩm phải có ít nhất 3 ký tự";
        } else if (nameStr.length > 200) {
          error = "Tên sản phẩm không được quá 200 ký tự";
        }
        break;
      
      case "description":
        const descStr = safeTrim(value);
        if (!descStr) {
          error = "Mô tả sản phẩm không được để trống";
        } else if (descStr.length < 10) {
          error = "Mô tả sản phẩm phải có ít nhất 10 ký tự";
        } else if (descStr.length > 1000) {
          error = "Mô tả sản phẩm không được quá 1000 ký tự";
        }
        break;
      
      case "price":
        // Price có thể là number hoặc string - chỉ validate là số và không âm
        const priceStr = toString(value);
        if (!priceStr || priceStr.trim() === "") {
          error = "Giá sản phẩm không được để trống";
        } else {
          const priceNum = parseFloat(priceStr);
          if (isNaN(priceNum)) {
            error = "Giá sản phẩm phải là một số hợp lệ";
          } else if (priceNum < 0) {
            error = "Giá sản phẩm không được âm";
          }
        }
        break;
      
      case "image":
        if (imageFile) {
          const allowedTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"];
          const maxSize = 5 * 1024 * 1024; // 5MB
          
          if (!allowedTypes.includes(imageFile.type)) {
            error = "File ảnh phải là định dạng JPG, PNG, GIF hoặc WEBP";
          } else if (imageFile.size > maxSize) {
            error = "Kích thước ảnh không được vượt quá 5MB";
          }
        } else if (!selectedId) {
          // Only require image for new products
          error = "Ảnh sản phẩm là bắt buộc khi tạo mới";
        }
        break;
      
      default:
        break;
    }

    return error;
  };

  // Validate entire form
  const validateForm = () => {
    const errors = {
      name: validateField("name", formData.name),
      description: validateField("description", formData.description),
      price: validateField("price", formData.price),
      image: validateField("image", null, formData.image)
    };

    setFieldErrors(errors);
    
    // Check if there are any errors
    const hasErrors = Object.values(errors).some(error => error !== "");
    if (hasErrors) {
      setError("Vui lòng kiểm tra lại thông tin đã nhập");
      return false;
    }

    setError("");
    return true;
  };

  const handleAdd = () => {
    if (!validateForm()) {
      return;
    }

    const data = new FormData();
    data.append("name", formData.name);
    data.append("description", formData.description);
    data.append("price", formData.price);
    if (formData.image) data.append("ImageUrl", formData.image);

    axios
      .post(API_URL, data, { 
        headers: { "Content-Type": "multipart/form-data" },
        withCredentials: true 
      })
      .then(() => window.location.reload())
      .catch((err) => {
        setError("Error adding product: " + (err.response?.data?.message || err.message));
      });
  };

  const handleUpdate = () => {
    if (!validateForm()) {
      return;
    }

    const data = new FormData();
    data.append("name", formData.name);
    data.append("description", formData.description);
    data.append("price", formData.price);
    if (formData.image) data.append("ImageUrl", formData.image);

    axios
      .put(`${API_URL}/${selectedId}`, data, { 
        headers: { "Content-Type": "multipart/form-data" },
        withCredentials: true
      })
      .then(() => window.location.reload())
      .catch((err) => {
        setError("Error updating product: " + (err.response?.data?.message || err.message));
      });
  };

  const handleDelete = (id) => {
    if (window.confirm("Are you sure you want to delete this product?")) {
      axios
        .delete(`${API_URL}/${id}`, { withCredentials: true })
        .then(() => window.location.reload())
        .catch((err) => {
          setError("Error deleting product: " + err.message);
        });
    }
  };

  const handleAddToCart = async () => {
    if (!selectedId) return;
    
    setIsAddingToCart(true);
    setError("");

    try {
      await addToCart(selectedId, quantity);
      alert("Đã thêm vào giỏ hàng thành công!");
      setShowForm(false);
      // Refresh cart count
      const cartData = await fetchCart();
      let items = [];
      if (Array.isArray(cartData)) {
        items = cartData;
      } else if (cartData?.items && Array.isArray(cartData.items)) {
        items = cartData.items;
      }
      const count = items.reduce((sum, item) => sum + (item.quantity || 0), 0);
      setCartItemCount(count);
    } catch (error) {
      setError(
        error.response?.data?.message || 
        "Không thể thêm vào giỏ hàng. Vui lòng đăng nhập lại."
      );
    } finally {
      setIsAddingToCart(false);
    }
  };

  const handleBuyNow = async () => {
    if (!selectedId) return;
    
    setIsAddingToCart(true);
    setError("");

    try {
      await addToCart(selectedId, quantity);
      setShowForm(false);
      navigate("/cart");
    } catch (error) {
      setError(
        error.response?.data?.message || 
        "Không thể thêm vào giỏ hàng. Vui lòng đăng nhập lại."
      );
    } finally {
      setIsAddingToCart(false);
    }
  };

  const isAuthenticated = () => {
    return !!localStorage.getItem('token');
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    navigate('/login');
  };

  return (
    <div className="app-container">
      <header>
        <h1>Product Management</h1>
        <div className="header-actions">
          {isAuthenticated() ? (
            <>
              <span className="user-info">
                Xin chào, {localStorage.getItem('username') || 'User'}!
              </span>
              <button 
                className="cart-btn" 
                onClick={() => navigate('/cart')}
                style={{
                  position: 'relative',
                  padding: '8px 16px',
                  background: '#3b82f6',
                  color: 'white',
                  border: 'none',
                  borderRadius: '8px',
                  cursor: 'pointer',
                  fontWeight: '600',
                  marginRight: '10px'
                }}
              >
                🛒 Giỏ hàng
                {cartItemCount > 0 && (
                  <span style={{
                    position: 'absolute',
                    top: '-8px',
                    right: '-8px',
                    background: '#ef4444',
                    color: 'white',
                    borderRadius: '50%',
                    width: '20px',
                    height: '20px',
                    fontSize: '12px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontWeight: 'bold'
                  }}>
                    {cartItemCount}
                  </span>
                )}
              </button>
              <button className="logout-btn" onClick={handleLogout}>
                Đăng xuất
              </button>
              <button
                className="add-btn"
                onClick={() => {
                  setFormData({ name: "", description: "", price: "", image: null });
                  setFieldErrors({ name: "", description: "", price: "", image: "" });
                  setShowForm(true);
                  setIsEditing(true);
                  setSelectedId(null);
                  setError("");
                  setQuantity(1);
                }}
              >
                ➕ Add Product
              </button>
            </>
          ) : (
            <div className="auth-buttons">
              <button className="login-btn" onClick={() => navigate('/login')}>
                Đăng nhập
              </button>
              <button className="register-btn" onClick={() => navigate('/register')}>
                Đăng ký
              </button>
            </div>
          )}
        </div>
      </header>

      {loading ? (
        <Loading />
      ) : error && products.length === 0 ? (
        <div className="error-message">{error}</div>
      ) : products.length === 0 ? (
        <div className="empty-state">No products found. Add your first product!</div>
      ) : (
        <div className="product-list">
          {products.map((item) => (
            <div key={item.id} className="product-card">
              <img src={item.imageUrl} alt={item.name} className="product-img" />
              <div className="product-info">
                <h3>{item.name}</h3>
                {item.description && (
                  <p className="product-description">{item.description}</p>
                )}
                {item.price && (
                  <p className="product-price">
                    {new Intl.NumberFormat('vi-VN', {
                      style: 'currency',
                      currency: 'VND'
                    }).format(item.price)}
                  </p>
                )}
              </div>
              <div className="actions">
                <button onClick={() => handleView(item.id)}>👁 View</button>
                <button
                  onClick={() => {
                    setFormData(item);
                    setFieldErrors({ name: "", description: "", price: "", image: "" });
                    setSelectedId(item.id);
                    setIsEditing(true);
                    setShowForm(true);
                    setError("");
                  }}
                >
                  ✏ Update
                </button>
                <button onClick={() => handleDelete(item.id)}>🗑 Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {showForm && (
        <div className="custom-modal-overlay" onClick={() => {
          setShowForm(false);
          setFieldErrors({ name: "", description: "", price: "", image: "" });
          setError("");
        }}>
          <div className="custom-modal-container" onClick={(e) => e.stopPropagation()}>
            <h2>
              {isEditing ? (selectedId ? "Update Product" : "Add Product") : "View Product"}
            </h2>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (isEditing) {
                  selectedId ? handleUpdate() : handleAdd();
                }
              }}
            >
              {error && <div className="error-message" style={{
                padding: '12px',
                marginBottom: '16px',
                backgroundColor: '#fee2e2',
                color: '#dc2626',
                borderRadius: '8px',
                border: '1px solid #fecaca',
                fontSize: '14px',
                fontWeight: '500'
              }}>{error}</div>}

              <div style={{ marginBottom: '20px' }}>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '8px', 
                  fontWeight: '600',
                  color: '#374151'
                }}>
                  Tên sản phẩm <span style={{ color: '#ef4444' }}>*</span>
                </label>
                <input
                  type="text"
                  value={formData.name || ""}
                  onChange={(e) => {
                    const value = e.target.value;
                    setFormData({ ...formData, name: value });
                    if (isEditing) {
                      const error = validateField("name", value);
                      setFieldErrors({ ...fieldErrors, name: error });
                    }
                  }}
                  onBlur={() => {
                    if (isEditing) {
                      const error = validateField("name", formData.name);
                      setFieldErrors({ ...fieldErrors, name: error });
                    }
                  }}
                  disabled={!isEditing}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: fieldErrors.name ? '2px solid #ef4444' : '2px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    transition: 'border-color 0.2s'
                  }}
                />
                {fieldErrors.name && (
                  <p style={{
                    color: '#ef4444',
                    fontSize: '12px',
                    marginTop: '4px',
                    marginBottom: '0'
                  }}>⚠️ {fieldErrors.name}</p>
                )}
              </div>

              <div style={{ marginBottom: '20px' }}>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '8px', 
                  fontWeight: '600',
                  color: '#374151'
                }}>
                  Mô tả sản phẩm <span style={{ color: '#ef4444' }}>*</span>
                </label>
                <textarea
                  value={formData.description || ""}
                  onChange={(e) => {
                    const value = e.target.value;
                    setFormData({ ...formData, description: value });
                    if (isEditing) {
                      const error = validateField("description", value);
                      setFieldErrors({ ...fieldErrors, description: error });
                    }
                  }}
                  onBlur={() => {
                    if (isEditing) {
                      const error = validateField("description", formData.description);
                      setFieldErrors({ ...fieldErrors, description: error });
                    }
                  }}
                  disabled={!isEditing}
                  rows={4}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: fieldErrors.description ? '2px solid #ef4444' : '2px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    fontFamily: 'inherit',
                    resize: 'vertical',
                    transition: 'border-color 0.2s'
                  }}
                />
                {fieldErrors.description && (
                  <p style={{
                    color: '#ef4444',
                    fontSize: '12px',
                    marginTop: '4px',
                    marginBottom: '0'
                  }}>⚠️ {fieldErrors.description}</p>
                )}
              </div>

              <div style={{ marginBottom: '20px' }}>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '8px', 
                  fontWeight: '600',
                  color: '#374151'
                }}>
                  Giá sản phẩm (VNĐ) <span style={{ color: '#ef4444' }}>*</span>
                </label>
                <input
                  type="number"
                  min="0"
                  value={formData.price || ""}
                  onChange={(e) => {
                    const value = e.target.value;
                    setFormData({ ...formData, price: value });
                    if (isEditing) {
                      const error = validateField("price", value);
                      setFieldErrors({ ...fieldErrors, price: error });
                    }
                  }}
                  onBlur={() => {
                    if (isEditing) {
                      const error = validateField("price", formData.price);
                      setFieldErrors({ ...fieldErrors, price: error });
                    }
                  }}
                  disabled={!isEditing}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: fieldErrors.price ? '2px solid #ef4444' : '2px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    transition: 'border-color 0.2s'
                  }}
                />
                {fieldErrors.price && (
                  <p style={{
                    color: '#ef4444',
                    fontSize: '12px',
                    marginTop: '4px',
                    marginBottom: '0'
                  }}>⚠️ {fieldErrors.price}</p>
                )}
              </div>

              <div style={{ marginBottom: '20px' }}>
                <label style={{ 
                  display: 'block', 
                  marginBottom: '8px', 
                  fontWeight: '600',
                  color: '#374151'
                }}>
                  Ảnh sản phẩm {!selectedId && <span style={{ color: '#ef4444' }}>*</span>}
                </label>
                <input
                  type="file"
                  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                  onChange={(e) => {
                    const file = e.target.files[0];
                    setFormData({ ...formData, image: file || null });
                    if (isEditing && file) {
                      const error = validateField("image", null, file);
                      setFieldErrors({ ...fieldErrors, image: error });
                    } else {
                      setFieldErrors({ ...fieldErrors, image: "" });
                    }
                  }}
                  disabled={!isEditing}
                  style={{
                    width: '100%',
                    padding: '12px',
                    border: fieldErrors.image ? '2px solid #ef4444' : '2px solid #e5e7eb',
                    borderRadius: '8px',
                    fontSize: '14px',
                    transition: 'border-color 0.2s'
                  }}
                />
                {fieldErrors.image && (
                  <p style={{
                    color: '#ef4444',
                    fontSize: '12px',
                    marginTop: '4px',
                    marginBottom: '0'
                  }}>⚠️ {fieldErrors.image}</p>
                )}
                {!selectedId && (
                  <p style={{
                    color: '#6b7280',
                    fontSize: '12px',
                    marginTop: '4px',
                    marginBottom: '0'
                  }}>💡 Chỉ chấp nhận file JPG, PNG, GIF, WEBP. Kích thước tối đa 5MB</p>
                )}
              </div>

              {formData.imageUrl && !formData.image && (
                <img src={formData.imageUrl} alt="preview" className="preview-img" />
              )}
              
              {formData.image && (
                <img
                  src={URL.createObjectURL(formData.image)}
                  alt="preview"
                  className="preview-img"
                />
              )}

              {isEditing && (
                <button 
                  type="submit" 
                  className="save-btn"
                  style={{
                    width: '100%',
                    padding: '14px 24px',
                    backgroundColor: Object.values(fieldErrors).some(err => err !== "") 
                      ? '#9ca3af' 
                      : '#3b82f6',
                    color: '#ffffff',
                    border: 'none',
                    borderRadius: '8px',
                    fontSize: '16px',
                    fontWeight: '600',
                    cursor: Object.values(fieldErrors).some(err => err !== "") 
                      ? 'not-allowed' 
                      : 'pointer',
                    transition: 'all 0.2s ease',
                    marginTop: '8px'
                  }}
                  disabled={Object.values(fieldErrors).some(err => err !== "")}
                >
                  {selectedId ? '💾 Lưu thay đổi' : '➕ Tạo sản phẩm'}
                </button>
              )}

              {/* Add to Cart section - chỉ hiển thị khi View (không phải Edit) */}
              {!isEditing && selectedId && (
                <div className="cart-actions">
                  <label>Số lượng:</label>
                  <input
                    type="number"
                    min="1"
                    value={quantity}
                    onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 1))}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      border: '2px solid #e5e7eb',
                      borderRadius: '8px',
                      fontSize: '14px'
                    }}
                  />
                  <div className="cart-buttons">
                    <button
                      type="button"
                      onClick={handleAddToCart}
                      disabled={isAddingToCart}
                      className="add-to-cart-btn"
                    >
                      {isAddingToCart ? "Đang thêm..." : "🛒 Thêm vào giỏ hàng"}
                    </button>
                    <button
                      type="button"
                      onClick={handleBuyNow}
                      disabled={isAddingToCart}
                      className="buy-now-btn"
                    >
                      💳 Mua ngay & Thanh toán
                    </button>
                  </div>
                </div>
              )}

              <button 
                type="button" 
                className="cancel-btn" 
                onClick={() => {
                  setShowForm(false);
                  setFieldErrors({ name: "", description: "", price: "", image: "" });
                  setError("");
                }}
                style={{
                  width: '100%',
                  padding: '14px 24px',
                  backgroundColor: '#ffffff',
                  color: '#4b5563',
                  border: '2px solid #e5e7eb',
                  borderRadius: '8px',
                  fontSize: '16px',
                  fontWeight: '600',
                  cursor: 'pointer',
                  marginTop: '12px',
                  transition: 'all 0.2s ease'
                }}
              >
                ✖️ Đóng
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;