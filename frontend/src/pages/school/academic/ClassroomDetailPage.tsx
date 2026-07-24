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

const schema = z.object({
  name: z.string().min(1, "Name is required"),
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
      toast.success("Classroom updated");
      setEditModalOpen(false);
    } catch {
      toast.error("Failed to update classroom");
    }
  };

  const handleWithdraw = async (enrollmentId: string) => {
    try {
      await withdrawStudent.mutateAsync({ enrollmentId, schoolId });
      toast.success("Student withdrawn from classroom");
    } catch {
      toast.error("Failed to withdraw student");
    }
  };

  if (classroomsLoading) {
    return (
      <DashboardTemplate title="Classroom" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (!classroom) {
    return (
      <DashboardTemplate title="Classroom">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">
            Classroom not found.
          </p>
        </div>
      </DashboardTemplate>
    );
  }

  const rosterColumns: TableProps<RosterStudentDto>["columns"] = [
    {
      title: "Student Code",
      dataIndex: "studentCode",
      key: "studentCode",
      render: (code: string) => (
        <span className="font-mono text-sm">{code}</span>
      ),
    },
    {
      title: "First Name",
      dataIndex: "firstName",
      key: "firstName",
    },
    {
      title: "Last Name",
      dataIndex: "lastName",
      key: "lastName",
    },
    {
      title: "Status",
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
          {status}
        </span>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) =>
        record.enrollmentStatus === "Active" ? (
          <Popconfirm
            title="Withdraw student"
            description="Are you sure you want to withdraw this student from the classroom?"
            onConfirm={() => handleWithdraw(record.enrollmentId)}
            okText="Withdraw"
            cancelText="Cancel"
          >
            <Button variant="ghost" type="link" size="small" className="p-0 text-[var(--color-danger)]">
              Withdraw
            </Button>
          </Popconfirm>
        ) : null,
    },
  ];

  const taColumns: TableProps<ClassRoomTeachingAssignmentDto>["columns"] = [
    {
      title: "Subject",
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
      title: "Teacher",
      key: "teacher",
      render: (_, record) =>
        `${record.teacherFirstName} ${record.teacherLastName}`,
    },
    {
      title: "Term",
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
            Back
          </Button>
          <Button
            variant="primary"
            icon={<Pencil className="h-4 w-4" />}
            onClick={openEdit}
          >
            Edit
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        {/* Info Cards */}
        <div className="grid gap-4 sm:grid-cols-2">
          <InfoCard title="Classroom Information">
            <dl className="grid gap-3">
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  Grade Level
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.gradeLevel ?? classroom.gradeLevelName ?? "—"}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  Academic Year
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.academicYear ?? classroom.academicYearName ?? "—"}
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <Users className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  Enrolled
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.enrolledStudents ?? classroom.studentCount ?? 0}{" "}
                  students
                </dd>
              </div>
              <div className="flex items-center gap-3">
                <BookOpen className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-28 text-sm text-[var(--color-text-secondary)]">
                  Assignments
                </dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                  {classroom.teachingAssignmentCount ?? 0} teaching
                  assignments
                </dd>
              </div>
            </dl>
          </InfoCard>
        </div>

        {/* Teaching Assignments */}
        <InfoCard title="Teaching Assignments">
          <Table<ClassRoomTeachingAssignmentDto>
            columns={taColumns}
            dataSource={teachingAssignments ?? []}
            rowKey="id"
            loading={taLoading}
            pagination={false}
            size="small"
            locale={{ emptyText: "No teaching assignments." }}
          />
        </InfoCard>

        {/* Student Roster */}
        <InfoCard
          title="Student Roster"
          extra={
            <span className="text-sm text-[var(--color-text-secondary)]">
              {roster?.totalCount ?? 0} students
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
            locale={{ emptyText: "No students enrolled." }}
          />
        </InfoCard>
      </div>

      {/* Edit Modal */}
      <Modal
        title="Edit Classroom"
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
            label="Name"
            validateStatus={errors.name ? "error" : ""}
            help={errors.name?.message}
          >
            <Controller
              name="name"
              control={control}
              render={({ field }) => (
                <Input {...field} placeholder="e.g. Grade 10-A" />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setEditModalOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              htmlType="submit"
              loading={updateClassroom.isPending}
            >
              Save
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
