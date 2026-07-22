import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Users, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, DatePicker, type TableProps } from "antd";
import dayjs from "dayjs";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import SearchInput from "@/components/molecules/SearchInput";
import EmptyState from "@/components/molecules/EmptyState";
import { useAuthStore } from "@/store/authStore";
import { useStudents, useCreateStudent, useDeleteStudent } from "@/features/students/hooks";
import ImportExportButtons from "@/components/importExport/ImportExportButtons";
import type { StudentListDto } from "@/types/student.types";

const schema = z
  .object({
    studentCode: z.string().min(1, "Student code is required"),
    firstName: z.string().min(1, "First name is required"),
    lastName: z.string().min(1, "Last name is required"),
    dateOfBirth: z.string().min(1, "Date of birth is required"),
    nationalId: z.string().optional().or(z.literal("")),
    email: z.string().email("Invalid email").optional().or(z.literal("")),
    password: z.string().optional().or(z.literal("")),
  })
  .refine((d) => !d.email || (d.password && d.password.length >= 6), {
    message: "Password must be at least 6 characters when email is provided",
    path: ["password"],
  });

type FormData = z.infer<typeof schema>;

export default function StudentsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const pageSize = 20;

  const { data, isLoading, isError } = useStudents({ searchTerm: search, page, pageSize });
  const createStudent = useCreateStudent();
  const deleteStudent = useDeleteStudent();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      studentCode: "",
      firstName: "",
      lastName: "",
      dateOfBirth: "",
      nationalId: "",
      email: "",
      password: "",
    },
  });

  const openCreate = () => {
    reset({
      studentCode: "",
      firstName: "",
      lastName: "",
      dateOfBirth: "",
      nationalId: "",
      email: "",
      password: "",
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      await createStudent.mutateAsync({
        schoolId,
        studentCode: values.studentCode,
        firstName: values.firstName,
        lastName: values.lastName,
        dateOfBirth: values.dateOfBirth,
        nationalId: values.nationalId || undefined,
        email: values.email || undefined,
        password: values.password || undefined,
      });
      toast.success("Student created");
      setModalOpen(false);
    } catch {
      toast.error("Failed to create student");
    }
  };

  const columns: TableProps<StudentListDto>["columns"] = [
    {
      title: "Student Code",
      dataIndex: "studentCode",
      key: "studentCode",
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
      title: "Email",
      dataIndex: "email",
      key: "email",
      render: (email: string) => (
        <span className="text-sm text-[var(--color-text-secondary)]">
          {email || "—"}
        </span>
      ),
    },
    {
      title: "Date of Birth",
      dataIndex: "dateOfBirth",
      key: "dateOfBirth",
      render: (d: string) => (d ? dayjs(d).format("MMM D, YYYY") : "—"),
    },
    {
      title: "Has Login",
      dataIndex: "hasLogin",
      key: "hasLogin",
      align: "center",
      render: (hasLogin: boolean) => (
        <span
          className={
            hasLogin
              ? "inline-flex items-center rounded-full bg-emerald-50 px-2.5 py-0.5 text-xs font-medium text-[var(--color-success)]"
              : "inline-flex items-center rounded-full bg-gray-50 px-2.5 py-0.5 text-xs font-medium text-[var(--color-text-secondary)]"
          }
        >
          {hasLogin ? "Yes" : "No"}
        </span>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 140,
      render: (_, record) => (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="ghost"
            type="link"
            size="small"
            className="p-0"
            onClick={() => navigate(`/people/students/${record.id}`)}
          >
            View
          </Button>
          <Button
            variant="ghost"
            type="link"
            size="small"
            className="p-0 text-[var(--color-danger)]"
            onClick={() => {
              Modal.confirm({
                title: "Delete Student",
                content: `Are you sure you want to delete ${record.firstName} ${record.lastName}?`,
                okText: "Delete",
                okType: "danger",
                onOk: async () => {
                  try {
                    await deleteStudent.mutateAsync(record.id);
                    toast.success("Student deleted");
                  } catch {
                    toast.error("Failed to delete student");
                  }
                },
              });
            }}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Students"
      subtitle="Manage student records"
      actions={
        <>
          <ImportExportButtons
            kind="students"
            schoolId={schoolId}
            createFn={(d: any) => createStudent.mutateAsync(d)}
            exportFileName="students"
            exportSheetName="students"
            exportRecords={data?.items ?? []}
            exportColumns={[
              { header: "studentCode", accessor: (r: StudentListDto) => r.studentCode },
              { header: "firstName", accessor: (r: StudentListDto) => r.firstName },
              { header: "lastName", accessor: (r: StudentListDto) => r.lastName },
              { header: "dateOfBirth", accessor: (r: StudentListDto) => r.dateOfBirth },
            ]}
          />
          <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
            Add Student
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
          placeholder="Search students..."
          className="max-w-md"
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load students.</p>
        </div>
      ) : !isLoading && (data?.items.length ?? 0) === 0 ? (
        <EmptyState
          icon={<Users className="h-6 w-6" />}
          title="No students found"
          description="Add your first student to get started."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              Add Student
            </Button>
          }
        />
      ) : (
        <Table<StudentListDto>
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
            showTotal: (total) => `Total ${total} students`,
          }}
        />
      )}

      <Modal title="Add Student" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label="Student Code"
            validateStatus={errors.studentCode ? "error" : ""}
            help={errors.studentCode?.message}
          >
            <Controller
              name="studentCode"
              control={control}
              render={({ field }) => <Input {...field} placeholder="e.g. STU-001" />}
            />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller
                name="firstName"
                control={control}
                render={({ field }) => <Input {...field} placeholder="First name" />}
              />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller
                name="lastName"
                control={control}
                render={({ field }) => <Input {...field} placeholder="Last name" />}
              />
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
            <Controller
              name="nationalId"
              control={control}
              render={({ field }) => <Input {...field} placeholder="e.g. 1234567890" />}
            />
          </Form.Item>
          <p className="mb-3 text-xs text-[var(--color-text-muted)]">
            Optionally create a login account for this student.
          </p>
          <Form.Item label="Email" validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller
              name="email"
              control={control}
              render={({ field }) => <Input {...field} placeholder="student@school.com" />}
            />
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
            <Button variant="primary" htmlType="submit" loading={createStudent.isPending}>
              Create
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
