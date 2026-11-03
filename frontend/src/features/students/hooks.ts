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

export const useDeleteStudent = () => {
  const queryClient = useQueryClient();
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: (studentId: string) => StudentsApi.deleteStudent(studentId, schoolId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });
};

export const useResetStudentPassword = () => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: (studentId: string) => StudentsApi.resetStudentPassword(studentId, schoolId!),
  });
};

export const useSetStudentLogin = () => {
  const client = useQueryClient();
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: ({ studentId, enabled, email, password }: { studentId: string; enabled: boolean; email?: string; password?: string }) =>
      StudentsApi.setStudentLogin(studentId, schoolId!, { enabled, email, password }),
    onSuccess: (_data, variables) => {
      client.invalidateQueries({ queryKey: ["student", schoolId, variables.studentId] });
      client.invalidateQueries({ queryKey: ["students"] });
    },
  });
};

export const useUploadStudentDocument = () => {
  const client = useQueryClient();
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: ({ studentId, file }: { studentId: string; file: File }) => StudentsApi.uploadDocument(studentId, schoolId!, file),
    onSuccess: (_data, variables) => client.invalidateQueries({ queryKey: ["student", schoolId, variables.studentId] }),
  });
};

export const useLinkStudentGuardian = () => {
  const queryClient = useQueryClient();
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: (data: { studentId: string; parentId: string; relationshipType: string; isPrimaryContact: boolean; canViewGrades: boolean; canViewBilling: boolean }) =>
      StudentsApi.linkGuardian({ ...data, schoolId: schoolId! }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["student"] }),
  });
};
