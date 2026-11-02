import { ar } from "./ar";

export type Language = "en" | "ar";

export interface LanguageOption {
  code: Language;
  label: string;
  nativeLabel: string;
  dir: "ltr" | "rtl";
}

export const LANGUAGES: LanguageOption[] = [
  { code: "en", label: "English", nativeLabel: "English", dir: "ltr" },
  { code: "ar", label: "Arabic", nativeLabel: "العربية", dir: "rtl" },
];

const STORAGE_KEY = "school-management-language";

function readStoredLanguage(): Language {
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    if (stored === "ar" || stored === "en") return stored;
  } catch {
    // localStorage may be unavailable (private mode, SSR, tests).
  }
  return "en";
}

let currentLanguage: Language = readStoredLanguage();

export function getLanguage(): Language {
  return currentLanguage;
}

export function isRtl(language: Language = currentLanguage): boolean {
  return language === "ar";
}

export function getDirection(language: Language = currentLanguage): "ltr" | "rtl" {
  return isRtl(language) ? "rtl" : "ltr";
}

/** Applies the persisted language to the document (lang + dir attributes). */
export function initLanguage(): void {
  if (typeof document === "undefined") return;
  document.documentElement.lang = currentLanguage;
  document.documentElement.dir = getDirection();
  document.title = t("School Management");
}

/**
 * Persists the selected language and reloads the page.
 *
 * A reload guarantees module-level translated constants (validation
 * messages, option lists, breadcrumb labels, ...) are re-evaluated in the
 * newly selected language.
 */
export function setLanguage(language: Language): void {
  if (language === currentLanguage) return;
  currentLanguage = language;
  try {
    window.localStorage.setItem(STORAGE_KEY, language);
  } catch {
    // Ignore storage failures; the in-memory language still changes.
  }
  document.documentElement.lang = language;
  document.documentElement.dir = getDirection(language);
  document.title = t("School Management");
  window.location.reload();
}

type InterpolationValues = Record<string, string | number>;

/**
 * Selects a value stored in both languages. Falls back to the other language
 * when the active translation has not been filled in yet.
 */
export function localize(
  english?: string | null,
  arabic?: string | null
): string {
  const en = english?.trim() ?? "";
  const ar = arabic?.trim() ?? "";
  return currentLanguage === "ar" ? ar || en : en || ar;
}

/**
 * Translates an English source string to the active language.
 *
 * English is used as the lookup key so that untranslated strings degrade
 * gracefully to readable English instead of raw keys. Values may contain
 * `{placeholder}` tokens, e.g. `t("Hello {name}", { name: "Sara" })`.
 */
export function t(key: string, values?: InterpolationValues): string {
  const dictionary = currentLanguage === "ar" ? ar : undefined;
  let text = dictionary?.[key] ?? key;
  if (values) {
    text = text.replace(/\{(\w+)\}/g, (match, name: string) =>
      Object.prototype.hasOwnProperty.call(values, name)
        ? String(values[name])
        : match
    );
  }
  return text;
}

export { ar };
