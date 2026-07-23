export interface School {
  id: string;
  name: string;
  subdomainCode: string;
  status: string;
  totalStudents: number;
  totalTeachers: number;
  createdAt: string;
}

export interface PlatformDashboardData {
  totalSchools: number;
  activeSchools: number;
  suspendedSchools: number;
  totalStudents: number;
  totalParents: number;
  totalTeachers: number;
  totalSchoolAdmins: number;
  totalUsers: number;
}

export interface ActivityItem {
  id: string;
  type: "school_created" | "admin_registered" | "subscription_changed" | "student_enrolled";
  message: string;
  timestamp: string;
  icon?: string;
}

export interface SystemHealth {
  apiStatus: "healthy" | "degraded" | "down";
  dbStatus: "healthy" | "degraded" | "down";
  uptime: string;
  lastChecked: string;
}

export interface SchoolDashboardData {
  schoolName: string;
  totalStudents: number;
  totalTeachers: number;
  totalParents: number;
  totalClassRooms: number;
  activeEnrollments: number;
  currentAcademicYear: string | null;
  recentStudents: RecentStudent[];
  attendanceSummary: AttendanceSummary;
}

export interface RecentStudent {
  id: string;
  name: string;
  code: string;
  gradeLevel: string;
  enrolledAt: string;
  status: "Active" | "Inactive";
}

export interface EventItem {
  id: string;
  title: string;
  date: string;
  type: "exam" | "holiday" | "meeting" | "event";
}

export interface Announcement {
  id: string;
  title: string;
  content: string;
  date?: string;
  createdAtUtc?: string;
  author: string;
}

export interface AttendanceSummary {
  present: number;
  absent: number;
  late: number;
  excused: number;
}

export interface TeacherDashboardData {
  teacherId: string;
  currentTermId: string | null;
  totalClasses: number;
  totalStudents: number;
  todayLessons: number;
  pendingAttendance: number;
  pendingAssignments: number;
  todaySchedule: ScheduleItem[];
  myClasses: TeacherClass[];
  announcements: Announcement[];
}

export interface ScheduleItem {
  classScheduleId: string;
  teachingAssignmentId: string;
  className: string;
  subject: string;
  startTime: string;
  endTime: string;
  room: string;
  status: "upcoming" | "completed" | "in_progress";
}

export interface TeacherClass {
  teachingAssignmentId: string;
  classRoomId: string;
  name: string;
  subject: string;
  studentCount: number;
  gradeLevel: string;
}
