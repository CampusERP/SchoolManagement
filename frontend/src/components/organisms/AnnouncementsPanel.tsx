import { cn } from "@/lib/utils";
import dayjs from "dayjs";

interface Announcement {
  id: string;
  title: string;
  content: string;
  date: string;
  author: string;
}

interface AnnouncementsPanelProps {
  announcements: Announcement[];
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

export default function AnnouncementsPanel({
  announcements,
  className,
}: AnnouncementsPanelProps) {
  return (
    <InfoCard className={className}>
      <h3 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">
        Announcements
      </h3>

      {announcements.length === 0 ? (
        <p className="text-sm text-[var(--color-text-muted)]">No announcements</p>
      ) : (
        <div className="space-y-4">
          {announcements.map((a) => (
            <div key={a.id} className="border-b border-[var(--color-border)] pb-4 last:border-0 last:pb-0">
              <h4 className="text-sm font-medium text-[var(--color-text-primary)]">
                {a.title}
              </h4>
              <p className="mt-1 line-clamp-2 text-sm text-[var(--color-text-secondary)]">
                {a.content}
              </p>
              <div className="mt-2 flex items-center gap-2 text-xs text-[var(--color-text-muted)]">
                <span>{a.author}</span>
                <span>·</span>
                <span>{dayjs(a.date).format("MMM D, YYYY")}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </InfoCard>
  );
}
