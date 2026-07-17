import { useQuery } from "@tanstack/react-query";
import { DashboardApi } from "./api";

export const usePlatformDashboard = () =>
  useQuery({
    queryKey: ["dashboard", "platform"],
    queryFn: DashboardApi.getPlatformDashboard,
  });

export const useSchoolDashboard = () =>
  useQuery({
    queryKey: ["dashboard", "school"],
    queryFn: DashboardApi.getSchoolDashboard,
  });

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
