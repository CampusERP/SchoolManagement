import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { SchoolsApi } from "./api";

export const useCurrentSchool = () => {
  const schoolId = useAuthStore((state) => state.user?.schoolId);
  return useQuery({
    queryKey: ["school-settings", schoolId],
    queryFn: () => SchoolsApi.getById(schoolId!),
    enabled: !!schoolId,
  });
};
