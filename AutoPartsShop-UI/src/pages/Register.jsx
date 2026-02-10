import { useState } from "react";
import { registerApi } from "../api/apiClient";
import { Link, useNavigate } from "react-router-dom";

function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [info, setInfo] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setInfo("");
    setError("");

    try {
      await registerApi(email, password);
      setInfo("Account created, you can login now.");
      setTimeout(() => navigate("/login"), 800);
    } catch (err) {
      setError(err.message || "Register failed");
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card card shadow-sm">
        <div className="auth-card-body card-body">
          <h2 className="auth-title">Register</h2>

          {info && <div className="alert alert-success">{info}</div>}
          {error && <div className="alert alert-danger">{error}</div>}

          <div className="auth-form">
            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">Email</label>
                <input className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} />
              </div>

                <div className="mb-3">
                <label className="form-label">New password</label>

                <div className="input-group">
                    <input
                    type={showPassword ? "text" : "password"}
                    className="form-control"
                    value={password}
                    onChange={(e) => setNewPassword(e.target.value)}
                    />
                    <button
                    type="button"
                    className="btn btn-outline-secondary password-toggle-btn"
                    onClick={() => setShowPassword((v) => !v)}
                    >
                    {showPassword ? "Hide" : "Show"}
                    </button>
                </div>
                </div>

              <button className="btn btn-primary w-100" type="submit">
                Create account
              </button>
            </form>
          </div>

          <div className="auth-footer center">
            <Link to="/login">Back to login</Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Register;