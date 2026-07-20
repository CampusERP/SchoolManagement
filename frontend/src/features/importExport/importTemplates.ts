import type { CreateStudentCommand } from "@/types/student.types";
import type { CreateTeacherCommand } from "@/types/teacher.types";
import type { CreateParentCommand } from "@/types/parent.types";

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
    { name: "studentCode", required: true, description: "Unique code for the student (e.g. STU-001)", example: "STU-001" },
    { name: "firstName", required: true, description: "Student's first name", example: "Jane" },
    { name: "lastName", required: true, description: "Student's last name", example: "Doe" },
    { name: "dateOfBirth", required: true, description: "Date of birth in YYYY-MM-DD format", example: "2012-05-14" },
    { name: "nationalId", required: false, description: "Optional national identification number", example: "123456789" },
    { name: "email", required: false, description: "Optional login email. Required only if you want to create a login account.", example: "jane.doe@school.com" },
    { name: "password", required: false, description: "Optional. Min 6 characters. Only used when email is provided.", example: "Temp@123" },
  ],
};

export const teacherImportTemplate: ImportFieldTemplate = {
  key: "teachers",
  columns: [
    { name: "employeeCode", required: true, description: "Unique employee code (e.g. EMP-001)", example: "EMP-001" },
    { name: "firstName", required: true, description: "Teacher's first name", example: "John" },
    { name: "lastName", required: true, description: "Teacher's last name", example: "Smith" },
    { name: "email", required: true, description: "Login email (must be unique)", example: "john.smith@school.com" },
    { name: "password", required: true, description: "Temporary password, min 6 characters", example: "Temp@123" },
  ],
};

export const parentImportTemplate: ImportFieldTemplate = {
  key: "parents",
  columns: [
    { name: "firstName", required: true, description: "Parent's first name", example: "Mary" },
    { name: "lastName", required: true, description: "Parent's last name", example: "Doe" },
    { name: "email", required: true, description: "Login email (must be unique)", example: "mary.doe@email.com" },
    { name: "password", required: true, description: "Temporary password, min 6 characters", example: "Temp@123" },
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

function get(values: Record<string, string>, key: string): string {
  return values[key] ?? "";
}

export function mapStudentRow(
  values: Record<string, string>
): { data?: CreateStudentCommand; error?: string } {
  const studentCode = get(values, "studentCode");
  const firstName = get(values, "firstName");
  const lastName = get(values, "lastName");
  const dateOfBirth = get(values, "dateOfBirth");
  const email = get(values, "email");
  const password = get(values, "password");
  const nationalId = get(values, "nationalId");

  if (!studentCode) return { error: "Missing studentCode" };
  if (!firstName) return { error: "Missing firstName" };
  if (!lastName) return { error: "Missing lastName" };
  if (!dateOfBirth) return { error: "Missing dateOfBirth" };

  if (email && password && password.length < 6)
    return { error: "Password must be at least 6 characters when email is provided" };

  return {
    data: {
      schoolId: "",
      studentCode,
      firstName,
      lastName,
      dateOfBirth,
      nationalId: nationalId || undefined,
      email: email || undefined,
      password: password || undefined,
    },
  };
}

export function mapTeacherRow(
  values: Record<string, string>
): { data?: CreateTeacherCommand; error?: string } {
  const employeeCode = get(values, "employeeCode");
  const firstName = get(values, "firstName");
  const lastName = get(values, "lastName");
  const email = get(values, "email");
  const password = get(values, "password");

  if (!employeeCode) return { error: "Missing employeeCode" };
  if (!firstName) return { error: "Missing firstName" };
  if (!lastName) return { error: "Missing lastName" };
  if (!email) return { error: "Missing email" };
  if (!password || password.length < 6)
    return { error: "Password must be at least 6 characters" };

  return {
    data: {
      schoolId: "",
      employeeCode,
      firstName,
      lastName,
      email,
      password,
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

  if (!firstName) return { error: "Missing firstName" };
  if (!lastName) return { error: "Missing lastName" };
  if (!email) return { error: "Missing email" };
  if (!password || password.length < 6)
    return { error: "Password must be at least 6 characters" };

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
