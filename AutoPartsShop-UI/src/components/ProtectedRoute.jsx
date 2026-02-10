import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

function ProtectedRoute({ children }) {
  const { accessToken, authReady } = useAuth();

  // waiting for bootstrap auth (refresh on app start)
  if (!authReady) {
    return (
      <div className="container py-5">
        <p>Loading...</p>
      </div>
    );
  }

  if (!accessToken) {
    return <Navigate to="/login" replace />;
  }

  return children;
}

export default ProtectedRoute;