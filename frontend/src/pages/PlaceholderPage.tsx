import { Construction } from "lucide-react";
import { ParentChildrenPage, ParentDashboardPage, ParentResourcePage, StudentDashboardPage, StudentResourcePage } from "@/pages/portal/PortalPages";
import { useAuthStore } from "@/store/authStore";
import { t } from "@/i18n";

interface PlaceholderPageProps {
  title?: string;
  description?: string;
}

export default function PlaceholderPage({
  title = "Coming Soon",
  description = "This page is under development.",
}: PlaceholderPageProps) {
  const role = useAuthStore((s) => s.user?.role);

  if (role === "student") {
    if (title === "Student Dashboard") return <StudentDashboardPage />;
    if (title === "My Classes") return <StudentResourcePage title={t("My classes")} resource="classes" />;
    if (title === "My Grades") return <StudentResourcePage title={t("My grades")} resource="exams" />;
    if (title === "My Schedule") return <StudentResourcePage title={t("My timetable")} resource="schedule" />;
    if (title === "My Attendance") return <StudentResourcePage title={t("My attendance")} resource="attendance" />;
    if (title === "My Profile") return <StudentResourcePage title={t("My profile")} resource="profile" />;
  }

  if (role === "parent") {
    if (title === "Parent Dashboard") return <ParentDashboardPage />;
    if (title === "My Children") return <ParentChildrenPage />;
    if (title === "Children's Grades") return <ParentResourcePage title={t("Children's grades")} resource="grades" />;
    if (title === "Children's Attendance") return <ParentResourcePage title={t("Children's attendance")} resource="attendance" />;
    if (title === "My Profile") return <ParentResourcePage title={t("My profile")} resource="profile" />;
  }

  return (
    <div className="flex flex-col items-center justify-center py-24 text-center">
      <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--color-primary-light)] text-[var(--color-primary)]">
        <Construction className="h-8 w-8" />
      </div>
      <h2 className="text-xl font-semibold text-[var(--color-text-primary)]">{t(title)}</h2>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{t(description)}</p>
    </div>
  );
}
