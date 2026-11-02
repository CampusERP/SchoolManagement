import api from "@/lib/axios";

export interface CreateSchoolPayload {
  name: string;
  subdomainCode: string;
}

export interface UpdateSchoolPayload {
  name: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  status?: string;
}

export interface AddCampusPayload {
  name: string;
  address: string;
}

export interface RegisterSchoolAdminPayload {
  schoolId: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface CampusDto {
  id: string;
  name: string;
  address: string;
}

export interface SchoolAdminDto {
  userId: string;
  firstName: string;
  lastName: string;
  email?: string | null;
}

export interface SchoolDetail {
  id: string;
  name: string;
  subdomainCode: string;
  status: string;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  campuses: CampusDto[];
  admins: SchoolAdminDto[];
}

export const SchoolsApi = {
  create: (data: CreateSchoolPayload) =>
    api.post<{ id: string }>("/schools", data).then((r) => r.data),
  getById: (id: string) =>
    api.get<SchoolDetail>(`/schools/${id}`).then((r) => r.data),
  update: (id: string, data: UpdateSchoolPayload) =>
    api.put(`/schools/${id}`, data).then((r) => r.data),
  addCampus: (schoolId: string, data: AddCampusPayload) =>
    api.post<{ id: string }>(`/schools/${schoolId}/campuses`, data).then((r) => r.data),
  updateCampus: (schoolId: string, campusId: string, data: AddCampusPayload) =>
    api.put(`/schools/${schoolId}/campuses/${campusId}`, data).then((r) => r.data),
  deleteCampus: (schoolId: string, campusId: string) =>
    api.delete(`/schools/${schoolId}/campuses/${campusId}`).then((r) => r.data),
  suspend: (schoolId: string) =>
    api.post(`/schools/${schoolId}/suspend`).then((r) => r.data),
  reactivate: (schoolId: string) =>
    api.post(`/schools/${schoolId}/reactivate`).then((r) => r.data),
  registerAdmin: (data: RegisterSchoolAdminPayload) =>
    api.post<{ id: string }>("/auth/register-school-admin", data).then((r) => r.data),
};
