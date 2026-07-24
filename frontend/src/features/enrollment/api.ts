import api from "@/lib/axios";
import type {
  EnrollStudentCommand,
  EnrollTeacherCommand,
  AssignTeacherCommand,
  UpdateTeachingAssignmentCommand,
  DeleteTeachingAssignmentCommand,
} from "@/types/enrollment.types";

export const EnrollmentApi = {
  enrollStudent: (data: EnrollStudentCommand) =>
    api.post<{ id: string }>("/enrollment/students", data).then((r) => r.data),

  enrollTeacher: (data: EnrollTeacherCommand) =>
    api.post<{ id: string }>("/enrollment/teachers/enroll", data).then((r) => r.data),

  assignTeacher: (data: AssignTeacherCommand) =>
    api.post<{ id: string }>("/enrollment/teachers/assign", data).then((r) => r.data),

  updateTeachingAssignment: (data: UpdateTeachingAssignmentCommand) =>
    api.put<string>(`/enrollment/teachers/assignments/${data.teachingAssignmentId}`, data).then((r) => r.data),

  deleteTeachingAssignment: (data: DeleteTeachingAssignmentCommand) =>
    api.delete(`/enrollment/teachers/assignments/${data.teachingAssignmentId}`, { params: { schoolId: data.schoolId } }).then((r) => r.data),
};
