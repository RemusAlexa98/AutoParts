import { useEffect, useState } from "react";
import { getProducts, addToCart } from "../api/apiClient";
import { useAuth } from "../context/AuthContext";

function Products() {
  const { accessToken, setAccessToken } = useAuth();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  useEffect(() => {
    async function loadProducts() {
      try {
        const data = await getProducts(accessToken, setAccessToken);
        console.log("PRODUCTS API RESPONSE:", data);
        setProducts(Array.isArray(data) ? data : data.items ?? []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    loadProducts();
  }, [accessToken]);

  async function handleAddToCart(productId) {
  setError("");
  setInfo("");

  try {
    await addToCart(accessToken, setAccessToken, productId, 1);
    setInfo("Added to cart ✅");
    setTimeout(() => setInfo(""), 1500);
  } catch (err) {
    setError(err.message);
  }
}


  if (loading) {
    return <p>Loading products...</p>;
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>;
  }

  return (
    <div>
      <h2 className="page-title mb-4">Products</h2>
      {info && <div className="alert alert-success">{info}</div>}
      <div className="row">
        {products.map((product) => (
          <div
            key={product.id}
            className="col-12 col-sm-6 col-md-4 col-lg-3 mb-4"
          >
            <div className="card h-100 shadow-sm product-card">
              <div className="card-body d-flex flex-column">
                <h5 className="card-title">
                  {product.name}
                </h5>

                <h6 className="card-subtitle mb-2 text-muted product-meta">
                  {product.manufacturer}
                </h6>

                <p className="card-text mt-auto">
                  <strong>Price:</strong> {product.price} RON
                </p>

                <p className="card-text">
                  <strong>Stock:</strong> {product.stock}
                </p>

                <button
                  className="btn btn-outline-primary mt-auto"
                  disabled={product.stock === 0}
                  onClick={() => handleAddToCart(product.id)}
                >
                  {product.stock === 0 ? "Out of stock" : "Add to cart"}
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Products;