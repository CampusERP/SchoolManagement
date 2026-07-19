import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { DashboardApi } from "./api";

export const usePlatformDashboard = () =>
  useQuery({
    queryKey: ["dashboard", "platform"],
    queryFn: DashboardApi.getPlatformDashboard,
  });

export const useSchoolDashboard = () => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["dashboard", "school", schoolId],
    queryFn: () => schoolId ? DashboardApi.getSchoolDashboard(schoolId) : null,
    enabled: !!schoolId,
  });
};

export const useTeacherDashboard = () =>
  useQuery({
    queryKey: ["dashboard", "teacher"],
    queryFn: DashboardApi.getTeacherDashboard,
  });

export const useSchools = (params?: { page?: number; pageSize?: number; search?: string }) =>
  useQuery({
    queryKey: ["schools", params],
    queryFn: () => DashboardApi.getSchools(params),
  });
