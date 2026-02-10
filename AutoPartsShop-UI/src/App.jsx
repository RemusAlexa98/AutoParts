import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Products from "./pages/Products";
import Cart from "./pages/Cart";
import Orders from "./pages/Orders";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import "./App.css";
import Register from "./pages/Register";
import ForgotPassword from "./pages/ForgotPassword";
import ResetPassword from "./pages/ResetPassword";
import Account from "./pages/Account";

function App() {
  return (
<BrowserRouter>
  <Navbar />

  <Routes>
    {/* Login is its own centered page */}
    <Route path="/login" element={<Login />} />
    <Route path="/register" element={<Register />} />
    <Route path="/forgot-password" element={<ForgotPassword />} />
    <Route path="/reset-password" element={<ResetPassword />} />

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

    <Route
      path="/account"
      element={
        <div className="app-layout">
          <div className="app-container">
            <ProtectedRoute>
              <Account />
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