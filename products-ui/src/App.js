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

  const validateForm = () => {
    if (!formData.name) {
      setError("Product Name is required!");
      return false;
    }
    if (!formData.description) {
      setError("Product Description is required!");
      return false;
    }
    if (!formData.price || parseFloat(formData.price) < 0) {
      setError("Price is required and must be a non-negative number!");
      return false;
    }
    if (!selectedId && !formData.image) {
      setError("Image is required for a new product!");
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
        <div className="custom-modal-overlay" onClick={() => setShowForm(false)}>
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
              {error && <div className="error-message">{error}</div>}

              <label>Name</label>
              <input
                type="text"
                value={formData.name || ""}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                disabled={!isEditing}
              />

              <label>Description</label>
              <input
                type="text"
                value={formData.description || ""}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                disabled={!isEditing}
              />

              <label>Price</label>
              <input
                type="number"
                value={formData.price || ""}
                onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                disabled={!isEditing}
              />

              <label>Image</label>
              <input
                type="file"
                onChange={(e) => setFormData({ ...formData, image: e.target.files[0] })}
                disabled={!isEditing}
              />

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
                <button type="submit" className="save-btn">
                   Save
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

              <button type="button" className="cancel-btn" onClick={() => setShowForm(false)}>
                 Close
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;