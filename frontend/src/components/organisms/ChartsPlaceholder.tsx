import { BarChart3 } from "lucide-react";
import InfoCard from "@/components/molecules/InfoCard";

interface ChartsPlaceholderProps {
  title: string;
  className?: string;
}

export default function ChartsPlaceholder({
  title,
  className,
}: ChartsPlaceholderProps) {
  return (
    <InfoCard title={title} className={className}>
      <div className="flex flex-col items-center justify-center rounded-[var(--border-radius)] border-2 border-dashed border-[var(--color-border)] py-12 text-center">
        <BarChart3 className="mb-3 h-10 w-10 text-[var(--color-text-muted)]" />
        <p className="text-sm text-[var(--color-text-muted)]">Chart coming soon</p>
      </div>
    </InfoCard>
  );
}
