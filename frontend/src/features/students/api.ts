import api from "@/lib/axios";
import type { PagedResult } from "@/types/api.types";
import type {
  StudentListDto,
  StudentDetailDto,
  CreateStudentCommand,
  UpdateStudentCommand,
} from "@/types/student.types";

export interface GetStudentsParams {
  schoolId: string;
  searchTerm?: string;
  gradeLevelId?: string;
  classRoomId?: string;
  page?: number;
  pageSize?: number;
}

export const StudentsApi = {
  getStudents: (params: GetStudentsParams) =>
    api
      .get<PagedResult<StudentListDto>>("/people/students", {
        params: {
          schoolId: params.schoolId,
          searchTerm: params.searchTerm || undefined,
          gradeLevelId: params.gradeLevelId || undefined,
          classRoomId: params.classRoomId || undefined,
          "Pagination.Page": params.page ?? 1,
          "Pagination.PageSize": params.pageSize ?? 20,
        },
      })
      .then((r) => r.data),

  getStudentById: (studentId: string, schoolId: string) =>
    api
      .get<StudentDetailDto>(`/people/students/${studentId}`, {
        params: { schoolId },
      })
      .then((r) => r.data),

  createStudent: (data: CreateStudentCommand) =>
    api.post<{ id: string }>("/people/students", data).then((r) => r.data),

  updateStudent: (studentId: string, data: UpdateStudentCommand) =>
    api.put(`/people/students/${studentId}`, data).then((r) => r.data),

  deleteStudent: (studentId: string, schoolId: string) =>
    api.delete(`/people/students/${studentId}`, { params: { schoolId } }).then((r) => r.data),

  linkGuardian: (data: { studentId: string; parentId: string; relationshipType: string; isPrimaryContact: boolean; canViewGrades: boolean; canViewBilling: boolean; schoolId: string }) =>
    api.post(`/people/students/${data.studentId}/guardians`, data).then((r) => r.data),
};
