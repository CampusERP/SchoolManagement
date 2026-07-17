import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthenticatedUser, LoginResponse } from "@/types/auth.types";

interface AuthState {
  user: AuthenticatedUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  setAuth: (payload: LoginResponse) => void;
  clearAuth: () => void;
  isAuthenticated: boolean;
  hasPermission: (perm: string) => boolean;
  isRole: (role: string) => boolean;
}

function decodeJwt(token: string): any {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error("Failed to decode JWT", error);
    return null;
  }
}

function mapBackendRole(role: string): any {
  switch (role) {
    case "SuperAdmin":
      return "platform_admin";
    case "SchoolAdmin":
      return "school_admin";
    case "Teacher":
      return "teacher";
    case "Student":
      return "student";
    case "Parent":
      return "parent";
    default:
      return "student";
  }
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      setAuth: (payload) => {
        const decoded = decodeJwt(payload.accessToken);
        const rawPermissions = decoded?.permission;
        const permissions = Array.isArray(rawPermissions)
          ? rawPermissions
          : rawPermissions
          ? [rawPermissions]
          : [];

        const emailPrefix = payload.email.split("@")[0] || "User";
        const parts = emailPrefix.split(/[\._-]/);
        const firstName = parts[0] ? parts[0].charAt(0).toUpperCase() + parts[0].slice(1) : "User";
        const lastName = parts[1] ? parts[1].charAt(0).toUpperCase() + parts[1].slice(1) : "Account";

        const user: AuthenticatedUser = {
          id: payload.userId,
          email: payload.email,
          firstName,
          lastName,
          role: mapBackendRole(payload.role),
          schoolId: payload.schoolId || undefined,
          permissions,
          isPlatformAdmin: decoded?.is_platform_admin === "true",
        };

        set({
          user,
          accessToken: payload.accessToken,
          refreshToken: payload.refreshToken,
          isAuthenticated: true,
        });
      },
      clearAuth: () =>
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),
      hasPermission: (perm) => {
        const u = get().user;
        if (!u) return false;
        if (u.isPlatformAdmin) return true;
        return u.permissions.includes(perm);
      },
      isRole: (role) => get().user?.role === role,
    }),
    { name: "auth-storage" }
  )
);
