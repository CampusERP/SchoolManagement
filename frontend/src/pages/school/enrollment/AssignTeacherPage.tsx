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
import { Input } from "@/components/atoms/Input";
import { useTeachers } from "@/features/teachers/hooks";
import { useClassrooms, useAcademicYears } from "@/features/academics/hooks";
import { useAssignTeacher } from "@/features/enrollment/hooks";
import { useAuthStore } from "@/store/authStore";

const DAYS_OF_WEEK = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

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
      scheduleSlots: [{ dayOfWeek: "", startTime: "", endTime: "" }],
    },
  });

  const { fields, append, remove } = useFieldArray({ control, name: "scheduleSlots" });

  const onSubmit = async (values: FormData) => {
    try {
      await assignTeacher.mutateAsync({ schoolId, ...values });
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
              render={({ field }) => <Input {...field} placeholder="Subject ID" size="large" />}
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
              onClick={() => append({ dayOfWeek: "", startTime: "", endTime: "" })}
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
