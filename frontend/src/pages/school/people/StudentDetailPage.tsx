import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Hash, CalendarDays, User, KeyRound } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, DatePicker, type TableProps } from "antd";
import dayjs from "dayjs";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import { useAuthStore } from "@/store/authStore";
import { useStudent, useUpdateStudent } from "@/features/students/hooks";
import type { EnrollmentSummary, GuardianSummary } from "@/types/student.types";

const schema = z.object({
  studentCode: z.string().min(1, "Student code is required"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  dateOfBirth: z.string().min(1, "Date of birth is required"),
  nationalId: z.string().optional().or(z.literal("")),
});

type FormData = z.infer<typeof schema>;

export default function StudentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const { data: student, isLoading, isError } = useStudent(id);
  const updateStudent = useUpdateStudent();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const openEdit = () => {
    if (!student) return;
    reset({
      studentCode: student.studentCode,
      firstName: student.firstName,
      lastName: student.lastName,
      dateOfBirth: student.dateOfBirth,
      nationalId: student.nationalId ?? "",
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (!id) return;
    try {
      await updateStudent.mutateAsync({ studentId: id, data: { schoolId, ...values, nationalId: values.nationalId || undefined } });
      toast.success("Student updated");
      setModalOpen(false);
    } catch {
      toast.error("Failed to update student");
    }
  };

  if (isLoading) {
    return (
      <DashboardTemplate title="Student" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !student) {
    return (
      <DashboardTemplate title="Student">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Student not found.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const enrollmentColumns: TableProps<EnrollmentSummary>["columns"] = [
    {
      title: "Academic Year",
      key: "academicYear",
      render: (_, r) => r.academicYear ?? r.academicYearName ?? "—",
    },
    {
      title: "Classroom",
      key: "classroom",
      render: (_, r) => r.classRoom ?? r.classroomName ?? "—",
    },
    { title: "Status", dataIndex: "status", key: "status" },
  ];

  const guardianColumns: TableProps<GuardianSummary>["columns"] = [
    {
      title: "Name",
      key: "name",
      render: (_, r) => `${r.firstName} ${r.lastName}`,
    },
    { title: "Relationship", dataIndex: "relationshipType", key: "relationshipType" },
    {
      title: "Primary Contact",
      dataIndex: "isPrimaryContact",
      key: "isPrimaryContact",
      align: "center",
      render: (v: boolean) => (v ? "Yes" : "No"),
    },
  ];

  return (
    <DashboardTemplate
      title={`${student.firstName} ${student.lastName}`}
      subtitle={`Student Code: ${student.studentCode}`}
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />} onClick={() => navigate("/people/students")}>
            Back
          </Button>
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openEdit}>
            Edit
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        <InfoCard title="Student Information">
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Code</dt>
              <dd className="font-mono text-sm text-[var(--color-text-primary)]">{student.studentCode}</dd>
            </div>
            <div className="flex items-center gap-3">
              <User className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Name</dt>
              <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                {student.firstName} {student.lastName}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <CalendarDays className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Date of Birth</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">
                {student.dateOfBirth ? dayjs(student.dateOfBirth).format("MMM D, YYYY") : "—"}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">National ID</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">{student.nationalId ?? "—"}</dd>
            </div>
            <div className="flex items-center gap-3">
              <KeyRound className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Has Login</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">{student.hasLogin ? "Yes" : "No"}</dd>
            </div>
          </dl>
        </InfoCard>

        <InfoCard title="Enrollments">
          <Table<EnrollmentSummary>
            columns={enrollmentColumns}
            dataSource={student.enrollments ?? []}
            rowKey={(r) => r.id ?? r.enrollmentId ?? `${r.academicYearName}-${r.classroomName}`}
            pagination={false}
            size="small"
            locale={{ emptyText: "No enrollments." }}
          />
        </InfoCard>

        <InfoCard title="Guardians">
          <Table<GuardianSummary>
            columns={guardianColumns}
            dataSource={student.guardians ?? []}
            rowKey={(r) => r.id ?? r.parentId ?? `${r.firstName}-${r.lastName}`}
            pagination={false}
            size="small"
            locale={{ emptyText: "No guardians linked." }}
          />
        </InfoCard>
      </div>

      <Modal title="Edit Student" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label="Student Code"
            validateStatus={errors.studentCode ? "error" : ""}
            help={errors.studentCode?.message}
          >
            <Controller name="studentCode" control={control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller name="lastName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
          </div>
          <Form.Item
            label="Date of Birth"
            validateStatus={errors.dateOfBirth ? "error" : ""}
            help={errors.dateOfBirth?.message}
          >
            <Controller
              name="dateOfBirth"
              control={control}
              render={({ field }) => (
                <DatePicker
                  className="w-full"
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label="National ID"
            validateStatus={errors.nationalId ? "error" : ""}
            help={errors.nationalId?.message}
          >
            <Controller name="nationalId" control={control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateStudent.isPending}>
              Save
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
