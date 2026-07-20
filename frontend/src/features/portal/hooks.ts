import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { PortalApi } from "./api";

const useSchoolId = () => useAuthStore((s) => s.user?.schoolId);
export const useStudentProfile = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "student", "profile", schoolId], queryFn: () => PortalApi.studentProfile(schoolId!), enabled: !!schoolId }); };
export const useStudentDashboard = (enrollmentId?: string) => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "student", "dashboard", schoolId, enrollmentId], queryFn: () => PortalApi.studentDashboard(schoolId!, enrollmentId!), enabled: !!schoolId && !!enrollmentId }); };
export const useStudentResource = (enrollmentId: string | undefined, resource: string) => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "student", resource, schoolId, enrollmentId], queryFn: () => PortalApi.studentResource(schoolId!, enrollmentId!, resource), enabled: !!schoolId && !!enrollmentId }); };
export const useStudentNotifications = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "student", "notifications", schoolId], queryFn: () => PortalApi.studentNotifications(schoolId!), enabled: !!schoolId }); };
export const useParentDashboard = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "parent", "dashboard", schoolId], queryFn: () => PortalApi.parentDashboard(schoolId!), enabled: !!schoolId }); };
export const useParentChildren = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "parent", "children", schoolId], queryFn: () => PortalApi.parentChildren(schoolId!), enabled: !!schoolId }); };
export const useParentResource = (studentId: string | undefined, resource: string) => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "parent", resource, schoolId, studentId], queryFn: () => PortalApi.parentResource(schoolId!, studentId!, resource), enabled: !!schoolId && !!studentId }); };
export const useParentNotifications = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "parent", "notifications", schoolId], queryFn: () => PortalApi.parentNotifications(schoolId!), enabled: !!schoolId }); };
export const useParentBilling = () => { const schoolId = useSchoolId(); return useQuery({ queryKey: ["portal", "parent", "billing", schoolId], queryFn: () => PortalApi.parentBilling(schoolId!), enabled: !!schoolId }); };
