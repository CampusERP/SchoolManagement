import { useNavigate } from "react-router-dom";
import {
  Users,
  GraduationCap,
  Heart,
  BookOpen,
  ClipboardCheck,
  UserPlus,
  UserCog,
  LayoutGrid,
  CalendarDays,
} from "lucide-react";
import { useSchoolDashboard } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import AnalyticsCardsGrid from "@/components/organisms/AnalyticsCardsGrid";
import RecentStudentsTable from "@/components/organisms/RecentStudentsTable";
import AnnouncementsPanel from "@/components/organisms/AnnouncementsPanel";
import AttendanceSummary from "@/components/organisms/AttendanceSummary";
import ChartsPlaceholder from "@/components/organisms/ChartsPlaceholder";
import QuickActionCard from "@/components/molecules/QuickActionCard";

export default function SchoolDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError } = useSchoolDashboard();

  if (isLoading) {
    return (
      <DashboardTemplate title="School Dashboard" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !data) {
    return (
      <DashboardTemplate title="School Dashboard">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load dashboard data.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const statCards = [
    { title: "Students", value: data.totalStudents, icon: <Users className="h-5 w-5" /> },
    { title: "Teachers", value: data.totalTeachers, icon: <GraduationCap className="h-5 w-5" /> },
    { title: "Parents", value: data.totalParents, icon: <Heart className="h-5 w-5" /> },
    { title: "Classes", value: data.totalClasses, icon: <BookOpen className="h-5 w-5" /> },
    { title: "Today's Attendance", value: `${data.todayAttendance}%`, icon: <ClipboardCheck className="h-5 w-5" /> },
    { title: "Pending Enrollments", value: data.pendingEnrollments, icon: <Users className="h-5 w-5" /> },
  ];

  return (
    <DashboardTemplate title="School Dashboard" subtitle="Overview of your school's performance">
      <AnalyticsCardsGrid cards={statCards} />

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <QuickActionCard
          icon={<UserPlus className="h-6 w-6" />}
          label="Add Student"
          onClick={() => navigate("/people/students/new")}
        />
        <QuickActionCard
          icon={<UserCog className="h-6 w-6" />}
          label="Add Teacher"
          onClick={() => navigate("/people/teachers/new")}
        />
        <QuickActionCard
          icon={<LayoutGrid className="h-6 w-6" />}
          label="Create Classroom"
          onClick={() => navigate("/academics/classrooms/new")}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <RecentStudentsTable data={data.recentStudents} />
        </div>
        <div className="space-y-6">
          <AnnouncementsPanel announcements={data.announcements} />

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)]">
            <div className="mb-4 flex items-center gap-2">
              <CalendarDays className="h-5 w-5 text-[var(--color-primary)]" />
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
                Upcoming Events
              </h3>
            </div>
            {data.upcomingEvents.length === 0 ? (
              <p className="text-sm text-[var(--color-text-muted)]">No upcoming events.</p>
            ) : (
              <ul className="space-y-3">
                {data.upcomingEvents.map((event) => (
                  <li key={event.id} className="flex items-start gap-3">
                    <span className="mt-0.5 h-2 w-2 shrink-0 rounded-full bg-[var(--color-primary)]" />
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-[var(--color-text-primary)]">
                        {event.title}
                      </p>
                      <p className="text-xs text-[var(--color-text-muted)]">
                        {new Date(event.date).toLocaleDateString()} · {event.type}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <AttendanceSummary data={data.attendanceSummary} />
        <ChartsPlaceholder title="Enrollment Trend" />
      </div>
    </DashboardTemplate>
  );
}
