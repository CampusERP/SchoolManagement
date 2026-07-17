import { cn } from "@/lib/utils";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import InfoCard from "@/components/molecules/InfoCard";

dayjs.extend(relativeTime);

interface Activity {
  id: string;
  type: string;
  message: string;
  timestamp: string;
}

interface TimelinePanelProps {
  activities: Activity[];
  className?: string;
}

const typeColors: Record<string, string> = {
  school_created: "bg-emerald-500",
  admin_registered: "bg-[var(--color-primary)]",
  subscription_changed: "bg-amber-500",
  student_enrolled: "bg-sky-500",
};

export default function TimelinePanel({
  activities,
  className,
}: TimelinePanelProps) {
  return (
    <InfoCard className={className}>
      <h3 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">
        Activity Timeline
      </h3>

      {activities.length === 0 ? (
        <p className="text-sm text-[var(--color-text-muted)]">No recent activity</p>
      ) : (
        <div className="relative ml-3 space-y-4">
          <div className="absolute left-0 top-1 bottom-1 w-px bg-[var(--color-border)]" />

          {activities.map((activity) => (
            <div key={activity.id} className="relative flex gap-3 pl-6">
              <div
                className={cn(
                  "absolute left-0 top-1.5 h-2.5 w-2.5 -translate-x-1/2 rounded-full ring-2 ring-[var(--color-surface-card)]",
                  typeColors[activity.type] ?? "bg-[var(--color-text-muted)]"
                )}
              />
              <div className="min-w-0 flex-1">
                <p className="text-sm text-[var(--color-text-primary)]">{activity.message}</p>
                <p className="mt-0.5 text-xs text-[var(--color-text-muted)]">
                  {dayjs(activity.timestamp).fromNow()}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </InfoCard>
  );
}
