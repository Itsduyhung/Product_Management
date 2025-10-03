import React, { useEffect, useState } from "react";
import axios from "axios";
import "./App.css";

const API_URL = "https://product-management-4.onrender.com";

function App() {
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

  useEffect(() => {
    axios
      .get(API_URL)
      .then((res) => setProducts(res.data))
      .catch((err) => console.error("Error fetching products:", err));
  }, []);

  const handleView = (id) => {
    setError(""); // Xóa lỗi cũ
    axios.get(`${API_URL}/${id}`).then((res) => {
      setFormData(res.data);
      setSelectedId(id);
      setIsEditing(false);
      setShowForm(true);
    });
  };

  // (Validation)
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
      .post(API_URL, data, { headers: { "Content-Type": "multipart/form-data" } })
      .then(() => window.location.reload())
      .catch((err) => {
        setError("Error adding product: " + err.message);
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
      .put(`${API_URL}/${selectedId}`, data, { headers: { "Content-Type": "multipart/form-data" } })
      .then(() => window.location.reload())
      .catch((err) => {
        setError("Error updating product: " + err.message);
      });
  };

  const handleDelete = (id) => {
    axios.delete(`${API_URL}/${id}`).then(() => window.location.reload());
  };

  return (
    <div className="app-container">
      <header>
        <h1>Product Management</h1>
        <button
          className="add-btn"
          onClick={() => {
            setFormData({ name: "", description: "", price: "", image: null });
            setShowForm(true);
            setIsEditing(true);
            setSelectedId(null);
            setError("");
          }}
        >
          ➕ Add Product
        </button>
      </header>

      <div className="product-list">
        {products.map((item) => (
          <div key={item.id} className="product-card">
            <img src={item.imageUrl} alt={item.name} className="product-img" />
            <h3>{item.name}</h3>
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

      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal-container" onClick={(e) => e.stopPropagation()}>
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

              {formData.image &&
                (typeof formData.image === "string" ? (
                  <img src={formData.image} alt="preview" className="preview-img" />
                ) : (
                  <img
                    src={URL.createObjectURL(formData.image)}
                    alt="preview"
                    className="preview-img"
                  />
                ))}

              {isEditing && (
                <button type="submit" className="save-btn">
                   Save
                </button>
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