export interface StudentListDto {
  id: string;
  studentCode: string;
  firstName: string;
  lastName: string;
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
  dateOfBirth: string;
  nationalId?: string | null;
  hasLogin: boolean;
  enrollments: EnrollmentSummary[];
  guardians: GuardianSummary[];
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
  studentCode: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  nationalId?: string;
  email?: string;
  password?: string;
}

export interface UpdateStudentCommand {
  schoolId: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  nationalId?: string;
}
