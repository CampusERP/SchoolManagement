export interface TeacherListDto {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  employmentStatus: string;
  // Additional field returned by the backend
  assignedClassesCount?: number;
}

export interface TeacherDetailDto {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email?: string | null;
  employmentStatus: string;
  assignments: TeachingAssignment[];
}

export interface TeachingAssignment {
  id: string;
  subjectId: string;
  subject: string;
  subjectName?: string;
  classRoomId: string;
  classroomName: string;
  classRoomName?: string;
  termId: string;
  termName: string;
  schedules: TeachingScheduleSlot[];
}

export interface TeachingScheduleSlot {
  id: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  roomId: string;
  roomName: string;
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
