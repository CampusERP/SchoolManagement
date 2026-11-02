import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import dayjs from "dayjs";
import "dayjs/locale/ar";
import "@/styles/globals.css";
import App from "@/App";
import { useThemeStore } from "@/store/themeStore";
import { getLanguage, initLanguage } from "@/i18n";
import { initZodErrorMap } from "@/i18n/zod";

// Apply the persisted theme and language before the first paint to avoid a
// flash of the wrong mode/direction.
const initialTheme = useThemeStore.getState().theme;
document.documentElement.setAttribute("data-theme", initialTheme);
document.documentElement.style.colorScheme = initialTheme;
initLanguage();
initZodErrorMap();
dayjs.locale(getLanguage() === "ar" ? "ar" : "en");

if (import.meta.env.DEV) {
  const originalConsoleError = console.error;
  console.error = (...args: unknown[]) => {
    if (
      typeof args[0] === "string" &&
      args[0].includes("findDOMNode is deprecated")
    )
      return;
    originalConsoleError(...args);
  };
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
