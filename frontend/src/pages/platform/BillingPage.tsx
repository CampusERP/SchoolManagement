import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  BadgeDollarSign,
  CreditCard,
  Download,
  Eye,
  Pencil,
  Plus,
  Search,
  Trash2,
  Undo2,
} from "lucide-react";
import {
  Input as AntInput,
  InputNumber,
  Modal,
  Select,
  Switch,
  Table,
  Tabs,
  type TableProps,
} from "antd";
import dayjs from "dayjs";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import StatusBadge from "@/components/molecules/StatusBadge";
import Button from "@/components/atoms/Button";
import Input from "@/components/atoms/Input";
import {
  useCancelSubscription,
  useGenerateInvoice,
  usePlatformInvoices,
  usePlatformSubscriptions,
  usePlans,
  useReactivateSubscription,
  useRecordPayment,
  useSavePlan,
  useSuspendSubscription,
} from "@/features/billing/hooks";
import type {
  InvoiceList,
  PaymentMethodValue,
  SavePlanPayload,
  SubscriptionPlan,
  SubscriptionStatus,
} from "@/features/billing/api";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/errors";
import { t } from "@/i18n";
import {
  localizedBenefitDescriptions,
  localizedPlanDescription,
} from "@/features/billing/display";
import { downloadInvoicePdf, openInvoicePdf } from "@/features/billing/pdf";

const money = (value: number) =>
  `$${Number(value || 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;

const PAYMENT_METHODS: { value: PaymentMethodValue; label: string }[] = [
  { value: "BankTransfer", label: t("Bank transfer") },
  { value: "CreditCard", label: t("Credit card") },
  { value: "Cash", label: t("Cash") },
  { value: "Online", label: t("Online") },
];

const emptyPlanForm: SavePlanPayload = {
  name: "",
  descriptionEn: "",
  descriptionAr: "",
  benefits: [],
  priceMonthly: 0,
  maxStudents: 100,
  maxTeachers: 10,
  hasParentPortal: false,
  hasExamModule: false,
  hasAnalytics: false,
  isActive: true,
};

export default function BillingPage() {
  const navigate = useNavigate();
  const [tab, setTab] = useState("subscriptions");

  const [subscriptionPage, setSubscriptionPage] = useState(1);
  const [subscriptionSearch, setSubscriptionSearch] = useState("");
  const debouncedSubscriptionSearch = useDebouncedValue(subscriptionSearch);

  const [invoicePage, setInvoicePage] = useState(1);
  const [invoiceSearch, setInvoiceSearch] = useState("");
  const [invoiceStatus, setInvoiceStatus] = useState<string | undefined>();
  const debouncedInvoiceSearch = useDebouncedValue(invoiceSearch);

  const subscriptions = usePlatformSubscriptions({
    page: subscriptionPage,
    pageSize: 20,
    search: debouncedSubscriptionSearch || undefined,
  });
  const invoices = usePlatformInvoices({
    page: invoicePage,
    pageSize: 20,
    search: debouncedInvoiceSearch || undefined,
    status: invoiceStatus,
  });
  const plans = usePlans();

  const suspend = useSuspendSubscription();
  const reactivate = useReactivateSubscription();
  const cancel = useCancelSubscription();
  const generateInvoice = useGenerateInvoice();
  const recordPayment = useRecordPayment();
  const savePlan = useSavePlan();

  // ── Record payment ────────────────────────────────────────────────
  const [paymentInvoice, setPaymentInvoice] = useState<InvoiceList | null>(null);
  const [paymentAmount, setPaymentAmount] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethodValue>("BankTransfer");
  const [paymentReference, setPaymentReference] = useState("");

  const openPayment = (invoice: InvoiceList) => {
    setPaymentInvoice(invoice);
    setPaymentAmount(Number(invoice.balanceDue.toFixed(2)));
    setPaymentMethod("BankTransfer");
    setPaymentReference("");
  };

  const confirmPayment = async () => {
    if (!paymentInvoice) return;
    try {
      await recordPayment.mutateAsync({
        invoiceId: paymentInvoice.id,
        data: {
          amount: paymentAmount,
          method: paymentMethod,
          reference: paymentReference || undefined,
        },
      });
      toast.success(
        t("Payment recorded. The subscription period starts or renews automatically when the invoice is fully paid.")
      );
      setPaymentInvoice(null);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to record payment.")));
    }
  };

  // ── Generate invoice ──────────────────────────────────────────────
  const [invoiceSchool, setInvoiceSchool] = useState<SubscriptionStatus | null>(null);
  const [invoiceAmount, setInvoiceAmount] = useState(0);
  const [invoiceNotes, setInvoiceNotes] = useState("");

  const openInvoice = (subscription: SubscriptionStatus) => {
    setInvoiceSchool(subscription);
    setInvoiceAmount(subscription.priceMonthly);
    setInvoiceNotes("");
  };

  const confirmInvoice = async () => {
    if (!invoiceSchool) return;
    try {
      await generateInvoice.mutateAsync({
        schoolId: invoiceSchool.schoolId,
        data: {
          amount: invoiceAmount,
          notes: invoiceNotes || undefined,
        },
      });
      toast.success(t("Invoice created."));
      setInvoiceSchool(null);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to create invoice.")));
    }
  };

  const viewInvoicePdf = async (invoice: InvoiceList) => {
    try {
      await openInvoicePdf(invoice.id);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to open invoice PDF.")));
    }
  };

  const saveInvoicePdf = async (invoice: InvoiceList) => {
    try {
      await downloadInvoicePdf(invoice.id);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to download invoice PDF.")));
    }
  };

  // ── Plan editor ───────────────────────────────────────────────────
  const [planModalOpen, setPlanModalOpen] = useState(false);
  const [editingPlanId, setEditingPlanId] = useState<string | undefined>();
  const [planForm, setPlanForm] = useState<SavePlanPayload>(emptyPlanForm);

  const openPlan = (plan?: SubscriptionPlan) => {
    setEditingPlanId(plan?.id);
    setPlanForm(
      plan
        ? {
            name: plan.name,
            descriptionEn: plan.descriptionEn ?? "",
            descriptionAr: plan.descriptionAr ?? "",
            benefits: (plan.benefits ?? []).map((benefit) => ({
              descriptionEn: benefit.descriptionEn ?? "",
              descriptionAr: benefit.descriptionAr ?? "",
            })),
            priceMonthly: plan.priceMonthly,
            maxStudents: plan.maxStudents,
            maxTeachers: plan.maxTeachers,
            hasParentPortal: plan.hasParentPortal,
            hasExamModule: plan.hasExamModule,
            hasAnalytics: plan.hasAnalytics,
            isActive: plan.isActive,
          }
        : emptyPlanForm
    );
    setPlanModalOpen(true);
  };

  const updatePlanBenefit = (
    index: number,
    field: "descriptionEn" | "descriptionAr",
    value: string
  ) => {
    setPlanForm((current) => ({
      ...current,
      benefits: current.benefits.map((benefit, benefitIndex) =>
        benefitIndex === index ? { ...benefit, [field]: value } : benefit
      ),
    }));
  };

  const addPlanBenefit = () => {
    setPlanForm((current) => ({
      ...current,
      benefits: [...current.benefits, { descriptionEn: "", descriptionAr: "" }],
    }));
  };

  const removePlanBenefit = (index: number) => {
    setPlanForm((current) => ({
      ...current,
      benefits: current.benefits.filter((_, benefitIndex) => benefitIndex !== index),
    }));
  };

  const confirmPlan = async () => {
    const data: SavePlanPayload = {
      ...planForm,
      descriptionEn: planForm.descriptionEn?.trim() || undefined,
      descriptionAr: planForm.descriptionAr?.trim() || undefined,
      benefits: planForm.benefits
        .map((benefit) => ({
          descriptionEn: benefit.descriptionEn?.trim() || undefined,
          descriptionAr: benefit.descriptionAr?.trim() || undefined,
        }))
        .filter((benefit) => benefit.descriptionEn || benefit.descriptionAr),
    };

    try {
      await savePlan.mutateAsync({ planId: editingPlanId, data });
      toast.success(editingPlanId ? t("Plan updated.") : t("Plan created."));
      setPlanModalOpen(false);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to save plan.")));
    }
  };

  const handleSubscriptionStatus = async (
    subscription: SubscriptionStatus,
    action: "suspend" | "reactivate" | "cancel"
  ) => {
    try {
      if (action === "suspend") await suspend.mutateAsync(subscription.schoolId);
      if (action === "reactivate") await reactivate.mutateAsync(subscription.schoolId);
      if (action === "cancel") await cancel.mutateAsync(subscription.schoolId);
      toast.success(t("Subscription updated."));
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to update subscription.")));
    }
  };

  const subscriptionColumns: TableProps<SubscriptionStatus>["columns"] = useMemo(
    () => [
      {
        title: t("School"),
        dataIndex: "schoolName",
        key: "schoolName",
        render: (name: string, record) => (
          <button
            className="font-medium text-[var(--color-primary)] hover:underline"
            onClick={() => navigate(`/platform/schools/${record.schoolId}`)}
          >
            {name}
          </button>
        ),
      },
      { title: t("Plan"), dataIndex: "planName", key: "planName" },
      {
        title: t("Status"),
        dataIndex: "status",
        key: "status",
        render: (status: string) => <StatusBadge status={status} />,
      },
      {
        title: t("Period"),
        key: "period",
        render: (_: unknown, record) =>
          `${dayjs(record.startDate).format("MMM D, YYYY")} – ${dayjs(record.endDate).format("MMM D, YYYY")}`,
      },
      {
        title: t("Days left"),
        dataIndex: "daysRemaining",
        key: "daysRemaining",
        align: "center" as const,
        render: (days: number, record) => (
          <span
            className={
              record.status === "Active" && days <= 7
                ? "font-semibold text-[var(--color-danger)]"
                : "text-[var(--color-text-secondary)]"
            }
          >
            {record.status === "Active" ? days : "—"}
          </span>
        ),
      },
      {
        title: t("Outstanding"),
        dataIndex: "outstandingBalance",
        key: "outstandingBalance",
        align: "right" as const,
        render: (value: number) => money(value),
      },
      {
        title: t("Actions"),
        key: "actions",
        align: "center" as const,
        render: (_: unknown, record) => (
          <div className="flex flex-wrap items-center justify-center gap-2">
            <Button
              variant="ghost"
              size="small"
              className="p-0"
              onClick={() => openInvoice(record)}
            >
              {t("New invoice")}
            </Button>
            {record.status === "Suspended" ? (
              <Button
                variant="ghost"
                size="small"
                className="p-0"
                onClick={() => handleSubscriptionStatus(record, "reactivate")}
              >
                {t("Reactivate")}
              </Button>
            ) : record.status === "Active" ? (
              <Button
                variant="ghost"
                size="small"
                className="p-0"
                onClick={() => handleSubscriptionStatus(record, "suspend")}
              >
                {t("Suspend")}
              </Button>
            ) : null}
            <Button
              variant="ghost"
              size="small"
              className="p-0"
              onClick={() => navigate(`/platform/schools/${record.schoolId}`)}
            >
              {t("Manage")}
            </Button>
          </div>
        ),
      },
    ],
    // openInvoice/handleSubscriptionStatus are intentionally recreated per render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [navigate]
  );

  const invoiceColumns: TableProps<InvoiceList>["columns"] = [
    {
      title: t("School"),
      dataIndex: "schoolName",
      key: "schoolName",
      render: (name: string, record) => (
        <button
          className="font-medium text-[var(--color-primary)] hover:underline"
          onClick={() => navigate(`/platform/schools/${record.schoolId}`)}
        >
          {name}
        </button>
      ),
    },
    {
      title: t("Amount"),
      dataIndex: "amount",
      key: "amount",
      align: "right",
      render: (value: number) => money(value),
    },
    {
      title: t("Paid"),
      dataIndex: "paidAmount",
      key: "paidAmount",
      align: "right",
      render: (value: number) => money(value),
    },
    {
      title: t("Balance"),
      dataIndex: "balanceDue",
      key: "balanceDue",
      align: "right",
      render: (value: number) => money(value),
    },
    {
      title: t("Due date"),
      dataIndex: "dueDate",
      key: "dueDate",
      render: (date: string) => dayjs(date).format("MMM D, YYYY"),
    },
    {
      title: t("Status"),
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      render: (_: unknown, record) => (
        <div className="flex flex-wrap items-center justify-center gap-3">
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => viewInvoicePdf(record)}>
            <span className="inline-flex items-center gap-1"><Eye className="h-4 w-4" />{t("View PDF")}</span>
          </Button>
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => saveInvoicePdf(record)}>
            <span className="inline-flex items-center gap-1"><Download className="h-4 w-4" />{t("Download PDF")}</span>
          </Button>
          {record.status !== "Paid" && record.status !== "Cancelled" ? (
            <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openPayment(record)}>
              {t("Record payment")}
            </Button>
          ) : (
            <span className="text-xs text-[var(--color-text-muted)]">{t("Settled")}</span>
          )}
        </div>
      ),
    },
  ];

  const planColumns: TableProps<SubscriptionPlan>["columns"] = [
    {
      title: t("Plan"),
      dataIndex: "name",
      key: "name",
      render: (name: string, record) => (
        <div>
          <p className="font-medium text-[var(--color-text-primary)]">{name}</p>
          <p className="text-xs text-[var(--color-text-muted)]">{localizedPlanDescription(record)}</p>
        </div>
      ),
    },
    {
      title: t("Price / month"),
      dataIndex: "priceMonthly",
      key: "priceMonthly",
      align: "right",
      render: (value: number) => money(value),
    },
    {
      title: t("Limits"),
      key: "limits",
      render: (_: unknown, record) =>
        `${record.maxStudents.toLocaleString()} ${t("students")} · ${record.maxTeachers.toLocaleString()} ${t("teachers")}`,
    },
    {
      title: t("Features"),
      key: "features",
      render: (_: unknown, record) =>
        [
          record.hasParentPortal ? t("Parent portal") : null,
          record.hasExamModule ? t("Exams") : null,
          record.hasAnalytics ? t("Analytics") : null,
        ]
          .filter(Boolean)
          .join(" · ") || "—",
    },
    {
      title: t("Status"),
      dataIndex: "isActive",
      key: "isActive",
      render: (active: boolean) => <StatusBadge status={active ? "Active" : "Inactive"} />,
    },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      render: (_: unknown, record) => (
        <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openPlan(record)}>
          {t("Edit")}
        </Button>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Billing & Subscriptions")}
      subtitle={t("Manage school plans, subscriptions, invoices, and payments.")}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={() => openPlan()}>
          {t("New plan")}
        </Button>
      }
    >
      <Tabs
        activeKey={tab}
        onChange={setTab}
        items={[
          {
            key: "subscriptions",
            label: (
              <span className="inline-flex items-center gap-2">
                <BadgeDollarSign className="h-4 w-4" />
                {t("Subscriptions")}
              </span>
            ),
            children: (
              <div className="space-y-4">
                <Input
                  placeholder={t("Search by school, plan, or status...")}
                  prefix={<Search className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  value={subscriptionSearch}
                  onChange={(e) => {
                    setSubscriptionSearch(e.target.value);
                    setSubscriptionPage(1);
                  }}
                  allowClear
                  className="w-full sm:max-w-md"
                />
                <Table<SubscriptionStatus>
                  rowKey="subscriptionId"
                  columns={subscriptionColumns}
                  dataSource={subscriptions.data?.items || []}
                  loading={subscriptions.isLoading}
                  scroll={{ x: "max-content" }}
                  pagination={{
                    current: subscriptionPage,
                    pageSize: 20,
                    total: subscriptions.data?.totalCount || 0,
                    onChange: setSubscriptionPage,
                    showSizeChanger: false,
                  }}
                />
              </div>
            ),
          },
          {
            key: "invoices",
            label: (
              <span className="inline-flex items-center gap-2">
                <CreditCard className="h-4 w-4" />
                {t("Invoices")}
              </span>
            ),
            children: (
              <div className="space-y-4">
                <div className="flex flex-col gap-3 sm:flex-row">
                  <Input
                    placeholder={t("Search by school or invoice id...")}
                    prefix={<Search className="h-4 w-4 text-[var(--color-text-muted)]" />}
                    value={invoiceSearch}
                    onChange={(e) => {
                      setInvoiceSearch(e.target.value);
                      setInvoicePage(1);
                    }}
                    allowClear
                    className="w-full sm:max-w-md"
                  />
                  <Select
                    allowClear
                    placeholder={t("All statuses")}
                    value={invoiceStatus}
                    onChange={(value) => {
                      setInvoiceStatus(value);
                      setInvoicePage(1);
                    }}
                    className="w-full sm:w-48"
                    options={["Issued", "Paid", "Overdue", "Cancelled", "Draft"].map((status) => ({
                      value: status,
                      label: t(status),
                    }))}
                  />
                </div>
                <Table<InvoiceList>
                  rowKey="id"
                  columns={invoiceColumns}
                  dataSource={invoices.data?.items || []}
                  loading={invoices.isLoading}
                  scroll={{ x: "max-content" }}
                  pagination={{
                    current: invoicePage,
                    pageSize: 20,
                    total: invoices.data?.totalCount || 0,
                    onChange: setInvoicePage,
                    showSizeChanger: false,
                  }}
                />
              </div>
            ),
          },
          {
            key: "plans",
            label: (
              <span className="inline-flex items-center gap-2">
                <Undo2 className="h-4 w-4" />
                {t("Plans")}
              </span>
            ),
            children: (
              <Table<SubscriptionPlan>
                rowKey="id"
                columns={planColumns}
                dataSource={plans.data || []}
                loading={plans.isLoading}
                scroll={{ x: "max-content" }}
                pagination={false}
              />
            ),
          },
        ]}
      />

      {/* Record payment */}
      <Modal
        title={t("Record payment")}
        open={!!paymentInvoice}
        onCancel={() => setPaymentInvoice(null)}
        onOk={confirmPayment}
        confirmLoading={recordPayment.isPending}
        okText={t("Record payment")}
        destroyOnClose
      >
        {paymentInvoice && (
          <div className="space-y-4 py-2">
            <p className="text-sm text-[var(--color-text-secondary)]">
              {t("Invoice for {school}", { school: paymentInvoice.schoolName })} ·{" "}
              {t("Balance")}: <strong>{money(paymentInvoice.balanceDue)}</strong>
            </p>
            <div>
              <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
                {t("Amount")}
              </label>
              <InputNumber
                min={0.01}
                max={paymentInvoice.balanceDue}
                step={0.01}
                value={paymentAmount}
                onChange={(value) => setPaymentAmount(Number(value ?? 0))}
                className="w-full"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
                {t("Payment method")}
              </label>
              <Select
                value={paymentMethod}
                onChange={setPaymentMethod}
                className="w-full"
                options={PAYMENT_METHODS}
              />
            </div>
            <div>
              <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
                {t("Reference (optional)")}
              </label>
              <Input
                value={paymentReference}
                onChange={(e) => setPaymentReference(e.target.value)}
                placeholder={t("Bank transfer reference, receipt number...")}
              />
            </div>
            <p className="text-xs text-[var(--color-text-muted)]">
              {t("When the invoice is fully paid, the school's subscription period starts or extends automatically.")}
            </p>
          </div>
        )}
      </Modal>

      {/* Generate invoice */}
      <Modal
        title={t("Create invoice")}
        open={!!invoiceSchool}
        onCancel={() => setInvoiceSchool(null)}
        onOk={confirmInvoice}
        confirmLoading={generateInvoice.isPending}
        okText={t("Create invoice")}
        destroyOnClose
      >
        {invoiceSchool && (
          <div className="space-y-4 py-2">
            <p className="text-sm text-[var(--color-text-secondary)]">
              {t("Invoice for {school}", { school: invoiceSchool.schoolName })} ·{" "}
              {invoiceSchool.planName}
            </p>
            <div>
              <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
                {t("Amount")}
              </label>
              <InputNumber
                min={0}
                step={0.01}
                value={invoiceAmount}
                onChange={(value) => setInvoiceAmount(Number(value ?? 0))}
                className="w-full"
              />
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                {t("Use 0 to bill the plan's current monthly price.")}
              </p>
            </div>
            <div>
              <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
                {t("Notes (optional)")}
              </label>
              <AntInput.TextArea
                value={invoiceNotes}
                onChange={(e) => setInvoiceNotes(e.target.value)}
                rows={3}
                placeholder={t("Renewal for next month, custom arrangement...")}
              />
            </div>
          </div>
        )}
      </Modal>

      {/* Plan editor */}
      <Modal
        title={editingPlanId ? t("Edit plan") : t("New plan")}
        open={planModalOpen}
        onCancel={() => setPlanModalOpen(false)}
        onOk={confirmPlan}
        confirmLoading={savePlan.isPending}
        okText={editingPlanId ? t("Save changes") : t("Create plan")}
        width={640}
        destroyOnClose
      >
        <div className="grid gap-4 py-2 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Name")}
            </label>
            <Input
              value={planForm.name}
              onChange={(e) => setPlanForm({ ...planForm, name: e.target.value })}
              placeholder={t("e.g. Standard")}
            />
          </div>
          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("English description")}
            </label>
            <AntInput.TextArea
              value={planForm.descriptionEn}
              onChange={(e) => setPlanForm({ ...planForm, descriptionEn: e.target.value })}
              rows={2}
              maxLength={500}
            />
          </div>
          <div className="sm:col-span-2">
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Arabic description")}
            </label>
            <AntInput.TextArea
              value={planForm.descriptionAr}
              onChange={(e) => setPlanForm({ ...planForm, descriptionAr: e.target.value })}
              rows={2}
              maxLength={500}
              dir="rtl"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Price per month")}
            </label>
            <InputNumber
              min={0}
              step={1}
              value={planForm.priceMonthly}
              onChange={(value) => setPlanForm({ ...planForm, priceMonthly: Number(value ?? 0) })}
              className="w-full"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Status")}
            </label>
            <div className="flex h-8 items-center gap-2">
              <Switch
                checked={planForm.isActive}
                onChange={(checked) => setPlanForm({ ...planForm, isActive: checked })}
              />
              <span className="text-sm text-[var(--color-text-secondary)]">
                {planForm.isActive ? t("Active") : t("Inactive")}
              </span>
            </div>
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Max students")}
            </label>
            <InputNumber
              min={1}
              value={planForm.maxStudents}
              onChange={(value) => setPlanForm({ ...planForm, maxStudents: Number(value ?? 1) })}
              className="w-full"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Max teachers")}
            </label>
            <InputNumber
              min={1}
              value={planForm.maxTeachers}
              onChange={(value) => setPlanForm({ ...planForm, maxTeachers: Number(value ?? 1) })}
              className="w-full"
            />
          </div>
          <div className="flex flex-col gap-2 sm:col-span-2">
            {[
              { key: "hasParentPortal" as const, label: t("Parent portal") },
              { key: "hasExamModule" as const, label: t("Exam & results module") },
              { key: "hasAnalytics" as const, label: t("Advanced analytics") },
            ].map((feature) => (
              <label key={feature.key} className="flex items-center justify-between gap-4">
                <span className="text-sm text-[var(--color-text-primary)]">{feature.label}</span>
                <Switch
                  checked={planForm[feature.key]}
                  onChange={(checked) => setPlanForm({ ...planForm, [feature.key]: checked })}
                />
              </label>
            ))}
          </div>
        </div>
      </Modal>
    </DashboardTemplate>
  );
}
