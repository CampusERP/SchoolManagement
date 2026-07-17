import { Button as AntButton, type ButtonProps as AntButtonProps } from "antd";
import { cn } from "@/lib/utils";

type Variant = "primary" | "secondary" | "ghost" | "danger";

export interface ButtonProps extends Omit<AntButtonProps, "type" | "variant"> {
  variant?: Variant;
  children?: React.ReactNode;
}

const variantStyles: Record<Variant, string> = {
  primary:
    "bg-[var(--color-primary)] !text-white hover:bg-[var(--color-primary-hover)] border-none",
  secondary:
    "bg-[var(--color-surface)] text-[var(--color-text-primary)] border border-[var(--color-border)] hover:bg-[var(--color-surface-card)] hover:border-[var(--color-primary)]",
  ghost:
    "bg-transparent text-[var(--color-text-primary)] border-none hover:bg-[var(--color-primary-light)]",
  danger:
    "bg-[var(--color-danger)] !text-white hover:bg-red-600 border-none",
};

export default function Button({
  variant = "primary",
  className,
  children,
  ...props
}: ButtonProps) {
  return (
    <AntButton
      className={cn(variantStyles[variant], className)}
      {...props}
    >
      {children}
    </AntButton>
  );
}
