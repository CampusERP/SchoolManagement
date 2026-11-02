import { useCallback } from "react";
import { Menu, Sun, Moon } from "lucide-react";
import { useLocation, useNavigate } from "react-router-dom";
import { useThemeStore } from "@/store/themeStore";
import { useUiStore } from "@/store/uiStore";
import { useAuthStore } from "@/store/authStore";
import BreadcrumbNav from "@/components/molecules/BreadcrumbNav";
import GlobalSearch from "@/components/molecules/GlobalSearch";
import NotificationBell from "@/components/molecules/NotificationBell";
import UserMenu from "@/components/molecules/UserMenu";
import LanguageSwitcher from "@/components/molecules/LanguageSwitcher";
import { t } from "@/i18n";

const routeLabels: Record<string, string> = {
  platform: t("Platform"),
  schools: t("Schools"),
  billing: t("Billing"),
  analytics: t("Analytics"),
  invoices: t("Invoices"),
  subscriptions: t("Subscriptions"),
  plans: t("Plans"),
  school: t("School"),
  teacher: t("Teacher"),
  student: t("Student"),
  parent: t("Parent"),
  people: t("People"),
  students: t("Students"),
  teachers: t("Teachers"),
  parents: t("Parents"),
  academics: t("Academics"),
  years: t("Academic Years"),
  classrooms: t("Classrooms"),
  "grade-levels": t("Grade Levels"),
  "education-stages": t("Education Stages"),
  rooms: t("Rooms"),
  "student-guardians": t("Student Guardians"),
  enrollment: t("Enrollment"),
  "assign-teacher": t("Assign Teacher"),
  classes: t("My Classes"),
  attendance: t("Attendance"),
  grades: t("Grades"),
  exams: t("Exams"),
  schedule: t("Schedule"),
  children: t("Children"),
  notices: t("Notices"),
  profile: t("Profile"),
  "registration-requests": t("Registration Requests"),
  admins: t("School Admins"),
  new: t("New"),
  edit: t("Edit"),
};

export default function TopBar() {
  const theme = useThemeStore((s) => s.theme);
  const toggleTheme = useThemeStore((s) => s.toggleTheme);
  const toggleSidebar = useUiStore((s) => s.toggleSidebar);
  const setMobileSidebarOpen = useUiStore((s) => s.setMobileSidebarOpen);
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const location = useLocation();
  const navigate = useNavigate();

  const canSearch = user?.role === "platform_admin" || user?.role === "school_admin";

  const handleLogout = useCallback(() => {
    clearAuth();
    navigate("/auth/login", { replace: true });
  }, [clearAuth, navigate]);

  const handleMenuClick = () => {
    if (window.innerWidth < 768) {
      setMobileSidebarOpen(true);
    } else {
      toggleSidebar();
    }
  };

  const breadcrumbItems = location.pathname
    .split("/")
    .filter(Boolean)
    .filter((segment) => !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(segment))
    .map((segment) => ({
      label: routeLabels[segment] || segment.charAt(0).toUpperCase() + segment.slice(1),
    }));

  return (
    <header className="sticky top-0 z-20 flex h-[var(--topbar-height)] shrink-0 items-center gap-1.5 border-b border-[var(--color-border)] bg-[var(--color-surface-card)] px-2.5 sm:gap-4 sm:px-6">
      <div className="flex min-w-0 flex-1 items-center gap-2 sm:gap-4">
        <button
          onClick={handleMenuClick}
          aria-label={t("Toggle navigation")}
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)] transition-colors"
        >
          <Menu className="h-5 w-5" />
        </button>
        <BreadcrumbNav
          className="min-w-0 overflow-hidden"
          items={
            breadcrumbItems.length > 0
              ? breadcrumbItems
              : [{ label: t("Dashboard") }]
          }
        />
      </div>

      <div className="flex shrink-0 items-center gap-1 sm:gap-3">
        {canSearch && (
          <div className="hidden w-full max-w-64 sm:block">
            <GlobalSearch />
          </div>
        )}

        <NotificationBell />

        <LanguageSwitcher className="shrink-0" />

        <button
          onClick={toggleTheme}
          aria-label={theme === "light" ? t("Switch to dark mode") : t("Switch to light mode")}
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] transition-colors hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)]"
        >
          {theme === "light" ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
        </button>

        {user && (
          <UserMenu
            user={{
              firstName: user.firstName,
              lastName: user.lastName,
              email: user.email,
              role: user.role,
            }}
            onLogout={handleLogout}
          />
        )}
      </div>
    </header>
  );
}
