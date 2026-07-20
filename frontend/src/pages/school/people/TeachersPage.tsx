import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, GraduationCap } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import SearchInput from "@/components/molecules/SearchInput";
import EmptyState from "@/components/molecules/EmptyState";
import StatusBadge from "@/components/molecules/StatusBadge";
import { useAuthStore } from "@/store/authStore";
import { useTeachers, useCreateTeacher } from "@/features/teachers/hooks";
import ImportExportButtons from "@/components/importExport/ImportExportButtons";
import type { TeacherListDto } from "@/types/teacher.types";

const schema = z.object({
  employeeCode: z.string().min(1, "Employee code is required"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().min(1, "Email is required").email("Invalid email"),
  password: z.string().min(8, "Password must be at least 8 characters"),
});

type FormData = z.infer<typeof schema>;

export default function TeachersPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const pageSize = 20;

  const { data, isLoading, isError } = useTeachers({ searchTerm: search, page, pageSize });
  const createTeacher = useCreateTeacher();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { employeeCode: "", firstName: "", lastName: "", email: "", password: "" },
  });

  const openCreate = () => {
    reset({ employeeCode: "", firstName: "", lastName: "", email: "", password: "" });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      await createTeacher.mutateAsync({ schoolId, ...values });
      toast.success("Teacher created");
      setModalOpen(false);
    } catch {
      toast.error("Failed to create teacher");
    }
  };

  const columns: TableProps<TeacherListDto>["columns"] = [
    {
      title: "Employee Code",
      dataIndex: "employeeCode",
      key: "employeeCode",
      render: (code: string) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">{code}</span>
      ),
    },
    {
      title: "Name",
      key: "name",
      render: (_, record) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {record.firstName} {record.lastName}
        </span>
      ),
    },
    {
      title: "Employment Status",
      dataIndex: "employmentStatus",
      key: "employmentStatus",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: "Assigned Classes",
      dataIndex: "assignedClassesCount",
      key: "assignedClassesCount",
      align: "center",
      render: (count: number) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">
          {count ?? 0}
        </span>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) => (
        <Button
          variant="ghost"
          type="link"
          size="small"
          className="p-0"
          onClick={() => navigate(`/people/teachers/${record.id}`)}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Teachers"
      subtitle="Manage teacher records"
      actions={
        <>
          <ImportExportButtons
            kind="teachers"
            schoolId={schoolId}
            createFn={(d: any) => createTeacher.mutateAsync(d)}
            exportFileName="teachers"
            exportSheetName="teachers"
            exportRecords={data?.items ?? []}
            exportColumns={[
              { header: "employeeCode", accessor: (r: TeacherListDto) => r.employeeCode },
              { header: "firstName", accessor: (r: TeacherListDto) => r.firstName },
              { header: "lastName", accessor: (r: TeacherListDto) => r.lastName },
              { header: "employmentStatus", accessor: (r: TeacherListDto) => r.employmentStatus },
            ]}
          />
          <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
            Add Teacher
          </Button>
        </>
      }
    >
      <div className="mb-6">
        <SearchInput
          value={search}
          onChange={(v) => {
            setSearch(v);
            setPage(1);
          }}
          placeholder="Search teachers..."
          className="max-w-md"
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load teachers.</p>
        </div>
      ) : !isLoading && (data?.items.length ?? 0) === 0 ? (
        <EmptyState
          icon={<GraduationCap className="h-6 w-6" />}
          title="No teachers found"
          description="Add your first teacher to get started."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              Add Teacher
            </Button>
          }
        />
      ) : (
        <Table<TeacherListDto>
          columns={columns}
          dataSource={data?.items ?? []}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: page,
            pageSize,
            total: data?.totalCount ?? 0,
            onChange: setPage,
            showSizeChanger: false,
            showTotal: (total) => `Total ${total} teachers`,
          }}
        />
      )}

      <Modal title="Add Teacher" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label="Employee Code"
            validateStatus={errors.employeeCode ? "error" : ""}
            help={errors.employeeCode?.message}
          >
            <Controller
              name="employeeCode"
              control={control}
              render={({ field }) => <Input {...field} placeholder="e.g. EMP-001" />}
            />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} placeholder="First name" />} />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller name="lastName" control={control} render={({ field }) => <Input {...field} placeholder="Last name" />} />
            </Form.Item>
          </div>
          <Form.Item label="Email" validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller name="email" control={control} render={({ field }) => <Input {...field} placeholder="teacher@school.com" />} />
          </Form.Item>
          <Form.Item label="Password" validateStatus={errors.password ? "error" : ""} help={errors.password?.message}>
            <Controller
              name="password"
              control={control}
              render={({ field }) => <InputPassword {...field} placeholder="Temporary password" />}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={createTeacher.isPending}>
              Create
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
