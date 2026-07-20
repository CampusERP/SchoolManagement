import { useMutation, useQueryClient } from "@tanstack/react-query";
import { EnrollmentApi } from "./api";
import type {
  EnrollStudentCommand,
  AssignTeacherCommand,
} from "@/types/enrollment.types";

export const useEnrollStudent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EnrollStudentCommand) =>
      EnrollmentApi.enrollStudent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
      queryClient.invalidateQueries({ queryKey: ["student"] });
      queryClient.invalidateQueries({ queryKey: ["classrooms"] });
    },
  });
};

export const useAssignTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AssignTeacherCommand) =>
      EnrollmentApi.assignTeacher(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
      queryClient.invalidateQueries({ queryKey: ["teacher"] });
      queryClient.invalidateQueries({ queryKey: ["classrooms"] });
    },
  });
};
