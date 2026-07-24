import { useNavigate } from "react-router-dom";
import { BookOpen, ClipboardCheck, GraduationCap, Heart, LayoutGrid, UserCog, UserPlus, Users } from "lucide-react";
import { useSchoolDashboard } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import AnalyticsCardsGrid from "@/components/organisms/AnalyticsCardsGrid";
import RecentStudentsTable from "@/components/organisms/RecentStudentsTable";
import AttendanceSummary from "@/components/organisms/AttendanceSummary";
import QuickActionCard from "@/components/molecules/QuickActionCard";

export default function SchoolDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError } = useSchoolDashboard();
  if (isLoading) return <DashboardTemplate title="School Dashboard" loading><div /></DashboardTemplate>;
  if (isError || !data) return <DashboardTemplate title="School Dashboard"><div className="flex items-center justify-center py-24"><p className="text-[var(--color-text-secondary)]">Failed to load school statistics.</p></div></DashboardTemplate>;

  const { present, absent, late, excused } = data.attendanceSummary;
  const totalAttendance = present + absent + late + excused;
  const attendancePct = totalAttendance ? Math.round((present / totalAttendance) * 100) : 0;
  const statCards = [
    { title: "Students", value: data.totalStudents, icon: <Users className="h-5 w-5" /> },
    { title: "Teachers", value: data.totalTeachers, icon: <GraduationCap className="h-5 w-5" /> },
    { title: "Parents", value: data.totalParents, icon: <Heart className="h-5 w-5" /> },
    { title: "Classrooms", value: data.totalClassRooms, icon: <BookOpen className="h-5 w-5" /> },
    { title: "Today's Attendance", value: `${attendancePct}%`, icon: <ClipboardCheck className="h-5 w-5" /> },
    { title: "Active Enrollments", value: data.activeEnrollments, icon: <Users className="h-5 w-5" /> },
  ];
  return <DashboardTemplate title="School Dashboard" subtitle="Overview of your school's current activity">
    <AnalyticsCardsGrid cards={statCards} />
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
      <QuickActionCard icon={<UserPlus className="h-6 w-6" />} label="Add Student" onClick={() => navigate("/people/students")} />
      <QuickActionCard icon={<UserCog className="h-6 w-6" />} label="Add Teacher" onClick={() => navigate("/people/teachers")} />
      <QuickActionCard icon={<LayoutGrid className="h-6 w-6" />} label="Create Classroom" onClick={() => navigate("/academics/classrooms")} />
    </div>
    <div className="grid gap-6 lg:grid-cols-3">
      <div className="lg:col-span-2 rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)]"><h2 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">Recently enrolled students</h2><RecentStudentsTable data={data.recentStudents} /></div>
      <AttendanceSummary data={data.attendanceSummary} />
    </div>
  </DashboardTemplate>;
}
