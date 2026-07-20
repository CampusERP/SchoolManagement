export type UserRole = "platform_admin" | "school_admin" | "teacher" | "student" | "parent";

export interface AuthenticatedUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  schoolId?: string;
  permissions: string[];
  isPlatformAdmin: boolean;
}

export interface LoginPayload {
  email: string;
  password: string;
  schoolId?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  userId: string;
  email: string;
  schoolId: string | null;
  role: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
}
