import { useState } from "react";
import { forgotPasswordApi } from "../api/apiClient";
import { Link, useNavigate } from "react-router-dom";

function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [result, setResult] = useState(null); // { message, resetToken, expiresAt }
  const [error, setError] = useState("");
  const [copied, setCopied] = useState(false);
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setCopied(false);
    setResult(null);

    try {
      const data = await forgotPasswordApi(email);
      setResult(data);
    } catch (err) {
      setError(err.message || "Failed to generate token");
    }
  }

  async function handleCopy() {
    if (!result?.resetToken) return;
    await navigator.clipboard.writeText(result.resetToken);
    setCopied(true);
    setTimeout(() => setCopied(false), 1200);
  }

  function goToReset() {
    if (!result?.resetToken) return;
    navigate(`/reset-password?token=${encodeURIComponent(result.resetToken)}`);
  }

  return (
    <div className="auth-page forgot-page">
      <div className="auth-card card shadow-sm">
        <div className="auth-card-body card-body">
          <h2 className="auth-title">Forgot password</h2>

          {error && <div className="alert alert-danger">{error}</div>}

          <div className="auth-form">
            {!result ? (
                <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label className="form-label">Email</label>
                    <input
                    className="form-control"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    />
                </div>

                {/* 🔹 SPACER: înlocuiește câmpul Password */}
                <div className="mb-3" style={{ visibility: "hidden" }}>
                    <label className="form-label">Spacer</label>
                    <input className="form-control" />
                </div>

                <button className="btn btn-primary w-100" type="submit">
                    Generate reset token
                </button>
                </form>
            ) : (
              <>
                <div className="alert alert-success">
                  {result.message || "Token generated"}
                </div>

                <div className="mb-2 small text-muted">
                  Expires at:{" "}
                  <strong>
                    {result.expiresAt
                      ? new Date(result.expiresAt).toLocaleString()
                      : "-"}
                  </strong>
                </div>

                <label className="form-label">Reset token (DEV)</label>
                <textarea
                  className="form-control"
                  rows={4}
                  value={result.resetToken || ""}
                  readOnly
                />

                <div className="copy-slot mt-2">
                  {copied ? <span className="text-success small">Copied</span> : null}
                </div>

                <div className="d-flex gap-2 mt-2">
                  <button
                    type="button"
                    className="btn btn-outline-secondary w-100"
                    onClick={handleCopy}
                  >
                    Copy token
                  </button>

                  <button
                    type="button"
                    className="btn btn-success w-100"
                    onClick={goToReset}
                  >
                    Go to reset
                  </button>
                </div>
              </>
            )}
          </div>

          <div className="auth-footer center">
            <Link to="/login">Back to login</Link>
            <span />
          </div>
        </div>
      </div>
    </div>
  );
}

export default ForgotPassword;