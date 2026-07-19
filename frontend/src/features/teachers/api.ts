import api from "@/lib/axios";
import type { PagedResult } from "@/types/api.types";
import type {
  TeacherListDto,
  TeacherDetailDto,
  CreateTeacherCommand,
  UpdateTeacherCommand,
} from "@/types/teacher.types";

export interface GetTeachersParams {
  schoolId: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export const TeachersApi = {
  getTeachers: (params: GetTeachersParams) =>
    api
      .get<PagedResult<TeacherListDto>>("/people/teachers", {
        params: {
          schoolId: params.schoolId,
          searchTerm: params.searchTerm || undefined,
          "Pagination.Page": params.page ?? 1,
          "Pagination.PageSize": params.pageSize ?? 20,
        },
      })
      .then((r) => r.data),

  getTeacherById: (teacherId: string, schoolId: string) =>
    api
      .get<TeacherDetailDto>(`/people/teachers/${teacherId}`, {
        params: { schoolId },
      })
      .then((r) => r.data),

  createTeacher: (data: CreateTeacherCommand) =>
    api.post<{ id: string }>("/people/teachers", data).then((r) => r.data),

  updateTeacher: (teacherId: string, data: UpdateTeacherCommand) =>
    api.put(`/people/teachers/${teacherId}`, data).then((r) => r.data),
};
