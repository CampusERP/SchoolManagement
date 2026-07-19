import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { EducationStagesApi } from "./api";
import type {
  CreateEducationStageCommand,
  UpdateEducationStageCommand,
} from "@/types/academic.types";

export const useEducationStages = () => {
  return useQuery({
    queryKey: ["education-stages"],
    queryFn: () => EducationStagesApi.getAll(),
  });
};

export const useCreateEducationStage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateEducationStageCommand) =>
      EducationStagesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["education-stages"] });
    },
  });
};

export const useUpdateEducationStage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateEducationStageCommand;
    }) => EducationStagesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["education-stages"] });
    },
  });
};

export const useDeleteEducationStage = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => EducationStagesApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["education-stages"] });
    },
  });
};
