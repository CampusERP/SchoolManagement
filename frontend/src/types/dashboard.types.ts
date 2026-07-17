export interface School {
  id: string;
  name: string;
  subdomainCode: string;
  ownerName: string;
  totalStudents: number;
  totalTeachers: number;
  subscriptionStatus: "Active" | "Trial" | "Expired";
  createdAt: string;
  status: "Active" | "Inactive";
}

export interface PlatformDashboardData {
  totalSchools: number;
  activeSchools: number;
  totalStudents: number;
  totalTeachers: number;
  totalRevenue: number;
  activeSubscriptions: number;
  recentSchools: School[];
  recentActivity: ActivityItem[];
  systemHealth: SystemHealth;
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
  totalStudents: number;
  totalTeachers: number;
  totalParents: number;
  totalClasses: number;
  todayAttendance: number;
  pendingEnrollments: number;
  recentStudents: RecentStudent[];
  upcomingEvents: EventItem[];
  announcements: Announcement[];
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
  date: string;
  author: string;
}

export interface AttendanceSummary {
  present: number;
  absent: number;
  late: number;
  excused: number;
}

export interface TeacherDashboardData {
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
  id: string;
  className: string;
  subject: string;
  startTime: string;
  endTime: string;
  room: string;
  status: "upcoming" | "completed" | "in_progress";
}

export interface TeacherClass {
  id: string;
  name: string;
  subject: string;
  studentCount: number;
  gradeLevel: string;
}
