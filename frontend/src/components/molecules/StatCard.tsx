import { type ReactNode } from "react";
import { TrendingUp, TrendingDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface StatCardProps {
  title: string;
  value: string | number;
  icon: ReactNode;
  trend?: { value: number; isPositive: boolean };
  className?: string;
}

export default function StatCard({
  title,
  value,
  icon,
  trend,
  className,
}: StatCardProps) {
  return (
    <div
      className={cn(
        "flex items-center gap-4 bg-[var(--color-surface-card)] rounded-[var(--card-radius)] shadow-[var(--shadow-card)] p-5",
        className
      )}
    >
      <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-[var(--color-primary-light)] text-[var(--color-primary)]">
        {icon}
      </div>

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-[var(--color-text-secondary)]">
          {title}
        </p>
        <p className="mt-0.5 text-2xl font-semibold text-[var(--color-text-primary)]">
          {value}
        </p>
      </div>

      {trend && (
        <span
          className={cn(
            "inline-flex shrink-0 items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium",
            trend.isPositive
              ? "bg-emerald-50 text-[var(--color-success)]"
              : "bg-red-50 text-[var(--color-danger)]"
          )}
        >
          {trend.isPositive ? (
            <TrendingUp className="h-3 w-3" />
          ) : (
            <TrendingDown className="h-3 w-3" />
          )}
          {Math.abs(trend.value)}%
        </span>
      )}
    </div>
  );
}
