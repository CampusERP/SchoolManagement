import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, Hash, CalendarDays, User, KeyRound, Upload, FileText, Trash2, Mail } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, DatePicker, Select, Switch, type TableProps } from "antd";
import dayjs from "dayjs";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import { useAuthStore } from "@/store/authStore";
import { useCurrentSchool } from "@/features/schools/hooks";
import { CountryDatalistInput } from "@/features/students/CountryDatalistInput";
import { useStudent, useUpdateStudent, useResetStudentPassword, useSetStudentLogin, useUploadStudentDocument } from "@/features/students/hooks";
import { useDeleteDocument } from "@/features/documents/hooks";
import { openDocument } from "@/features/documents/api";
import type { EnrollmentSummary, GuardianSummary } from "@/types/student.types";
import { t } from "@/i18n";

const schema = z.object({
  studentCode: z.string().min(1, t("Student code is required")),
  firstName: z.string().min(1, t("First name is required")),
  secondName: z.string().optional().or(z.literal("")),
  thirdName: z.string().optional().or(z.literal("")),
  lastName: z.string().min(1, t("Last name is required")),
  nationalityCountry: z.string().optional().or(z.literal("")),
  residenceCountry: z.string().optional().or(z.literal("")),
  campusId: z.string().optional(),
  dateOfBirth: z.string().min(1, t("Date of birth is required")),
  nationalId: z.string().optional().or(z.literal("")),
});

type FormData = z.infer<typeof schema>;

export default function StudentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const school = useCurrentSchool();
  const campusOptions = (school.data?.campuses ?? []).map((campus) => ({ value: campus.id, label: campus.name }));

  const { data: student, isLoading, isError } = useStudent(id);
  const updateStudent = useUpdateStudent();
  const resetStudentPassword = useResetStudentPassword();
  const setStudentLogin = useSetStudentLogin();
  const uploadDocument = useUploadStudentDocument();
  const deleteDocument = useDeleteDocument();

  const [modalOpen, setModalOpen] = useState(false);
  const [loginModalOpen, setLoginModalOpen] = useState(false);
  const [loginEnabled, setLoginEnabled] = useState(false);
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
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
      secondName: student.secondName ?? "",
      thirdName: student.thirdName ?? "",
      lastName: student.lastName,
      nationalityCountry: student.nationalityCountry ?? "",
      residenceCountry: student.residenceCountry ?? "",
      campusId: student.campusId ?? "",
      dateOfBirth: student.dateOfBirth,
      nationalId: student.nationalId ?? "",
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (!id) return;
    if (campusOptions.length && !values.campusId) {
      toast.error(t("Select a campus for this student"));
      return;
    }
    try {
      await updateStudent.mutateAsync({ studentId: id, data: { schoolId, ...values, nationalId: values.nationalId || undefined, secondName: values.secondName || undefined, thirdName: values.thirdName || undefined, nationalityCountry: values.nationalityCountry || undefined, residenceCountry: values.residenceCountry || undefined, campusId: values.campusId || undefined } });
      toast.success(t("Student updated"));
      setModalOpen(false);
    } catch {
      toast.error(t("Failed to update student"));
    }
  };

  if (isLoading) {
    return (
      <DashboardTemplate title={t("Student")} loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !student) {
    return (
      <DashboardTemplate title={t("Student")}>
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Student not found.")}</p>
        </div>
      </DashboardTemplate>
    );
  }

  const enrollmentColumns: TableProps<EnrollmentSummary>["columns"] = [
    {
      title: t("Academic Year"),
      key: "academicYear",
      render: (_, r) => r.academicYear ?? r.academicYearName ?? "—",
    },
    {
      title: t("Classroom"),
      key: "classroom",
      render: (_, r) => r.classRoom ?? r.classroomName ?? "—",
    },
    { title: t("Status"), dataIndex: "status", key: "status", render: (v: string) => t(v) },
  ];

  const guardianColumns: TableProps<GuardianSummary>["columns"] = [
    {
      title: t("Name"),
      key: "name",
      render: (_, r) => `${r.firstName} ${r.lastName}`,
    },
    { title: t("Relationship"), dataIndex: "relationshipType", key: "relationshipType", render: (v: string) => t(v) },
    {
      title: t("Primary Contact"),
      dataIndex: "isPrimaryContact",
      key: "isPrimaryContact",
      align: "center",
      render: (v: boolean) => (v ? t("Yes") : t("No")),
    },
  ];

  return (
    <DashboardTemplate
      title={[student.firstName, student.secondName, student.thirdName, student.lastName].filter(Boolean).join(" ")}
      subtitle={`${t("Student Code")}: ${student.studentCode}`}
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />} onClick={() => navigate("/people/students")}>
            {t("Back")}
          </Button>
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openEdit}>
            {t("Edit")}
          </Button>
          <Button variant="secondary" icon={<KeyRound className="h-4 w-4" />} onClick={() => {
            setLoginEnabled(student.hasLogin);
            setLoginEmail(student.email ?? "");
            setLoginPassword("");
            setLoginModalOpen(true);
          }}>{t("Manage Login")}</Button>
          {student.hasLogin && <Button variant="secondary" icon={<KeyRound className="h-4 w-4" />} loading={resetStudentPassword.isPending} onClick={async () => {
            try {
              await resetStudentPassword.mutateAsync(student.id);
              toast.success(t("Password reset link sent to the student."));
            } catch {
              toast.error(t("Failed to send password reset link."));
            }
          }}>{t("Reset password")}</Button>}
        </div>
      }
    >
      <div className="space-y-6">
        <InfoCard title={t("Student Information")}>
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Code")}</dt>
              <dd className="font-mono text-sm text-[var(--color-text-primary)]">{student.studentCode}</dd>
            </div>
            <div className="flex items-center gap-3">
              <User className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Name")}</dt>
              <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                {[student.firstName, student.secondName, student.thirdName, student.lastName].filter(Boolean).join(" ")}
              </dd>
            </div>
            <div className="flex items-center gap-3"><dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Nationality")}</dt><dd className="text-sm text-[var(--color-text-primary)]">{student.nationalityCountry || "—"}</dd></div>
            <div className="flex items-center gap-3"><dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Residence Country")}</dt><dd className="text-sm text-[var(--color-text-primary)]">{student.residenceCountry || "—"}</dd></div>
            <div className="flex items-center gap-3"><dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Campus")}</dt><dd className="text-sm text-[var(--color-text-primary)]">{school.data?.campuses.find((campus) => campus.id === student.campusId)?.name || "—"}</dd></div>
            <div className="flex items-center gap-3">
              <CalendarDays className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Date of Birth")}</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">
                {student.dateOfBirth ? dayjs(student.dateOfBirth).format("MMM D, YYYY") : "—"}
              </dd>
            </div>
            <div className="flex items-center gap-3">
              <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("National ID")}</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">{student.nationalId ?? "—"}</dd>
            </div>
            <div className="flex items-center gap-3">
              <KeyRound className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Has Login")}</dt>
              <dd className="text-sm text-[var(--color-text-primary)]">{student.hasLogin ? t("Yes") : t("No")}</dd>
            </div>
            <div className="flex items-center gap-3"><Mail className="h-4 w-4 text-[var(--color-text-muted)]" /><dt className="w-32 text-sm text-[var(--color-text-secondary)]">{t("Email")}</dt><dd className="break-all text-sm text-[var(--color-text-primary)]">{student.email || "—"}</dd></div>
          </dl>
        </InfoCard>

        <InfoCard title={t("Student Documents")}>
          <div className="space-y-3">
            <label className="inline-flex cursor-pointer items-center gap-2 rounded-md bg-[var(--color-primary)] px-3 py-2 text-sm font-medium text-white">
              <Upload className="h-4 w-4" /> {uploadDocument.isPending ? t("Uploading…") : t("Upload document")}
              <input type="file" className="hidden" accept=".pdf,.doc,.docx,.jpg,.jpeg,.png" disabled={uploadDocument.isPending} onChange={async (event) => { const file = event.target.files?.[0]; event.target.value = ""; if (!file || !id) return; try { await uploadDocument.mutateAsync({ studentId: id, file }); toast.success(t("Document uploaded")); } catch { toast.error(t("Failed to upload document")); } }} />
            </label>
            <p className="text-xs text-[var(--color-text-muted)]">{t("PDF, Word, or JPG/PNG files up to 25 MB")}</p>
            {(student.documents ?? []).length ? <ul className="divide-y divide-[var(--color-border)]">{student.documents!.map((document) => <li key={document.id} className="flex flex-wrap items-center justify-between gap-2 py-2"><button className="flex min-w-0 items-center gap-2 text-left text-sm text-[var(--color-primary)] hover:underline" onClick={async () => { try { await openDocument(schoolId, document.id); } catch { toast.error(t("Failed to open document")); } }}><FileText className="h-4 w-4 shrink-0" /><span className="truncate">{document.fileName}</span></button><div className="flex items-center gap-3 text-xs text-[var(--color-text-muted)]"><span>{Math.ceil(document.fileSizeBytes / 1024)} KB</span><Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-4 w-4" />} aria-label={t("Delete document")} onClick={() => Modal.confirm({ title: t("Delete document?"), content: document.fileName, okType: "danger", onOk: async () => { try { await deleteDocument.mutateAsync(document.id); toast.success(t("Document deleted")); } catch { toast.error(t("Failed to delete document")); } } })} /></div></li>)}</ul> : <p className="text-sm text-[var(--color-text-muted)]">{t("No documents uploaded yet.")}</p>}
          </div>
        </InfoCard>

        <InfoCard title={t("Enrollments")}>
          <Table<EnrollmentSummary>
            columns={enrollmentColumns}
            dataSource={student.enrollments ?? []}
            rowKey={(r) => r.id ?? r.enrollmentId ?? `${r.academicYearName}-${r.classroomName}`}
            pagination={false}
            size="small"
            locale={{ emptyText: t("No enrollments.") }}
          />
        </InfoCard>

        <InfoCard title={t("Guardians")}>
          <Table<GuardianSummary>
            columns={guardianColumns}
            dataSource={student.guardians ?? []}
            rowKey={(r) => r.id ?? r.parentId ?? `${r.firstName}-${r.lastName}`}
            pagination={false}
            size="small"
            locale={{ emptyText: t("No guardians linked.") }}
          />
        </InfoCard>
      </div>

      <Modal title={t("Edit Student")} open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label={t("Student Code")}
            validateStatus={errors.studentCode ? "error" : ""}
            help={errors.studentCode?.message}
          >
            <Controller name="studentCode" control={control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label={t("First Name")}
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
            <Form.Item
              label={t("Second Name")}
            >
              <Controller name="secondName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
          </div>
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 sm:gap-4">
            <Form.Item label={t("Third Name")}><Controller name="thirdName" control={control} render={({ field }) => <Input {...field} />} /></Form.Item>
            <Form.Item label={t("Fourth Name")} validateStatus={errors.lastName ? "error" : ""} help={errors.lastName?.message}><Controller name="lastName" control={control} render={({ field }) => <Input {...field} />} /></Form.Item>
            <Form.Item label={t("Nationality Country")}><Controller name="nationalityCountry" control={control} render={({ field }) => <CountryDatalistInput {...field} listId="student-detail-nationality-countries" />} /></Form.Item>
            <Form.Item label={t("Residence Country")}><Controller name="residenceCountry" control={control} render={({ field }) => <CountryDatalistInput {...field} listId="student-detail-residence-countries" />} /></Form.Item>
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
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label={t("National ID")}
            validateStatus={errors.nationalId ? "error" : ""}
            help={errors.nationalId?.message}
          >
            <Controller name="nationalId" control={control} render={({ field }) => <Input {...field} />} />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateStudent.isPending}>
              {t("Save")}
            </Button>
          </div>
        </Form>
      </Modal>

      <Modal
        title={t("Manage Login")}
        open={loginModalOpen}
        onCancel={() => setLoginModalOpen(false)}
        onOk={async () => {
          if (loginEnabled && !student.email && !loginEmail.trim()) {
            toast.error(t("Enter an email address to create a login."));
            return;
          }
          if (loginEnabled && !student.email && (loginPassword.length < 8 || !/[A-Z]/.test(loginPassword) || !/[a-z]/.test(loginPassword) || !/[0-9]/.test(loginPassword) || !/[^a-zA-Z0-9]/.test(loginPassword))) {
            toast.error(t("Password must be at least 8 characters and include uppercase, lowercase, a number, and a symbol."));
            return;
          }
          try {
            await setStudentLogin.mutateAsync({
              studentId: student.id,
              enabled: loginEnabled,
              email: loginEmail.trim() || undefined,
              password: loginPassword || undefined,
            });
            toast.success(t(loginEnabled ? "Student login enabled" : "Student login disabled"));
            setLoginModalOpen(false);
          } catch {
            toast.error(t("Failed to update student login."));
          }
        }}
        okText={t("Save")}
        confirmLoading={setStudentLogin.isPending}
        destroyOnHidden
      >
        <div className="space-y-4">
          <label className="flex items-center justify-between gap-4 text-sm font-medium text-[var(--color-text-primary)]">
            <span>{t("Has Login")}</span>
            <Switch checked={loginEnabled} onChange={setLoginEnabled} />
          </label>
          {loginEnabled && !student.email && <>
            <Form.Item label={t("Email")} required>
              <Input type="email" autoComplete="email" value={loginEmail} onChange={(event) => setLoginEmail(event.target.value)} />
            </Form.Item>
            <Form.Item label={t("Temporary password")} required>
              <InputPassword autoComplete="new-password" value={loginPassword} onChange={(event) => setLoginPassword(event.target.value)} />
            </Form.Item>
            <p className="text-xs text-[var(--color-text-muted)]">{t("A welcome email with the login details will be sent to the student.")}</p>
          </>}
          {loginEnabled && student.email && <p className="text-sm text-[var(--color-text-secondary)]">{t("Login email")}: {student.email}</p>}
        </div>
      </Modal>
    </DashboardTemplate>
  );
}
