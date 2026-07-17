import { create } from "zustand";

interface UiState {
  sidebarCollapsed: boolean;
  mobileSidebarOpen: boolean;
  activeSchoolId: string | null;
  toggleSidebar: () => void;
  setMobileSidebarOpen: (open: boolean) => void;
  setActiveSchool: (id: string) => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarCollapsed: false,
  mobileSidebarOpen: false,
  activeSchoolId: null,
  toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
  setMobileSidebarOpen: (open) => set({ mobileSidebarOpen: open }),
  setActiveSchool: (id) => set({ activeSchoolId: id }),
}));
