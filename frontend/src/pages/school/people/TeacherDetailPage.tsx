import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Plus, Trash2, Hash, User, BadgeCheck, Mail } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, TimePicker, type TableProps, Space } from "antd";
import dayjs from "dayjs";
import { useForm, Controller, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import StatusBadge from "@/components/molecules/StatusBadge";
import { useAuthStore } from "@/store/authStore";
import { useTeacher, useUpdateTeacher } from "@/features/teachers/hooks";
import { useAssignTeacher, useUpdateTeachingAssignment, useDeleteTeachingAssignment } from "@/features/enrollment/hooks";
import { useSubjects, useClassrooms, useAcademicYears, useRooms } from "@/features/academics/hooks";
import type { TeachingAssignment } from "@/types/teacher.types";
import type { ScheduleSlot } from "@/types/enrollment.types";

const EMPLOYMENT_STATUSES = ["Active", "OnLeave", "Terminated"];

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

const teacherSchema = z.object({
  employeeCode: z.string().min(1, "Employee code is required"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  employmentStatus: z.enum(["Active", "OnLeave", "Terminated"], {
    errorMap: () => ({ message: "Employment status is required" }),
  }),
});

type TeacherFormData = z.infer<typeof teacherSchema>;

const assignmentSchema = z.object({
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

type AssignmentFormData = z.infer<typeof assignmentSchema>;

export default function TeacherDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const { data: teacher, isLoading, isError, refetch } = useTeacher(id);
  const updateTeacher = useUpdateTeacher();
  const assignTeacher = useAssignTeacher();
  const updateAssignment = useUpdateTeachingAssignment();
  const deleteAssignment = useDeleteTeachingAssignment();

  const { data: subjects } = useSubjects();
  const { data: classrooms } = useClassrooms({});
  const { data: years } = useAcademicYears();
  const { data: rooms } = useRooms();

  const [teacherModalOpen, setTeacherModalOpen] = useState(false);
  const [assignmentModalOpen, setAssignmentModalOpen] = useState(false);
  const [editingAssignmentId, setEditingAssignmentId] = useState<string | null>(null);

  const teacherForm = useForm<TeacherFormData>({ resolver: zodResolver(teacherSchema) });
  const assignmentForm = useForm<AssignmentFormData>({
    resolver: zodResolver(assignmentSchema),
    defaultValues: {
      teacherId: id || "",
      subjectId: "",
      classRoomId: "",
      termId: "",
      scheduleSlots: [{ dayOfWeek: "", startTime: "", endTime: "", roomId: "" }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: assignmentForm.control,
    name: "scheduleSlots",
  });

  const openTeacherEdit = () => {
    if (!teacher) return;
    teacherForm.reset({
      employeeCode: teacher.employeeCode,
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      employmentStatus: teacher.employmentStatus as "Active" | "OnLeave" | "Terminated",
    });
    setTeacherModalOpen(true);
  };

  const openAddAssignment = () => {
    assignmentForm.reset({
      teacherId: id || "",
      subjectId: "",
      classRoomId: "",
      termId: "",
      scheduleSlots: [{ dayOfWeek: "", startTime: "", endTime: "", roomId: "" }],
    });
    setEditingAssignmentId(null);
    setAssignmentModalOpen(true);
  };

  const openEditAssignment = (assignment: TeachingAssignment) => {
    const slots = (assignment.schedules ?? []).map((s) => ({
      dayOfWeek: DAYS_OF_WEEK[s.dayOfWeek] || "",
      startTime: s.startTime,
      endTime: s.endTime,
      roomId: s.roomId,
    }));

    assignmentForm.reset({
      teacherId: id || "",
      subjectId: assignment.subjectId,
      classRoomId: assignment.classRoomId,
      termId: assignment.termId,
      scheduleSlots: slots.length > 0 ? slots : [{ dayOfWeek: "", startTime: "", endTime: "", roomId: "" }],
    });
    setEditingAssignmentId(assignment.id);
    setAssignmentModalOpen(true);
  };

  const onTeacherSubmit = async (values: TeacherFormData) => {
    if (!id) return;
    try {
      await updateTeacher.mutateAsync({ teacherId: id, data: { schoolId, ...values } });
      toast.success("Teacher updated");
      setTeacherModalOpen(false);
    } catch {
      toast.error("Failed to update teacher");
    }
  };

  const onAssignmentSubmit = async (values: AssignmentFormData) => {
    if (!id) return;
    try {
      const payload = {
        schoolId,
        teacherId: id,
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

      if (editingAssignmentId) {
        await updateAssignment.mutateAsync({ ...payload, teachingAssignmentId: editingAssignmentId });
        toast.success("Assignment updated");
      } else {
        await assignTeacher.mutateAsync(payload);
        toast.success("Assignment created");
      }
      setAssignmentModalOpen(false);
      refetch();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { error?: string; errors?: Record<string, string[]> } } })
          ?.response?.data?.error ??
        Object.values(
          (err as { response?: { data?: { errors?: Record<string, string[]> } } })
            ?.response?.data?.errors ?? {}
        )
          .flat()
          .join(", ") ??
        "Failed to save assignment";
      toast.error(msg);
    }
  };

  const onDeleteAssignment = async (assignmentId: string) => {
    try {
      await deleteAssignment.mutateAsync({ schoolId, teachingAssignmentId: assignmentId });
      toast.success("Assignment deleted");
      refetch();
    } catch {
      toast.error("Failed to delete assignment");
    }
  };

  if (isLoading) {
    return (
      <DashboardTemplate title="Teacher" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !teacher) {
    return (
      <DashboardTemplate title="Teacher">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Teacher not found.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const termOptions = (years ?? []).flatMap((y) =>
    (y.terms ?? []).map((t) => ({ value: t.id, label: `${y.name} — ${t.name}` }))
  );

  const subjectOptions = (subjects ?? []).map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` }));
  const classroomOptions = (classrooms ?? []).map((c) => ({ value: c.id, label: c.name }));
  const roomOptions = (rooms ?? []).map((r) => ({ value: r.id, label: r.name }));

  const assignmentColumns: TableProps<TeachingAssignment>["columns"] = [
    {
      title: "Subject",
      key: "subject",
      render: (_, r) => r.subject ?? r.subjectName ?? "—",
    },
    {
      title: "Classroom",
      key: "classroom",
      render: (_, r) => r.classroomName ?? r.classRoomName ?? "—",
    },
    { title: "Term", dataIndex: "termName", key: "termName" },
    {
      title: "Schedule",
      key: "schedule",
      render: (_, r) => {
        const slots = r.schedules ?? [];
        if (slots.length === 0) return "—";
        return slots.map((s) => `${DAYS_OF_WEEK[s.dayOfWeek] ?? "Day"} ${s.startTime}-${s.endTime}`).join(", ");
      },
    },
    {
      title: "",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <Space>
          <button
            onClick={() => openEditAssignment(record)}
            className="rounded-lg border border-[var(--color-border)] px-2 py-1 text-xs text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]"
          >
            Edit
          </button>
          <button
            onClick={() => onDeleteAssignment(record.id)}
            className="rounded-lg border border-red-200 px-2 py-1 text-xs text-red-600 hover:bg-red-50"
          >
            Delete
          </button>
        </Space>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={`${teacher.firstName} ${teacher.lastName}`}
      subtitle={`Employee Code: ${teacher.employeeCode}`}
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />} onClick={() => navigate("/people/teachers")}>
            Back
          </Button>
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openTeacherEdit}>
            Edit Teacher
          </Button>
          <Button
            variant="secondary"
            icon={<Plus className="h-4 w-4" />}
            onClick={openAddAssignment}
          >
            Add Assignment
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        <InfoCard title="Teacher Information">
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Employee Code</dt>
              <dd className="font-mono text-sm text-[var(--color-text-primary)]">{teacher.employeeCode}</dd>
            </div>
            <div className="flex items-center gap-3">
              <User className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Name</dt>
              <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                {teacher.firstName} {teacher.lastName}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <Mail className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Email</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">
                {teacher.email || "—"}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <BadgeCheck className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Status</dt>
              <dd>
                <StatusBadge status={teacher.employmentStatus} />
              </dd>
            </div>
          </dl>
        </InfoCard>

        <InfoCard
          title="Teaching Assignments"
          extra={
            <Button
              size="small"
              icon={<Plus className="h-4 w-4" />}
              onClick={openAddAssignment}
            >
              Add
            </Button>
          }
        >
          <Table<TeachingAssignment>
            columns={assignmentColumns}
            dataSource={teacher.assignments ?? []}
            rowKey="id"
            pagination={false}
            size="small"
            locale={{ emptyText: "No assignments." }}
          />
        </InfoCard>
      </div>

      <Modal title="Edit Teacher" open={teacherModalOpen} onCancel={() => setTeacherModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={teacherForm.handleSubmit(onTeacherSubmit)} requiredMark={false}>
          <Form.Item
            label="Employee Code"
            validateStatus={teacherForm.formState.errors.employeeCode ? "error" : ""}
            help={teacherForm.formState.errors.employeeCode?.message}
          >
            <Controller name="employeeCode" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={teacherForm.formState.errors.firstName ? "error" : ""}
              help={teacherForm.formState.errors.firstName?.message}
            >
              <Controller name="firstName" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={teacherForm.formState.errors.lastName ? "error" : ""}
              help={teacherForm.formState.errors.lastName?.message}
            >
              <Controller name="lastName" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
          </div>
          <Form.Item
            label="Employment Status"
            validateStatus={teacherForm.formState.errors.employmentStatus ? "error" : ""}
            help={teacherForm.formState.errors.employmentStatus?.message}
          >
            <Controller
              name="employmentStatus"
              control={teacherForm.control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder="Select status"
                  options={EMPLOYMENT_STATUSES.map((s) => ({ value: s, label: s }))}
                />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setTeacherModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateTeacher.isPending}>
              Save
            </Button>
          </div>
        </Form>
      </Modal>

      <Modal
        title={editingAssignmentId ? "Edit Assignment" : "Add Assignment"}
        open={assignmentModalOpen}
        onCancel={() => setAssignmentModalOpen(false)}
        onOk={assignmentForm.handleSubmit(onAssignmentSubmit)}
        confirmLoading={updateAssignment.isPending || assignTeacher.isPending}
        okText={editingAssignmentId ? "Update" : "Create"}
        width={720}
        destroyOnClose
      >
        <Form layout="vertical" requiredMark={false}>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item label="Subject" validateStatus={assignmentForm.formState.errors.subjectId ? "error" : ""} help={assignmentForm.formState.errors.subjectId?.message}>
              <Controller
                name="subjectId"
                control={assignmentForm.control}
                render={({ field }) => (
                  <Select {...field} placeholder="Select subject" options={subjectOptions} className="w-full" />
                )}
              />
            </Form.Item>
            <Form.Item label="Classroom" validateStatus={assignmentForm.formState.errors.classRoomId ? "error" : ""} help={assignmentForm.formState.errors.classRoomId?.message}>
              <Controller
                name="classRoomId"
                control={assignmentForm.control}
                render={({ field }) => (
                  <Select {...field} placeholder="Select classroom" options={classroomOptions} className="w-full" />
                )}
              />
            </Form.Item>
          </div>
          <Form.Item label="Term" validateStatus={assignmentForm.formState.errors.termId ? "error" : ""} help={assignmentForm.formState.errors.termId?.message}>
            <Controller
              name="termId"
              control={assignmentForm.control}
              render={({ field }) => (
                <Select {...field} placeholder="Select term" options={termOptions} className="w-full" />
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

          {typeof assignmentForm.formState.errors.scheduleSlots?.message === "string" && (
            <p className="mb-2 text-sm text-[var(--color-danger)]">{assignmentForm.formState.errors.scheduleSlots.message}</p>
          )}

          <div className="space-y-3">
            {fields.map((slot, index) => (
              <div key={slot.id} className="flex items-end gap-3">
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.dayOfWeek`}
                    control={assignmentForm.control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder="Day"
                        className="w-full"
                        options={DAYS_OF_WEEK.map((d) => ({ value: d, label: d }))}
                        status={assignmentForm.formState.errors.scheduleSlots?.[index]?.dayOfWeek ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.startTime`}
                    control={assignmentForm.control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={assignmentForm.formState.errors.scheduleSlots?.[index]?.startTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.endTime`}
                    control={assignmentForm.control}
                    render={({ field }) => (
                      <TimePicker
                        className="w-full"
                        format="HH:mm"
                        value={field.value ? dayjs(field.value, "HH:mm") : null}
                        onChange={(t) => field.onChange(t ? t.format("HH:mm") : "")}
                        status={assignmentForm.formState.errors.scheduleSlots?.[index]?.endTime ? "error" : undefined}
                      />
                    )}
                  />
                </div>
                <div className="flex-1">
                  <Controller
                    name={`scheduleSlots.${index}.roomId`}
                    control={assignmentForm.control}
                    render={({ field }) => (
                      <Select
                        {...field}
                        placeholder="Room"
                        options={roomOptions}
                        status={assignmentForm.formState.errors.scheduleSlots?.[index]?.roomId ? "error" : undefined}
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
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
