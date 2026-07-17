import { cn } from "@/lib/utils";

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
  { key: "present", label: "Present", color: "bg-emerald-500" },
  { key: "absent", label: "Absent", color: "bg-red-500" },
  { key: "late", label: "Late", color: "bg-amber-500" },
  { key: "excused", label: "Excused", color: "bg-sky-500" },
] as const;

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

export default function AttendanceSummary({
  data,
  className,
}: AttendanceSummaryProps) {
  const total = data.present + data.absent + data.late + data.excused;

  return (
    <InfoCard className={className}>
      <h3 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">
        Attendance Summary
      </h3>

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
