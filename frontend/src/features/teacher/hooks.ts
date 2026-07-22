import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { TeacherApi } from "./api";

export const useTeacherRoster = (classRoomId: string | null, schoolId: string | null) =>
  useQuery({
    queryKey: ["teacher", "roster", classRoomId],
    queryFn: () => TeacherApi.getRoster(classRoomId!, schoolId!),
    enabled: !!classRoomId && !!schoolId,
  });

export const useTeacherMyClasses = (termId: string | null) =>
  useQuery({
    queryKey: ["teacher", "myClasses", termId],
    queryFn: () => TeacherApi.getMyClasses(termId!),
    enabled: !!termId,
  });

export const useAttendanceSheet = (classScheduleId: string | null, schoolId: string | null, date: string | null) =>
  useQuery({
    queryKey: ["teacher", "attendance", classScheduleId, schoolId, date],
    queryFn: () => TeacherApi.getAttendanceSheet(classScheduleId!, schoolId!, date!),
    enabled: !!classScheduleId && !!schoolId && !!date,
  });

export const useRecordAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.recordAttendance,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["teacher", "attendance", variables.classScheduleId, variables.date],
      });
    },
  });
};

export const useLockAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ sessionId, schoolId }: { sessionId: string; schoolId: string }) =>
      TeacherApi.lockAttendanceSession(sessionId, schoolId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher", "attendance"] });
    },
  });
};

export const useClassAssignments = (teachingAssignmentId: string | null) =>
  useQuery({
    queryKey: ["teacher", "assignments", teachingAssignmentId],
    queryFn: () => TeacherApi.getClassAssignments(teachingAssignmentId!),
    enabled: !!teachingAssignmentId,
  });

export const useGradeSubmission = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.gradeSubmission,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["teacher", "assignments", variables.assignmentId],
      });
    },
  });
};

export const useTeacherSchedule = (teacherId: string | null, schoolId: string | null, termId: string | null) =>
  useQuery({
    queryKey: ["teacher", "schedule", teacherId, schoolId, termId],
    queryFn: () => TeacherApi.getSchedule(teacherId!, schoolId!, termId!),
    enabled: !!teacherId && !!schoolId && !!termId,
  });

export const useTeacherExams = (schoolId: string | null, termId: string | null) =>
  useQuery({
    queryKey: ["teacher", "exams", schoolId, termId],
    queryFn: () => TeacherApi.getExams(schoolId!, termId!),
    enabled: !!schoolId && !!termId,
  });

export const useCreateExam = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.createExam,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher", "exams"] });
    },
  });
};

export const useAddExamSchedule = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.addExamSchedule,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher", "exams"] });
    },
  });
};

export const useRecordExamResults = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.recordExamResults,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher", "exams"] });
    },
  });
};

export const useLockExam = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TeacherApi.lockExam,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher", "exams"] });
    },
  });
};

export const useClassExamResults = (schoolId: string | null, examScheduleId: string | null) =>
  useQuery({
    queryKey: ["teacher", "examResults", schoolId, examScheduleId],
    queryFn: () => TeacherApi.getClassExamResults(schoolId!, examScheduleId!),
    enabled: !!schoolId && !!examScheduleId,
  });

export const useExamSchedules = (examId: string | null) =>
  useQuery({
    queryKey: ["teacher", "examSchedules", examId],
    queryFn: () => TeacherApi.getExamSchedules(examId!),
    enabled: !!examId,
  });

export const useSubjects = (gradeLevelId?: string) =>
  useQuery({
    queryKey: ["teacher", "subjects", gradeLevelId],
    queryFn: () => TeacherApi.getSubjects(gradeLevelId),
  });

export const useRooms = (schoolId: string | null) =>
  useQuery({
    queryKey: ["teacher", "rooms", schoolId],
    queryFn: () => TeacherApi.getRooms(schoolId!),
    enabled: !!schoolId,
  });
