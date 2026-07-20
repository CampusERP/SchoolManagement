import api from "@/lib/axios";
import type {
  PlatformDashboardData,
  SchoolDashboardData,
  TeacherDashboardData,
  School,
  ActivityItem,
  SystemHealth,
  RecentStudent,
  EventItem,
  Announcement,
  AttendanceSummary,
  ScheduleItem,
  TeacherClass,
} from "@/types/dashboard.types";

interface SchoolApiItem {
  id: string;
  name: string;
  subdomainCode: string;
  status: string;
  totalStudents: number;
  totalTeachers: number;
  createdAtUtc: string;
}

// Mock data for development when backend is not available
const mockActivity: ActivityItem[] = [
  { id: "1", type: "school_created", message: "Hillcrest Academy was created", timestamp: "2025-12-01T10:30:00Z" },
  { id: "2", type: "admin_registered", message: "Sarah Johnson registered as School Admin for Sunrise International", timestamp: "2025-10-15T14:20:00Z" },
  { id: "3", type: "student_enrolled", message: "15 new students enrolled at Greenfield Academy", timestamp: "2026-01-05T09:00:00Z" },
  { id: "4", type: "subscription_changed", message: "Hillcrest Academy subscription expired", timestamp: "2026-06-01T00:00:00Z" },
];

const mockSystemHealth: SystemHealth = {
  apiStatus: "healthy",
  dbStatus: "healthy",
  uptime: "99.98%",
  lastChecked: new Date().toISOString(),
};

const mockRecentStudents: RecentStudent[] = [
  { id: "1", name: "Alice Johnson", code: "STU-001", gradeLevel: "Grade 10", enrolledAt: "2026-01-10", status: "Active" },
  { id: "2", name: "Bob Williams", code: "STU-002", gradeLevel: "Grade 8", enrolledAt: "2026-01-12", status: "Active" },
  { id: "3", name: "Carol Brown", code: "STU-003", gradeLevel: "Grade 11", enrolledAt: "2026-01-15", status: "Active" },
  { id: "4", name: "Daniel Wilson", code: "STU-004", gradeLevel: "Grade 9", enrolledAt: "2026-02-01", status: "Active" },
  { id: "5", name: "Eva Martinez", code: "STU-005", gradeLevel: "Grade 7", enrolledAt: "2026-02-05", status: "Active" },
];

const mockEvents: EventItem[] = [
  { id: "1", title: "Mid-Term Exams Begin", date: "2026-03-15", type: "exam" },
  { id: "2", title: "Spring Break", date: "2026-04-01", type: "holiday" },
  { id: "3", title: "Parent-Teacher Conference", date: "2026-03-20", type: "meeting" },
  { id: "4", title: "Science Fair", date: "2026-03-25", type: "event" },
];

const mockAnnouncements: Announcement[] = [
  { id: "1", title: "New Academic Year Registration", content: "Registration for the new academic year is now open.", date: "2026-01-15", author: "Admin" },
  { id: "2", title: "Staff Meeting", content: "Mandatory staff meeting this Friday at 3 PM.", date: "2026-01-10", author: "Principal" },
  { id: "3", title: "Holiday Notice", content: "School will be closed next Monday for the holiday.", date: "2026-01-08", author: "Admin" },
];

const mockAttendanceSummary: AttendanceSummary = { present: 280, absent: 15, late: 8, excused: 5 };

const mockSchedule: ScheduleItem[] = [
  { id: "1", className: "Grade 10-A", subject: "Mathematics", startTime: "08:00", endTime: "09:00", room: "Room 101", status: "completed" },
  { id: "2", className: "Grade 10-A", subject: "Physics", startTime: "09:15", endTime: "10:15", room: "Room 203", status: "completed" },
  { id: "3", className: "Grade 11-B", subject: "Mathematics", startTime: "10:30", endTime: "11:30", room: "Room 102", status: "in_progress" },
  { id: "4", className: "Grade 9-A", subject: "Mathematics", startTime: "13:00", endTime: "14:00", room: "Room 101", status: "upcoming" },
];

const mockTeacherClasses: TeacherClass[] = [
  { id: "1", name: "Grade 10-A", subject: "Mathematics", studentCount: 35, gradeLevel: "Grade 10" },
  { id: "2", name: "Grade 11-B", subject: "Mathematics", studentCount: 32, gradeLevel: "Grade 11" },
  { id: "3", name: "Grade 9-A", subject: "Mathematics", studentCount: 38, gradeLevel: "Grade 9" },
];

export const DashboardApi = {
  getPlatformDashboard: async () => {
    const response = await api.get("/schools/analytics");
    const data = response.data;
    return {
      ...data,
      recentSchools: data.recentSchools ?? [],
      recentActivity: data.recentActivity ?? [],
      systemHealth: data.systemHealth ?? {
        apiStatus: "unknown",
        dbStatus: "unknown",
        uptime: "N/A",
        lastChecked: new Date().toISOString(),
      },
    } as PlatformDashboardData;
  },

  getSchoolDashboard: async (schoolId: string) => {
    const response = await api.get(`/schools/${schoolId}/dashboard`);
    const data = response.data;
    return {
      ...data,
      recentStudents: data.recentStudents ?? [],
      upcomingEvents: data.upcomingEvents ?? [],
      announcements: data.announcements ?? [],
      attendanceSummary: data.attendanceSummary ?? { present: 0, absent: 0, late: 0, excused: 0 },
    } as SchoolDashboardData;
  },

  getTeacherDashboard: async () => {
    const response = await api.get("/teacher/dashboard");
    return response.data as TeacherDashboardData;
  },

  getSchools: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const response = await api.get("/schools", {
      params: {
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 20,
        search: params?.search,
      },
    });
    const data = response.data;
    return {
      items: data.items?.map((item: SchoolApiItem) => ({
        id: item.id,
        name: item.name,
        subdomainCode: item.subdomainCode,
        status: item.status,
        totalStudents: item.totalStudents,
        totalTeachers: item.totalTeachers,
        createdAt: item.createdAtUtc,
      })) || [],
      totalCount: data.totalCount || 0,
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      totalPages: Math.ceil((data.totalCount || 0) / (params?.pageSize ?? 20)),
    };
  },
};
