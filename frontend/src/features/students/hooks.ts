import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { StudentsApi, type GetStudentsParams } from "./api";
import type {
  CreateStudentCommand,
  UpdateStudentCommand,
} from "@/types/student.types";

export const useStudents = (
  params: Omit<GetStudentsParams, "schoolId">
) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["students", schoolId, params],
    queryFn: () => StudentsApi.getStudents({ schoolId: schoolId!, ...params }),
    enabled: !!schoolId,
  });
};

export const useStudent = (studentId: string | undefined) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["student", schoolId, studentId],
    queryFn: () => StudentsApi.getStudentById(studentId!, schoolId!),
    enabled: !!schoolId && !!studentId,
  });
};

export const useCreateStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateStudentCommand) => StudentsApi.createStudent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });
};

export const useUpdateStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      studentId,
      data,
    }: {
      studentId: string;
      data: UpdateStudentCommand;
    }) => StudentsApi.updateStudent(studentId, data),
    onSuccess: (_res, variables) => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
      queryClient.invalidateQueries({
        queryKey: ["student"],
      });
      void variables;
    },
  });
};
