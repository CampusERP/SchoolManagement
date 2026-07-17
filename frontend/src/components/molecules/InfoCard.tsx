import { type ReactNode } from "react";
import { cn } from "@/lib/utils";

interface InfoCardProps {
  title?: string;
  children: ReactNode;
  className?: string;
  extra?: ReactNode;
}

export default function InfoCard({
  title,
  children,
  className,
  extra,
}: InfoCardProps) {
  return (
    <div
      className={cn(
        "bg-[var(--color-surface-card)] rounded-[var(--card-radius)] shadow-[var(--shadow-card)]",
        className
      )}
    >
      {title && (
        <div className="flex items-center justify-between border-b border-[var(--color-border)] px-5 py-4">
          <h3 className="text-base font-semibold text-[var(--color-text-primary)]">
            {title}
          </h3>
          {extra && <div>{extra}</div>}
        </div>
      )}
      <div className="p-5">{children}</div>
    </div>
  );
}
