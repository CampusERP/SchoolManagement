import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { TeachersApi, type GetTeachersParams } from "./api";
import type {
  CreateTeacherCommand,
  UpdateTeacherCommand,
} from "@/types/teacher.types";

export const useTeachers = (params: Omit<GetTeachersParams, "schoolId">) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["teachers", schoolId, params],
    queryFn: () => TeachersApi.getTeachers({ schoolId: schoolId!, ...params }),
    enabled: !!schoolId,
  });
};

export const useTeacher = (teacherId: string | undefined) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["teacher", schoolId, teacherId],
    queryFn: () => TeachersApi.getTeacherById(teacherId!, schoolId!),
    enabled: !!schoolId && !!teacherId,
  });
};

export const useCreateTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTeacherCommand) => TeachersApi.createTeacher(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });
};

export const useUpdateTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      teacherId,
      data,
    }: {
      teacherId: string;
      data: UpdateTeacherCommand;
    }) => TeachersApi.updateTeacher(teacherId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
      queryClient.invalidateQueries({ queryKey: ["teacher"] });
    },
  });
};
