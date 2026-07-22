import axios from "axios";
import { useAuthStore } from "@/store/authStore";
import type { RefreshTokenResponse } from "@/types/auth.types";

const api = axios.create({
  baseURL: "/api/v1",
  timeout: 15_000,
  headers: { "Content-Type": "application/json" },
});

// Refresh tokens are rotated by the API. When several requests receive a 401
// at the same time, they must share one refresh request; otherwise the first
// request revokes the token and the remaining requests incorrectly log out.
let refreshPromise: Promise<RefreshTokenResponse> | null = null;

const refreshAccessToken = () => {
  if (!refreshPromise) {
    const refreshToken = useAuthStore.getState().refreshToken;
    if (!refreshToken) return Promise.reject(new Error("No refresh token"));

    refreshPromise = api
      .post<RefreshTokenResponse>("/auth/refresh", { token: refreshToken })
      .then((response) => {
        useAuthStore.getState().setAuthFromRefresh(response.data);
        return response.data;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
};

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const originalRequest = err.config;
    const isRefreshRequest = originalRequest?.url?.includes("/auth/refresh");
    if (err.response?.status === 401 && !isRefreshRequest && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const data = await refreshAccessToken();
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return api(originalRequest);
      } catch {
        useAuthStore.getState().clearAuth();
        window.location.href = "/auth/login";
      }
    }
    return Promise.reject(err);
  }
);

export default api;
