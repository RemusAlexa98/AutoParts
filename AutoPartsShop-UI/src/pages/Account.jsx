import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { deactivateSelf, deleteSelf, getMe } from "../api/apiClient";
import { useNavigate } from "react-router-dom";

function Account() {
  const { accessToken, setAccessToken, logout } = useAuth();
  const [me, setMe] = useState(null);
  const [loading, setLoading] = useState(true);

  const [info, setInfo] = useState("");
  const [error, setError] = useState("");

  const navigate = useNavigate();

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError("");
      try {
        const data = await getMe(accessToken, setAccessToken);
        setMe(data);
      } catch (err) {
        setError(err.message || "Failed to load account.");
      } finally {
        setLoading(false);
      }
    }

    if (accessToken) load();
  }, [accessToken, setAccessToken]);

  async function handleDeactivate() {
    setInfo("");
    setError("");

    const ok = window.confirm("Deactivate your account? You will be logged out.");
    if (!ok) return;

    try {
      const msg = await deactivateSelf(accessToken, setAccessToken);
      setInfo(msg);

      // logout (removes refresh token + access token)
      await logout();
      navigate("/login");
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleDelete() {
    setInfo("");
    setError("");

    const ok = window.confirm("Delete your account? This is irreversible (soft delete).");
    if (!ok) return;

    try {
      const msg = await deleteSelf(accessToken, setAccessToken);
      setInfo(msg);

      await logout();
      navigate("/login");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div>
      <h2 className="page-title mb-3">Account</h2>

      {info && <div className="alert alert-success">{info}</div>}
      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card shadow-sm mb-3">
        <div className="card-body">
          <h5 className="mb-3">Profile</h5>

                {loading ? (
                <div className="text-muted">Loading...</div>
                ) : me ? (
                <div className="row g-3">
                    <div className="col-12">
                    <div className="small text-muted">Email</div>
                    <div className="fw-semibold">{me.email ?? me.Email ?? "-"}</div>
                    </div>

                    <div className="col-6">
                    <div className="small text-muted">User ID</div>
                    <div className="fw-semibold">{me.userId ?? me.UserId ?? "-"}</div>
                    </div>

                    <div className="col-6">
                    <div className="small text-muted">Role</div>
                    <div className="fw-semibold">{me.role ?? me.Role ?? "-"}</div>
                    </div>
                </div>
                ) : (
                <div className="text-muted">No account info.</div>
                )}
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="card-body">
          <h5 className="mb-3 text-danger">Danger zone</h5>

            <div className="d-grid gap-3" style={{ maxWidth: 360 }}>
            <div>
                <button className="btn btn-warning w-100" onClick={handleDeactivate}>
                Deactivate account
                </button>
                <div className="small text-muted mt-1">
                Disables login (you will be logged out).
                </div>
            </div>

            <div>
                <button className="btn btn-danger w-100" onClick={handleDelete}>
                Delete account
                </button>
                <div className="small text-muted mt-1">
                Marks your account as deleted (soft delete).
                </div>
            </div>
            </div>

        </div>
      </div>
    </div>
  );
}

export default Account;