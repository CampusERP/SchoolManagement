import api from "@/lib/axios";
import type {
  EducationStageDto,
  CreateEducationStageCommand,
  UpdateEducationStageCommand,
} from "@/types/academic.types";

export const EducationStagesApi = {
  getAll: () =>
    api.get<EducationStageDto[]>("/education-stages").then((r) => r.data),

  create: (data: CreateEducationStageCommand) =>
    api
      .post<{ id: string }>("/education-stages", data)
      .then((r) => r.data),

  update: (id: string, data: UpdateEducationStageCommand) =>
    api.put(`/education-stages/${id}`, data).then((r) => r.data),

  remove: (id: string) =>
    api.delete(`/education-stages/${id}`).then((r) => r.data),
};
