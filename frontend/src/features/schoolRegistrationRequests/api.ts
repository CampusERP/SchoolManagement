import api from "@/lib/axios";

export interface CreateSchoolRegistrationRequestPayload {
  schoolName: string;
  subdomainCode: string;
  address?: string;
  phone?: string;
  schoolEmail?: string;
  adminFirstName: string;
  adminLastName: string;
  adminEmail: string;
  adminPassword: string;
  subscriptionPlanId?: string;
}

export interface UpdateSchoolRegistrationRequestPayload {
  schoolName: string;
  subdomainCode: string;
  address?: string;
  phone?: string;
  schoolEmail?: string;
  adminFirstName: string;
  adminLastName: string;
  adminEmail: string;
  adminPassword?: string;
  reviewNotes?: string;
  subscriptionPlanId?: string;
}

export interface SchoolRegistrationRequest {
  id: string;
  schoolName: string;
  subdomainCode: string;
  address?: string;
  phone?: string;
  schoolEmail?: string;
  adminFirstName: string;
  adminLastName: string;
  adminEmail: string;
  subscriptionPlanId?: string;
  planName?: string;
  planPriceMonthly?: number;
  status: "Pending" | "Approved" | "Rejected";
  reviewNotes?: string;
  reviewedByUserId?: string;
  reviewedAtUtc?: string;
  createdSchoolId?: string;
  createdAdminUserId?: string;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export interface SchoolRegistrationRequestsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
}

export const SchoolRegistrationRequestsApi = {
  create: (data: CreateSchoolRegistrationRequestPayload) =>
    api
      .post<{ id: string }>("/schoolregistrationrequests", data)
      .then((r) => r.data),

  list: (params: SchoolRegistrationRequestsParams = {}) =>
    api
      .get<PagedResult<SchoolRegistrationRequest>>("/schoolregistrationrequests", {
        params,
      })
      .then((r) => r.data),

  getById: (id: string) =>
    api
      .get<SchoolRegistrationRequest>(`/schoolregistrationrequests/${id}`)
      .then((r) => r.data),

  update: (id: string, data: UpdateSchoolRegistrationRequestPayload) =>
    api
      .put(`/schoolregistrationrequests/${id}`, data)
      .then((r) => r.data),

  delete: (id: string) => api.delete(`/schoolregistrationrequests/${id}`).then((r) => r.data),

  approve: (id: string, notes?: string) =>
    api
      .post<{ id?: string }>(`/schoolregistrationrequests/${id}/approve`, { notes })
      .then((r) => r.data),

  reject: (id: string, reason?: string) =>
    api
      .post(`/schoolregistrationrequests/${id}/reject`, { reason })
      .then((r) => r.data),
};
