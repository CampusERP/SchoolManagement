import { type ReactNode } from "react";
import { cn } from "@/lib/utils";

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
  className?: string;
}

export default function EmptyState({
  icon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center py-16 text-center",
        className
      )}
    >
      {icon && (
        <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-[var(--color-surface)] text-[var(--color-text-muted)]">
          {icon}
        </div>
      )}

      <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">
        {title}
      </h3>

      {description && (
        <p className="mt-1 max-w-sm text-sm text-[var(--color-text-secondary)]">
          {description}
        </p>
      )}

      {action && <div className="mt-6">{action}</div>}
    </div>
  );
}
