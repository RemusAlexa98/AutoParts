const API_URL = import.meta.env.VITE_API_URL;

export async function login(email, password) {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Login failed");
  }

  return response.json();
}

export async function getProducts(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/products`;

  const response = await fetchWithAuth(
    url,
    {},
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    throw new Error("Failed to load products");
  }

  return response.json();
}

export async function logoutApi(accessToken, refreshToken) {
  const response = await fetch(`${API_URL}/api/auth/logout`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      refreshToken: refreshToken,
    }),
  });

  if (!response.ok) {
    throw new Error("Logout failed");
  }
}

export async function refreshAccessToken(refreshToken) {
  const response = await fetch(`${API_URL}/api/auth/refresh`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ refreshToken }),
  });

  if (!response.ok) {
    throw new Error("Refresh token invalid or expired");
  }

  return response.json(); // { accessToken, refreshToken }
}

export async function fetchWithAuth(url, options = {}, accessToken, onTokenRefreshed) {
  // 1) try request with current access token
  const firstResponse = await fetch(url, {
    ...options,
    headers: {
      ...(options.headers || {}),
      Authorization: `Bearer ${accessToken}`,
    },
  });

  // if it's not 401 -> return normally
  if (firstResponse.status !== 401) {
    return firstResponse;
  }

  // 2) try refresh
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) {
    throw new Error("No refresh token found");
  }

  const refreshed = await refreshAccessToken(refreshToken);

  // save new refresh token (rotation)
  localStorage.setItem("refreshToken", refreshed.refreshToken);

  // notify app to update access token in memory
  if (onTokenRefreshed) {
    onTokenRefreshed(refreshed.accessToken);
  }

  // 3) retry original request with new access token
  const retryResponse = await fetch(url, {
    ...options,
    headers: {
      ...(options.headers || {}),
      Authorization: `Bearer ${refreshed.accessToken}`,
    },
  });

  return retryResponse;
}

export async function addToCart(accessToken, onTokenRefreshed, productId, quantity = 1) {
  const url = `${API_URL}/api/cart/items`;

  const response = await fetchWithAuth(
    url,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ productId, quantity }),
    },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    let msg = "Failed to add to cart";
    try {
      const err = await response.json();
      msg = err.detail || msg;
    } catch {}
    throw new Error(msg);
  }
}

export async function getCart(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/cart`;

  const response = await fetchWithAuth(url, {}, accessToken, onTokenRefreshed);

  if (!response.ok) {
    throw new Error("Failed to load cart");
  }

  return response.json(); // expect { items: [...], total, totalItems } OR just array
}

export async function updateCartItem(accessToken, onTokenRefreshed, cartItemId, quantity) {
  const url = `${API_URL}/api/cart/items/${cartItemId}`;

  const response = await fetchWithAuth(
    url,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ quantity }),
    },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    throw new Error("Failed to update cart item");
  }
}

export async function removeCartItem(accessToken, onTokenRefreshed, cartItemId) {
  const url = `${API_URL}/api/cart/items/${cartItemId}`;

  const response = await fetchWithAuth(
    url,
    { method: "DELETE" },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    throw new Error("Failed to remove cart item");
  }
}

export async function placeOrder(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/orders`;

  const response = await fetchWithAuth(
    url,
    { method: "POST" },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    let msg = "Failed to place order";
    try {
      const err = await response.json();
      msg = err.detail || msg;
    } catch {}
    throw new Error(msg);
  }

  return response.json(); // if backend returns something (optional)
}

export async function getMyOrders(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/orders`;

  const response = await fetchWithAuth(url, {}, accessToken, onTokenRefreshed);

  if (!response.ok) {
    throw new Error("Failed to load orders");
  }

  return response.json();
}

export async function cancelOrder(accessToken, onTokenRefreshed, orderId) {
  const url = `${API_URL}/api/orders/${orderId}/cancel`;

  const response = await fetchWithAuth(
    url,
    { method: "POST" },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    let msg = "Failed to cancel order";
    try {
      const err = await response.json();
      msg = err.detail || msg;
    } catch {}
    throw new Error(msg);
  }

  return response.json(); // optional
}

export async function registerApi(email, password) {
  const response = await fetch(`${API_URL}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    let msg = "Register failed";
    try {
      const err = await response.json();
      msg = err.detail || err.message || msg;
    } catch {}
    throw new Error(msg);
  }

  return response.json(); // dacă backend returnează ceva
}

export async function forgotPasswordApi(email) {
  const response = await fetch(`${API_URL}/api/auth/forgot-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email }),
  });

  if (!response.ok) {
    let msg = "Forgot password failed";
    try {
      const err = await response.json();
      msg = err.detail || err.message || msg;
    } catch {}
    throw new Error(msg);
  }

  // ✅ backend returns { message, resetToken, expiresAt } in DEV
  return response.json();
}

export async function resetPasswordApi(token, newPassword) {
  const response = await fetch(`${API_URL}/api/auth/reset-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token, newPassword }),
  });

  if (!response.ok) {
    let msg = "Reset password failed";
    try {
      const err = await response.json();
      msg = err.detail || err.message || msg;
    } catch {}
    throw new Error(msg);
  }
}

export async function getMe(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/auth/me`;

  const response = await fetchWithAuth(url, {}, accessToken, onTokenRefreshed);

  if (!response.ok) {
    throw new Error("Failed to load account details");
  }

  return response.json(); // { Email, userId, role }
}

export async function deactivateSelf(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/auth/deactivate`;
  const response = await fetchWithAuth(
    url,
    { method: "POST" },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    let msg = "Deactivate failed";
    try {
      const err = await response.json();
      msg = err.detail || err.message || msg;
    } catch {}
    throw new Error(msg);
  }

  try {
    const data = await response.json(); // { message }
    return data.message || "Account deactivated.";
  } catch {
    return "Account deactivated.";
  }
}

export async function deleteSelf(accessToken, onTokenRefreshed) {
  const url = `${API_URL}/api/auth/delete`;
  const response = await fetchWithAuth(
    url,
    { method: "DELETE" },
    accessToken,
    onTokenRefreshed
  );

  if (!response.ok) {
    let msg = "Delete account failed";
    try {
      const err = await response.json();
      msg = err.detail || err.message || msg;
    } catch {}
    throw new Error(msg);
  }

  try {
    const data = await response.json(); // { message }
    return data.message || "Account deleted.";
  } catch {
    return "Account deleted.";
  }
}