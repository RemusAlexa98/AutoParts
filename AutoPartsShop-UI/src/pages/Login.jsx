import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../api/apiClient";
import { useAuth } from "../context/AuthContext";

function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const { login: saveAuth } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    try {
      const result = await login(email, password);
      saveAuth(result.accessToken, result.refreshToken);
      navigate("/products");
    } catch (err) {
      setError(err.message);
    }
  }

return (
  <div className="login-page">
    <div className="login-card">
      <div className="card shadow-lg">
        <div className="card-body p-4">
          <h3 className="login-title text-center mb-4">
            AutoPartsShop Login
          </h3>

          {error && (
            <div className="alert alert-danger">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input
                type="email"
                className="form-control form-control-lg"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="mb-4">
              <label className="form-label">Password</label>
              <input
                type="password"
                className="form-control form-control-lg"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button className="btn btn-primary btn-lg w-100">
              Login
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
);
}

export default Login;