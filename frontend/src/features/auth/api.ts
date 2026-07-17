import api from "@/lib/axios";
import type { LoginPayload, LoginResponse } from "@/types/auth.types";

export const AuthApi = {
  login: (data: LoginPayload) =>
    api.post<LoginResponse>("/auth/login", data).then((r) => r.data),
  refresh: (token: string) =>
    api.post<LoginResponse>("/auth/refresh", { token }).then((r) => r.data),
  registerSchoolAdmin: (data: Record<string, unknown>) =>
    api.post("/auth/register-school-admin", data).then((r) => r.data),
};
