import type { DocumentDto } from "@/features/documents/api";

export interface StudentListDto {
  id: string;
  studentCode?: string;
  firstName: string;
  lastName: string;
  secondName?: string | null;
  thirdName?: string | null;
  campusId?: string | null;
  nationalId?: string | null;
  nationalityCountry?: string | null;
  residenceCountry?: string | null;
  email?: string | null;
  dateOfBirth: string;
  hasLogin: boolean;
  // Additional fields returned by the backend
  currentClass?: string | null;
  enrollmentStatus?: string | null;
}

export interface StudentDetailDto {
  id: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  secondName?: string | null;
  thirdName?: string | null;
  nationalityCountry?: string | null;
  residenceCountry?: string | null;
  campusId?: string | null;
  dateOfBirth: string;
  nationalId?: string | null;
  hasLogin: boolean;
  email?: string | null;
  enrollments: EnrollmentSummary[];
  guardians: GuardianSummary[];
  documents?: DocumentDto[];
}

export interface EnrollmentSummary {
  id: string;
  enrollmentId?: string;
  classroomName: string;
  classRoom?: string;
  academicYearName: string;
  academicYear?: string;
  gradeLevel?: string;
  status: string;
}

export interface GuardianSummary {
  id: string;
  parentId?: string;
  firstName: string;
  lastName: string;
  relationshipType: string;
  isPrimaryContact: boolean;
}

export interface CreateStudentCommand {
  schoolId: string;
  studentCode?: string;
  firstName: string;
  lastName: string;
  secondName?: string;
  thirdName?: string;
  nationalityCountry?: string;
  residenceCountry?: string;
  campusId?: string;
  dateOfBirth: string;
  nationalId?: string;
  email?: string;
  password?: string;
}

export interface UpdateStudentCommand {
  schoolId: string;
  studentCode: string;
  firstName: string;
  secondName?: string;
  thirdName?: string;
  lastName: string;
  nationalityCountry?: string;
  residenceCountry?: string;
  campusId?: string;
  dateOfBirth: string;
  nationalId?: string;
}
