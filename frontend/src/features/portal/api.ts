import api from "@/lib/axios";

export const PortalApi = {
  studentProfile: (schoolId: string) => api.get("/people/me/student-profile", { params: { schoolId } }).then((r) => r.data),
  studentDashboard: (schoolId: string, enrollmentId: string) => api.get(`/portal/student/${enrollmentId}/dashboard`, { params: { schoolId } }).then((r) => r.data),
  studentResource: (schoolId: string, enrollmentId: string, resource: string) => {
    if (resource === "profile") return api.get("/portal/student/profile", { params: { schoolId } }).then((r) => r.data);
    // The current timetable endpoint requires a term id. Classes include the student's published schedule slots,
    // so use that supported read model until an active-term timetable endpoint is available.
    const endpoint = resource === "schedule" ? "classes" : resource;
    return api.get(`/portal/student/${enrollmentId}/${endpoint}`, { params: { schoolId } }).then((r) => r.data);
  },
  studentNotifications: (schoolId: string) => api.get("/portal/student/notifications", { params: { schoolId } }).then((r) => r.data),
  parentDashboard: (schoolId: string) => api.get("/portal/parent/dashboard", { params: { schoolId } }).then((r) => r.data),
  parentChildren: (schoolId: string) => api.get("/people/me/children", { params: { schoolId } }).then((r) => r.data),
  parentResource: (schoolId: string, studentId: string, resource: string) => api.get(`/portal/parent/children/${studentId}/${resource}`, { params: { schoolId } }).then((r) => r.data),
  parentNotifications: (schoolId: string) => api.get("/portal/parent/notifications", { params: { schoolId } }).then((r) => r.data),
  parentBilling: (schoolId: string) => api.get("/portal/parent/billing", { params: { schoolId } }).then((r) => r.data),
};
