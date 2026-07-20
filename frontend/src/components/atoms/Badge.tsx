import { cn } from "@/lib/utils";

type BadgeVariant = "success" | "warning" | "danger" | "info" | "default";

export interface BadgeProps {
  variant?: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

const variantStyles: Record<BadgeVariant, string> = {
  success:
    "bg-emerald-50 text-[var(--color-success)]",
  warning:
    "bg-amber-50 text-[var(--color-warning)]",
  danger:
    "bg-red-50 text-[var(--color-danger)]",
  info:
    "bg-blue-50 text-[var(--color-info)]",
  default:
    "bg-[var(--color-surface)] text-[var(--color-text-secondary)]",
};

export default function Badge({
  variant = "default",
  children,
  className,
}: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium leading-6",
        variantStyles[variant],
        className,
      )}
    >
      {children}
    </span>
  );
}
