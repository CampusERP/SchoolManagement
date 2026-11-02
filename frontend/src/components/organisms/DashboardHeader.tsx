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
    <div className={cn("flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between sm:gap-4", className)}>
      <div className="min-w-0">
        <h1 className="text-xl font-semibold text-[var(--color-text-primary)] sm:text-2xl">
          {title}
        </h1>
        {subtitle && (
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            {subtitle}
          </p>
        )}
      </div>
      {actions && <div className="flex w-full flex-wrap items-center gap-2 sm:mt-0 sm:w-auto">{actions}</div>}
    </div>
  );
}
