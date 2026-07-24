import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthenticatedUser, LoginResponse, RefreshTokenResponse } from "@/types/auth.types";

interface AuthState {
  user: AuthenticatedUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (payload: LoginResponse) => void;
  setAuthFromRefresh: (payload: RefreshTokenResponse) => void;
  clearAuth: () => void;
  hasPermission: (perm: string) => boolean;
  isRole: (role: string) => boolean;
}

function decodeJwt(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

function mapBackendRole(role: string): AuthenticatedUser["role"] {
  const map: Record<string, AuthenticatedUser["role"]> = {
    SuperAdmin: "platform_admin",
    SchoolAdmin: "school_admin",
    Teacher: "teacher",
    Student: "student",
    Parent: "parent",
  };
  return map[role] ?? "student";
}

function buildUserFromToken(accessToken: string, userId: string, email: string, schoolId: string | null, role: string): AuthenticatedUser {
  const decoded = decodeJwt(accessToken);

  const rawPermissions = decoded?.permission;
  const permissions = Array.isArray(rawPermissions)
    ? rawPermissions
    : rawPermissions
      ? [rawPermissions as string]
      : [];

  const isPlatformAdmin = String(decoded?.is_platform_admin).toLowerCase() === "true";

  const emailPrefix = email.split("@")[0] || "User";
  const parts = emailPrefix.split(/[._-]/);
  const firstName = parts[0] ? parts[0].charAt(0).toUpperCase() + parts[0].slice(1) : "User";
  const lastName = parts[1] ? parts[1].charAt(0).toUpperCase() + parts[1].slice(1) : "Account";

  return {
    id: userId,
    email,
    firstName,
    lastName,
    role: mapBackendRole(role),
    schoolId: schoolId ?? undefined,
    permissions,
    isPlatformAdmin,
  };
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (payload) => {
        const user = buildUserFromToken(
          payload.accessToken,
          payload.userId,
          payload.email,
          payload.schoolId,
          payload.role
        );
        set({
          user,
          accessToken: payload.accessToken,
          refreshToken: payload.refreshToken,
          isAuthenticated: true,
        });
      },

      setAuthFromRefresh: (payload) => {
        const current = get().user;
        if (!current) return;

        const newAccessToken = payload.accessToken;
        const decoded = decodeJwt(newAccessToken);
        const rawPermissions = decoded?.permission;
        const permissions = Array.isArray(rawPermissions)
          ? rawPermissions
          : rawPermissions
            ? [rawPermissions as string]
            : [];
        const isPlatformAdmin = String(decoded?.is_platform_admin).toLowerCase() === "true";

        set({
          user: { ...current, permissions, isPlatformAdmin },
          accessToken: newAccessToken,
          refreshToken: payload.refreshToken,
        });
      },

      clearAuth: () =>
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),

      hasPermission: (perm) => {
        const u = get().user;
        if (!u) return false;
        return u.permissions.includes(perm);
      },

      isRole: (role) => get().user?.role === role,
    }),
    { name: "auth-storage" }
  )
);
