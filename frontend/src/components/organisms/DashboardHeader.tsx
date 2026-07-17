import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

interface DashboardHeaderProps {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  className?: string;
}

export default function DashboardHeader({
  title,
  subtitle,
  actions,
  className,
}: DashboardHeaderProps) {
  return (
    <div className={cn("flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between", className)}>
      <div>
        <h1 className="text-2xl font-semibold text-[var(--color-text-primary)]">
          {title}
        </h1>
        {subtitle && (
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            {subtitle}
          </p>
        )}
      </div>
      {actions && <div className="mt-3 flex items-center gap-2 sm:mt-0">{actions}</div>}
    </div>
  );
}
