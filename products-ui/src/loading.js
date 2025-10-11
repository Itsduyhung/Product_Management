import React, { Component } from "react";
import "./Loading.css"; // Import hiệu ứng CSS

class Loading extends Component {
  render() {
    return (
      <div className="loading-overlay">
        <div className="loading-spinner"></div>
        <p className="loading-text">Đang tải dữ liệu...</p>
      </div>
    );
  }
}

export default Loading;
