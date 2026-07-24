import { useState } from "react";
import { Select, Button, message, Tag } from "antd";
import { CheckCircle, XCircle, Clock, AlertCircle, Save, Lock } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { useAuthStore } from "@/store/authStore";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import { useAttendanceSheet, useRecordAttendance, useLockAttendance } from "@/features/teacher/hooks";
import type { ClassAttendanceStudent } from "@/features/teacher/api";
import { cn } from "@/lib/utils";

const STATUS_OPTIONS = [
  { value: 0, label: "Present", color: "bg-green-100 text-green-700 border-green-300", icon: CheckCircle },
  { value: 1, label: "Absent", color: "bg-red-100 text-red-700 border-red-300", icon: XCircle },
  { value: 2, label: "Late", color: "bg-amber-100 text-amber-700 border-amber-300", icon: Clock },
  { value: 3, label: "Excused", color: "bg-blue-100 text-blue-700 border-blue-300", icon: AlertCircle },
];

const STATUS_LABELS: Record<number, string> = { 0: "Present", 1: "Absent", 2: "Late", 3: "Excused" };

export default function TeacherAttendancePage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const { data: dashboard, isLoading: dashLoading } = useTeacherDashboard();

  const todaySlots = dashboard?.todaySchedule ?? [];
  const [selectedSlotId, setSelectedSlotId] = useState<string | null>(null);
  const today = new Date().toISOString().split("T")[0];

  const { data: attendance, isLoading: attLoading } = useAttendanceSheet(selectedSlotId, schoolId, today);
  const recordMutation = useRecordAttendance();
  const lockMutation = useLockAttendance();

  const [statuses, setStatuses] = useState<Record<string, number>>({});
  const [notes, setNotes] = useState<Record<string, string>>({});

  const records = attendance?.records ?? [];
  const isLocked = attendance?.isLocked ?? false;
  const sessionId = attendance?.sessionId ?? null;

  const selectedSlot = todaySlots.find((s) => s.classScheduleId === selectedSlotId);

  const currentStatuses: Record<string, number> = {};
  for (const r of records) {
    currentStatuses[r.studentEnrollmentId] = statuses[r.studentEnrollmentId] ?? r.status;
  }

  const hasChanges = records.some(
    (r) => (statuses[r.studentEnrollmentId] ?? r.status) !== r.status
  );

  const handleSave = async () => {
    if (!selectedSlotId) return;
    const entries = records.map((r) => ({
      studentEnrollmentId: r.studentEnrollmentId,
      status: currentStatuses[r.studentEnrollmentId] ?? r.status,
      note: notes[r.studentEnrollmentId] || undefined,
    }));

    try {
      await recordMutation.mutateAsync({
        schoolId,
        classScheduleId: selectedSlotId,
        date: today,
        entries,
      });
      message.success("Attendance saved");
    } catch {
      message.error("Failed to save attendance");
    }
  };

  const handleLock = async () => {
    if (!sessionId || sessionId === "00000000-0000-0000-0000-000000000000") return;
    try {
      await lockMutation.mutateAsync({ sessionId, schoolId });
      message.success("Attendance session locked");
    } catch {
      message.error("Failed to lock session");
    }
  };

  return (
    <DashboardTemplate
      title="Take Attendance"
      subtitle={today}
      loading={dashLoading}
    >
      <div className="space-y-4">
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            Select a class from today's schedule
          </label>
          <Select
            className="w-full max-w-md"
            placeholder="Choose a class..."
            value={selectedSlotId}
            onChange={(val) => {
              setSelectedSlotId(val);
              setStatuses({});
              setNotes({});
            }}
            options={todaySlots.map((s) => ({
              value: s.classScheduleId,
              label: `${s.subject} - ${s.className} (${s.startTime?.slice(0, 5)} - ${s.endTime?.slice(0, 5)})`,
            }))}
            loading={dashLoading}
            disabled={todaySlots.length === 0}
          />
          {todaySlots.length === 0 && (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              No classes scheduled for today.
            </p>
          )}
        </div>

        {selectedSlotId && (
          <DashboardTemplate
            title={selectedSlot ? `${selectedSlot.subject} — ${selectedSlot.className}` : "Attendance"}
            subtitle={selectedSlot ? `${selectedSlot.room} · ${selectedSlot.startTime?.slice(0, 5)} – ${selectedSlot.endTime?.slice(0, 5)}` : ""}
            loading={attLoading}
            actions={
              <div className="flex gap-2">
                <Button
                  type="primary"
                  icon={<Save className="h-4 w-4" />}
                  onClick={handleSave}
                  disabled={isLocked || !hasChanges}
                  loading={recordMutation.isPending}
                >
                  Save
                </Button>
                <Button
                  danger
                  icon={<Lock className="h-4 w-4" />}
                  onClick={handleLock}
                  disabled={isLocked || records.length === 0}
                  loading={lockMutation.isPending}
                >
                  Lock
                </Button>
              </div>
            }
          >
            {isLocked && (
              <div className="mb-4 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-700">
                This attendance session is locked and cannot be modified.
              </div>
            )}

            {records.length === 0 ? (
              <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-8 text-center">
                <p className="text-sm text-[var(--color-text-secondary)]">
                  No attendance records found for this class on this date.
                </p>
              </div>
            ) : (
              <div className="space-y-2">
                {records.map((r) => (
                  <StudentAttendanceRow
                    key={r.studentEnrollmentId}
                    student={r}
                    status={currentStatuses[r.studentEnrollmentId] ?? r.status}
                    note={notes[r.studentEnrollmentId] ?? r.note ?? ""}
                    disabled={isLocked}
                    onStatusChange={(s) =>
                      setStatuses((prev) => ({ ...prev, [r.studentEnrollmentId]: s }))
                    }
                    onNoteChange={(n) =>
                      setNotes((prev) => ({ ...prev, [r.studentEnrollmentId]: n }))
                    }
                  />
                ))}
              </div>
            )}
          </DashboardTemplate>
        )}
      </div>
    </DashboardTemplate>
  );
}

function StudentAttendanceRow({
  student,
  status,
  note,
  disabled,
  onStatusChange,
  onNoteChange,
}: {
  student: ClassAttendanceStudent;
  status: number;
  note: string;
  disabled: boolean;
  onStatusChange: (status: number) => void;
  onNoteChange: (note: string) => void;
}) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-3">
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-[var(--color-text-primary)]">
          {student.studentFirstName} {student.studentLastName}
        </p>
        <p className="font-mono text-xs text-[var(--color-text-muted)]">{student.studentCode}</p>
      </div>

      <div className="flex gap-1">
        {STATUS_OPTIONS.map((opt) => {
          const Icon = opt.icon;
          const isActive = status === opt.value;
          return (
            <button
              key={opt.value}
              disabled={disabled}
              onClick={() => onStatusChange(opt.value)}
              className={cn(
                "flex items-center gap-1 rounded-lg border px-2.5 py-1.5 text-xs font-medium transition-all",
                isActive ? opt.color : "border-[var(--color-border)] text-[var(--color-text-muted)] hover:bg-[var(--color-surface)]",
                disabled && "cursor-not-allowed opacity-60"
              )}
            >
              <Icon className="h-3.5 w-3.5" />
              <span className="hidden sm:inline">{opt.label}</span>
            </button>
          );
        })}
      </div>

      <input
        type="text"
        placeholder="Note"
        value={note}
        disabled={disabled}
        onChange={(e) => onNoteChange(e.target.value)}
        className="w-24 rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] px-2 py-1 text-xs text-[var(--color-text-primary)] placeholder:text-[var(--color-text-muted)] focus:border-[var(--color-primary)] focus:outline-none disabled:opacity-50"
      />
    </div>
  );
}
