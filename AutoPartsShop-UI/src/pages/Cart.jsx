import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { getCart, updateCartItem, removeCartItem } from "../api/apiClient";

function Cart() {
  const { accessToken, setAccessToken } = useAuth();

  const [cart, setCart] = useState({ items: [], total: 0, totalItems: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  async function loadCart() {
    setError("");
    setInfo("");
    setLoading(true);

    try {
      const data = await getCart(accessToken, setAccessToken);

      // backend might return array or an object
      if (Array.isArray(data)) {
        setCart({ items: data, total: 0, totalItems: 0 });
      } else {
        setCart({
          items: data.items ?? [],
          total: data.total ?? 0,
          totalItems: data.totalItems ?? 0,
        });
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCart();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleQtyChange(productId, newQty) {
    setError("");
    setInfo("");

    const qty = Number(newQty);
    if (!Number.isFinite(qty) || qty < 1) return;

    try {
      await updateCartItem(accessToken, setAccessToken, productId, qty);
      setInfo("Cart updated ✅");
      await loadCart();
      setTimeout(() => setInfo(""), 1200);
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleRemove(productId) {
    setError("");
    setInfo("");

    try {
      await removeCartItem(accessToken, setAccessToken, productId);
      setInfo("Item removed ✅");
      await loadCart();
      setTimeout(() => setInfo(""), 1200);
    } catch (err) {
      setError(err.message);
    }
  }

  if (loading) return <p>Loading cart...</p>;

  return (
    <div>
      <h2 className="page-title mb-3">Cart</h2>

      {info && <div className="alert alert-success">{info}</div>}
      {error && <div className="alert alert-danger">{error}</div>}

      {cart.items.length === 0 ? (
        <div className="alert alert-secondary">Your cart is empty.</div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table align-middle">
              <thead>
                <tr>
                  <th>Product</th>
                  <th className="text-nowrap">Price</th>
                  <th style={{ width: 140 }}>Qty</th>
                  <th className="text-nowrap">Line total</th>
                  <th></th>
                </tr>
              </thead>

              <tbody>
                {cart.items.map((it) => (
                  <tr key={it.id}>
                    <td>
                      <div className="fw-semibold">
                        {it.productName ?? it.product?.name ?? it.name ?? "Product"}
                      </div>
                      <div className="text-muted small">
                        {it.manufacturer ?? it.product?.manufacturer ?? ""}
                      </div>
                    </td>

                    <td className="text-nowrap">
                      {(it.unitPrice ?? it.price ?? it.product?.price ?? 0)} RON
                    </td>

                    <td>
                      <input
                        type="number"
                        className="form-control"
                        min="1"
                        value={it.quantity ?? it.qty ?? 1}
                        onChange={(e) =>
                          handleQtyChange(it.id, e.target.value)
                        }
                      />
                    </td>

                    <td className="text-nowrap">
                      {(it.lineTotal ?? (it.quantity ?? 1) * (it.unitPrice ?? it.price ?? it.product?.price ?? 0))}{" "}
                      RON
                    </td>

                    <td className="text-end">
                      <button
                        className="btn btn-outline-danger btn-sm"
                        onClick={() => handleRemove(it.id)}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="d-flex justify-content-end">
            <div className="card shadow-sm" style={{ maxWidth: 360, width: "100%" }}>
              <div className="card-body">
                <div className="d-flex justify-content-between">
                  <span className="text-muted">Total items</span>
                  <span className="fw-semibold">{cart.totalItems}</span>
                </div>

                <div className="d-flex justify-content-between mt-2">
                  <span className="text-muted">Total</span>
                  <span className="fw-bold">{cart.total} RON</span>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default Cart;