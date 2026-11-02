import { cn } from "@/lib/utils";
import { t } from "@/i18n";

interface StatusBadgeProps {
  status: string;
  className?: string;
}

const STATUS_STYLES: Record<string, string> = {
  Active: "bg-[var(--color-success-soft)] text-[var(--color-success)]",
  Trial: "bg-[var(--color-success-soft)] text-[var(--color-success)]",
  Healthy: "bg-[var(--color-success-soft)] text-[var(--color-success)]",
  Paid: "bg-[var(--color-success-soft)] text-[var(--color-success)]",
  Expired: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Inactive: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Suspended: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Overdue: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Cancelled: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Down: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
  Pending: "bg-[var(--color-warning-soft)] text-[var(--color-warning)]",
  PendingPayment: "bg-[var(--color-warning-soft)] text-[var(--color-warning)]",
  Issued: "bg-[var(--color-warning-soft)] text-[var(--color-warning)]",
  Degraded: "bg-[var(--color-warning-soft)] text-[var(--color-warning)]",
  Approved: "bg-[var(--color-success-soft)] text-[var(--color-success)]",
  Rejected: "bg-[var(--color-danger-soft)] text-[var(--color-danger)]",
};

export default function StatusBadge({ status, className }: StatusBadgeProps) {
  const style =
    STATUS_STYLES[status] ??
    "bg-[var(--color-neutral-soft)] text-[var(--color-text-secondary)]";

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium capitalize",
        style,
        className
      )}
    >
      {t(status)}
    </span>
  );
}
