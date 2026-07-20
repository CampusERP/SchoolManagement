import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { Bell, CalendarDays, ClipboardList, GraduationCap, House, LogOut, Users } from "lucide-react";
import { useAuthStore } from "@/store/authStore";
import { cn } from "@/lib/utils";
import type { LucideIcon } from "lucide-react";

type PortalNavItem = [string, string, LucideIcon];
const studentNav: PortalNavItem[] = [["Home", "/student", House], ["Classes", "/student/classes", GraduationCap], ["Assignments", "/student/assignments", ClipboardList], ["Attendance", "/student/attendance", CalendarDays], ["Grades", "/student/grades", GraduationCap]];
const parentNav: PortalNavItem[] = [["Home", "/parent", House], ["Children", "/parent/children", Users], ["Notices", "/parent/notifications", Bell]];

export default function PortalLayout() {
  const user = useAuthStore((s) => s.user); const clearAuth = useAuthStore((s) => s.clearAuth); const navigate = useNavigate();
  const nav = user?.role === "parent" ? parentNav : studentNav;
  return <div className="min-h-screen bg-[var(--color-surface)]"><header className="sticky top-0 z-20 border-b border-[var(--color-border)] bg-[var(--color-surface-card)]"><div className="mx-auto flex h-16 max-w-7xl items-center gap-6 px-4 sm:px-6"><Link to={user?.role === "parent" ? "/parent" : "/student"} className="font-bold text-[var(--color-primary)]">SchoolManager <span className="font-medium text-[var(--color-text-secondary)]">Portal</span></Link><nav className="hidden flex-1 gap-1 md:flex">{nav.map(([label, path, Icon]) => <NavLink key={String(path)} to={String(path)} end={path === "/student" || path === "/parent"} className={({isActive}) => cn("flex items-center gap-2 rounded-lg px-3 py-2 text-sm", isActive ? "bg-[var(--color-primary-light)] text-[var(--color-primary)]" : "text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]")}><Icon className="h-4 w-4" />{label}</NavLink>)}</nav><span className="ml-auto hidden text-sm text-[var(--color-text-secondary)] sm:block">{user?.firstName} {user?.lastName}</span><button onClick={() => { clearAuth(); navigate("/auth/login", { replace: true }); }} className="rounded-lg p-2 text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]" aria-label="Sign out"><LogOut className="h-5 w-5" /></button></div></header><main className="mx-auto max-w-7xl px-4 py-6 pb-24 sm:px-6"><Outlet /></main><nav className="fixed inset-x-0 bottom-0 z-20 flex justify-around border-t border-[var(--color-border)] bg-[var(--color-surface-card)] py-2 md:hidden">{nav.slice(0, 5).map(([label, path, Icon]) => <NavLink key={String(path)} to={String(path)} end={path === "/student" || path === "/parent"} className={({isActive}) => cn("flex flex-col items-center gap-1 text-xs", isActive ? "text-[var(--color-primary)]" : "text-[var(--color-text-muted)]")}><Icon className="h-5 w-5" />{label}</NavLink>)}</nav></div>;
}
