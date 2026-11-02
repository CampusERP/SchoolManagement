import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Form, Select, TimePicker } from "antd";
import dayjs from "dayjs";
import { useForm, Controller, useFieldArray, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { getApiErrorMessage } from "@/lib/errors";
import { useTeachers } from "@/features/teachers/hooks";
import { useClassrooms, useAcademicYears, useSubjects, useRooms } from "@/features/academics/hooks";
import { useAssignTeacher } from "@/features/enrollment/hooks";
import { useAuthStore } from "@/store/authStore";
import type { AssignTeacherCommand } from "@/types/enrollment.types";
import { t } from "@/i18n";

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
  teacherId: z.string().min(1, t("Teacher is required")),
  subjectId: z.string().min(1, t("Subject is required")),
  classRoomId: z.string().min(1, t("Classroom is required")),
  termId: z.string().min(1, t("Term is required")),
  scheduleSlots: z
    .array(
      z.object({
        dayOfWeek: z.string().min(1, t("Day is required")),
        startTime: z.string().min(1, t("Start time is required")),
        endTime: z.string().min(1, t("End time is required")),
        roomId: z.string().min(1, t("Room is required")),
      })
    )
    .min(1, t("Add at least one schedule slot")),
});

type FormData = z.infer<typeof schema>;

export default function AssignTeacherPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [teacherSearch, setTeacherSearch] = useState("");
  const { data: teachers } = useTeachers({ searchTerm: teacherSearch, page: 1, pageSize: 50 });
  const { data: years } = useAcademicYears();
  const { control, setValue, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      teacherId: "",
      subjectId: "",
      classRoomId: "",
      termId: "",
      scheduleSlots: [{ dayOfWeek: "", startTime: "", endTime: "", roomId: "" }],
    },
  });
  const { data: classrooms, isLoading: classroomsLoading, isError: classroomsError } = useClassrooms({});
  const selectedClassRoomId = useWatch({ control, name: "classRoomId" });
  const selectedClassRoom = classrooms?.find((classroom) => classroom.id === selectedClassRoomId);
  const { data: subjects, isLoading: subjectsLoading } = useSubjects(selectedClassRoom?.gradeLevelId);
  const { data: rooms } = useRooms();
  const assignTeacher = useAssignTeacher();

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
      toast.success(t("Teacher assigned successfully"));
      navigate(`/people/teachers/${values.teacherId}`);
    } catch (error) {
      toast.error(getApiErrorMessage(error, t("Failed to assign teacher")));
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
    <DashboardTemplate title={t("Assign Teacher to Class")} subtitle={t("Assign a teacher and set the schedule")}>
      <div className="mx-auto max-w-2xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-8">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label={t("Teacher")} validateStatus={errors.teacherId ? "error" : ""} help={errors.teacherId?.message}>
            <Controller
              name="teacherId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  placeholder={t("Search and select a teacher")}
                  filterOption={false}
                  onSearch={setTeacherSearch}
                  options={teacherOptions}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item
            label={t("Classroom")}
            validateStatus={errors.classRoomId ? "error" : ""}
            help={errors.classRoomId?.message}
          >
            <Controller
              name="classRoomId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder={classroomsError ? t("Failed to load classrooms") : t("Select classroom")}
                  options={classroomOptions}
                  loading={classroomsLoading}
                  disabled={classroomsLoading || classroomsError}
                  notFoundContent={classroomsError ? t("Failed to load classrooms") : t("No classrooms found")}
                  size="large"
                  onChange={(value) => {
                    field.onChange(value);
                    setValue("subjectId", "");
                  }}
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Subject")} validateStatus={errors.subjectId ? "error" : ""} help={errors.subjectId?.message}>
            <Controller
              name="subjectId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  optionFilterProp="label"
                  placeholder={t("Select a subject")}
                  options={subjectOptions}
                  loading={subjectsLoading}
                  disabled={!selectedClassRoomId || subjectsLoading || !subjectOptions.length}
                  notFoundContent={selectedClassRoomId ? t("No subjects assigned to this classroom's grade") : t("Select a classroom first")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Term")} validateStatus={errors.termId ? "error" : ""} help={errors.termId?.message}>
            <Controller
              name="termId"
              control={control}
              render={({ field }) => (
                <Select {...field} placeholder={t("Select term")} options={termOptions} size="large" />
              )}
            />
          </Form.Item>

          <div className="mb-2 flex items-center justify-between">
            <label className="text-sm font-medium text-[var(--color-text-primary)]">{t("Schedule Slots")}</label>
            <Button
              variant="secondary"
              size="small"
              icon={<Plus className="h-4 w-4" />}
              onClick={() => append({ dayOfWeek: "", startTime: "", endTime: "", roomId: "" })}
            >
              {t("Add Slot")}
            </Button>
          </div>

          {typeof errors.scheduleSlots?.message === "string" && (
            <p className="mb-2 text-sm text-[var(--color-danger)]">{errors.scheduleSlots.message}</p>
          )}

          <div className="space-y-3">
            {fields.map((slot, index) => (
              <div key={slot.id} className="grid grid-cols-1 items-end gap-3 rounded-xl border border-[var(--color-border)] p-3 sm:grid-cols-2 lg:grid-cols-[minmax(130px,1fr)_minmax(140px,1fr)_minmax(140px,1fr)_minmax(150px,1fr)_auto]">
                <div className="min-w-0">
                  <Controller
                    name={`scheduleSlots.${index}.dayOfWeek`}
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder={t("Day")}
                        className="w-full"
                        options={DAYS_OF_WEEK.map((d) => ({ value: d, label: t(d) }))}
                        status={errors.scheduleSlots?.[index]?.dayOfWeek ? "error" : undefined}
                        size="large"
                      />
                    )}
                  />
                </div>
                <div className="min-w-0">
                  <Controller
                    name={`scheduleSlots.${index}.startTime`}
                    control={control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        size="large"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={errors.scheduleSlots?.[index]?.startTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="min-w-0">
                  <Controller
                    name={`scheduleSlots.${index}.endTime`}
                    control={control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        size="large"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={errors.scheduleSlots?.[index]?.endTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="min-w-0">
                  <Controller
                    name={`scheduleSlots.${index}.roomId`}
                    control={control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder={t("Room")}
                        className="w-full"
                        options={roomOptions}
                        status={errors.scheduleSlots?.[index]?.roomId ? "error" : undefined}
                        size="large"
                      />
                    )}
                  />
                </div>
                <Button
                  variant="ghost"
                  icon={<Trash2 className="h-4 w-4" />}
                  className="w-full justify-center sm:col-span-2 sm:w-auto lg:col-span-1"
                  disabled={fields.length === 1}
                  onClick={() => remove(index)}
                />
              </div>
            ))}
          </div>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate("/school")}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={assignTeacher.isPending}>
              {t("Assign")}
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
