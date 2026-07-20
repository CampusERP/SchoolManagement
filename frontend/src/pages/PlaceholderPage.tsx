import { Construction } from "lucide-react";
import { ParentChildrenPage, ParentDashboardPage, ParentResourcePage, StudentDashboardPage, StudentResourcePage } from "@/pages/portal/PortalPages";
import { useAuthStore } from "@/store/authStore";

interface PlaceholderPageProps {
  title?: string;
  description?: string;
}

export default function PlaceholderPage({
  title = "Coming Soon",
  description = "This page is under development.",
}: PlaceholderPageProps) {
  const role = useAuthStore((s) => s.user?.role);
  if (title === "Student Dashboard") return <StudentDashboardPage />;
  if (title === "My Classes") return <StudentResourcePage title="My classes" resource="classes" />;
  if (title === "My Grades") return <StudentResourcePage title="My grades" resource="exams" />;
  if (title === "My Schedule") return <StudentResourcePage title="My timetable" resource="schedule" />;
  if (title === "My Attendance") return <StudentResourcePage title="My attendance" resource="attendance" />;
  if (title === "My Profile" && role === "student") return <StudentResourcePage title="My profile" resource="profile" />;
  if (title === "Parent Dashboard") return <ParentDashboardPage />;
  if (title === "My Children") return <ParentChildrenPage />;
  if (title === "Children's Grades") return <ParentResourcePage title="Children's grades" resource="grades" />;
  if (title === "Children's Attendance") return <ParentResourcePage title="Children's attendance" resource="attendance" />;
  if (title === "My Profile" && role === "parent") return <ParentResourcePage title="My profile" resource="profile" />;
  return (
    <div className="flex flex-col items-center justify-center py-24 text-center">
      <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--color-primary-light)] text-[var(--color-primary)]">
        <Construction className="h-8 w-8" />
      </div>
      <h2 className="text-xl font-semibold text-[var(--color-text-primary)]">{title}</h2>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{description}</p>
    </div>
  );
}
