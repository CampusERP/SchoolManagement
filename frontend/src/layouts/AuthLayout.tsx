import { Outlet } from "react-router-dom";
import { useThemeStore } from "@/store/themeStore";

export default function AuthLayout() {
  const theme = useThemeStore((s) => s.theme);

  return (
    <div data-theme={theme} className="flex h-screen bg-[var(--color-surface)]">
      <div className="hidden w-1/2 items-center justify-center bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-accent)] lg:flex">
        <div className="max-w-md px-8 text-center">
          <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-2xl bg-white/20 text-white font-bold text-2xl backdrop-blur-sm">
            SM
          </div>
          <h1 className="text-3xl font-bold text-white">SchoolManager</h1>
          <p className="mt-3 text-lg text-white/80">
            A comprehensive platform for managing schools, teachers, students, and everything in between.
          </p>
        </div>
      </div>

      <div className="flex w-full items-center justify-center p-6 lg:w-1/2">
        <div className="w-full max-w-md">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
