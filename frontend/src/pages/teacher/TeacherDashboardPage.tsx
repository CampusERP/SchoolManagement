import { useNavigate } from "react-router-dom";
import {
  GraduationCap,
  Users,
  BookOpen,
  ClipboardCheck,
  ClipboardList,
  PenLine,
  CalendarClock,
} from "lucide-react";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import AnalyticsCardsGrid from "@/components/organisms/AnalyticsCardsGrid";
import MyClassesTable from "@/components/organisms/MyClassesTable";
import UpcomingLessons from "@/components/organisms/UpcomingLessons";
import AnnouncementsPanel from "@/components/organisms/AnnouncementsPanel";
import ChartsPlaceholder from "@/components/organisms/ChartsPlaceholder";
import QuickActionCard from "@/components/molecules/QuickActionCard";

export default function TeacherDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError } = useTeacherDashboard();

  if (isLoading) {
    return (
      <DashboardTemplate title="Teacher Dashboard" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !data) {
    return (
      <DashboardTemplate title="Teacher Dashboard">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load dashboard data.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const statCards = [
    { title: "My Classes", value: data.totalClasses, icon: <GraduationCap className="h-5 w-5" /> },
    { title: "Total Students", value: data.totalStudents, icon: <Users className="h-5 w-5" /> },
    { title: "Today's Lessons", value: data.todayLessons, icon: <BookOpen className="h-5 w-5" /> },
    { title: "Pending Attendance", value: data.pendingAttendance, icon: <ClipboardCheck className="h-5 w-5" /> },
  ];

  return (
    <DashboardTemplate title="Teacher Dashboard" subtitle="Your classes and schedule at a glance">
      <AnalyticsCardsGrid cards={statCards} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
        <QuickActionCard
          icon={<ClipboardList className="h-6 w-6" />}
          label="Take Attendance"
          onClick={() => navigate("/teacher/attendance")}
        />
        <QuickActionCard
          icon={<PenLine className="h-6 w-6" />}
          label="Enter Grades"
          onClick={() => navigate("/teacher/grades")}
        />
        <QuickActionCard
          icon={<CalendarClock className="h-6 w-6" />}
          label="View Schedule"
          onClick={() => navigate("/teacher/classes")}
        />
        <QuickActionCard
          icon={<ClipboardCheck className="h-6 w-6" />}
          label="Manage Exams"
          onClick={() => navigate("/teacher/exams")}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <MyClassesTable data={data.myClasses} />
        </div>
        <div className="space-y-6">
          <UpcomingLessons items={data.todaySchedule} />
          <AnnouncementsPanel announcements={data.announcements} />
        </div>
      </div>

      <ChartsPlaceholder title="Performance Summary" />
    </DashboardTemplate>
  );
}
