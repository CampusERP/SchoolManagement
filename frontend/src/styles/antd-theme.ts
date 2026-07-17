import type { ThemeConfig } from "antd";

export const antdTheme: ThemeConfig = {
  token: {
    colorPrimary: "#4F46E5",
    colorSuccess: "#10B981",
    colorWarning: "#F59E0B",
    colorError: "#EF4444",
    colorInfo: "#3B82F6",
    borderRadius: 8,
    fontFamily: "'Inter', sans-serif",
    colorBgContainer: "var(--color-surface-card)",
    colorBorder: "var(--color-border)",
    colorText: "var(--color-text-primary)",
    colorTextSecondary: "var(--color-text-secondary)",
    colorBgLayout: "var(--color-surface)",
  },
  components: {
    Table: {
      headerBg: "var(--color-surface)",
      borderRadius: 12,
    },
    Menu: { itemBorderRadius: 8 },
    Button: { borderRadius: 8 },
    Card: { borderRadius: 12 },
    Input: { borderRadius: 8 },
    Select: { borderRadius: 8 },
  },
};
