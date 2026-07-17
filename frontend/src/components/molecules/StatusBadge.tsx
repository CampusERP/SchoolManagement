import { cn } from "@/lib/utils";

interface StatusBadgeProps {
  status: string;
  className?: string;
}

const STATUS_STYLES: Record<string, string> = {
  Active: "bg-emerald-50 text-[var(--color-success)]",
  Trial: "bg-emerald-50 text-[var(--color-success)]",
  Healthy: "bg-emerald-50 text-[var(--color-success)]",
  Expired: "bg-red-50 text-[var(--color-danger)]",
  Inactive: "bg-red-50 text-[var(--color-danger)]",
  Down: "bg-red-50 text-[var(--color-danger)]",
  Pending: "bg-amber-50 text-[var(--color-warning)]",
  Degraded: "bg-amber-50 text-[var(--color-warning)]",
};

export default function StatusBadge({ status, className }: StatusBadgeProps) {
  const style =
    STATUS_STYLES[status] ?? "bg-gray-50 text-[var(--color-text-secondary)]";

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium capitalize",
        style,
        className
      )}
    >
      {status}
    </span>
  );
}
