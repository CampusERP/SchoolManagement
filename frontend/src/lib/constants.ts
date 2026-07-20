import type { UserRole } from "@/types/auth.types";

export const ROLE_HOME: Record<UserRole, string> = {
  platform_admin: "/platform",
  school_admin: "/school",
  teacher: "/teacher",
  student: "/student",
  parent: "/parent",
};
