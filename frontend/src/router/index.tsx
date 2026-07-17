import { Suspense } from "react";
import { BrowserRouter, useRoutes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { ConfigProvider } from "antd";
import { Toaster } from "sonner";
import { queryClient } from "@/lib/queryClient";
import { antdTheme } from "@/styles/antd-theme";
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
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={antdTheme}>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
        <Toaster position="top-right" richColors />
      </ConfigProvider>
    </QueryClientProvider>
  );
}
