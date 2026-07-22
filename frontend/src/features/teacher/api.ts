import api from "@/lib/axios";

export interface RosterStudent {
  enrollmentId: string;
  studentId: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  enrollmentStatus: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface TeachingAssignmentSummary {
  id: string;
  subjectName: string;
  classRoomName: string;
  termName: string;
}

export interface ClassAttendanceStudent {
  studentEnrollmentId: string;
  studentFirstName: string;
  studentLastName: string;
  studentCode: string;
  status: number;
  note: string | null;
}

export interface ClassAttendance {
  sessionId: string;
  date: string;
  isLocked: boolean;
  records: ClassAttendanceStudent[];
}

export interface AssignmentSummary {
  id: string;
  title: string;
  dueDate: string;
  maxScore: number | null;
  totalSubmissions: number;
  gradedSubmissions: number;
  pendingSubmissions: number;
}

export interface ScheduleSlot {
  teachingAssignmentId: string;
  subjectName: string;
  classRoomName: string;
  gradeLevelName: string;
  roomName: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

export interface ExamListItem {
  id: string;
  name: string;
  subjectName: string;
  termName: string;
  maxScore: number;
  isLocked: boolean;
  scheduleCount: number;
}

export interface ClassExamResult {
  enrollmentId: string;
  studentCode: string;
  firstName: string;
  lastName: string;
  score: number;
  percentage: number;
  remarks: string | null;
}

export interface ExamScheduleItem {
  id: string;
  classRoomId: string;
  classRoomName: string;
  roomId: string;
  roomName: string;
  examDate: string;
}

export interface SubjectItem {
  id: string;
  code: string;
  name: string;
  description: string | null;
}

export interface RoomItem {
  id: string;
  name: string;
  capacity: number;
}

export const TeacherApi = {
  getRoster: async (classRoomId: string, schoolId: string, page = 1, pageSize = 50) => {
    const response = await api.get(`/portal/classrooms/${classRoomId}/roster`, {
      params: { schoolId, page, pageSize },
    });
    return response.data as PagedResult<RosterStudent>;
  },

  getMyClasses: async (termId: string) => {
    const response = await api.get("/me/classes", {
      params: { termId },
    });
    return response.data as TeachingAssignmentSummary[];
  },

  getAttendanceSheet: async (classScheduleId: string, schoolId: string, date: string) => {
    const response = await api.get("/attendance/sessions", {
      params: { classScheduleId, schoolId, date },
    });
    return response.data as ClassAttendance;
  },

  recordAttendance: async (payload: {
    schoolId: string;
    classScheduleId: string;
    date: string;
    entries: { studentEnrollmentId: string; status: number; note?: string }[];
  }) => {
    const response = await api.post("/attendance/sessions", payload);
    return response.data;
  },

  lockAttendanceSession: async (sessionId: string, schoolId: string) => {
    const response = await api.post(`/attendance/sessions/${sessionId}/lock`, null, {
      params: { schoolId },
    });
    return response.data;
  },

  getClassAssignments: async (teachingAssignmentId: string, page = 1, pageSize = 20) => {
    const response = await api.get(`/assignments/class/${teachingAssignmentId}`, {
      params: { page, pageSize },
    });
    return response.data as PagedResult<AssignmentSummary>;
  },

  gradeSubmission: async (payload: {
    schoolId: string;
    assignmentId: string;
    submissionId: string;
    grade: number;
    feedback?: string;
  }) => {
    const response = await api.patch(
      `/assignments/${payload.assignmentId}/submissions/${payload.submissionId}/grade`,
      { schoolId: payload.schoolId, grade: payload.grade, feedback: payload.feedback }
    );
    return response.data;
  },

  getSchedule: async (teacherId: string, schoolId: string, termId: string) => {
    const response = await api.get(`/portal/teacher/${teacherId}/schedule`, {
      params: { schoolId, termId },
    });
    return response.data as ScheduleSlot[];
  },

  getNotifications: async (schoolId: string, page = 1, pageSize = 20) => {
    const response = await api.get("/notifications", {
      params: { schoolId, page, pageSize },
    });
    return response.data;
  },

  getExams: async (schoolId: string, termId: string, page = 1, pageSize = 20) => {
    const response = await api.get("/exams", {
      params: { schoolId, termId, page, pageSize },
    });
    return response.data as PagedResult<ExamListItem>;
  },

  createExam: async (payload: {
    schoolId: string;
    subjectId: string;
    termId: string;
    name: string;
    maxScore: number;
  }) => {
    const response = await api.post("/exams", payload);
    return response.data as { id: string };
  },

  addExamSchedule: async (payload: {
    schoolId: string;
    examId: string;
    classRoomId: string;
    roomId: string;
    examDate: string;
  }) => {
    const response = await api.post(`/exams/${payload.examId}/schedules`, {
      schoolId: payload.schoolId,
      examId: payload.examId,
      classRoomId: payload.classRoomId,
      roomId: payload.roomId,
      examDate: payload.examDate,
    });
    return response.data as { id: string };
  },

  recordExamResults: async (payload: {
    schoolId: string;
    examId: string;
    examScheduleId: string;
    results: { studentEnrollmentId: string; score: number; remarks?: string }[];
  }) => {
    const response = await api.post(`/exams/${payload.examId}/results`, {
      schoolId: payload.schoolId,
      examScheduleId: payload.examScheduleId,
      results: payload.results,
    });
    return response.data;
  },

  getClassExamResults: async (schoolId: string, examScheduleId: string) => {
    const response = await api.get(`/exams/schedules/${examScheduleId}/results`, {
      params: { schoolId },
    });
    return response.data as ClassExamResult[];
  },

  getExamSchedules: async (examId: string) => {
    const response = await api.get(`/exams/${examId}/schedules`);
    return response.data as ExamScheduleItem[];
  },

  getSubjects: async (gradeLevelId?: string) => {
    const response = await api.get("/subjects", {
      params: gradeLevelId ? { gradeLevelId } : {},
    });
    return response.data as SubjectItem[];
  },

  getRooms: async (schoolId: string) => {
    const response = await api.get("/academics/rooms", {
      params: { schoolId },
    });
    return response.data as RoomItem[];
  },

  lockExam: async (payload: { examId: string; schoolId: string }) => {
    const response = await api.patch(`/exams/${payload.examId}/lock`, null, {
      params: { schoolId: payload.schoolId },
    });
    return response.data;
  },
};
