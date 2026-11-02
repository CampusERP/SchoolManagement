import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  ArrowLeft,
  Pencil,
  Users,
  BookOpen,
  UserMinus,
} from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Popconfirm, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import { useAuthStore } from "@/store/authStore";
import {
  useClassrooms,
  useUpdateClassroom,
  useClassroomRoster,
  useClassroomTeachingAssignments,
  useWithdrawStudent,
} from "@/features/academics/hooks";
import type {
  RosterStudentDto,
  ClassRoomTeachingAssignmentDto,
} from "@/types/academic.types";
import { t } from "@/i18n";

const schema = z.object({
  name: z.string().min(1, t("Name is required")),
});

type FormData = z.infer<typeof schema>;

export default function ClassroomDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const [rosterPage, setRosterPage] = useState(1);
  const pageSize = 20;

  const { data: classrooms, isLoading: classroomsLoading } = useClassrooms({});
  const classroom = classrooms?.find((c) => c.id === id);

  const { data: roster, isLoading: rosterLoading } = useClassroomRoster(
    id,
    rosterPage,
    pageSize
  );
  const { data: teachingAssignments, isLoading: taLoading } =
    useClassroomTeachingAssignments(id);

  const updateClassroom = useUpdateClassroom();
  const withdrawStudent = useWithdrawStudent();

  const [editModalOpen, setEditModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "" },
  });

  const openEdit = () => {
    if (!classroom) return;
    reset({ name: classroom.name });
    setEditModalOpen(true);
  };

  const onEditSubmit = async (values: FormData) => {
    if (!id) return;
    try {
      await updateClassroom.mutateAsync({
        id,
        data: { schoolId, name: values.name },
      });
      toast.success(t("Classroom updated"));
      setEditModalOpen(false);
    } catch {
      toast.error(t("Failed to update classroom"));
    }
  };

  const handleWithdraw = async (enrollmentId: string) => {
    try {
      await withdrawStudent.mutateAsync({ enrollmentId, schoolId });
      toast.success(t("Student withdrawn from classroom"));
    } catch {
      toast.error(t("Failed to withdraw student"));
    }
  };

  if (classroomsLoading) {
    return (
      <DashboardTemplate title={t("Classroom")} loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (!classroom) {
    return (
      <DashboardTemplate title={t("Classroom")}>
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">
            {t("Classroom not found.")}
          </p>
        </div>
      </DashboardTemplate>
    );
  }

  const rosterColumns: TableProps<RosterStudentDto>["columns"] = [
    {
      title: t("Student Code"),
      dataIndex: "studentCode",
      key: "studentCode",
      render: (code: string) => (
        <span className="font-mono text-sm">{code}</span>
      ),
    },
    {
      title: t("First Name"),
      dataIndex: "firstName",
      key: "firstName",
    },
    {
      title: t("Last Name"),
      dataIndex: "lastName",
      key: "lastName",
    },
    {
      title: t("Status"),
      dataIndex: "enrollmentStatus",
      key: "enrollmentStatus",
      render: (status: string) => (
        <span
          className={
            status === "Active"
              ? "text-[var(--color-success)]"
              : "text-[var(--color-text-secondary)]"
          }
        >
          {t(status)}
        </span>
      ),
    },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) =>
        record.enrollmentStatus === "Active" ? (
          <Popconfirm
            title={t("Withdraw student")}
            description={t("Are you sure you want to withdraw this student from the classroom?")}
            onConfirm={() => handleWithdraw(record.enrollmentId)}
            okText={t("Withdraw")}
            cancelText={t("Cancel")}
          >
            <Button variant="ghost" type="link" size="small" className="p-0 text-[var(--color-danger)]">
              {t("Withdraw")}
            </Button>
          </Popconfirm>
        ) : null,
    },
  ];

  const taColumns: TableProps<ClassRoomTeachingAssignmentDto>["columns"] = [
    {
      title: t("Subject"),
      key: "subject",
      render: (_, record) => (
        <span className="font-medium">
          {record.subjectName}
          <span className="ml-1 text-[var(--color-text-muted)]">
            ({record.subjectCode})
          </span>
        </span>
      ),
    },
    {
      title: t("Teacher"),
      key: "teacher",
      render: (_, record) =>
        `${record.teacherFirstName} ${record.teacherLastName}`,
    },
    {
      title: t("Term"),
      dataIndex: "termName",
      key: "termName",
    },
  ];

  return (
    <DashboardTemplate
      title={classroom.name}
      subtitle={`${classroom.gradeLevel ?? classroom.gradeLevelName ?? "—"} · ${classroom.academicYear ?? classroom.academicYearName ?? "—"}`}
      actions={
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            icon={<ArrowLeft className="h-4 w-4" />}
            onClick={() => navigate("/academics/classrooms")}
          >
            {t("Back")}
          </Button>
          <Button
            variant="primary"
            icon={<Pencil className="h-4 w-4" />}
            onClick={openEdit}
          >
            {t("Edit")}
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        {/* Info Cards */}
        <div className="grid gap-4 sm:grid-cols-2">
          <InfoCard title={t("Classroom Information")}>
            <dl className="grid gap-3">
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  {t("Grade Level")}
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.gradeLevel ?? classroom.gradeLevelName ?? "—"}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  {t("Academic Year")}
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.academicYear ?? classroom.academicYearName ?? "—"}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  {t("Room")}
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.roomName || "—"}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <Users className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  {t("Enrolled")}
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {t("{count} students", {
                    count: String(classroom.enrolledStudents ?? classroom.studentCount ?? 0),
                  })}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  {t("Assignments")}
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.teachingAssignmentCount ?? 0} {t("teaching assignments")}
                </dd>
              </div>
            </dl>
          </InfoCard>
        </div>

        {/* Teaching Assignments */}
        <InfoCard title={t("Teaching Assignments")}>
          <Table<ClassRoomTeachingAssignmentDto>
            columns={taColumns}
            dataSource={teachingAssignments ?? []}
            rowKey="id"
            loading={taLoading}
            pagination={false}
            size="small"
            locale={{ emptyText: t("No teaching assignments.") }}
          />
        </InfoCard>

        {/* Student Roster */}
        <InfoCard
          title={t("Student Roster")}
          extra={
            <span className="text-sm text-[var(--color-text-secondary)]">
              {t("{count} students", { count: String(roster?.totalCount ?? 0) })}
            </span>
          }
        >
          <Table<RosterStudentDto>
            columns={rosterColumns}
            dataSource={roster?.items ?? []}
            rowKey="enrollmentId"
            loading={rosterLoading}
            pagination={{
              current: rosterPage,
              pageSize,
              total: roster?.totalCount ?? 0,
              onChange: setRosterPage,
              showSizeChanger: false,
            }}
            size="small"
            locale={{ emptyText: t("No students enrolled.") }}
          />
        </InfoCard>
      </div>

      {/* Edit Modal */}
      <Modal
        title={t("Edit Classroom")}
        open={editModalOpen}
        onCancel={() => setEditModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form
          layout="vertical"
          onFinish={handleSubmit(onEditSubmit)}
          requiredMark={false}
        >
          <Form.Item
            label={t("Name")}
            validateStatus={errors.name ? "error" : ""}
            help={errors.name?.message}
          >
            <Controller
              name="name"
              control={control}
              render={({ field }) => (
                <Input {...field} placeholder={t("e.g. Grade 10-A")} />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setEditModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button
              variant="primary"
              htmlType="submit"
              loading={updateClassroom.isPending}
            >
              {t("Save")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
