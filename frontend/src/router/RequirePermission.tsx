import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import { ROLE_HOME } from "@/lib/constants";
import type { UserRole } from "@/types/auth.types";

interface RequirePermissionProps {
  permission: string;
  requiredRole?: UserRole;
}

export default function RequirePermission({ permission, requiredRole }: RequirePermissionProps) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const hasPermission = useAuthStore((s) => s.hasPermission);

  if (!isAuthenticated || !user) {
    return <Navigate to="/auth/login" replace />;
  }

  if (requiredRole && user.role !== requiredRole && user.role !== "platform_admin") {
    return <Navigate to={ROLE_HOME[user.role]} replace />;
  }

  if (user.role !== "platform_admin" && !hasPermission(permission)) {
    return <Navigate to={ROLE_HOME[user.role] ?? "/auth/login"} replace />;
  }

  return <Outlet />;
}
