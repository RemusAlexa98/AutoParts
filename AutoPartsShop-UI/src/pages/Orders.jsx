import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { cancelOrder, getMyOrders } from "../api/apiClient";

function Orders() {
    const { accessToken, setAccessToken } = useAuth();

  function formatDate(value) {
    if (!value) return "-";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);

    return d.toLocaleString(undefined, {
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  function getStatusBadgeClass(status) {
    const s = (status || "").toLowerCase();

    if (s === "pending" || s === "placed") return "text-bg-warning";
    if (s === "completed" || s === "delivered") return "text-bg-success";
    if (s === "cancelled" || s === "canceled") return "text-bg-danger";

    return "text-bg-secondary";
  }

  function canCancel(status) {
    const s = (status || "").toLowerCase();
    return s !== "cancelled" && s !== "canceled" && s !== "completed";
  }

  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  async function loadOrders() {
    setError("");
    setInfo("");
    setLoading(true);

    try {
      const data = await getMyOrders(accessToken, setAccessToken);
      setOrders(Array.isArray(data) ? data : data.items ?? []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadOrders();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleCancel(orderId) {
    setError("");
    setInfo("");

    try {
      await cancelOrder(accessToken, setAccessToken, orderId);
      setInfo("Order cancelled ✅");
      await loadOrders();
      setTimeout(() => setInfo(""), 1200);
    } catch (err) {
      setError(err.message);
    }
  }

  if (loading) return <p>Loading orders...</p>;

  return (
    <div>
      <h2 className="page-title mb-3">My Orders</h2>

      {info && <div className="alert alert-success">{info}</div>}
      {error && <div className="alert alert-danger">{error}</div>}

      {orders.length === 0 ? (
        <div className="alert alert-secondary">No orders yet.</div>
      ) : (
        <div className="row">
          {orders.map((o) => (
            <div className="col-12 col-lg-6 mb-4" key={o.id}>
              <div className="card shadow-sm">
                <div className="card-body">
                  <div className="d-flex justify-content-between align-items-start">
                    <div>
                      <div className="fw-bold">Order #{o.id}</div>
                      <div className="text-muted small">
                        {formatDate(o.createdAt ?? o.createdOn)}
                      </div>
                    </div>

                    <span className={`badge ${getStatusBadgeClass(o.status)}`}>
                      {o.status ?? "Unknown"}
                    </span>
                  </div>

                  <hr />

                  <div className="mb-2">
                    <span className="text-muted">Total:</span>{" "}
                    <span className="fw-bold">
                      {o.total ?? o.totalAmount ?? 0} RON
                    </span>
                  </div>

                  <div className="mb-3">
                    <div className="text-muted mb-1">Items:</div>
                    <ul className="mb-0">
                      {(o.items ?? o.orderItems ?? []).map((it, idx) => (
                        <li key={it.id ?? idx}>
                          {(it.productName ?? it.name ?? "Item")} ×{" "}
                          {(it.quantity ?? 1)}
                        </li>
                      ))}
                    </ul>
                  </div>
                  {canCancel(o.status) ? (
                    <button
                      className="btn btn-outline-danger"
                      onClick={() => handleCancel(o.id)}
                    >
                      Cancel order
                    </button>
                  ) : (
                    <div className="text-muted small">
                      This order has been cancelled.
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default Orders;