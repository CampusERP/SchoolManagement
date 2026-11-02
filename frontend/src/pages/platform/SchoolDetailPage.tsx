import { useCallback, useEffect, useState, type ReactNode } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import {
  Building2,
  CalendarDays,
  CreditCard,
  Download,
  Eye,
  Hash,
  Mail,
  MapPin,
  PauseCircle,
  Pencil,
  Phone,
  PlayCircle,
  Plus,
  UserPlus,
  Trash2,
} from "lucide-react";
import { InputNumber, Modal, Select, Table, type TableProps } from "antd";
import dayjs from "dayjs";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import StatusBadge from "@/components/molecules/StatusBadge";
import Button from "@/components/atoms/Button";
import { Input, TextArea } from "@/components/atoms/Input";
import { SchoolsApi, type SchoolDetail } from "@/features/schools/api";
import {
  useAssignSubscription,
  useCancelSubscription,
  useGenerateInvoice,
  usePlans,
  useReactivateSubscription,
  useRecordPayment,
  useSchoolInvoices,
  useSchoolSubscription,
  useSuspendSubscription,
  useUpgradeSubscription,
} from "@/features/billing/hooks";
import type { InvoiceList, PaymentMethodValue } from "@/features/billing/api";
import { downloadInvoicePdf, openInvoicePdf } from "@/features/billing/pdf";
import { getApiErrorMessage } from "@/lib/errors";
import { t } from "@/i18n";

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

function InfoRow({
  icon,
  label,
  children,
}: {
  icon: ReactNode;
  label: string;
  children: ReactNode;
}) {
  return (
    <div className="flex items-start gap-3">
      <span className="mt-0.5 shrink-0 text-[var(--color-text-muted)]">{icon}</span>
      <dt className="w-24 shrink-0 text-sm text-[var(--color-text-secondary)] sm:w-32">
        {label}
      </dt>
      <dd className="min-w-0 flex-1 break-words text-sm font-medium text-[var(--color-text-primary)]">
        {children}
      </dd>
    </div>
  );
}

export default function SchoolDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [school, setSchool] = useState<SchoolDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [campusOpen, setCampusOpen] = useState(false);
  const [campusName, setCampusName] = useState("");
  const [campusAddress, setCampusAddress] = useState("");
  const [editingCampusId, setEditingCampusId] = useState<string | null>(null);
  const [campusSaving, setCampusSaving] = useState(false);

  const [statusConfirmOpen, setStatusConfirmOpen] = useState(false);
  const [statusSaving, setStatusSaving] = useState(false);

  // ── Billing ────────────────────────────────────────────────────────
  const subscription = useSchoolSubscription(id);
  const plans = usePlans();
  const [invoicePage, setInvoicePage] = useState(1);
  const invoices = useSchoolInvoices(id, { page: invoicePage, pageSize: 10 });

  const upgrade = useUpgradeSubscription();
  const assign = useAssignSubscription();
  const suspend = useSuspendSubscription();
  const reactivate = useReactivateSubscription();
  const cancel = useCancelSubscription();
  const generateInvoice = useGenerateInvoice();
  const recordPayment = useRecordPayment();

  const [planModalOpen, setPlanModalOpen] = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState<string | undefined>();

  const [invoiceModalOpen, setInvoiceModalOpen] = useState(false);
  const [invoiceAmount, setInvoiceAmount] = useState(0);
  const [invoiceNotes, setInvoiceNotes] = useState("");

  const [paymentInvoice, setPaymentInvoice] = useState<InvoiceList | null>(null);
  const [paymentAmount, setPaymentAmount] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethodValue>("BankTransfer");
  const [paymentReference, setPaymentReference] = useState("");

  const sub = subscription.data;
  const subLoading = subscription.isLoading;
  const hasSubscription = !!sub;

  const load = useCallback(
    async (silent = false) => {
      if (!id) return;
      if (!silent) setIsLoading(true);
      try {
        const data = await SchoolsApi.getById(id);
        setSchool(data);
      } catch (err) {
        toast.error(getApiErrorMessage(err, t("Failed to load school.")));
      } finally {
        if (!silent) setIsLoading(false);
      }
    },
    [id]
  );

  useEffect(() => {
    load();
  }, [load]);

  const isActive = school?.status === "Active";

  const handleAddCampus = async () => {
    if (!id) return;
    const name = campusName.trim();
    const address = campusAddress.trim();
    if (!name || !address) {
      toast.error(t("Campus name and address are required."));
      return;
    }
    setCampusSaving(true);
    try {
      if (editingCampusId) {
        await SchoolsApi.updateCampus(id, editingCampusId, { name, address });
        toast.success(t("Campus updated"));
      } else {
        await SchoolsApi.addCampus(id, { name, address });
        toast.success(t("Campus added"));
      }
      setCampusOpen(false);
      setEditingCampusId(null);
      setCampusName("");
      setCampusAddress("");
      await load(true);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to add campus.")));
    } finally {
      setCampusSaving(false);
    }
  };

  const confirmDeleteCampus = (campusId: string, campusName: string) => {
    if (!id) return;
    Modal.confirm({
      title: t("Delete campus"),
      content: t("Delete campus {name}?", { name: campusName }),
      okText: t("Delete"),
      okType: "danger",
      onOk: async () => {
        try {
          await SchoolsApi.deleteCampus(id, campusId);
          toast.success(t("Campus deleted"));
          await load(true);
        } catch (err) {
          toast.error(getApiErrorMessage(err, t("Failed to delete campus.")));
        }
      },
    });
  };

  const handleToggleStatus = async () => {
    if (!id || !school) return;
    setStatusSaving(true);
    try {
      if (isActive) await SchoolsApi.suspend(id);
      else await SchoolsApi.reactivate(id);
      toast.success(isActive ? t("School suspended") : t("School reactivated"));
      setStatusConfirmOpen(false);
      await Promise.all([
        load(true),
        queryClient.invalidateQueries({ queryKey: ["schools"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard", "platform"] }),
      ]);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to update school status.")));
    } finally {
      setStatusSaving(false);
    }
  };

  // ── Billing handlers ──────────────────────────────────────────────
  const openPlanModal = () => {
    setSelectedPlanId(
      sub && sub.status !== "Cancelled"
        ? sub.planId
        : plans.data?.find((p) => p.isActive)?.id
    );
    setPlanModalOpen(true);
  };

  const handleChangePlan = async () => {
    if (!id || !selectedPlanId) {
      toast.error(t("Select a plan."));
      return;
    }
    try {
      if (hasSubscription && sub?.status !== "Cancelled") {
        await upgrade.mutateAsync({ schoolId: id, planId: selectedPlanId });
        toast.success(t("Subscription plan updated."));
      } else {
        // A cancelled subscription cannot be revived through upgrade; assigning a
        // plan creates a fresh active subscription for the school.
        await assign.mutateAsync({ schoolId: id, data: { subscriptionPlanId: selectedPlanId } });
        toast.success(t("Subscription assigned."));
      }
      setPlanModalOpen(false);
      await subscription.refetch();
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to update subscription.")));
    }
  };

  const openInvoiceModal = () => {
    setInvoiceAmount(sub?.priceMonthly ?? 0);
    setInvoiceNotes("");
    setInvoiceModalOpen(true);
  };

  const handleGenerateInvoice = async () => {
    if (!id) return;
    try {
      await generateInvoice.mutateAsync({
        schoolId: id,
        data: { amount: invoiceAmount, notes: invoiceNotes || undefined },
      });
      toast.success(t("Invoice created."));
      setInvoiceModalOpen(false);
      await invoices.refetch();
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

  const openPayment = (invoice: InvoiceList) => {
    setPaymentInvoice(invoice);
    setPaymentAmount(Number(invoice.balanceDue.toFixed(2)));
    setPaymentMethod("BankTransfer");
    setPaymentReference("");
  };

  const handleRecordPayment = async () => {
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
      await Promise.all([invoices.refetch(), subscription.refetch()]);
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to record payment.")));
    }
  };

  const handleSubscriptionStatus = async (action: "suspend" | "reactivate" | "cancel") => {
    if (!id) return;
    try {
      if (action === "suspend") await suspend.mutateAsync(id);
      if (action === "reactivate") await reactivate.mutateAsync(id);
      if (action === "cancel") await cancel.mutateAsync(id);
      toast.success(t("Subscription updated."));
      await subscription.refetch();
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to update subscription.")));
    }
  };

  const confirmSubscriptionStatus = (action: "suspend" | "reactivate" | "cancel") => {
    Modal.confirm({
      title:
        action === "cancel"
          ? t("Cancel subscription?")
          : action === "suspend"
            ? t("Suspend subscription?")
            : t("Reactivate subscription?"),
      content:
        action === "cancel"
          ? t("The school will lose access to the platform. This cannot be undone from this screen.")
          : action === "suspend"
            ? t("The school's users will be blocked until the subscription is reactivated.")
            : t("This restores platform access for the school. Expired periods restart from now."),
      okText:
        action === "cancel" ? t("Cancel subscription") : action === "suspend" ? t("Suspend") : t("Reactivate"),
      okButtonProps: { danger: action !== "reactivate" },
      cancelText: t("Keep as is"),
      onOk: () => handleSubscriptionStatus(action),
    });
  };

  const invoiceColumns: TableProps<InvoiceList>["columns"] = [
    {
      title: t("Amount"),
      dataIndex: "amount",
      key: "amount",
      align: "right" as const,
      render: (value: number) => money(value),
    },
    {
      title: t("Paid"),
      dataIndex: "paidAmount",
      key: "paidAmount",
      align: "right" as const,
      render: (value: number) => money(value),
    },
    {
      title: t("Balance"),
      dataIndex: "balanceDue",
      key: "balanceDue",
      align: "right" as const,
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
      align: "center" as const,
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

  if (isLoading) {
    return (
      <DashboardTemplate title={t("School Details")} loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (!school) {
    return (
      <DashboardTemplate title={t("School Details")}>
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("School not found.")}</p>
        </div>
      </DashboardTemplate>
    );
  }

  return (
    <DashboardTemplate
      title={school.name}
      subtitle={t("Subdomain: {code}", { code: school.subdomainCode })}
      actions={
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            icon={<UserPlus className="h-4 w-4" />}
            onClick={() => navigate(`/platform/admins/new?schoolId=${school.id}`)}
          >
            {t("Register Admin")}
          </Button>
          <Button
            variant="primary"
            icon={<Pencil className="h-4 w-4" />}
            onClick={() => navigate(`/platform/schools/${school.id}/edit`)}
          >
            {t("Edit")}
          </Button>
        </div>
      }
    >
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <h3 className="mb-4 text-sm font-semibold text-[var(--color-text-primary)]">
              {t("General Information")}
            </h3>
            <dl className="space-y-3">
              <InfoRow icon={<Building2 className="h-4 w-4" />} label={t("Name")}>
                {school.name}
              </InfoRow>
              <InfoRow icon={<Hash className="h-4 w-4" />} label={t("Subdomain")}>
                <span className="font-mono">{school.subdomainCode}</span>
              </InfoRow>
              <InfoRow icon={<MapPin className="h-4 w-4" />} label={t("Status")}>
                <StatusBadge status={school.status} />
              </InfoRow>
              <InfoRow icon={<MapPin className="h-4 w-4" />} label={t("Address")}>
                {school.address ? (
                  <span className="whitespace-pre-line">{school.address}</span>
                ) : (
                  <span className="font-normal text-[var(--color-text-muted)]">
                    {t("Not set")}
                  </span>
                )}
              </InfoRow>
              <InfoRow icon={<Phone className="h-4 w-4" />} label={t("Phone")}>
                {school.phone || (
                  <span className="font-normal text-[var(--color-text-muted)]">
                    {t("Not set")}
                  </span>
                )}
              </InfoRow>
              <InfoRow icon={<Mail className="h-4 w-4" />} label={t("Email")}>
                {school.email || (
                  <span className="font-normal text-[var(--color-text-muted)]">
                    {t("Not set")}
                  </span>
                )}
              </InfoRow>
            </dl>
          </div>

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
                {t("Campuses")}
              </h3>
              <Button
                variant="secondary"
                size="small"
                icon={<Plus className="h-4 w-4" />}
                onClick={() => { setEditingCampusId(null); setCampusName(""); setCampusAddress(""); setCampusOpen(true); }}
              >
                {t("Add Campus")}
              </Button>
            </div>
            {school.campuses.length === 0 ? (
              <p className="text-sm text-[var(--color-text-muted)]">{t("No campuses registered.")}</p>
            ) : (
              <ul className="space-y-3">
                {school.campuses.map((campus) => (
                  <li key={campus.id} className="flex items-start gap-3">
                    <MapPin className="mt-0.5 h-4 w-4 text-[var(--color-text-muted)]" />
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-[var(--color-text-primary)]">
                        {campus.name}
                      </p>
                      <p className="text-xs text-[var(--color-text-muted)]">{campus.address}</p>
                    </div>
                    <div className="ml-auto flex shrink-0 gap-2">
                      <Button variant="ghost" size="small" onClick={() => { setEditingCampusId(campus.id); setCampusName(campus.name); setCampusAddress(campus.address); setCampusOpen(true); }}>{t("Edit")}</Button>
                      <Button variant="ghost" size="small" className="text-[var(--color-danger)]" onClick={() => confirmDeleteCampus(campus.id, campus.name)}><Trash2 className="h-4 w-4" /></Button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="space-y-6">
          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <h3 className="mb-2 text-sm font-semibold text-[var(--color-text-primary)]">
              {t("School Status")}
            </h3>
            <div className="mb-4 flex items-center gap-2">
              <StatusBadge status={school.status} />
              <span className="text-sm text-[var(--color-text-secondary)]">
                {isActive
                  ? t("The school is active and accessible.")
                  : t("The school is suspended and its users cannot sign in.")}
              </span>
            </div>
            {isActive ? (
              <Button
                variant="danger"
                icon={<PauseCircle className="h-4 w-4" />}
                onClick={() => setStatusConfirmOpen(true)}
              >
                {t("Suspend School")}
              </Button>
            ) : (
              <Button
                variant="primary"
                icon={<PlayCircle className="h-4 w-4" />}
                onClick={() => setStatusConfirmOpen(true)}
              >
                {t("Reactivate School")}
              </Button>
            )}
          </div>

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
                {t("Subscription")}
              </h3>
              <Button variant="secondary" size="small" onClick={openPlanModal}>
                {hasSubscription && sub?.status !== "Cancelled" ? t("Change plan") : t("Assign plan")}
              </Button>
            </div>
            {subLoading ? (
              <p className="text-sm text-[var(--color-text-muted)]">{t("Loading subscription...")}</p>
            ) : !sub ? (
              <p className="text-sm text-[var(--color-text-muted)]">
                {t("This school has no subscription yet. Assign a plan to activate billing.")}
              </p>
            ) : (
              <>
                <div className="mb-3 flex items-center gap-2">
                  <span className="text-sm font-medium text-[var(--color-text-primary)]">
                    {sub.planName}
                  </span>
                  <StatusBadge status={sub.status} />
                </div>
                <dl className="space-y-2 text-sm">
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-[var(--color-text-secondary)]">{t("Period")}</dt>
                    <dd className="text-[var(--color-text-primary)]">
                      {dayjs(sub.startDate).format("MMM D, YYYY")} –{" "}
                      {dayjs(sub.endDate).format("MMM D, YYYY")}
                    </dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-[var(--color-text-secondary)]">{t("Days left")}</dt>
                    <dd className="text-[var(--color-text-primary)]">
                      {sub.status === "Active" ? sub.daysRemaining : "—"}
                    </dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-[var(--color-text-secondary)]">{t("Outstanding")}</dt>
                    <dd className="text-[var(--color-text-primary)]">{money(sub.outstandingBalance)}</dd>
                  </div>
                </dl>
                <div className="mt-4 flex flex-wrap gap-2">
                  <Button
                    variant="secondary"
                    size="small"
                    icon={<CreditCard className="h-4 w-4" />}
                    onClick={openInvoiceModal}
                  >
                    {t("New invoice")}
                  </Button>
                  {sub.status === "Suspended" ? (
                    <Button variant="secondary" size="small" onClick={() => confirmSubscriptionStatus("reactivate")}>
                      {t("Reactivate")}
                    </Button>
                  ) : sub.status === "Active" ? (
                    <>
                      <Button variant="secondary" size="small" onClick={() => confirmSubscriptionStatus("suspend")}>
                        {t("Suspend")}
                      </Button>
                      <Button variant="ghost" size="small" onClick={() => confirmSubscriptionStatus("cancel")}>
                        {t("Cancel")}
                      </Button>
                    </>
                  ) : sub.status !== "Cancelled" ? (
                    <Button variant="ghost" size="small" onClick={() => confirmSubscriptionStatus("cancel")}>
                      {t("Cancel")}
                    </Button>
                  ) : null}
                </div>
              </>
            )}
          </div>

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <h3 className="mb-2 text-sm font-semibold text-[var(--color-text-primary)]">
              {t("School Admin")}
            </h3>
            <p className="mb-4 text-sm text-[var(--color-text-secondary)]">
              {t("Every school must have a school admin to manage its day-to-day operations.")}
            </p>
            {(school.admins ?? []).length > 0 && <div className="mb-4 space-y-2">
              {school.admins.map((admin) => <div key={admin.userId} className="rounded-md bg-[var(--color-surface-muted)] p-3">
                <p className="text-sm font-medium text-[var(--color-text-primary)]">{admin.firstName} {admin.lastName}</p>
                <p className="text-xs text-[var(--color-text-secondary)]">{admin.email || "—"}</p>
              </div>)}
            </div>}
            <Button
              variant="primary"
              icon={<UserPlus className="h-4 w-4" />}
              onClick={() => navigate(`/platform/admins/new?schoolId=${school.id}`)}
            >
              {t("Register School Admin")}
            </Button>
          </div>
        </div>
      </div>

      <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
          <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-[var(--color-text-primary)]">
            <CalendarDays className="h-4 w-4" />
            {t("Invoices")}
          </h3>
          <Button
            variant="secondary"
            size="small"
            icon={<Plus className="h-4 w-4" />}
            onClick={openInvoiceModal}
            disabled={!hasSubscription}
          >
            {t("New invoice")}
          </Button>
        </div>
        <Table<InvoiceList>
          rowKey="id"
          columns={invoiceColumns}
          dataSource={invoices.data?.items || []}
          loading={invoices.isLoading}
          scroll={{ x: "max-content" }}
          locale={{ emptyText: t("No invoices yet.") }}
          pagination={{
            current: invoicePage,
            pageSize: 10,
            total: invoices.data?.totalCount || 0,
            onChange: setInvoicePage,
            showSizeChanger: false,
          }}
        />
      </div>

      <Modal
        title={editingCampusId ? t("Edit Campus") : t("Add Campus")}
        open={campusOpen}
        onCancel={() => setCampusOpen(false)}
        onOk={handleAddCampus}
        confirmLoading={campusSaving}
        okText={editingCampusId ? t("Save") : t("Add Campus")}
        destroyOnClose
      >
        <div className="space-y-4 py-2">
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Campus Name")}
            </label>
            <Input
              value={campusName}
              onChange={(e) => setCampusName(e.target.value)}
              placeholder={t("e.g. Main Campus")}
              maxLength={200}
            />
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Address")}
            </label>
            <TextArea
              value={campusAddress}
              onChange={(e) => setCampusAddress(e.target.value)}
              placeholder={t("Street, city, postal code")}
              autoSize={{ minRows: 2, maxRows: 4 }}
              maxLength={500}
            />
          </div>
        </div>
      </Modal>

      <Modal
        title={isActive ? t("Suspend school?") : t("Reactivate school?")}
        open={statusConfirmOpen}
        onCancel={() => setStatusConfirmOpen(false)}
        onOk={handleToggleStatus}
        confirmLoading={statusSaving}
        okText={isActive ? t("Suspend") : t("Reactivate")}
        okButtonProps={{ danger: isActive }}
        cancelText={t("Cancel")}
        destroyOnClose
      >
        <p className="text-sm text-[var(--color-text-secondary)]">
          {isActive
            ? t("Suspending \"{school}\" will prevent its students, teachers, and admins from signing in until it is reactivated.", { school: school.name })
            : t("Reactivating \"{school}\" will restore access for all of its users.", { school: school.name })}
        </p>
      </Modal>

      {/* Change / assign plan */}
      <Modal
        title={hasSubscription ? t("Change subscription plan") : t("Assign subscription plan")}
        open={planModalOpen}
        onCancel={() => setPlanModalOpen(false)}
        onOk={handleChangePlan}
        confirmLoading={upgrade.isPending || assign.isPending}
        okText={t("Save")}
        destroyOnClose
      >
        <div className="space-y-4 py-2">
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Plan")}
            </label>
            <Select
              value={selectedPlanId}
              onChange={setSelectedPlanId}
              className="w-full"
              placeholder={t("Select a plan")}
              loading={plans.isLoading}
              options={(plans.data ?? [])
                .filter((plan) => plan.isActive)
                .map((plan) => ({
                  value: plan.id,
                  label: `${plan.name} — ${money(plan.priceMonthly)}/${t("month")}`,
                }))}
            />
          </div>
          {!hasSubscription && (
            <p className="text-xs text-[var(--color-text-muted)]">
              {t("The subscription starts immediately for one month and can be adjusted afterwards.")}
            </p>
          )}
        </div>
      </Modal>

      {/* Generate invoice */}
      <Modal
        title={t("Create invoice")}
        open={invoiceModalOpen}
        onCancel={() => setInvoiceModalOpen(false)}
        onOk={handleGenerateInvoice}
        confirmLoading={generateInvoice.isPending}
        okText={t("Create invoice")}
        destroyOnClose
      >
        <div className="space-y-4 py-2">
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Amount")}
            </label>
            <InputNumber
              min={0.01}
              step={0.01}
              value={invoiceAmount}
              onChange={(value) => setInvoiceAmount(Number(value ?? 0))}
              className="w-full"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm text-[var(--color-text-secondary)]">
              {t("Notes (optional)")}
            </label>
            <TextArea
              value={invoiceNotes}
              onChange={(e) => setInvoiceNotes(e.target.value)}
              placeholder={t("Renewal for next month, custom arrangement...")}
              autoSize={{ minRows: 2, maxRows: 4 }}
            />
          </div>
        </div>
      </Modal>

      {/* Record payment */}
      <Modal
        title={t("Record payment")}
        open={!!paymentInvoice}
        onCancel={() => setPaymentInvoice(null)}
        onOk={handleRecordPayment}
        confirmLoading={recordPayment.isPending}
        okText={t("Record payment")}
        destroyOnClose
      >
        {paymentInvoice && (
          <div className="space-y-4 py-2">
            <p className="text-sm text-[var(--color-text-secondary)]">
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
    </DashboardTemplate>
  );
}
