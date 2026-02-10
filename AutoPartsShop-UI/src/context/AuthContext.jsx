import { createContext, useContext, useEffect, useState } from "react";
import { logoutApi, refreshAccessToken } from "../api/apiClient";

let refreshInProgress = false;

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [accessToken, setAccessToken] = useState(null);
  const [authReady, setAuthReady] = useState(false);

  //Bootstrap auth on app start / page refresh:
  //if refreshToken exists -> try refresh -> set access token in memory
  useEffect(() => {
    async function bootstrap() {
      const rt = localStorage.getItem("refreshToken");

      if (!rt) {
        setAuthReady(true);
        return;
      }

      if (refreshInProgress) {
        return;
      }

      refreshInProgress = true;

      try {
        const refreshed = await refreshAccessToken(rt);
        setAccessToken(refreshed.accessToken);
        localStorage.setItem("refreshToken", refreshed.refreshToken);
      } catch {
        localStorage.removeItem("refreshToken");
        setAccessToken(null);
      } finally {
        refreshInProgress = false;
        setAuthReady(true);
      }
    }
    bootstrap();
  }, []);

  function login(accessToken, refreshToken) {
    setAccessToken(accessToken);
    localStorage.setItem("refreshToken", refreshToken);
  }

  async function logout() {
    const refreshToken = localStorage.getItem("refreshToken");

    try {
      if (accessToken && refreshToken) {
        await logoutApi(accessToken, refreshToken);
      }
    } catch (err) {
      console.warn("Logout API failed:", err.message);
    } finally {
      setAccessToken(null);
      localStorage.removeItem("refreshToken");
    }
  }

  return (
    <AuthContext.Provider
      value={{ accessToken, setAccessToken, authReady, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}