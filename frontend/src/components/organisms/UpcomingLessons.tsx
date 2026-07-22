import { cn } from "@/lib/utils";
import InfoCard from "@/components/molecules/InfoCard";

interface LessonItem {
  id?: string;
  classScheduleId?: string;
  className: string;
  subject: string;
  startTime: string;
  endTime: string;
  room: string;
  status: string;
}

interface UpcomingLessonsProps {
  items: LessonItem[];
  className?: string;
}

const statusStyles: Record<string, { dot: string; text: string; bg: string }> = {
  completed: { dot: "bg-gray-400", text: "text-gray-500", bg: "bg-gray-50" },
  in_progress: { dot: "bg-[var(--color-primary)]", text: "text-[var(--color-primary)]", bg: "bg-[var(--color-primary-light)]" },
  upcoming: { dot: "bg-gray-400", text: "text-gray-600", bg: "bg-gray-50" },
};

export default function UpcomingLessons({
  items,
  className,
}: UpcomingLessonsProps) {
  return (
    <InfoCard title="Upcoming Lessons" className={className}>
      {items.length === 0 ? (
        <p className="text-sm text-[var(--color-text-muted)]">No lessons scheduled</p>
      ) : (
        <div className="space-y-3">
          {items.map((item) => {
            const style = statusStyles[item.status] ?? statusStyles.upcoming;
            return (
              <div
                key={item.classScheduleId ?? item.id}
                className={cn(
                  "flex items-center gap-4 rounded-[var(--border-radius)] border border-[var(--color-border)] p-3",
                  style.bg
                )}
              >
                <div className="flex flex-col items-center text-xs text-[var(--color-text-secondary)]">
                  <span className="font-medium">
                    {item.startTime?.slice(0, 5)}
                  </span>
                  <span className="text-[var(--color-text-muted)]">
                    {item.endTime?.slice(0, 5)}
                  </span>
                </div>

                <div className={cn("h-8 w-0.5 shrink-0 rounded-full", style.dot)} />

                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium text-[var(--color-text-primary)]">
                    {item.className}
                  </p>
                  <p className="text-xs text-[var(--color-text-secondary)]">
                    {item.subject} · {item.room}
                  </p>
                </div>

                <span className={cn("text-xs font-medium capitalize", style.text)}>
                  {item.status.replace("_", " ")}
                </span>
              </div>
            );
          })}
        </div>
      )}
    </InfoCard>
  );
}
