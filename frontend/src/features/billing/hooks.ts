import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  BillingApi,
  type InvoiceListParams,
  type SubscriptionListParams,
} from "./api";

export const billingKeys = {
  publicPlans: ["billing", "plans", "public"] as const,
  plans: ["billing", "plans"] as const,
  subscriptions: (params: SubscriptionListParams) =>
    ["billing", "subscriptions", params] as const,
  subscription: (schoolId?: string) =>
    ["billing", "subscription", schoolId ?? "none"] as const,
  invoices: (params: InvoiceListParams) =>
    ["billing", "invoices", params] as const,
  schoolInvoices: (schoolId: string | undefined, params: InvoiceListParams) =>
    ["billing", "invoices", "school", schoolId ?? "none", params] as const,
};

export const usePublicPlans = () =>
  useQuery({
    queryKey: billingKeys.publicPlans,
    queryFn: BillingApi.getPublicPlans,
    staleTime: 5 * 60 * 1000,
  });

export const usePlans = (enabled = true) =>
  useQuery({
    queryKey: billingKeys.plans,
    queryFn: BillingApi.getPlans,
    enabled,
  });

export const usePlatformSubscriptions = (params: SubscriptionListParams = {}) =>
  useQuery({
    queryKey: billingKeys.subscriptions(params),
    queryFn: () => BillingApi.listSubscriptions(params),
  });

export const useSchoolSubscription = (schoolId?: string) =>
  useQuery({
    queryKey: billingKeys.subscription(schoolId),
    queryFn: () => BillingApi.getSchoolSubscription(schoolId!),
    enabled: !!schoolId,
    retry: false,
  });

export const usePlatformInvoices = (params: InvoiceListParams = {}) =>
  useQuery({
    queryKey: billingKeys.invoices(params),
    queryFn: () => BillingApi.listInvoices(params),
  });

/** Platform dashboard totals use all pages so they remain accurate beyond the first 100 rows. */
export const usePlatformBillingOverview = () =>
  useQuery({
    queryKey: ["billing", "overview"],
    queryFn: async () => {
      const [firstSubscriptions, firstInvoices] = await Promise.all([
        BillingApi.listSubscriptions({ page: 1, pageSize: 100 }),
        BillingApi.listInvoices({ page: 1, pageSize: 100 }),
      ]);
      const [subscriptionPages, invoicePages] = await Promise.all([
        Promise.all(Array.from({ length: Math.max(0, firstSubscriptions.totalPages - 1) }, (_, i) =>
          BillingApi.listSubscriptions({ page: i + 2, pageSize: 100 }))),
        Promise.all(Array.from({ length: Math.max(0, firstInvoices.totalPages - 1) }, (_, i) =>
          BillingApi.listInvoices({ page: i + 2, pageSize: 100 }))),
      ]);
      const subscriptions = [firstSubscriptions, ...subscriptionPages].flatMap((page) => page.items);
      const invoices = [firstInvoices, ...invoicePages].flatMap((page) => page.items);
      return {
        activeSubscriptions: subscriptions.filter((item) => item.status === "Active").length,
        awaitingPayment: subscriptions.filter((item) => item.status === "PendingPayment").length,
        outstandingBalance: invoices.reduce((sum, invoice) => sum + invoice.balanceDue, 0),
      };
    },
  });

export const useSchoolInvoices = (schoolId?: string, params: InvoiceListParams = {}) =>
  useQuery({
    queryKey: billingKeys.schoolInvoices(schoolId, params),
    queryFn: () => BillingApi.getSchoolInvoices(schoolId!, params),
    enabled: !!schoolId,
  });

const invalidateBilling = (queryClient: ReturnType<typeof useQueryClient>) => {
  queryClient.invalidateQueries({ queryKey: ["billing"] });
};

export const useAssignSubscription = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      schoolId,
      data,
    }: {
      schoolId: string;
      data: Parameters<typeof BillingApi.assignSubscription>[1];
    }) => BillingApi.assignSubscription(schoolId, data),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useUpgradeSubscription = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ schoolId, planId }: { schoolId: string; planId: string }) =>
      BillingApi.upgradeSubscription(schoolId, planId),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useSuspendSubscription = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (schoolId: string) => BillingApi.suspendSubscription(schoolId),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useReactivateSubscription = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (schoolId: string) => BillingApi.reactivateSubscription(schoolId),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useCancelSubscription = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (schoolId: string) => BillingApi.cancelSubscription(schoolId),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useGenerateInvoice = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      schoolId,
      data,
    }: {
      schoolId: string;
      data: Parameters<typeof BillingApi.generateInvoice>[1];
    }) => BillingApi.generateInvoice(schoolId, data),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useRecordPayment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      invoiceId,
      data,
    }: {
      invoiceId: string;
      data: Parameters<typeof BillingApi.recordPayment>[1];
    }) => BillingApi.recordPayment(invoiceId, data),
    onSuccess: () => invalidateBilling(queryClient),
  });
};

export const useSavePlan = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      planId,
      data,
    }: {
      planId?: string;
      data: Parameters<typeof BillingApi.createPlan>[0];
    }) =>
      planId ? BillingApi.updatePlan(planId, data) : BillingApi.createPlan(data),
    onSuccess: () => invalidateBilling(queryClient),
  });
};
