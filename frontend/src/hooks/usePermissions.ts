import { useAuthStore } from "@/store/authStore";

export function usePermissions() {
  const permissions = useAuthStore((s) => s.user?.permissions ?? []);
  const isPlatformAdmin = useAuthStore((s) => s.user?.isPlatformAdmin ?? false);
  const hasPermission = useAuthStore((s) => s.hasPermission);

  const can = (permission: string) => hasPermission(permission);

  const canAny = (...required: string[]) =>
    isPlatformAdmin || required.some((p) => permissions.includes(p));

  return { permissions, isPlatformAdmin, can, canAny };
}
