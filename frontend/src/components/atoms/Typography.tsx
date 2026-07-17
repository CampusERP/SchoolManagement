import { cn } from "@/lib/utils";

type TypographyVariant =
  | "h1"
  | "h2"
  | "h3"
  | "h4"
  | "body"
  | "caption"
  | "label";

export interface TypographyProps {
  variant?: TypographyVariant;
  children: React.ReactNode;
  className?: string;
  as?: React.ElementType;
}

const variantConfig: Record<
  TypographyVariant,
  { tag: React.ElementType; className: string }
> = {
  h1: {
    tag: "h1",
    className: "text-3xl font-bold text-[var(--color-text-primary)] leading-tight tracking-tight",
  },
  h2: {
    tag: "h2",
    className: "text-2xl font-semibold text-[var(--color-text-primary)] leading-tight tracking-tight",
  },
  h3: {
    tag: "h3",
    className: "text-xl font-semibold text-[var(--color-text-primary)] leading-snug",
  },
  h4: {
    tag: "h4",
    className: "text-lg font-medium text-[var(--color-text-primary)] leading-snug",
  },
  body: {
    tag: "p",
    className: "text-sm text-[var(--color-text-primary)] leading-relaxed",
  },
  caption: {
    tag: "span",
    className: "text-xs text-[var(--color-text-muted)] leading-normal",
  },
  label: {
    tag: "label",
    className: "text-sm font-medium text-[var(--color-text-secondary)] leading-normal",
  },
};

export default function Typography({
  variant = "body",
  children,
  className,
  as,
}: TypographyProps) {
  const config = variantConfig[variant];
  const Tag = as ?? config.tag;

  return <Tag className={cn(config.className, className)}>{children}</Tag>;
}
