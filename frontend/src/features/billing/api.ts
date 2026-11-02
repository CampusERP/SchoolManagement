import api from "@/lib/axios";

export type SubscriptionStatusValue =
  | "PendingPayment"
  | "Active"
  | "Suspended"
  | "Cancelled"
  | "Expired";

export type InvoiceStatusValue =
  | "Draft"
  | "Issued"
  | "Paid"
  | "Overdue"
  | "Cancelled";

export type PaymentMethodValue =
  | "BankTransfer"
  | "CreditCard"
  | "Cash"
  | "Online";

export interface SubscriptionPlanBenefit {
  id: string;
  descriptionEn?: string | null;
  descriptionAr?: string | null;
  sortOrder: number;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  descriptionEn?: string | null;
  descriptionAr?: string | null;
  benefits: SubscriptionPlanBenefit[];
  priceMonthly: number;
  maxStudents: number;
  maxTeachers: number;
  hasParentPortal: boolean;
  hasExamModule: boolean;
  hasAnalytics: boolean;
  isActive: boolean;
}

export interface SubscriptionStatus {
  subscriptionId: string;
  schoolId: string;
  schoolName: string;
  planId: string;
  planName: string;
  planDescriptionEn?: string | null;
  planDescriptionAr?: string | null;
  planBenefits: SubscriptionPlanBenefit[];
  priceMonthly: number;
  status: SubscriptionStatusValue;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  maxStudents: number;
  maxTeachers: number;
  hasParentPortal: boolean;
  hasExamModule: boolean;
  hasAnalytics: boolean;
  daysRemaining: number;
  isActive: boolean;
  outstandingBalance: number;
}

export interface Payment {
  id: string;
  amount: number;
  method: string;
  reference?: string | null;
  paidAtUtc: string;
}

export interface InvoiceList {
  id: string;
  subscriptionId: string;
  schoolId: string;
  schoolName: string;
  amount: number;
  paidAmount: number;
  balanceDue: number;
  dueDate: string;
  status: InvoiceStatusValue;
  createdAtUtc: string;
}

export interface InvoiceDetail extends InvoiceList {
  planName: string;
  notes?: string | null;
  payments: Payment[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export interface SubscriptionListParams {
  page?: number;
  pageSize?: number;
  schoolId?: string;
  search?: string;
}

export interface InvoiceListParams {
  page?: number;
  pageSize?: number;
  schoolId?: string;
  search?: string;
  status?: string;
}

export interface AssignSubscriptionPayload {
  subscriptionPlanId: string;
  startDate?: string;
  endDate?: string;
}

export interface GenerateInvoicePayload {
  amount: number;
  dueDate?: string;
  notes?: string;
}

export interface RecordPaymentPayload {
  amount: number;
  method: PaymentMethodValue;
  reference?: string;
}

export interface SavePlanBenefitPayload {
  descriptionEn?: string;
  descriptionAr?: string;
}

export interface SavePlanPayload {
  name: string;
  descriptionEn?: string;
  descriptionAr?: string;
  benefits: SavePlanBenefitPayload[];
  priceMonthly: number;
  maxStudents: number;
  maxTeachers: number;
  hasParentPortal: boolean;
  hasExamModule: boolean;
  hasAnalytics: boolean;
  isActive?: boolean;
}

export const BillingApi = {
  // Plans
  getPublicPlans: () =>
    api.get<SubscriptionPlan[]>("/billing/plans/public").then((r) => r.data),
  getPlans: () => api.get<SubscriptionPlan[]>("/billing/plans").then((r) => r.data),
  createPlan: (data: SavePlanPayload) =>
    api.post<{ id: string }>("/billing/plans", data).then((r) => r.data),
  updatePlan: (planId: string, data: SavePlanPayload) =>
    api.put(`/billing/plans/${planId}`, data).then((r) => r.data),

  // Subscriptions
  listSubscriptions: (params: SubscriptionListParams = {}) =>
    api
      .get<PagedResult<SubscriptionStatus>>("/billing/subscriptions", { params })
      .then((r) => r.data),
  getSchoolSubscription: (schoolId: string) =>
    api
      .get<SubscriptionStatus>(`/billing/schools/${schoolId}/subscription`)
      .then((r) => r.data),
  assignSubscription: (schoolId: string, data: AssignSubscriptionPayload) =>
    api
      .put<{ id: string }>(`/billing/schools/${schoolId}/subscription`, data)
      .then((r) => r.data),
  upgradeSubscription: (schoolId: string, newPlanId: string) =>
    api
      .patch(`/billing/schools/${schoolId}/subscription/upgrade`, { newPlanId })
      .then((r) => r.data),
  suspendSubscription: (schoolId: string) =>
    api.patch(`/billing/schools/${schoolId}/subscription/suspend`).then((r) => r.data),
  reactivateSubscription: (schoolId: string) =>
    api.patch(`/billing/schools/${schoolId}/subscription/reactivate`).then((r) => r.data),
  cancelSubscription: (schoolId: string) =>
    api.delete(`/billing/schools/${schoolId}/subscription`).then((r) => r.data),

  // Invoices & payments
  listInvoices: (params: InvoiceListParams = {}) =>
    api
      .get<PagedResult<InvoiceList>>("/billing/invoices", { params })
      .then((r) => r.data),
  getSchoolInvoices: (schoolId: string, params: InvoiceListParams = {}) =>
    api
      .get<PagedResult<InvoiceList>>(`/billing/schools/${schoolId}/invoices`, { params })
      .then((r) => r.data),
  generateInvoice: (schoolId: string, data: GenerateInvoicePayload) =>
    api
      .post<{ id: string }>(`/billing/schools/${schoolId}/invoices`, data)
      .then((r) => r.data),
  getInvoice: (invoiceId: string) =>
    api.get<InvoiceDetail>(`/billing/invoices/${invoiceId}`).then((r) => r.data),
  getInvoicePdf: (invoiceId: string) =>
    api.get<Blob>(`/billing/invoices/${invoiceId}/pdf`, { responseType: "blob" }).then((r) => r.data),
  recordPayment: (invoiceId: string, data: RecordPaymentPayload) =>
    api
      .post<{ id: string }>(`/billing/invoices/${invoiceId}/payments`, data)
      .then((r) => r.data),
};
