import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Products from "./pages/Products";
import Cart from "./pages/Cart";
import Orders from "./pages/Orders";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import "./App.css";

function App() {
  return (
<BrowserRouter>
  <Navbar />

  <Routes>
    {/* Login is its own centered page */}
    <Route path="/login" element={<Login />} />

    {/* Everything else uses the app container layout */}
    <Route
      path="/products"
      element={
        <div className="app-layout">
          <div className="app-container">
            <ProtectedRoute>
              <Products />
            </ProtectedRoute>
          </div>
        </div>
      }
    />

    <Route
      path="/cart"
      element={
        <div className="app-layout">
          <div className="app-container">
            <ProtectedRoute>
              <Cart />
            </ProtectedRoute>
          </div>
        </div>
      }
    />

    <Route
      path="/orders"
      element={
        <div className="app-layout">
          <div className="app-container">
            <ProtectedRoute>
              <Orders />
            </ProtectedRoute>
          </div>
        </div>
      }
    />

    <Route path="*" element={<Navigate to="/login" replace />} />
  </Routes>
</BrowserRouter>
  );
}

export default App;