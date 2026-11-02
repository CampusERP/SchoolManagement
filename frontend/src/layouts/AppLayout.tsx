import { Outlet } from "react-router-dom";
import { cn } from "@/lib/utils";
import { useThemeStore } from "@/store/themeStore";
import { useUiStore } from "@/store/uiStore";
import Sidebar from "@/components/organisms/Sidebar";
import TopBar from "@/components/organisms/TopBar";

export default function AppLayout() {
  const theme = useThemeStore((s) => s.theme);
  const collapsed = useUiStore((s) => s.sidebarCollapsed);

  return (
    <div data-theme={theme} className="flex h-dvh min-h-0 overflow-hidden bg-[var(--color-surface)]">
      <Sidebar />

      <div
        className={cn(
          "flex min-h-0 min-w-0 flex-1 flex-col transition-[margin] duration-200",
          collapsed ? "md:ms-[var(--sidebar-collapsed-w)]" : "md:ms-[var(--sidebar-width)]"
        )}
      >
        <TopBar />

        <main className="min-w-0 flex-1 overflow-x-hidden overflow-y-auto overscroll-y-contain px-3 py-4 pb-[calc(1rem+env(safe-area-inset-bottom))] sm:p-6 lg:p-[var(--content-padding)]">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
