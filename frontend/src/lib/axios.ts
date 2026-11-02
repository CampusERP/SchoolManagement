import axios from "axios";
import { useAuthStore } from "@/store/authStore";
import { t } from "@/i18n";
import type { RefreshTokenResponse } from "@/types/auth.types";

const api = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL.replace(/\/+$/, "")}/api/v1`,
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
    if (!refreshToken) return Promise.reject(new Error(t("No refresh token")));

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

    // A 402 from the subscription guard means the school's paid period has
    // ended (or is awaiting payment). Send the user to a page that explains
    // the situation instead of letting every screen fail one request at a time.
    if (
      err.response?.status === 402 &&
      err.response?.data?.code === "subscription_required" &&
      !window.location.pathname.startsWith("/subscription-expired")
    ) {
      window.location.href = "/subscription-expired";
      return Promise.reject(err);
    }

    // A suspended school is blocked by SchoolSuspensionMiddleware. Send every
    // account in that school to an explanation page instead of failing requests
    // one by one. The page does not call authenticated APIs, so this cannot loop.
    if (
      err.response?.status === 403 &&
      err.response?.data?.code === "school_suspended" &&
      !window.location.pathname.startsWith("/school-suspended")
    ) {
      window.location.href = "/school-suspended";
      return Promise.reject(err);
    }

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
