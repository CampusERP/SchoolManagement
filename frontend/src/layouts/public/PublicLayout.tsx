import { Outlet, useLocation } from "react-router-dom";
import PublicNavbar from "@/components/public/PublicNavbar";
import PublicFooter from "@/components/public/PublicFooter";
import { useThemeStore } from "@/store/themeStore";

export default function PublicLayout() {
  const theme = useThemeStore((s) => s.theme);
  const location = useLocation();
  
  return (
    <div data-theme={theme} className="min-h-screen bg-slate-50 dark:bg-slate-950">
      <PublicNavbar />
      <main className={location.pathname === "/" ? "" : "pt-16"}>
        <Outlet />
      </main>
      <PublicFooter />
    </div>
  );
}