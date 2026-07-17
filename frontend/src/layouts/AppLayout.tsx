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
    <div data-theme={theme} className="flex h-screen overflow-hidden bg-[var(--color-surface)]">
      <Sidebar />

      <div
        className={cn(
          "flex min-h-0 flex-1 flex-col transition-[margin-left] duration-200",
          collapsed ? "md:ml-[var(--sidebar-collapsed-w)]" : "md:ml-[var(--sidebar-width)]"
        )}
      >
        <TopBar />

        <main className="flex-1 overflow-y-auto p-[var(--content-padding)]">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
