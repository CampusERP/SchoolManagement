import { cn } from "@/lib/utils";
import InfoCard from "@/components/molecules/InfoCard";

interface AttendanceData {
  present: number;
  absent: number;
  late: number;
  excused: number;
}

interface AttendanceSummaryProps {
  data: AttendanceData;
  className?: string;
}

const categories = [
  { key: "present", label: "Present", color: "bg-[var(--color-success)]" },
  { key: "absent", label: "Absent", color: "bg-[var(--color-danger)]" },
  { key: "late", label: "Late", color: "bg-[var(--color-warning)]" },
  { key: "excused", label: "Excused", color: "bg-[var(--color-info)]" },
] as const;

export default function AttendanceSummary({
  data,
  className,
}: AttendanceSummaryProps) {
  const total = data.present + data.absent + data.late + data.excused;

  return (
    <InfoCard title="Attendance Summary" className={className}>
      <div className="mb-4 text-center">
        <span className="text-3xl font-bold text-[var(--color-text-primary)]">{total}</span>
        <span className="ml-1 text-sm text-[var(--color-text-muted)]">total records</span>
      </div>

      {total > 0 && (
        <div className="mb-5 flex h-2 overflow-hidden rounded-full bg-[var(--color-surface)]">
          {categories.map((cat) => {
            const count = data[cat.key];
            if (count === 0) return null;
            return (
              <div
                key={cat.key}
                className={cat.color}
                style={{ width: `${(count / total) * 100}%` }}
              />
            );
          })}
        </div>
      )}

      <div className="grid grid-cols-2 gap-3">
        {categories.map((cat) => {
          const count = data[cat.key];
          const pct = total > 0 ? Math.round((count / total) * 100) : 0;
          return (
            <div key={cat.key} className="flex items-center gap-2">
              <span className={cn("h-2.5 w-2.5 shrink-0 rounded-full", cat.color)} />
              <span className="text-sm text-[var(--color-text-secondary)]">{cat.label}</span>
              <span className="ml-auto text-sm font-medium text-[var(--color-text-primary)]">
                {count}
                <span className="ml-0.5 text-xs text-[var(--color-text-muted)]">
                  ({pct}%)
                </span>
              </span>
            </div>
          );
        })}
      </div>
    </InfoCard>
  );
}
