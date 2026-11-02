import { Outlet } from "react-router-dom";
import { useThemeStore } from "@/store/themeStore";
import Logo from "@/components/atoms/Logo";
import LanguageSwitcher from "@/components/molecules/LanguageSwitcher";
import { t } from "@/i18n";

export default function AuthLayout() {
  const theme = useThemeStore((s) => s.theme);

  return (
    <div data-theme={theme} className="flex h-screen bg-[var(--color-surface)]">
      <div className="hidden w-1/2 items-center justify-center bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-accent)] lg:flex">
        <div className="max-w-md px-8 text-center">
          <Logo className="mx-auto mb-6 h-16 w-16 rounded-2xl bg-white/20 p-2 shadow-lg backdrop-blur-sm" />
          <h1 className="text-3xl font-bold text-white">{t("CampusERP")}</h1>
          <p className="mt-3 text-lg text-white/80">
            {t("A comprehensive platform for managing schools, teachers, students, and everything in between.")}
          </p>
        </div>
      </div>

      <div className="flex w-full items-center justify-center p-6 lg:w-1/2">
        <div className="w-full max-w-md">
          <div className="mb-4 flex justify-end">
            <LanguageSwitcher />
          </div>
          <Outlet />
        </div>
      </div>
    </div>
  );
}
