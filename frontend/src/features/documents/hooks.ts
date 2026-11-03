import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "@/store/authStore";
import { DocumentsApi } from "./api";

export const useDeleteDocument = () => {
  const client = useQueryClient();
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  return useMutation({
    mutationFn: (documentId: string) => DocumentsApi.delete(schoolId!, documentId),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: ["student"] });
      client.invalidateQueries({ queryKey: ["teacher"] });
      client.invalidateQueries({ queryKey: ["grade-curriculum"] });
    },
  });
};
