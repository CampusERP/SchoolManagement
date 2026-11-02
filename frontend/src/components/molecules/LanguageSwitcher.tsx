import { Globe } from "lucide-react";
import { cn } from "@/lib/utils";
import { getLanguage, setLanguage, t, type Language } from "@/i18n";

interface LanguageSwitcherProps {
  className?: string;
}

/**
 * Compact English / Arabic toggle. Selecting a language persists it and
 * reloads the page so module-level translated constants are refreshed.
 */
export default function LanguageSwitcher({ className }: LanguageSwitcherProps) {
  const language = getLanguage();
  const next: Language = language === "en" ? "ar" : "en";
  const label = next === "ar" ? "العربية" : "English";

  return (
    <button
      type="button"
      onClick={() => setLanguage(next)}
      aria-label={
        next === "ar" ? t("Switch to Arabic") : t("Switch to English")
      }
      title={next === "ar" ? t("Switch to Arabic") : t("Switch to English")}
      className={cn(
        "flex h-9 items-center gap-1.5 rounded-[var(--border-radius)] px-2.5 text-sm font-medium text-[var(--color-text-secondary)] transition-colors hover:bg-[var(--color-surface)] hover:text-[var(--color-text-primary)]",
        className
      )}
    >
      <Globe className="h-4 w-4" />
      <span className="hidden sm:inline">{label}</span>
    </button>
  );
}
