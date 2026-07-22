import { useMutation, useQueryClient } from "@tanstack/react-query";
import { EnrollmentApi } from "./api";
import type {
  EnrollStudentCommand,
  EnrollTeacherCommand,
  AssignTeacherCommand,
  UpdateTeachingAssignmentCommand,
  DeleteTeachingAssignmentCommand,
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

export const useEnrollTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EnrollTeacherCommand) =>
      EnrollmentApi.enrollTeacher(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
      queryClient.invalidateQueries({ queryKey: ["teacher"] });
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

export const useUpdateTeachingAssignment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateTeachingAssignmentCommand) =>
      EnrollmentApi.updateTeachingAssignment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher"] });
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });
};

export const useDeleteTeachingAssignment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: DeleteTeachingAssignmentCommand) =>
      EnrollmentApi.deleteTeachingAssignment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher"] });
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });
};
