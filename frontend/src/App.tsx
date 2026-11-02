import { useEffect } from "react";
import ErrorBoundary from "@/components/organisms/ErrorBoundary";
import { useThemeStore } from "@/store/themeStore";
import Router from "@/router";

export default function App() {
  const theme = useThemeStore((s) => s.theme);

  // Apply the theme to the document root so CSS variables (and antd portals
  // rendered into <body> — dropdowns, modals, selects) inherit the palette.
  useEffect(() => {
    const root = document.documentElement;
    root.setAttribute("data-theme", theme);
    root.style.colorScheme = theme;
  }, [theme]);

  return (
    <ErrorBoundary>
      <Router />
    </ErrorBoundary>
  );
}
