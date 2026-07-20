export interface TeacherListDto {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  employmentStatus: string;
  // Additional field returned by the backend
  assignedClassesCount?: number;
}

export interface TeacherDetailDto {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  employmentStatus: string;
  assignments: TeachingAssignment[];
}

export interface TeachingAssignment {
  id: string;
  subject: string;
  subjectName?: string;
  classroomName: string;
  classRoomName?: string;
  termName: string;
}

export interface CreateTeacherCommand {
  schoolId: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface UpdateTeacherCommand {
  schoolId: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  employmentStatus: "Active" | "OnLeave" | "Terminated";
}
