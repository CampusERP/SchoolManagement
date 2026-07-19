import api from "@/lib/axios";
import type { PagedResult } from "@/types/api.types";
import type {
  ParentListDto,
  ParentDetailDto,
  CreateParentCommand,
  UpdateParentCommand,
} from "@/types/parent.types";

export interface GetParentsParams {
  schoolId: string;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export const ParentsApi = {
  getParents: (params: GetParentsParams) =>
    api
      .get<PagedResult<ParentListDto>>("/people/parents", {
        params: {
          schoolId: params.schoolId,
          searchTerm: params.searchTerm || undefined,
          "Pagination.Page": params.page ?? 1,
          "Pagination.PageSize": params.pageSize ?? 20,
        },
      })
      .then((r) => r.data),

  getParentById: (parentId: string, schoolId: string) =>
    api
      .get<ParentDetailDto>(`/people/parents/${parentId}`, {
        params: { schoolId },
      })
      .then((r) => r.data),

  createParent: (data: CreateParentCommand) =>
    api.post<{ id: string }>("/people/parents", data).then((r) => r.data),

  updateParent: (parentId: string, data: UpdateParentCommand) =>
    api.put(`/people/parents/${parentId}`, data).then((r) => r.data),
};
