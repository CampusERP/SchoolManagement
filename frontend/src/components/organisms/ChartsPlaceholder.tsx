import { BarChart3 } from "lucide-react";
import { cn } from "@/lib/utils";

interface ChartsPlaceholderProps {
  title: string;
  className?: string;
}

function InfoCard({ children, className }: { children: React.ReactNode; className?: string }) {
  return (
    <div
      className={cn(
        "rounded-[var(--card-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)]",
        className
      )}
    >
      {children}
    </div>
  );
}

export default function ChartsPlaceholder({
  title,
  className,
}: ChartsPlaceholderProps) {
  return (
    <InfoCard className={className}>
      <h3 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">
        {title}
      </h3>

      <div className="flex flex-col items-center justify-center rounded-[var(--border-radius)] border-2 border-dashed border-[var(--color-border)] py-12 text-center">
        <BarChart3 className="mb-3 h-10 w-10 text-[var(--color-text-muted)]" />
        <p className="text-sm text-[var(--color-text-muted)]">Chart coming soon</p>
      </div>
    </InfoCard>
  );
}
