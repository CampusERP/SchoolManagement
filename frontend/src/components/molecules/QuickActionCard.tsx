import { type ReactNode } from "react";
import { cn } from "@/lib/utils";

interface QuickActionCardProps {
  icon: ReactNode;
  label: string;
  onClick: () => void;
  className?: string;
}

export default function QuickActionCard({
  icon,
  label,
  onClick,
  className,
}: QuickActionCardProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        "flex min-w-0 flex-col items-center justify-center gap-2 rounded-[var(--card-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-3 text-center transition-shadow hover:shadow-[var(--shadow-elevated)] sm:gap-3 sm:p-5",
        className
      )}
    >
      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[var(--color-primary-light)] text-[var(--color-primary-soft-text)] sm:h-12 sm:w-12">
        {icon}
      </div>
      <span className="text-xs font-medium text-[var(--color-text-primary)] sm:text-sm">
        {label}
      </span>
    </button>
  );
}
