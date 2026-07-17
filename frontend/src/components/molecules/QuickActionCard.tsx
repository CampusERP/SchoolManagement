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
        "flex flex-col items-center justify-center gap-3 rounded-[var(--card-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-5 text-center transition-shadow hover:shadow-[var(--shadow-elevated)]",
        className
      )}
    >
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-[var(--color-primary-light)] text-[var(--color-primary)]">
        {icon}
      </div>
      <span className="text-sm font-medium text-[var(--color-text-primary)]">
        {label}
      </span>
    </button>
  );
}
