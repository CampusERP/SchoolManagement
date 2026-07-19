import { useState, useEffect, useMemo } from "react";
import { Link, useLocation } from "react-router-dom";
import {
  LayoutDashboard,
  Building2,
  BookOpen,
  Users,
  UserPlus,
  GraduationCap,
  ClipboardList,
  ChevronDown,
  Layers,
  FileSpreadsheet,
} from "lucide-react";
import * as Separator from "@radix-ui/react-separator";
import * as ScrollArea from "@radix-ui/react-scroll-area";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/store/authStore";
import { useUiStore } from "@/store/uiStore";

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  permission?: string;
}

interface NavGroup {
  label?: string;
  items: NavItem[];
  defaultOpen?: boolean;
}

function getNavigationForRole(
  role: string | undefined
): NavGroup[] {
  switch (role) {
    case "platform_admin":
      return [
        {
          items: [
            { label: "Analytics", path: "/platform", icon: <LayoutDashboard className="h-5 w-5" /> },
            { label: "Schools", path: "/platform/schools", icon: <Building2 className="h-5 w-5" /> },
            { label: "Import Guide", path: "/platform/import-guide", icon: <FileSpreadsheet className="h-5 w-5" /> },
          ],
        },
        {
          label: "Academics",
          defaultOpen: false,
          items: [
            { label: "Academic Years", path: "/academics/years", icon: <BookOpen className="h-5 w-5" /> },
            { label: "Classrooms", path: "/academics/classrooms", icon: <ClipboardList className="h-5 w-5" /> },
            { label: "Grade Levels", path: "/academics/grade-levels", icon: <GraduationCap className="h-5 w-5" /> },
            { label: "Education Stages", path: "/academics/education-stages", icon: <Layers className="h-5 w-5" /> },
            { label: "Rooms", path: "/academics/rooms", icon: <Building2 className="h-5 w-5" /> },
          ],
        },
        {
          label: "People",
          defaultOpen: false,
          items: [
            { label: "Students", path: "/people/students", icon: <Users className="h-5 w-5" /> },
            { label: "Teachers", path: "/people/teachers", icon: <UserPlus className="h-5 w-5" /> },
            { label: "Parents", path: "/people/parents", icon: <Users className="h-5 w-5" /> },
            { label: "Import Guide", path: "/people/import-guide", icon: <FileSpreadsheet className="h-5 w-5" /> },
          ],
        },
        {
          items: [
            { label: "Enrollment", path: "/enrollment", icon: <ClipboardList className="h-5 w-5" /> },
          ],
        },
      ];
    case "school_admin":
      return [
        {
          items: [
            { label: "Dashboard", path: "/school", icon: <LayoutDashboard className="h-5 w-5" />, permission: "school.dashboard" },
          ],
        },
        {
          label: "Academics",
          defaultOpen: true,
          items: [
            { label: "Academic Years", path: "/academics/years", icon: <BookOpen className="h-5 w-5" />, permission: "academicyear.read" },
            { label: "Classrooms", path: "/academics/classrooms", icon: <ClipboardList className="h-5 w-5" />, permission: "classroom.read" },
            { label: "Grade Levels", path: "/academics/grade-levels", icon: <GraduationCap className="h-5 w-5" />, permission: "gradelevel.read" },
            { label: "Education Stages", path: "/academics/education-stages", icon: <Layers className="h-5 w-5" />, permission: "educationstage.read" },
            { label: "Rooms", path: "/academics/rooms", icon: <Building2 className="h-5 w-5" />, permission: "room.read" },
          ],
        },
        {
          label: "People",
          defaultOpen: true,
          items: [
            { label: "Students", path: "/people/students", icon: <Users className="h-5 w-5" />, permission: "student.read" },
            { label: "Teachers", path: "/people/teachers", icon: <UserPlus className="h-5 w-5" />, permission: "teacher.read" },
            { label: "Parents", path: "/people/parents", icon: <Users className="h-5 w-5" />, permission: "parent.read" },
          ],
        },
        {
          items: [
            { label: "Enrollment", path: "/enrollment", icon: <ClipboardList className="h-5 w-5" />, permission: "enrollment.create" },
          ],
        },
      ];
    case "teacher":
      return [
        {
          items: [
            { label: "My Classes", path: "/teacher/classes", icon: <GraduationCap className="h-5 w-5" /> },
          ],
        },
      ];
    default:
      return [];
  }
}

function SidebarContent({ collapsed }: { collapsed: boolean }) {
  const location = useLocation();
  const user = useAuthStore((s) => s.user);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [openGroups, setOpenGroups] = useState<Record<number, boolean>>({});

  const navigation = useMemo(() => {
    const base = getNavigationForRole(user?.role);
    if (user?.role !== "school_admin") return base;
    return base
      .map((group) => ({
        ...group,
        items: group.items.filter(
          (item) => !item.permission || hasPermission(item.permission)
        ),
      }))
      .filter((group) => group.items.length > 0);
  }, [user?.role, hasPermission]);

  useEffect(() => {
    const initial: Record<number, boolean> = {};
    navigation.forEach((group, i) => {
      initial[i] = group.defaultOpen ?? false;
    });
    setOpenGroups(initial);
  }, [user?.role]);

  const toggleGroup = (index: number) => {
    setOpenGroups((prev) => ({ ...prev, [index]: !prev[index] }));
  };

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-16 shrink-0 items-center gap-2 px-4">
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-[var(--color-primary)] text-white font-bold text-sm">
          SM
        </div>
        {!collapsed && (
          <span className="text-lg font-semibold text-[var(--color-text-primary)] whitespace-nowrap">
            SchoolManager
          </span>
        )}
      </div>

      <Separator.Root className="h-px bg-[var(--color-border)]" />

      <ScrollArea.Root className="flex-1 overflow-hidden">
        <ScrollArea.Viewport className="h-full w-full">
          <nav className="px-3 py-4 space-y-2">
            {navigation.map((group, groupIndex) => (
              <div key={groupIndex}>
                {group.label && !collapsed && (
                  <div className="flex items-center justify-between px-3 py-1.5">
                    <span className="text-xs font-semibold uppercase tracking-wider text-[var(--color-text-muted)]">
                      {group.label}
                    </span>
                    <button
                      onClick={() => toggleGroup(groupIndex)}
                      className="text-[var(--color-text-muted)] hover:text-[var(--color-text-secondary)]"
                    >
                      <ChevronDown
                        className={cn(
                          "h-4 w-4 transition-transform",
                          openGroups[groupIndex] ? "rotate-0" : "-rotate-90"
                        )}
                      />
                    </button>
                  </div>
                )}

                {(!group.label || collapsed || openGroups[groupIndex]) && (
                  <div className="space-y-0.5">
                    {group.items.map((item) => {
                      const isActive =
                        item.path === "/"
                          ? location.pathname === "/"
                          : location.pathname === item.path ||
                            location.pathname.startsWith(item.path + "/");
                      return (
                        <Link
                          key={item.path}
                          to={item.path}
                          className={cn(
                            "flex items-center gap-3 rounded-[var(--border-radius)] px-3 py-2 text-sm font-medium transition-colors",
                            isActive
                              ? "bg-[var(--color-primary-light)] text-[var(--color-primary)]"
                              : "text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)]",
                            collapsed && "justify-center px-0"
                          )}
                          title={collapsed ? item.label : undefined}
                        >
                          {item.icon}
                          {!collapsed && <span>{item.label}</span>}
                        </Link>
                      );
                    })}
                  </div>
                )}
              </div>
            ))}
          </nav>
        </ScrollArea.Viewport>
        <ScrollArea.Scrollbar
          className="flex touch-none select-none p-0.5 transition-colors data-[orientation=vertical]:w-2"
          orientation="vertical"
        >
          <ScrollArea.Thumb className="relative flex-1 rounded-full bg-[var(--color-border)]" />
        </ScrollArea.Scrollbar>
      </ScrollArea.Root>
    </div>
  );
}

export default function Sidebar() {
  const collapsed = useUiStore((s) => s.sidebarCollapsed);
  const mobileSidebarOpen = useUiStore((s) => s.mobileSidebarOpen);
  const setMobileSidebarOpen = useUiStore((s) => s.setMobileSidebarOpen);

  useEffect(() => {
    if (mobileSidebarOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [mobileSidebarOpen]);

  return (
    <>
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-30 hidden bg-[var(--color-surface-card)] border-r border-[var(--color-border)] transition-[width] duration-200 md:block"
        )}
        style={{ width: collapsed ? "var(--sidebar-collapsed-w)" : "var(--sidebar-width)" }}
      >
        <SidebarContent collapsed={collapsed} />
      </aside>

      {mobileSidebarOpen && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div
            className="absolute inset-0 bg-black/50"
            onClick={() => setMobileSidebarOpen(false)}
          />
          <aside className="absolute inset-y-0 left-0 w-[var(--sidebar-width)] bg-[var(--color-surface-card)] shadow-xl">
            <SidebarContent collapsed={false} />
          </aside>
        </div>
      )}
    </>
  );
}
