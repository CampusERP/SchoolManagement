import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  SchoolRegistrationRequestsApi,
  type SchoolRegistrationRequestsParams,
} from "./api";

export const useSchoolRegistrationRequests = (params: SchoolRegistrationRequestsParams = {}) =>
  useQuery({
    queryKey: ["school-registration-requests", params],
    queryFn: () => SchoolRegistrationRequestsApi.list(params),
  });

export const useSchoolRegistrationRequest = (id?: string) =>
  useQuery({
    queryKey: ["school-registration-requests", id],
    queryFn: () => SchoolRegistrationRequestsApi.getById(id!),
    enabled: !!id,
  });

export const useCreateSchoolRegistrationRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: SchoolRegistrationRequestsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["school-registration-requests"] });
    },
  });
};

export const useUpdateSchoolRegistrationRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof SchoolRegistrationRequestsApi.update>[1] }) =>
      SchoolRegistrationRequestsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["school-registration-requests"] });
    },
  });
};

export const useDeleteSchoolRegistrationRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: SchoolRegistrationRequestsApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["school-registration-requests"] });
    },
  });
};

export const useApproveSchoolRegistrationRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
      SchoolRegistrationRequestsApi.approve(id, notes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["school-registration-requests"] });
      queryClient.invalidateQueries({ queryKey: ["schools"] });
    },
  });
};

export const useRejectSchoolRegistrationRequest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      SchoolRegistrationRequestsApi.reject(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["school-registration-requests"] });
    },
  });
};
