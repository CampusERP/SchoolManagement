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

const mockSchedule: ScheduleItem[] = [
  { classScheduleId: "1", teachingAssignmentId: "t1", className: "Grade 10-A", subject: "Mathematics", startTime: "08:00", endTime: "09:00", room: "Room 101", status: "completed" },
  { classScheduleId: "2", teachingAssignmentId: "t2", className: "Grade 10-A", subject: "Physics", startTime: "09:15", endTime: "10:15", room: "Room 203", status: "completed" },
  { classScheduleId: "3", teachingAssignmentId: "t3", className: "Grade 11-B", subject: "Mathematics", startTime: "10:30", endTime: "11:30", room: "Room 102", status: "in_progress" },
  { classScheduleId: "4", teachingAssignmentId: "t4", className: "Grade 9-A", subject: "Mathematics", startTime: "13:00", endTime: "14:00", room: "Room 101", status: "upcoming" },
];

const mockTeacherClasses: TeacherClass[] = [
  { teachingAssignmentId: "t1", classRoomId: "c1", name: "Grade 10-A", subject: "Mathematics", studentCount: 35, gradeLevel: "Grade 10" },
  { teachingAssignmentId: "t2", classRoomId: "c2", name: "Grade 11-B", subject: "Mathematics", studentCount: 32, gradeLevel: "Grade 11" },
  { teachingAssignmentId: "t3", classRoomId: "c3", name: "Grade 9-A", subject: "Mathematics", studentCount: 38, gradeLevel: "Grade 9" },
];

export const DashboardApi = {
  getPlatformDashboard: async () => {
    const response = await api.get("/schools/analytics");
    return response.data as PlatformDashboardData;
  },

  getSchoolDashboard: async (schoolId: string) => {
    const response = await api.get(`/schools/${schoolId}/dashboard`);
    const data = response.data;
    return {
      ...data,
      recentStudents: data.recentStudents ?? [],
      attendanceSummary: data.attendanceSummary ?? { present: 0, absent: 0, late: 0, excused: 0 },
    } as SchoolDashboardData;
  },

  getTeacherDashboard: async (schoolId: string) => {
    const response = await api.get("/portal/teacher/dashboard", { params: { schoolId } });
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
