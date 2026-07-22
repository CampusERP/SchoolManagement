export interface EnrollStudentCommand {
  schoolId: string;
  studentId: string;
  classRoomId: string;
  academicYearId: string;
}

export interface EnrollTeacherCommand {
  schoolId: string;
  teacherId: string;
  classRoomId: string;
  termId: string;
}

export interface AssignTeacherCommand {
  schoolId: string;
  teacherId: string;
  subjectId: string;
  classRoomId: string;
  termId: string;
  scheduleSlots: ScheduleSlot[];
}

export interface ScheduleSlot {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  roomId: string;
}

export interface UpdateTeachingAssignmentCommand {
  schoolId: string;
  teachingAssignmentId: string;
  teacherId: string;
  subjectId: string;
  classRoomId: string;
  termId: string;
  scheduleSlots: ScheduleSlot[];
}

export interface DeleteTeachingAssignmentCommand {
  schoolId: string;
  teachingAssignmentId: string;
}
