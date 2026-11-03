import type { CreateStudentCommand } from "@/types/student.types";
import type { CreateTeacherCommand } from "@/types/teacher.types";
import type { CreateParentCommand } from "@/types/parent.types";
import { t } from "@/i18n";

export interface ColumnDef {
  name: string;
  required: boolean;
  description: string;
  example: string;
}

export interface ImportFieldTemplate {
  key: string;
  columns: ColumnDef[];
}

export const studentImportTemplate: ImportFieldTemplate = {
  key: "students",
  columns: [
    { name: "studentCode", required: false, description: "Leave blank to generate a code starting with STU-", example: "STU-001" },
    { name: "firstName", required: true, description: "Student's first name", example: "Jane" },
    { name: "secondName", required: false, description: "Student's second name", example: "Marie" },
    { name: "thirdName", required: false, description: "Student's third name", example: "Anne" },
    { name: "lastName", required: true, description: "Student's last name", example: "Doe" },
    { name: "dateOfBirth", required: true, description: "Date of birth in YYYY-MM-DD format", example: "2012-05-14" },
    { name: "nationalId", required: false, description: "Optional national identification number", example: "123456789" },
    { name: "nationalityCountry", required: false, description: "Student's nationality country", example: "Egypt" },
    { name: "residenceCountry", required: false, description: "Country where the student lives", example: "Egypt" },
    { name: "campus", required: false, description: "Exact campus name or campus ID; required when campuses are configured", example: "Main Campus" },
    { name: "hasLogin", required: false, description: "Yes/No. Leave blank to infer from email and password.", example: "Yes" },
    { name: "email", required: false, description: "Login email. Required with password when hasLogin is Yes.", example: "jane.doe@school.com" },
    { name: "password", required: false, description: "At least 8 characters with uppercase, lowercase, number, and symbol when creating a login.", example: "Temp@123" },
  ],
};

export const teacherImportTemplate: ImportFieldTemplate = {
  key: "teachers",
  columns: [
    { name: "employeeCode", required: true, description: "Unique employee code (e.g. EMP-001)", example: "EMP-001" },
    { name: "firstName", required: true, description: "Teacher's first name", example: "John" },
    { name: "lastName", required: true, description: "Teacher's last name", example: "Smith" },
    { name: "email", required: true, description: "Login email (must be unique)", example: "john.smith@school.com" },
    { name: "password", required: true, description: "At least 8 characters with uppercase, lowercase, number, and symbol", example: "Temp@123" },
    { name: "campuses", required: false, description: "Campus names or IDs separated by semicolons; required when campuses are configured", example: "Main Campus; West Campus" },
  ],
};

export const parentImportTemplate: ImportFieldTemplate = {
  key: "parents",
  columns: [
    { name: "firstName", required: true, description: "Parent's first name", example: "Mary" },
    { name: "lastName", required: true, description: "Parent's last name", example: "Doe" },
    { name: "email", required: true, description: "Login email (must be unique)", example: "mary.doe@email.com" },
    { name: "password", required: true, description: "At least 8 characters with uppercase, lowercase, number, and symbol", example: "Temp@123" },
  ],
};

export interface ImportRowResult {
  rowNumber: number;
  ok: boolean;
  error?: string;
}

export interface ImportSummary {
  total: number;
  succeeded: number;
  failed: number;
  results: ImportRowResult[];
}

function normalizeHeader(value: string): string {
  return value.trim().replace(/^\uFEFF/, "").toLowerCase().replace(/[^a-z0-9]/g, "");
}

function get(values: Record<string, string>, ...keys: string[]): string {
  const normalized = new Map(Object.entries(values).map(([key, value]) => [normalizeHeader(key), value.trim()]));
  for (const key of keys) {
    const value = normalized.get(normalizeHeader(key));
    if (value) return value;
  }
  return "";
}

export interface CampusImportOption {
  id: string;
  name: string;
}

function resolveCampus(value: string, campuses: CampusImportOption[]): string | undefined {
  const needle = value.trim().toLocaleLowerCase();
  return campuses.find((campus) => campus.id.toLocaleLowerCase() === needle || campus.name.trim().toLocaleLowerCase() === needle)?.id;
}

function isYes(value: string): boolean {
  return ["yes", "y", "true", "1", "نعم"].includes(value.trim().toLocaleLowerCase());
}

function isNo(value: string): boolean {
  return ["no", "n", "false", "0", "لا"].includes(value.trim().toLocaleLowerCase());
}

function normalizeDate(value: string): string | undefined {
  const trimmed = value.trim();
  const isoParts = trimmed.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/);
  if (isoParts) {
    const [, year, month, day] = isoParts;
    const parsed = new Date(Date.UTC(Number(year), Number(month) - 1, Number(day)));
    if (parsed.getUTCFullYear() !== Number(year) || parsed.getUTCMonth() !== Number(month) - 1 || parsed.getUTCDate() !== Number(day)) return undefined;
    return parsed.toISOString().slice(0, 10);
  }
  if (/^\d{4}$/.test(trimmed)) return undefined;
  const parsed = new Date(trimmed);
  if (Number.isNaN(parsed.getTime())) return undefined;
  return parsed.toISOString().slice(0, 10);
}

function validEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function passwordError(password: string): string | undefined {
  if (password.length < 8) return t("Password must be at least 8 characters");
  if (!/[A-Z]/.test(password)) return t("Password must contain an uppercase letter.");
  if (!/[a-z]/.test(password)) return t("Password must contain a lowercase letter.");
  if (!/[0-9]/.test(password)) return t("Password must contain a number.");
  if (!/[^a-zA-Z0-9]/.test(password)) return t("Password must contain a symbol.");
  return undefined;
}

export function mapStudentRow(
  values: Record<string, string>,
  campuses: CampusImportOption[] = []
): { data?: CreateStudentCommand; error?: string } {
  const studentCode = get(values, "studentCode", "code");
  const firstName = get(values, "firstName", "first name");
  const secondName = get(values, "secondName", "second name");
  const thirdName = get(values, "thirdName", "third name");
  const lastName = get(values, "lastName", "fourthName", "last name", "fourth name");
  const rawDateOfBirth = get(values, "dateOfBirth", "date of birth", "dob");
  const dateOfBirth = normalizeDate(rawDateOfBirth);
  const email = get(values, "email", "loginEmail").toLowerCase();
  const password = get(values, "password", "loginPassword");
  const nationalId = get(values, "nationalId", "national id");
  const nationalityCountry = get(values, "nationalityCountry", "nationality", "nationality country");
  const residenceCountry = get(values, "residenceCountry", "residence country", "country of residence");
  const campus = get(values, "campus", "campusName", "campusId");
  const hasLoginValue = get(values, "hasLogin", "has login");

  if (!firstName) return { error: t("Missing firstName") };
  if (!lastName) return { error: t("Missing lastName") };
  if (!rawDateOfBirth) return { error: t("Missing dateOfBirth") };
  if (!dateOfBirth) return { error: t("Date of birth must use a valid date such as YYYY-MM-DD") };
  if (dateOfBirth >= new Date().toISOString().slice(0, 10)) return { error: t("Date of birth must be in the past.") };
  if (campuses.length > 0 && !campus) return { error: t("Campus is required for this school") };
  const campusId = campus ? resolveCampus(campus, campuses) : undefined;
  if (campus && !campusId) return { error: t("Campus '{campus}' was not found in this school", { campus }) };
  if (hasLoginValue && !isYes(hasLoginValue) && !isNo(hasLoginValue))
    return { error: t("hasLogin must be Yes or No") };

  const wantsLogin = hasLoginValue ? isYes(hasLoginValue) : Boolean(email || password);
  if (wantsLogin && (!email || !password))
    return { error: t("Email and password are required when hasLogin is Yes") };
  if (!wantsLogin && hasLoginValue && isNo(hasLoginValue) && (email || password))
    return { error: t("Clear email and password when hasLogin is No") };
  if (wantsLogin && !validEmail(email)) return { error: t("Invalid email") };
  if (wantsLogin) {
    const error = passwordError(password);
    if (error) return { error };
  }

  return {
    data: {
      schoolId: "",
      studentCode: studentCode || undefined,
      firstName,
      secondName: secondName || undefined,
      thirdName: thirdName || undefined,
      lastName,
      dateOfBirth,
      nationalId: nationalId || undefined,
      nationalityCountry: nationalityCountry || undefined,
      residenceCountry: residenceCountry || undefined,
      campusId,
      email: email || undefined,
      password: password || undefined,
    },
  };
}

export function mapTeacherRow(
  values: Record<string, string>,
  campuses: CampusImportOption[] = []
): { data?: CreateTeacherCommand; error?: string } {
  const employeeCode = get(values, "employeeCode", "employee code");
  const firstName = get(values, "firstName", "first name");
  const lastName = get(values, "lastName", "last name");
  const email = get(values, "email", "loginEmail").toLowerCase();
  const password = get(values, "password");
  const campusNames = get(values, "campuses", "campusNames", "campus");

  if (!employeeCode) return { error: t("Missing employeeCode") };
  if (!firstName) return { error: t("Missing firstName") };
  if (!lastName) return { error: t("Missing lastName") };
  if (!email) return { error: t("Missing email") };
  if (!validEmail(email)) return { error: t("Invalid email") };
  if (!password) return { error: t("Password is required") };
  const passwordValidationError = passwordError(password);
  if (passwordValidationError) return { error: passwordValidationError };
  if (campuses.length > 0 && !campusNames) return { error: t("At least one campus is required for this school") };
  const campusIds = campusNames ? campusNames.split(";").map((name) => name.trim()).filter(Boolean).map((name) => resolveCampus(name, campuses)) : [];
  if (campusIds.some((id) => !id)) return { error: t("One or more campuses were not found in this school") };

  return {
    data: {
      schoolId: "",
      employeeCode,
      firstName,
      lastName,
      email,
      password,
      campusIds: campusIds.filter((id): id is string => Boolean(id)),
    },
  };
}

export function mapParentRow(
  values: Record<string, string>
): { data?: CreateParentCommand; error?: string } {
  const firstName = get(values, "firstName");
  const lastName = get(values, "lastName");
  const email = get(values, "email");
  const password = get(values, "password");

  if (!firstName) return { error: t("Missing firstName") };
  if (!lastName) return { error: t("Missing lastName") };
  if (!email) return { error: t("Missing email") };
  if (!validEmail(email)) return { error: t("Invalid email") };
  if (!password) return { error: t("Password is required") };
  const passwordValidationError = passwordError(password);
  if (passwordValidationError) return { error: passwordValidationError };

  return {
    data: {
      schoolId: "",
      firstName,
      lastName,
      email,
      password,
    },
  };
}
