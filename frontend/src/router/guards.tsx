import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import { ROLE_HOME } from "@/lib/constants";
import type { UserRole } from "@/types/auth.types";

interface ProtectedRouteProps {
  requiredRole?: UserRole;
}

export default function ProtectedRoute({ requiredRole }: ProtectedRouteProps) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);

  if (!isAuthenticated || !user) {
    return <Navigate to="/auth/login" replace />;
  }

  if (requiredRole && user.role !== requiredRole) {
    return <Navigate to={ROLE_HOME[user.role]} replace />;
  }

  return <Outlet />;
}
