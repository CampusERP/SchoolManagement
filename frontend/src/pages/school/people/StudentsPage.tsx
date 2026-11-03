import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Users, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, DatePicker, Select, type TableProps } from "antd";
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
import { useCurrentSchool } from "@/features/schools/hooks";
import { StudentsApi } from "@/features/students/api";
import { CountryDatalistInput } from "@/features/students/CountryDatalistInput";
import ImportExportButtons from "@/components/importExport/ImportExportButtons";
import type { StudentListDto } from "@/types/student.types";
import { t } from "@/i18n";

const schema = z
  .object({
    firstName: z.string().min(1, t("First name is required")),
    secondName: z.string().optional().or(z.literal("")),
    thirdName: z.string().optional().or(z.literal("")),
    lastName: z.string().min(1, t("Last name is required")),
    nationalityCountry: z.string().optional().or(z.literal("")),
    residenceCountry: z.string().optional().or(z.literal("")),
    campusId: z.string().optional(),
    dateOfBirth: z.string().min(1, t("Date of birth is required")),
    nationalId: z.string().optional().or(z.literal("")),
    email: z.string().email(t("Invalid email")).optional().or(z.literal("")),
    password: z.string().optional().or(z.literal("")),
  })
  .refine((d) => !d.password || !!d.email, {
    message: t("Email is required when creating a login."),
    path: ["email"],
  })
  .refine((d) => (!d.email && !d.password) || (d.password && d.password.length >= 8), {
    message: t("Password must be at least 8 characters when email is provided"),
    path: ["password"],
  })
  .refine((d) => (!d.email && !d.password) || /[A-Z]/.test(d.password ?? ""), {
    message: t("Password must contain an uppercase letter."),
    path: ["password"],
  })
  .refine((d) => (!d.email && !d.password) || /[a-z]/.test(d.password ?? ""), {
    message: t("Password must contain a lowercase letter."),
    path: ["password"],
  })
  .refine((d) => (!d.email && !d.password) || /[0-9]/.test(d.password ?? ""), {
    message: t("Password must contain a number."),
    path: ["password"],
  })
  .refine((d) => (!d.email && !d.password) || /[^a-zA-Z0-9]/.test(d.password ?? ""), {
    message: t("Password must contain a symbol."),
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
    defaultValues: {
      firstName: "",
      secondName: "",
      thirdName: "",
      lastName: "",
      nationalityCountry: "",
      residenceCountry: "",
      campusId: "",
      dateOfBirth: "",
      nationalId: "",
      email: "",
      password: "",
    },
  });

  const openCreate = () => {
    reset({
      firstName: "",
      secondName: "",
      thirdName: "",
      lastName: "",
      nationalityCountry: "",
      residenceCountry: "",
      campusId: "",
      dateOfBirth: "",
      nationalId: "",
      email: "",
      password: "",
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (campusOptions.length && !values.campusId) {
      toast.error(t("Select a campus for this student"));
      return;
    }
    try {
      await createStudent.mutateAsync({
        schoolId,
        firstName: values.firstName,
        secondName: values.secondName || undefined,
        thirdName: values.thirdName || undefined,
        lastName: values.lastName,
        nationalityCountry: values.nationalityCountry || undefined,
        residenceCountry: values.residenceCountry || undefined,
        campusId: values.campusId || undefined,
        dateOfBirth: values.dateOfBirth,
        nationalId: values.nationalId || undefined,
        email: values.email || undefined,
        password: values.password || undefined,
      });
      toast.success(t("Student created"));
      setModalOpen(false);
    } catch {
      toast.error(t("Failed to create student"));
    }
  };

  const columns: TableProps<StudentListDto>["columns"] = [
    {
      title: t("Student Code"),
      dataIndex: "studentCode",
      key: "studentCode",
      render: (code: string) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">{code}</span>
      ),
    },
    {
      title: t("Name"),
      key: "name",
      render: (_, record) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {[record.firstName, record.secondName, record.thirdName, record.lastName].filter(Boolean).join(" ")}
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
      title: t("Campus"),
      dataIndex: "campusId",
      key: "campus",
      responsive: ["md"],
      render: (campusId?: string) => school.data?.campuses.find((campus) => campus.id === campusId)?.name ?? "—",
    },
    {
      title: t("Date of Birth"),
      dataIndex: "dateOfBirth",
      key: "dateOfBirth",
      render: (d: string) => (d ? dayjs(d).format("MMM D, YYYY") : "—"),
    },
    {
      title: t("Has Login"),
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
          {hasLogin ? t("Yes") : t("No")}
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
            onClick={() => navigate(`/people/students/${record.id}`)}
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
                title: t("Delete Student"),
                content: t("Are you sure you want to delete {name}?", { name: `${record.firstName} ${record.lastName}` }),
                okText: t("Delete"),
                okType: "danger",
                onOk: async () => {
                  try {
                    await deleteStudent.mutateAsync(record.id);
                    toast.success(t("Student deleted"));
                  } catch {
                    toast.error(t("Failed to delete student"));
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
      title={t("Students")}
      subtitle={t("Manage student records")}
      actions={
        <>
          <ImportExportButtons
            kind="students"
            schoolId={schoolId}
            createFn={(d: any) => createStudent.mutateAsync(d)}
            exportFileName="students"
            exportSheetName="students"
            campuses={school.data?.campuses ?? []}
            fetchAll={async () => {
              const all: StudentListDto[] = [];
              let currentPage = 1;
              let totalPages = 1;
              do {
                const result = await StudentsApi.getStudents({ schoolId, searchTerm: search, page: currentPage, pageSize: 100 });
                all.push(...result.items);
                totalPages = result.totalPages;
                currentPage += 1;
              } while (currentPage <= totalPages);
              return all;
            }}
            exportRecords={data?.items ?? []}
            exportColumns={[
              { header: "studentCode", accessor: (r: StudentListDto) => r.studentCode },
              { header: "firstName", accessor: (r: StudentListDto) => r.firstName },
              { header: "secondName", accessor: (r: StudentListDto) => r.secondName },
              { header: "thirdName", accessor: (r: StudentListDto) => r.thirdName },
              { header: "lastName", accessor: (r: StudentListDto) => r.lastName },
              { header: "dateOfBirth", accessor: (r: StudentListDto) => r.dateOfBirth ? dayjs(r.dateOfBirth).format("YYYY-MM-DD") : "" },
              { header: "nationalId", accessor: (r: StudentListDto) => r.nationalId },
              { header: "nationalityCountry", accessor: (r: StudentListDto) => r.nationalityCountry },
              { header: "residenceCountry", accessor: (r: StudentListDto) => r.residenceCountry },
              { header: "campus", accessor: (r: StudentListDto) => school.data?.campuses.find((campus) => campus.id === r.campusId)?.name ?? "" },
              { header: "hasLogin", accessor: (r: StudentListDto) => r.hasLogin ? "Yes" : "No" },
              { header: "email", accessor: (r: StudentListDto) => r.email },
            ]}
          />
          <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
            {t("Add Student")}
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
          placeholder={t("Search students...")}
          className="max-w-md"
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Failed to load students.")}</p>
        </div>
      ) : !isLoading && (data?.items.length ?? 0) === 0 ? (
        <EmptyState
          icon={<Users className="h-6 w-6" />}
          title={t("No students found")}
          description={t("Add your first student to get started.")}
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              {t("Add Student")}
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
            showTotal: (total) => t("Total {count} students", { count: total }),
          }}
          scroll={{ x: "max-content" }}
        />
      )}

      <Modal title={t("Add Student")} open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <p className="mb-3 text-xs text-[var(--color-text-muted)]">
            {t("A unique student code starting with STU- will be generated automatically.")}
          </p>
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 sm:gap-4">
            <Form.Item
              label={t("First Name")}
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller
                name="firstName"
                control={control}
                render={({ field }) => <Input {...field} placeholder={t("First name")} />}
              />
            </Form.Item>
            <Form.Item
              label={t("Second Name")}
            >
              <Controller
                name="secondName"
                control={control}
                render={({ field }) => <Input {...field} placeholder={t("Second name")} />}
              />
            </Form.Item>
          </div>
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 sm:gap-4">
            <Form.Item label={t("Third Name")}><Controller name="thirdName" control={control} render={({ field }) => <Input {...field} placeholder={t("Third name")} />} /></Form.Item>
            <Form.Item label={t("Fourth Name")} validateStatus={errors.lastName ? "error" : ""} help={errors.lastName?.message}><Controller name="lastName" control={control} render={({ field }) => <Input {...field} placeholder={t("Fourth name")} />} /></Form.Item>
            <Form.Item label={t("Nationality Country")}><Controller name="nationalityCountry" control={control} render={({ field }) => <CountryDatalistInput {...field} listId="student-nationality-countries" placeholder={t("Country of nationality")} />} /></Form.Item>
            <Form.Item label={t("Residence Country")}><Controller name="residenceCountry" control={control} render={({ field }) => <CountryDatalistInput {...field} listId="student-residence-countries" placeholder={t("Country of residence")} />} /></Form.Item>
            <Form.Item label={t("Campus")}><Controller name="campusId" control={control} render={({ field }) => <Select {...field} allowClear className="w-full" placeholder={t("Select campus")} options={campusOptions} disabled={!campusOptions.length} />} /></Form.Item>
          </div>
          <Form.Item
            label={t("Date of Birth")}
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
                  onChange={(d) => field.onChange(d ? d.format("YYYY-MM-DD") : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label={t("National ID")}
            validateStatus={errors.nationalId ? "error" : ""}
            help={errors.nationalId?.message}
          >
            <Controller
              name="nationalId"
              control={control}
              render={({ field }) => <Input {...field} placeholder={t("e.g. 1234567890")} />}
            />
          </Form.Item>
          <p className="mb-3 text-xs text-[var(--color-text-muted)]">
            {t("Optionally create a login account for this student.")}
          </p>
          <Form.Item label={t("Email")} validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller
              name="email"
              control={control}
              render={({ field }) => <Input {...field} placeholder={t("student@school.com")} />}
            />
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
            <Button variant="primary" htmlType="submit" loading={createStudent.isPending}>
              {t("Create")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
