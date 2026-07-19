import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Hash, User, BadgeCheck } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import StatusBadge from "@/components/molecules/StatusBadge";
import { useAuthStore } from "@/store/authStore";
import { useTeacher, useUpdateTeacher } from "@/features/teachers/hooks";
import type { TeachingAssignment } from "@/types/teacher.types";

const EMPLOYMENT_STATUSES = ["Active", "OnLeave", "Terminated"];

const schema = z.object({
  employeeCode: z.string().min(1, "Employee code is required"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  employmentStatus: z.string().min(1, "Employment status is required"),
});

type FormData = z.infer<typeof schema>;

export default function TeacherDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const { data: teacher, isLoading, isError } = useTeacher(id);
  const updateTeacher = useUpdateTeacher();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const openEdit = () => {
    if (!teacher) return;
    reset({
      employeeCode: teacher.employeeCode,
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      employmentStatus: teacher.employmentStatus,
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (!id) return;
    try {
      await updateTeacher.mutateAsync({ teacherId: id, data: { schoolId, ...values } });
      toast.success("Teacher updated");
      setModalOpen(false);
    } catch {
      toast.error("Failed to update teacher");
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
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openEdit}>
            Edit
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
              <BadgeCheck className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Status</dt>
              <dd>
                <StatusBadge status={teacher.employmentStatus} />
              </dd>
            </div>
          </dl>
        </InfoCard>

        <InfoCard title="Teaching Assignments">
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

      <Modal title="Edit Teacher" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label="Employee Code"
            validateStatus={errors.employeeCode ? "error" : ""}
            help={errors.employeeCode?.message}
          >
            <Controller name="employeeCode" control={control} render={({ field }) => <Input {...field} />} />
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
            label="Employment Status"
            validateStatus={errors.employmentStatus ? "error" : ""}
            help={errors.employmentStatus?.message}
          >
            <Controller
              name="employmentStatus"
              control={control}
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
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateTeacher.isPending}>
              Save
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
