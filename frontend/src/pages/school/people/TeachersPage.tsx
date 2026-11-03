import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, GraduationCap, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, type TableProps } from "antd";
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
import { useTeachers, useCreateTeacher, useDeleteTeacher } from "@/features/teachers/hooks";
import { useCurrentSchool } from "@/features/schools/hooks";
import { TeachersApi } from "@/features/teachers/api";
import ImportExportButtons from "@/components/importExport/ImportExportButtons";
import type { TeacherListDto } from "@/types/teacher.types";
import { t } from "@/i18n";

const schema = z.object({
  employeeCode: z.string().min(1, t("Employee code is required")),
  firstName: z.string().min(1, t("First name is required")),
  lastName: z.string().min(1, t("Last name is required")),
  email: z.string().min(1, t("Email is required")).email(t("Invalid email")),
  password: z.string().min(8, t("Password must be at least 8 characters"))
    .regex(/[A-Z]/, t("Password must contain an uppercase letter."))
    .regex(/[a-z]/, t("Password must contain a lowercase letter."))
    .regex(/[0-9]/, t("Password must contain a number."))
    .regex(/[^a-zA-Z0-9]/, t("Password must contain a symbol.")),
  campusIds: z.array(z.string()).optional(),
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
  const deleteTeacher = useDeleteTeacher();
  const school = useCurrentSchool();
  const campusOptions = (school.data?.campuses ?? []).map((campus) => ({ value: campus.id, label: campus.name }));

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { employeeCode: "", firstName: "", lastName: "", email: "", password: "", campusIds: [] },
  });

  const openCreate = () => {
    reset({ employeeCode: "", firstName: "", lastName: "", email: "", password: "", campusIds: [] });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (campusOptions.length && !values.campusIds?.length) {
      toast.error(t("Select at least one campus for this teacher"));
      return;
    }
    try {
      await createTeacher.mutateAsync({ schoolId, ...values, campusIds: values.campusIds ?? [] });
      toast.success(t("Teacher created"));
      setModalOpen(false);
    } catch {
      toast.error(t("Failed to create teacher"));
    }
  };

  const columns: TableProps<TeacherListDto>["columns"] = [
    {
      title: t("Employee Code"),
      dataIndex: "employeeCode",
      key: "employeeCode",
      render: (code: string) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">{code}</span>
      ),
    },
    {
      title: t("Name"),
      key: "name",
      render: (_, record) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {record.firstName} {record.lastName}
        </span>
      ),
    },
    {
      title: t("Email"),
      dataIndex: "email",
      key: "email",
      render: (email: string) => (
        <span className="text-sm text-[var(--color-text-secondary)]">
          {email || "—"}
        </span>
      ),
    },
    {
      title: t("Employment Status"),
      dataIndex: "employmentStatus",
      key: "employmentStatus",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: t("Assigned Classes"),
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
      title: t("Actions"),
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
            onClick={() => navigate(`/people/teachers/${record.id}`)}
          >
            {t("View")}
          </Button>
          <Button
            variant="ghost"
            type="link"
            size="small"
            className="p-0 text-[var(--color-danger)]"
            onClick={() => {
              Modal.confirm({
                title: t("Delete Teacher"),
                content: t("Are you sure you want to delete {name}?", { name: `${record.firstName} ${record.lastName}` }),
                okText: t("Delete"),
                okType: "danger",
                onOk: async () => {
                  try {
                    await deleteTeacher.mutateAsync(record.id);
                    toast.success(t("Teacher deleted"));
                  } catch {
                    toast.error(t("Failed to delete teacher"));
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
      title={t("Teachers")}
      subtitle={t("Manage teacher records")}
      actions={
        <>
          <ImportExportButtons
            kind="teachers"
            schoolId={schoolId}
            createFn={(d: any) => createTeacher.mutateAsync(d)}
            exportFileName="teachers"
            exportSheetName="teachers"
            campuses={school.data?.campuses ?? []}
            fetchAll={async () => {
              const all: TeacherListDto[] = [];
              let currentPage = 1;
              let totalPages = 1;
              do {
                const result = await TeachersApi.getTeachers({ schoolId, searchTerm: search, page: currentPage, pageSize: 100 });
                all.push(...result.items);
                totalPages = result.totalPages;
                currentPage += 1;
              } while (currentPage <= totalPages);
              return all;
            }}
            exportRecords={data?.items ?? []}
            exportColumns={[
              { header: "employeeCode", accessor: (r: TeacherListDto) => r.employeeCode },
              { header: "firstName", accessor: (r: TeacherListDto) => r.firstName },
              { header: "lastName", accessor: (r: TeacherListDto) => r.lastName },
              { header: "email", accessor: (r: TeacherListDto) => r.email },
              { header: "campuses", accessor: (r: TeacherListDto) => (r.campusIds ?? []).map((id) => school.data?.campuses.find((campus) => campus.id === id)?.name).filter(Boolean).join("; ") },
              { header: "hasLogin", accessor: (r: TeacherListDto) => r.email ? "Yes" : "No" },
              { header: "employmentStatus", accessor: (r: TeacherListDto) => r.employmentStatus },
              { header: "assignedClassesCount", accessor: (r: TeacherListDto) => r.assignedClassesCount ?? 0 },
            ]}
          />
          <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
            {t("Add Teacher")}
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
          placeholder={t("Search teachers...")}
          className="max-w-md"
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Failed to load teachers.")}</p>
        </div>
      ) : !isLoading && (data?.items.length ?? 0) === 0 ? (
        <EmptyState
          icon={<GraduationCap className="h-6 w-6" />}
          title={t("No teachers found")}
          description={t("Add your first teacher to get started.")}
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              {t("Add Teacher")}
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
            showTotal: (total) => t("Total {count} teachers", { count: total }),
          }}
          scroll={{ x: "max-content" }}
        />
      )}

      <Modal title={t("Add Teacher")} open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label={t("Employee Code")}
            validateStatus={errors.employeeCode ? "error" : ""}
            help={errors.employeeCode?.message}
          >
            <Controller
              name="employeeCode"
              control={control}
              render={({ field }) => <Input {...field} placeholder={t("e.g. EMP-001")} />}
            />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label={t("First Name")}
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} placeholder={t("First name")} />} />
            </Form.Item>
            <Form.Item
              label={t("Last Name")}
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller name="lastName" control={control} render={({ field }) => <Input {...field} placeholder={t("Last name")} />} />
            </Form.Item>
          </div>
          <Form.Item label={t("Campuses")}>
            <Controller name="campusIds" control={control} render={({ field }) => <Select {...field} mode="multiple" allowClear className="w-full" placeholder={t("Select the campuses where this teacher works")} options={campusOptions} disabled={!campusOptions.length} />} />
          </Form.Item>
          <Form.Item label={t("Email")} validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller name="email" control={control} render={({ field }) => <Input {...field} placeholder={t("teacher@school.com")} />} />
          </Form.Item>
          <Form.Item label={t("Password")} validateStatus={errors.password ? "error" : ""} help={errors.password?.message}>
            <Controller
              name="password"
              control={control}
              render={({ field }) => <InputPassword {...field} placeholder={t("Temporary password")} />}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={createTeacher.isPending}>
              {t("Create")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
