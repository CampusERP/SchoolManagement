import api from "@/lib/axios";

export interface CreateSchoolPayload {
  name: string;
  subdomainCode: string;
}

export interface UpdateSchoolPayload {
  name: string;
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

export interface SchoolDetail {
  id: string;
  name: string;
  subdomainCode: string;
  status: string;
  campuses: CampusDto[];
}

export const SchoolsApi = {
  create: (data: CreateSchoolPayload) =>
    api.post<{ id: string }>("/schools", data).then((r) => r.data),
  getById: (id: string) =>
    api.get<SchoolDetail>(`/schools/${id}`).then((r) => r.data),
  update: (id: string, data: UpdateSchoolPayload) =>
    api.put(`/schools/${id}`, data).then((r) => r.data),
  registerAdmin: (data: RegisterSchoolAdminPayload) =>
    api.post<{ id: string }>("/auth/register-school-admin", data).then((r) => r.data),
};
