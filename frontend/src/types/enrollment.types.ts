export interface EnrollStudentCommand {
  schoolId: string;
  studentId: string;
  classRoomId: string;
  academicYearId: string;
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
