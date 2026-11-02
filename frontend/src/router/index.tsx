import { Suspense } from "react";
import { BrowserRouter, useRoutes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { ConfigProvider } from "antd";
import arEG from "antd/locale/ar_EG";
import enUS from "antd/locale/en_US";
import { Toaster } from "sonner";
import { queryClient } from "@/lib/queryClient";
import { getAntdTheme } from "@/styles/antd-theme";
import { useThemeStore } from "@/store/themeStore";
import { getDirection, getLanguage } from "@/i18n";
import { routes } from "@/router/routes";
import Spinner from "@/components/atoms/Spinner";

function AppSuspense({ children }: { children: React.ReactNode }) {
  return (
    <Suspense
      fallback={
        <div className="flex h-screen items-center justify-center bg-[var(--color-surface)]">
          <Spinner size="lg" />
        </div>
      }
    >
      {children}
    </Suspense>
  );
}

function AppRoutes() {
  const element = useRoutes(routes);
  return <AppSuspense>{element}</AppSuspense>;
}

export default function Router() {
  const theme = useThemeStore((s) => s.theme);

  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider
        theme={getAntdTheme(theme)}
        locale={getLanguage() === "ar" ? arEG : enUS}
        direction={getDirection()}
      >
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
        <Toaster position="top-right" richColors />
      </ConfigProvider>
    </QueryClientProvider>
  );
}
