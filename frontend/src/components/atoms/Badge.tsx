import { cn } from "@/lib/utils";

type BadgeVariant = "success" | "warning" | "danger" | "info" | "default";

export interface BadgeProps {
  variant?: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

const variantStyles: Record<BadgeVariant, string> = {
  success:
    "bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300",
  warning:
    "bg-amber-50 text-amber-700 dark:bg-amber-950 dark:text-amber-300",
  danger:
    "bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-300",
  info:
    "bg-blue-50 text-blue-700 dark:bg-blue-950 dark:text-blue-300",
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
