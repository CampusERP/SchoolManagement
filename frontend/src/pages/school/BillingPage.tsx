import { useState } from "react";
import {
  BadgeDollarSign,
  CalendarDays,
  CheckCircle2,
  CreditCard,
  Download,
  Eye,
  Mail,
} from "lucide-react";
import { Table, type TableProps } from "antd";
import dayjs from "dayjs";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import StatusBadge from "@/components/molecules/StatusBadge";
import Button from "@/components/atoms/Button";
import { useAuthStore } from "@/store/authStore";
import { useSchoolInvoices, useSchoolSubscription } from "@/features/billing/hooks";
import { localizedBenefitDescriptions } from "@/features/billing/display";
import type { InvoiceList } from "@/features/billing/api";
import { downloadInvoicePdf, openInvoicePdf } from "@/features/billing/pdf";
import { getApiErrorMessage } from "@/lib/errors";
import { localize, t } from "@/i18n";

const money = (value: number) =>
  `$${Number(value || 0).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;

export default function BillingPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId);
  const [page, setPage] = useState(1);

  const subscription = useSchoolSubscription(schoolId);
  const invoices = useSchoolInvoices(schoolId, { page, pageSize: 10 });

  const sub = subscription.data;
  const hasSubscription = !!sub;

  const features = sub
    ? [
        sub.hasParentPortal ? t("Parent portal") : null,
        sub.hasExamModule ? t("Exam & results module") : null,
        sub.hasAnalytics ? t("Advanced analytics") : null,
      ].filter(Boolean) as string[]
    : [];

  const benefits = sub ? localizedBenefitDescriptions(sub.planBenefits) : [];
  const planDescription = sub
    ? localize(sub.planDescriptionEn, sub.planDescriptionAr)
    : "";

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

  const invoiceColumns: TableProps<InvoiceList>["columns"] = [
    {
      title: t("Invoice"),
      dataIndex: "id",
      key: "id",
      render: (id: string) => <span className="font-mono text-xs">{id.slice(0, 8)}</span>,
    },
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
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Billing & Subscription")}
      subtitle={t("Review your current plan, invoices, and outstanding balance.")}
    >
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6 lg:col-span-2">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
            <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-[var(--color-text-primary)]">
              <BadgeDollarSign className="h-4 w-4" />
              {t("Current plan")}
            </h3>
            {sub && <StatusBadge status={sub.status} />}
          </div>

          {subscription.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">{t("Loading subscription...")}</p>
          ) : !hasSubscription ? (
            <div className="space-y-2">
              <p className="text-sm text-[var(--color-text-secondary)]">
                {t("There is no subscription on file for your school yet.")}
              </p>
              <p className="text-sm text-[var(--color-text-muted)]">
                {t("Please contact the platform team to choose a plan and activate your account.")}
              </p>
              <Button
                variant="primary"
                icon={<Mail className="h-4 w-4" />}
                href="mailto:sales@campuserp.com"
              >
                {t("Contact us for payment")}
              </Button>
            </div>
          ) : (
            <>
              <div className="mb-4">
                <p className="text-lg font-semibold text-[var(--color-text-primary)]">
                  {sub.planName}
                </p>
                {planDescription && (
                  <p className="text-sm text-[var(--color-text-secondary)]">{planDescription}</p>
                )}
              </div>

              <dl className="grid gap-4 sm:grid-cols-2">
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Monthly price")}
                  </dt>
                  <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                    {money(sub.priceMonthly)}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Current period")}
                  </dt>
                  <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                    {dayjs(sub.startDate).format("MMM D, YYYY")} –{" "}
                    {dayjs(sub.endDate).format("MMM D, YYYY")}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Days remaining")}
                  </dt>
                  <dd
                    className={
                      sub.status === "Active" && sub.daysRemaining <= 7
                        ? "text-sm font-semibold text-[var(--color-danger)]"
                        : "text-sm font-medium text-[var(--color-text-primary)]"
                    }
                  >
                    {sub.status === "Active" ? sub.daysRemaining : "—"}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Outstanding balance")}
                  </dt>
                  <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                    {money(sub.outstandingBalance)}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Limits")}
                  </dt>
                  <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                    {sub.maxStudents.toLocaleString()} {t("students")} ·{" "}
                    {sub.maxTeachers.toLocaleString()} {t("teachers")}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Auto renew")}
                  </dt>
                  <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                    {sub.autoRenew ? t("Enabled") : t("Disabled")}
                  </dd>
                </div>
              </dl>

              {benefits.length > 0 && (
                <div className="mt-4">
                  <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                    {t("Benefits")}
                  </p>
                  <ul className="flex flex-wrap gap-x-4 gap-y-2">
                    {benefits.map((benefit) => (
                      <li
                        key={benefit}
                        className="inline-flex items-center gap-1.5 text-sm text-[var(--color-text-secondary)]"
                      >
                        <CheckCircle2 className="h-4 w-4 text-[var(--color-success)]" />
                        {benefit}
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {features.length > 0 && (
                <ul className="mt-4 flex flex-wrap gap-x-4 gap-y-2">
                  {features.map((feature) => (
                    <li
                      key={feature}
                      className="inline-flex items-center gap-1.5 text-sm text-[var(--color-text-secondary)]"
                    >
                      <CheckCircle2 className="h-4 w-4 text-[var(--color-success)]" />
                      {feature}
                    </li>
                  ))}
                </ul>
              )}
            </>
          )}
        </div>

        <div className="space-y-6">
          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
            <h3 className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-[var(--color-text-primary)]">
              <CreditCard className="h-4 w-4" />
              {t("Payments")}
            </h3>
            <p className="mb-4 text-sm text-[var(--color-text-secondary)]">
              {t("Payments are arranged with the platform team. Contact us to settle an invoice or renew your plan.")}
            </p>
            {sub && sub.outstandingBalance > 0 && (
              <p className="mb-3 rounded-[var(--border-radius)] bg-[var(--color-warning-soft)] px-3 py-2 text-sm font-medium text-[var(--color-warning)]">
                {t("Amount due: {amount}", { amount: money(sub.outstandingBalance) })}
              </p>
            )}
            <Button
              variant="primary"
              icon={<Mail className="h-4 w-4" />}
              href="mailto:sales@campuserp.com"
              block
            >
              {t("Contact us for payment")}
            </Button>
          </div>

          {sub?.status === "Suspended" && (
            <div className="rounded-[var(--card-radius)] bg-[var(--color-danger-soft)] p-4 text-sm text-[var(--color-danger)] shadow-[var(--shadow-card)] sm:p-6">
              {t("Your subscription is suspended. Contact the platform team to restore access.")}
            </div>
          )}
          {sub?.status === "PendingPayment" && (
            <div className="rounded-[var(--card-radius)] bg-[var(--color-warning-soft)] p-4 text-sm text-[var(--color-warning)] shadow-[var(--shadow-card)] sm:p-6">
              {t("Your subscription is awaiting payment. Once the first invoice is paid, your period starts automatically.")}
            </div>
          )}
        </div>
      </div>

      <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
        <h3 className="mb-4 inline-flex items-center gap-2 text-sm font-semibold text-[var(--color-text-primary)]">
          <CalendarDays className="h-4 w-4" />
          {t("Invoices")}
        </h3>
        <Table<InvoiceList>
          rowKey="id"
          columns={invoiceColumns}
          dataSource={invoices.data?.items || []}
          loading={invoices.isLoading}
          scroll={{ x: "max-content" }}
          locale={{ emptyText: t("No invoices yet.") }}
          pagination={{
            current: page,
            pageSize: 10,
            total: invoices.data?.totalCount || 0,
            onChange: setPage,
            showSizeChanger: false,
          }}
        />
      </div>
    </DashboardTemplate>
  );
}
