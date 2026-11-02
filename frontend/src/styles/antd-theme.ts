import { theme as antdAlgorithms, type ThemeConfig } from "antd";

export type ThemeMode = "light" | "dark";

// Brand + shape tokens shared by both modes. Actual surface/text/border
// colours are supplied per-mode so antd can derive the rest of its palette
// (elevated surfaces, fills, hover states) correctly for that mode.
const brandTokens: ThemeConfig["token"] = {
  colorPrimary: "#4F46E5",
  colorSuccess: "#10B981",
  colorWarning: "#F59E0B",
  colorError: "#EF4444",
  colorInfo: "#3B82F6",
  borderRadius: 8,
  fontFamily: "'Inter', sans-serif",
};

const lightTokens: ThemeConfig["token"] = {
  ...brandTokens,
  colorBgContainer: "#FFFFFF",
  colorBgElevated: "#FFFFFF",
  colorBgLayout: "#F8FAFC",
  colorBorder: "#E2E8F0",
  colorBorderSecondary: "#E2E8F0",
  colorText: "#0F172A",
  colorTextSecondary: "#64748B",
  colorTextTertiary: "#94A3B8",
};

const darkTokens: ThemeConfig["token"] = {
  ...brandTokens,
  colorBgContainer: "#1E293B",
  colorBgElevated: "#1E293B",
  colorBgLayout: "#0F172A",
  colorBorder: "#334155",
  colorBorderSecondary: "#334155",
  colorText: "#F1F5F9",
  colorTextSecondary: "#94A3B8",
  colorTextTertiary: "#64748B",
};

export function getAntdTheme(mode: ThemeMode): ThemeConfig {
  const isDark = mode === "dark";

  return {
    algorithm: isDark
      ? antdAlgorithms.darkAlgorithm
      : antdAlgorithms.defaultAlgorithm,
    token: isDark ? darkTokens : lightTokens,
    components: {
      Table: {
        headerBg: isDark ? "#0F172A" : "#F8FAFC",
        rowHoverBg: isDark ? "rgba(79, 70, 229, 0.18)" : "#EEF2FF",
        borderRadius: 12,
      },
      Menu: { itemBorderRadius: 8 },
      Button: { borderRadius: 8 },
      Card: { borderRadius: 12 },
      Input: { borderRadius: 8 },
      Select: { borderRadius: 8 },
    },
  };
}

// Kept for backwards compatibility with any non-reactive callers.
export const antdTheme = getAntdTheme("light");
