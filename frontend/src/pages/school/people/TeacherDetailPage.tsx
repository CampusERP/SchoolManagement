import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Plus, Trash2, Hash, User, BadgeCheck, Mail, Upload, FileText } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, TimePicker, type TableProps, Space } from "antd";
import dayjs from "dayjs";
import { useForm, Controller, useFieldArray, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import StatusBadge from "@/components/molecules/StatusBadge";
import { useAuthStore } from "@/store/authStore";
import { useCurrentSchool } from "@/features/schools/hooks";
import { useTeacher, useUpdateTeacher, useUploadTeacherDocument } from "@/features/teachers/hooks";
import { useDeleteDocument } from "@/features/documents/hooks";
import { openDocument } from "@/features/documents/api";
import { useAssignTeacher, useUpdateTeachingAssignment, useDeleteTeachingAssignment } from "@/features/enrollment/hooks";
import { useSubjects, useClassrooms, useAcademicYears, useRooms } from "@/features/academics/hooks";
import type { TeachingAssignment } from "@/types/teacher.types";
import type { ScheduleSlot } from "@/types/enrollment.types";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

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
  employeeCode: z.string().min(1, t("Employee code is required")),
  firstName: z.string().min(1, t("First name is required")),
  lastName: z.string().min(1, t("Last name is required")),
  employmentStatus: z.enum(["Active", "OnLeave", "Terminated"], {
    errorMap: () => ({ message: t("Employment status is required") }),
  }),
  campusIds: z.array(z.string()).optional(),
});

type TeacherFormData = z.infer<typeof teacherSchema>;

const assignmentSchema = z.object({
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

type AssignmentFormData = z.infer<typeof assignmentSchema>;

export default function TeacherDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const { data: teacher, isLoading, isError, refetch } = useTeacher(id);
  const school = useCurrentSchool();
  const campusOptions = (school.data?.campuses ?? []).map((campus) => ({ value: campus.id, label: campus.name }));
  const updateTeacher = useUpdateTeacher();
  const uploadDocument = useUploadTeacherDocument();
  const deleteDocument = useDeleteDocument();
  const assignTeacher = useAssignTeacher();
  const updateAssignment = useUpdateTeachingAssignment();
  const deleteAssignment = useDeleteTeachingAssignment();

  const { data: classrooms, isLoading: classroomsLoading, isError: classroomsError } = useClassrooms({});
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
  const selectedClassRoomId = useWatch({ control: assignmentForm.control, name: "classRoomId" });
  const selectedClassRoom = classrooms?.find((classroom) => classroom.id === selectedClassRoomId);
  const { data: subjects, isLoading: subjectsLoading } = useSubjects(selectedClassRoom?.gradeLevelId);

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
      campusIds: teacher.campusIds ?? [],
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
    if (campusOptions.length && !values.campusIds?.length) {
      toast.error(t("Select at least one campus for this teacher"));
      return;
    }
    try {
      await updateTeacher.mutateAsync({ teacherId: id, data: { schoolId, ...values } });
      toast.success(t("Teacher updated"));
      setTeacherModalOpen(false);
    } catch {
      toast.error(t("Failed to update teacher"));
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
        toast.success(t("Assignment updated"));
      } else {
        await assignTeacher.mutateAsync(payload);
        toast.success(t("Assignment created"));
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
        t("Failed to save assignment");
      toast.error(translateApiMessage(msg));
    }
  };

  const onDeleteAssignment = async (assignmentId: string) => {
    try {
      await deleteAssignment.mutateAsync({ schoolId, teachingAssignmentId: assignmentId });
      toast.success(t("Assignment deleted"));
      refetch();
    } catch {
      toast.error(t("Failed to delete assignment"));
    }
  };

  if (isLoading) {
    return (
      <DashboardTemplate title={t("Teacher")} loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !teacher) {
    return (
      <DashboardTemplate title={t("Teacher")}>
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Teacher not found.")}</p>
        </div>
      </DashboardTemplate>
    );
  }

  const termOptions = (years ?? []).flatMap((y) =>
    (y.terms ?? []).map((t) => ({ value: t.id, label: `${y.name} — ${t.name}` }))
  );

  const subjectOptions = (subjects ?? []).map((s) => ({ value: s.id, label: `${s.code} — ${s.name}` }));
  const classroomOptions = (classrooms ?? []).map((c) => ({ value: c.id, label: c.name }));
  const roomOptions = (rooms ?? [])
    .filter((room) => !selectedClassRoom?.campusId || room.campusId === selectedClassRoom.campusId)
    .map((r) => ({ value: r.id, label: r.name }));

  const assignmentColumns: TableProps<TeachingAssignment>["columns"] = [
    {
      title: t("Subject"),
      key: "subject",
      render: (_, r) => r.subject ?? r.subjectName ?? "—",
    },
    {
      title: t("Classroom"),
      key: "classroom",
      render: (_, r) => r.classroomName ?? r.classRoomName ?? "—",
    },
    { title: t("Term"), dataIndex: "termName", key: "termName" },
    {
      title: t("Schedule"),
      key: "schedule",
      render: (_, r) => {
        const slots = r.schedules ?? [];
        if (slots.length === 0) return "—";
        return slots.map((s) => `${t(DAYS_OF_WEEK[s.dayOfWeek] ?? "Day")} ${s.startTime}-${s.endTime}`).join(", ");
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
            {t("Edit")}
          </button>
          <button
            onClick={() => onDeleteAssignment(record.id)}
            className="rounded-lg border border-red-200 px-2 py-1 text-xs text-red-600 hover:bg-red-50"
          >
            {t("Delete")}
          </button>
        </Space>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={`${teacher.firstName} ${teacher.lastName}`}
      subtitle={`${t("Employee Code")}: ${teacher.employeeCode}`}
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />} onClick={() => navigate("/people/teachers")}>
            {t("Back")}
          </Button>
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openTeacherEdit}>
            {t("Edit Teacher")}
          </Button>
          <Button
            variant="secondary"
            icon={<Plus className="h-4 w-4" />}
            onClick={openAddAssignment}
          >
            {t("Add Assignment")}
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        <InfoCard title={t("Teacher Information")}>
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Employee Code")}</dt>
              <dd className="font-mono text-sm text-[var(--color-text-primary)]">{teacher.employeeCode}</dd>
            </div>
            <div className="flex items-center gap-3">
              <User className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Name")}</dt>
              <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                {teacher.firstName} {teacher.lastName}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <Mail className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Email")}</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">
                {teacher.email || "—"}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <BadgeCheck className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Status")}</dt>
              <dd>
                <StatusBadge status={teacher.employmentStatus} />
              </dd>
            </div>
            <div className="flex items-center gap-3"><dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Campuses")}</dt><dd className="text-sm text-[var(--color-text-primary)]">{(teacher.campusIds ?? []).map((campusId) => school.data?.campuses.find((campus) => campus.id === campusId)?.name).filter(Boolean).join(", ") || "—"}</dd></div>
          </dl>
        </InfoCard>

        <InfoCard title={t("Teacher Documents")}>
          <div className="space-y-3">
            <label className="inline-flex cursor-pointer items-center gap-2 rounded-md bg-[var(--color-primary)] px-3 py-2 text-sm font-medium text-white">
              <Upload className="h-4 w-4" /> {uploadDocument.isPending ? t("Uploading…") : t("Upload document")}
              <input type="file" className="hidden" accept=".pdf,.doc,.docx,.jpg,.jpeg,.png" disabled={uploadDocument.isPending} onChange={async (event) => { const file = event.target.files?.[0]; event.target.value = ""; if (!file || !id) return; try { await uploadDocument.mutateAsync({ teacherId: id, file }); toast.success(t("Document uploaded")); } catch { toast.error(t("Failed to upload document")); } }} />
            </label>
            <p className="text-xs text-[var(--color-text-muted)]">{t("PDF, Word, or JPG/PNG files up to 25 MB")}</p>
            {(teacher.documents ?? []).length ? <ul className="divide-y divide-[var(--color-border)]">{teacher.documents!.map((document) => <li key={document.id} className="flex flex-wrap items-center justify-between gap-2 py-2"><button className="flex min-w-0 items-center gap-2 text-left text-sm text-[var(--color-primary)] hover:underline" onClick={async () => { try { await openDocument(schoolId, document.id); } catch { toast.error(t("Failed to open document")); } }}><FileText className="h-4 w-4 shrink-0" /><span className="truncate">{document.fileName}</span></button><div className="flex items-center gap-3 text-xs text-[var(--color-text-muted)]"><span>{Math.ceil(document.fileSizeBytes / 1024)} KB</span><Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-4 w-4" />} aria-label={t("Delete document")} onClick={() => Modal.confirm({ title: t("Delete document?"), content: document.fileName, okType: "danger", onOk: async () => { try { await deleteDocument.mutateAsync(document.id); toast.success(t("Document deleted")); } catch { toast.error(t("Failed to delete document")); } } })} /></div></li>)}</ul> : <p className="text-sm text-[var(--color-text-muted)]">{t("No documents uploaded yet.")}</p>}
          </div>
        </InfoCard>

        <InfoCard
          title={t("Teaching Assignments")}
          extra={
            <Button
              size="small"
              icon={<Plus className="h-4 w-4" />}
              onClick={openAddAssignment}
            >
              {t("Add")}
            </Button>
          }
        >
          <Table<TeachingAssignment>
            columns={assignmentColumns}
            dataSource={teacher.assignments ?? []}
            rowKey="id"
            pagination={false}
            size="small"
            locale={{ emptyText: t("No assignments.") }}
          />
        </InfoCard>
      </div>

      <Modal title={t("Edit Teacher")} open={teacherModalOpen} onCancel={() => setTeacherModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={teacherForm.handleSubmit(onTeacherSubmit)} requiredMark={false}>
          <Form.Item
            label={t("Employee Code")}
            validateStatus={teacherForm.formState.errors.employeeCode ? "error" : ""}
            help={teacherForm.formState.errors.employeeCode?.message}
          >
            <Controller name="employeeCode" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <Form.Item label={t("Campuses")}>
            <Controller name="campusIds" control={teacherForm.control} render={({ field }) => <Select {...field} mode="multiple" allowClear className="w-full" placeholder={t("Select the campuses where this teacher works")} options={campusOptions} disabled={!campusOptions.length} />} />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label={t("First Name")}
              validateStatus={teacherForm.formState.errors.firstName ? "error" : ""}
              help={teacherForm.formState.errors.firstName?.message}
            >
              <Controller name="firstName" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
            <Form.Item
              label={t("Last Name")}
              validateStatus={teacherForm.formState.errors.lastName ? "error" : ""}
              help={teacherForm.formState.errors.lastName?.message}
            >
              <Controller name="lastName" control={teacherForm.control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
          </div>
          <Form.Item
            label={t("Employment Status")}
            validateStatus={teacherForm.formState.errors.employmentStatus ? "error" : ""}
            help={teacherForm.formState.errors.employmentStatus?.message}
          >
            <Controller
              name="employmentStatus"
              control={teacherForm.control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder={t("Select status")}
                  options={EMPLOYMENT_STATUSES.map((s) => ({ value: s, label: t(s) }))}
                />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setTeacherModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateTeacher.isPending}>
              {t("Save")}
            </Button>
          </div>
        </Form>
      </Modal>

      <Modal
        title={editingAssignmentId ? t("Edit Assignment") : t("Add Assignment")}
        open={assignmentModalOpen}
        onCancel={() => setAssignmentModalOpen(false)}
        onOk={assignmentForm.handleSubmit(onAssignmentSubmit)}
        confirmLoading={updateAssignment.isPending || assignTeacher.isPending}
        okText={editingAssignmentId ? t("Update") : t("Create")}
        width={720}
        destroyOnClose
      >
        <Form layout="vertical" requiredMark={false}>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Form.Item label={t("Classroom")} validateStatus={assignmentForm.formState.errors.classRoomId ? "error" : ""} help={assignmentForm.formState.errors.classRoomId?.message}>
              <Controller
                name="classRoomId"
                control={assignmentForm.control}
                render={({ field }) => (
                  <Select
                    {...field}
                    placeholder={classroomsError ? t("Failed to load classrooms") : t("Select classroom")}
                    options={classroomOptions}
                    loading={classroomsLoading}
                    disabled={classroomsLoading || classroomsError}
                    notFoundContent={classroomsError ? t("Failed to load classrooms") : t("No classrooms found")}
                    className="w-full"
                    onChange={(value) => {
                      field.onChange(value);
                      assignmentForm.setValue("subjectId", "");
                    }}
                  />
                )}
              />
            </Form.Item>
            <Form.Item label={t("Subject")} validateStatus={assignmentForm.formState.errors.subjectId ? "error" : ""} help={assignmentForm.formState.errors.subjectId?.message}>
              <Controller
                name="subjectId"
                control={assignmentForm.control}
                render={({ field }) => (
                  <Select
                    {...field}
                    showSearch
                    optionFilterProp="label"
                    placeholder={t("Select subject")}
                    options={subjectOptions}
                    loading={subjectsLoading}
                    disabled={!selectedClassRoomId || subjectsLoading || !subjectOptions.length}
                    notFoundContent={selectedClassRoomId ? t("No subjects assigned to this classroom's grade") : t("Select a classroom first")}
                    className="w-full"
                  />
                )}
              />
            </Form.Item>
          </div>
          <Form.Item label={t("Term")} validateStatus={assignmentForm.formState.errors.termId ? "error" : ""} help={assignmentForm.formState.errors.termId?.message}>
            <Controller
              name="termId"
              control={assignmentForm.control}
              render={({ field }) => (
                <Select {...field} placeholder={t("Select term")} options={termOptions} className="w-full" />
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
                        placeholder={t("Day")}
                        className="w-full"
                        options={DAYS_OF_WEEK.map((d) => ({ value: d, label: t(d) }))}
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
                        placeholder={t("Room")}
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
