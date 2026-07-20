import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { ParentsApi, type GetParentsParams } from "./api";
import type {
  CreateParentCommand,
  UpdateParentCommand,
} from "@/types/parent.types";

export const useParents = (params: Omit<GetParentsParams, "schoolId">) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["parents", schoolId, params],
    queryFn: () => ParentsApi.getParents({ schoolId: schoolId!, ...params }),
    enabled: !!schoolId,
  });
};

export const useParent = (parentId: string | undefined) => {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useQuery({
    queryKey: ["parent", schoolId, parentId],
    queryFn: () => ParentsApi.getParentById(parentId!, schoolId!),
    enabled: !!schoolId && !!parentId,
  });
};

export const useCreateParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateParentCommand) => ParentsApi.createParent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["parents"] });
    },
  });
};

export const useUpdateParent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      parentId,
      data,
    }: {
      parentId: string;
      data: UpdateParentCommand;
    }) => ParentsApi.updateParent(parentId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["parents"] });
      queryClient.invalidateQueries({ queryKey: ["parent"] });
    },
  });
};
