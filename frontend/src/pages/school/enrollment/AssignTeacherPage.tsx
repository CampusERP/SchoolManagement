import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Form, Select, TimePicker } from "antd";
import dayjs from "dayjs";
import { useForm, Controller, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { useTeachers } from "@/features/teachers/hooks";
import { useClassrooms, useAcademicYears, useSubjects, useRooms } from "@/features/academics/hooks";
import { useAssignTeacher } from "@/features/enrollment/hooks";
import { useAuthStore } from "@/store/authStore";
import type { AssignTeacherCommand } from "@/types/enrollment.types";

const DAY_OF_WEEK_MAP: Record<string, number> = {
  Sunday: 0,
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
};

const DAYS_OF_WEEK = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

const schema = z.object({
  teacherId: z.string().min(1, "Teacher is required"),
  subjectId: z.string().min(1, "Subject is required"),
  classRoomId: z.string().min(1, "Classroom is required"),
  termId: z.string().min(1, "Term is required"),
  scheduleSlots: z
    .array(
      z.object({
        dayOfWeek: z.string().min(1, "Day is required"),
        startTime: z.string().min(1, "Start time is required"),
        endTime: z.string().min(1, "End time is required"),
        roomId: z.string().min(1, "Room is required"),
      })
    )
    .min(1, "Add at least one schedule slot"),
});

type FormData = z.infer<typeof schema>;

export default function AssignTeacherPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [teacherSearch, setTeacherSearch] = useState("");
  const { data: teachers } = useTeachers({ searchTerm: teacherSearch, page: 1, pageSize: 50 });
  const { data: years } = useAcademicYears();
  const { data: classrooms } = useClassrooms({});
  const { data: subjects } = useSubjects();
  const { data: rooms } = useRooms();
  const assignTeacher = useAssignTeacher();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      teacherId: "",
      subjectId: "",
      classRoomId: "",
      termId: "",
      scheduleSlots: [{ dayOfWeek: "", startTime: "", endTime: "", roomId: "" }],
    },
  });

  const { fields, append, remove } = useFieldArray({ control, name: "scheduleSlots" });

  const onSubmit = async (values: FormData) => {
    try {
      const payload: AssignTeacherCommand = {
        schoolId,
        teacherId: values.teacherId,
        subjectId: values.subjectId,
        classRoomId: values.classRoomId,
        termId: values.termId,
        scheduleSlots: values.scheduleSlots.map((s) => ({
          dayOfWeek: DAY_OF_WEEK_MAP[s.dayOfWeek],
          startTime: s.startTime + ":00",
          endTime: s.endTime + ":00",
          roomId: s.roomId,
        })),
      };
      await assignTeacher.mutateAsync(payload);
      toast.success("Teacher assigned successfully");
      navigate(`/people/teachers/${values.teacherId}`);
    } catch {
      toast.error("Failed to assign teacher");
    }
  };

  const teacherOptions = (teachers?.items ?? []).map((t) => ({
    value: t.id,
    label: `${t.firstName} ${t.lastName} (${t.employeeCode})`,
  }));
  const classroomOptions = (classrooms ?? []).map((c) => ({ value: c.id, label: c.name }));
  const subjectOptions = (subjects ?? []).map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` }));
  const roomOptions = (rooms ?? []).map((r) => ({ value: r.id, label: r.name }));
  const termOptions = (years ?? []).flatMap((y) =>
    (y.terms ?? []).map((t) => ({ value: t.id, label: `${y.name} — ${t.name}` }))
  );

  return (
    <DashboardTemplate title="Assign Teacher to Class" subtitle="Assign a teacher and set the schedule">
      <div className="mx-auto max-w-2xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-card)]">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="Teacher" validateStatus={errors.teacherId ? "error" : ""} help={errors.teacherId?.message}>
            <Controller
              name="teacherId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  placeholder="Search and select a teacher"
                  filterOption={false}
                  onSearch={setTeacherSearch}
                  options={teacherOptions}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Subject" validateStatus={errors.subjectId ? "error" : ""} help={errors.subjectId?.message}>
            <Controller
              name="subjectId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  placeholder="Select a subject"
                  options={subjectOptions}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Classroom"
            validateStatus={errors.classRoomId ? "error" : ""}
            help={errors.classRoomId?.message}
          >
            <Controller
              name="classRoomId"
              control={control}
              render={({ field }) => (
                <Select {...field} placeholder="Select classroom" options={classroomOptions} size="large" />
              )}
            />
          </Form.Item>

          <Form.Item label="Term" validateStatus={errors.termId ? "error" : ""} help={errors.termId?.message}>
            <Controller
              name="termId"
              control={control}
              render={({ field }) => (
                <Select {...field} placeholder="Select term" options={termOptions} size="large" />
              )}
            />
          </Form.Item>

          <div className="mb-2 flex items-center justify-between">
            <label className="text-sm font-medium text-[var(--color-text-primary)]">Schedule Slots</label>
            <Button
              variant="secondary"
              size="small"
              icon={<Plus className="h-4 w-4" />}
              onClick={() => append({ dayOfWeek: "", startTime: "", endTime: "", roomId: "" })}
            >
              Add Slot
            </Button>
          </div>

          {typeof errors.scheduleSlots?.message === "string" && (
            <p className="mb-2 text-sm text-[var(--color-danger)]">{errors.scheduleSlots.message}</p>
          )}

          <div className="space-y-3">
            {fields.map((slot, index) => (
              <div key={slot.id} className="flex items-end gap-3">
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.dayOfWeek`}
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder="Day"
                        className="w-full"
                        options={DAYS_OF_WEEK.map((d) => ({ value: d, label: d }))}
                        status={errors.scheduleSlots?.[index]?.dayOfWeek ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.startTime`}
                    control={control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={errors.scheduleSlots?.[index]?.startTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.endTime`}
                    control={control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={errors.scheduleSlots?.[index]?.endTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.roomId`}
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder="Room"
                        options={roomOptions}
                        status={errors.scheduleSlots?.[index]?.roomId ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <Button
                  variant="ghost"
                  icon={<Trash2 className="h-4 w-4" />}
                  disabled={fields.length === 1}
                  onClick={() => remove(index)}
                />
              </div>
            ))}
          </div>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate("/school")}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={assignTeacher.isPending}>
              Assign
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
