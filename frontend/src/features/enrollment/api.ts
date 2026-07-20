import api from "@/lib/axios";
import type {
  EnrollStudentCommand,
  AssignTeacherCommand,
} from "@/types/enrollment.types";

export const EnrollmentApi = {
  enrollStudent: (data: EnrollStudentCommand) =>
    api.post<{ id: string }>("/enrollment/students", data).then((r) => r.data),

  assignTeacher: (data: AssignTeacherCommand) =>
    api.post<{ id: string }>("/enrollment/teachers", data).then((r) => r.data),
};
