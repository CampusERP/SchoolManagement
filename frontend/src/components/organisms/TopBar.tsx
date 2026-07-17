import { useState } from "react";
import { Menu, Sun, Moon, Search } from "lucide-react";
import { useLocation } from "react-router-dom";
import { useThemeStore } from "@/store/themeStore";
import { useUiStore } from "@/store/uiStore";
import { useAuthStore } from "@/store/authStore";
import BreadcrumbNav from "@/components/molecules/BreadcrumbNav";
import NotificationBell from "@/components/molecules/NotificationBell";
import UserMenu from "@/components/molecules/UserMenu";

const routeLabels: Record<string, string> = {
  platform: "Platform",
  school: "School",
  teacher: "Teacher",
  students: "Students",
  teachers: "Teachers",
  parents: "Parents",
  academics: "Academics",
  enrollment: "Enrollment",
  classes: "My Classes",
};

export default function TopBar() {
  const theme = useThemeStore((s) => s.theme);
  const toggleTheme = useThemeStore((s) => s.toggleTheme);
  const toggleSidebar = useUiStore((s) => s.toggleSidebar);
  const setMobileSidebarOpen = useUiStore((s) => s.setMobileSidebarOpen);
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const location = useLocation();

  const [searchValue] = useState("");

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
    .map((segment) => ({
      label: routeLabels[segment] || segment.charAt(0).toUpperCase() + segment.slice(1),
    }));

  return (
    <header className="sticky top-0 z-20 flex h-[var(--topbar-height)] items-center border-b border-[var(--color-border)] bg-[var(--color-surface-card)] px-6">
      <div className="flex items-center gap-4">
        <button
          onClick={handleMenuClick}
          className="flex h-9 w-9 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)] transition-colors"
        >
          <Menu className="h-5 w-5" />
        </button>
        <BreadcrumbNav
          items={
            breadcrumbItems.length > 0
              ? breadcrumbItems
              : [{ label: "Dashboard" }]
          }
        />
      </div>

      <div className="ml-auto flex items-center gap-3">
        <div className="hidden sm:flex items-center gap-2 rounded-[var(--border-radius)] border border-[var(--color-border)] bg-[var(--color-surface)] px-3 py-1.5 text-sm text-[var(--color-text-muted)]">
          <Search className="h-4 w-4" />
          <span>Search...</span>
        </div>

        <NotificationBell />

        <button
          onClick={toggleTheme}
          className="flex h-9 w-9 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)] transition-colors"
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
            onLogout={clearAuth}
          />
        )}
      </div>
    </header>
  );
}
