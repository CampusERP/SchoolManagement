import { useMemo } from "react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { useAuthStore } from "@/store/authStore";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import { useTeacherSchedule } from "@/features/teacher/hooks";
import type { ScheduleSlot } from "@/features/teacher/api";
import { cn } from "@/lib/utils";

const DAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const DAY_COLORS = [
  "bg-purple-50 border-purple-200 text-purple-800",
  "bg-blue-50 border-blue-200 text-blue-800",
  "bg-green-50 border-green-200 text-green-800",
  "bg-amber-50 border-amber-200 text-amber-800",
  "bg-rose-50 border-rose-200 text-rose-800",
  "bg-cyan-50 border-cyan-200 text-cyan-800",
  "bg-indigo-50 border-indigo-200 text-indigo-800",
];

function formatTime(ts: string) {
  if (!ts) return "";
  const parts = ts.split(":");
  const h = parseInt(parts[0] ?? "0", 10);
  const m = parts[1] ?? "00";
  const ampm = h >= 12 ? "PM" : "AM";
  const h12 = h % 12 || 12;
  return `${h12}:${m} ${ampm}`;
}

export default function TeacherSchedulePage() {
  const { data: dashboard, isLoading: dashLoading } = useTeacherDashboard();
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const teacherId = dashboard?.teacherId ?? null;
  const termId = dashboard?.currentTermId ?? null;
  const { data: slots, isLoading: slotsLoading } = useTeacherSchedule(teacherId, schoolId, termId);

  const weekGrid = useMemo<Record<string, ScheduleSlot[]>>(() => {
    if (!slots) return {};
    const byDay: Record<string, ScheduleSlot[]> = {};
    for (const day of DAYS) byDay[day] = [];
    for (const s of slots) {
      byDay[s.dayOfWeek]?.push(s);
    }
    return byDay;
  }, [slots]);

  const hasSchedule = slots && slots.length > 0;

  return (
    <DashboardTemplate
      title="My Schedule"
      subtitle="Your weekly class timetable"
      loading={dashLoading || slotsLoading}
    >
      {!hasSchedule && !slotsLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-8 text-center">
          <p className="text-sm text-[var(--color-text-secondary)]">
            No schedule data available. Contact your administrator to set up your class timetable.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {DAYS.map((day, dayIdx) => {
            const daySlots = weekGrid[day] ?? [];
            return (
              <div
                key={day}
                className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] overflow-hidden"
              >
                <div className={cn("border-b px-4 py-2.5 text-sm font-semibold", DAY_COLORS[dayIdx] ?? "bg-gray-50 text-gray-800")}>
                  {day}
                </div>
                <div className="p-3 space-y-2">
                  {daySlots.length === 0 ? (
                    <p className="py-4 text-center text-xs text-[var(--color-text-muted)]">No classes</p>
                  ) : (
                    daySlots.map((slot, i) => (
                      <div
                        key={i}
                        className="rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] p-3"
                      >
                        <p className="text-sm font-medium text-[var(--color-text-primary)]">
                          {slot.subjectName}
                        </p>
                        <p className="text-xs text-[var(--color-text-secondary)]">
                          {slot.classRoomName} &middot; {slot.roomName}
                        </p>
                        <p className="mt-1 text-xs font-medium text-[var(--color-primary)]">
                          {formatTime(slot.startTime)} &ndash; {formatTime(slot.endTime)}
                        </p>
                      </div>
                    ))
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </DashboardTemplate>
  );
}
