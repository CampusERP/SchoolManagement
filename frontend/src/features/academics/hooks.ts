import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { AcademicsApi, type GetClassroomsParams } from "./api";
import type {
  CreateAcademicYearCommand,
  UpdateAcademicYearCommand,
  CreateTermCommand,
  CreateClassRoomCommand,
  UpdateClassRoomCommand,
  CreateGradeLevelCommand,
  UpdateGradeLevelCommand,
  CreateRoomCommand,
  UpdateRoomCommand,
} from "@/types/academic.types";

// ── Academic Years ────────────────────────────────────────────────
export const useAcademicYears = () => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["academic-years", schoolId],
    queryFn: () => AcademicsApi.getAcademicYears(schoolId!),
    enabled: !!schoolId,
  });
};

export const useCreateAcademicYear = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateAcademicYearCommand) =>
      AcademicsApi.createAcademicYear(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["academic-years"] });
    },
  });
};

export const useUpdateAcademicYear = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateAcademicYearCommand;
    }) => AcademicsApi.updateAcademicYear(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["academic-years"] });
    },
  });
};

export const useCreateTerm = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTermCommand) => AcademicsApi.createTerm(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["academic-years"] });
    },
  });
};

// ── Classrooms ────────────────────────────────────────────────────
export const useClassrooms = (
  params: Omit<GetClassroomsParams, "schoolId">
) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["classrooms", schoolId, params],
    queryFn: () =>
      AcademicsApi.getClassrooms({ schoolId: schoolId!, ...params }),
    enabled: !!schoolId,
  });
};

export const useCreateClassroom = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateClassRoomCommand) =>
      AcademicsApi.createClassroom(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classrooms"] });
    },
  });
};

export const useUpdateClassroom = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateClassRoomCommand }) =>
      AcademicsApi.updateClassroom(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classrooms"] });
    },
  });
};

// ── Grade Levels ──────────────────────────────────────────────────
export const useGradeLevels = () => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["grade-levels", schoolId],
    queryFn: () => AcademicsApi.getGradeLevels(schoolId!),
    enabled: !!schoolId,
  });
};

export const useCreateGradeLevel = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateGradeLevelCommand) =>
      AcademicsApi.createGradeLevel(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["grade-levels"] });
    },
  });
};

export const useUpdateGradeLevel = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateGradeLevelCommand }) =>
      AcademicsApi.updateGradeLevel(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["grade-levels"] });
    },
  });
};

// ── Rooms ─────────────────────────────────────────────────────────
export const useRooms = () => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["rooms", schoolId],
    queryFn: () => AcademicsApi.getRooms(schoolId!),
    enabled: !!schoolId,
  });
};

export const useCreateRoom = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoomCommand) => AcademicsApi.createRoom(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
};

export const useUpdateRoom = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoomCommand }) =>
      AcademicsApi.updateRoom(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
};

// ── Subjects ───────────────────────────────────────────────────────
export const useSubjects = (gradeLevelId?: string) => {
  return useQuery({
    queryKey: ["subjects", gradeLevelId],
    queryFn: () => AcademicsApi.getSubjects(gradeLevelId),
  });
};
