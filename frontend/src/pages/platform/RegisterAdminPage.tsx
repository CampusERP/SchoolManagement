import { useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form, Select } from "antd";
import { Building2, User, Mail, Lock } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import { useSchools } from "@/features/dashboard/hooks";
import { SchoolsApi } from "@/features/schools/api";
import type { School } from "@/types/dashboard.types";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

const schema = z.object({
  schoolId: z.string().min(1, t("Please select a school")),
  firstName: z.string().min(1, t("First name is required")),
  lastName: z.string().min(1, t("Last name is required")),
  email: z.string().min(1, t("Email is required")).email(t("Invalid email address")),
  password: z.string().min(6, t("Password must be at least 6 characters")),
});

type FormData = z.infer<typeof schema>;

export default function RegisterAdminPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const presetSchoolId = searchParams.get("schoolId") ?? undefined;
  const [loading, setLoading] = useState(false);
  const [schoolSearch, setSchoolSearch] = useState("");
  const { data: schools } = useSchools({ page: 1, pageSize: 50, search: schoolSearch });

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ defaultValues: { schoolId: presetSchoolId }, resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    setLoading(true);
    try {
      await SchoolsApi.registerAdmin(data);
      toast.success(t("School admin registered successfully"));
      navigate(presetSchoolId ? `/platform/schools/${presetSchoolId}` : "/platform/schools");
    } catch (err: unknown) {
      const message =
        err && typeof err === "object" && "response" in err
          ? ((err as { response?: { data?: { error?: string; message?: string } } }).response?.data?.error ||
             (err as { response?: { data?: { message?: string } } }).response?.data?.message)
          : err instanceof Error ? err.message : t("Failed to register admin.");
      toast.error(translateApiMessage(message ?? t("Failed to register admin.")));
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardTemplate title={t("Register School Admin")} subtitle={t("Create an admin account for a school")}>
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)] sm:p-8">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label={t("School")} validateStatus={errors.schoolId ? "error" : ""} help={errors.schoolId?.message}>
            <Controller
              name="schoolId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  disabled={!!presetSchoolId}
                  placeholder={t("Select a school")}
                  showSearch
                  filterOption={false}
                  onSearch={setSchoolSearch}
                  size="large"
                  options={(schools?.items ?? []).map((s: School) => ({ value: s.id, label: s.name }))}
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("First Name")} validateStatus={errors.firstName ? "error" : ""} help={errors.firstName?.message}>
            <Controller
              name="firstName"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<User className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder={t("First name")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Last Name")} validateStatus={errors.lastName ? "error" : ""} help={errors.lastName?.message}>
            <Controller
              name="lastName"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  placeholder={t("Last name")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Email")} validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<Mail className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder={t("admin@school.com")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Password")} validateStatus={errors.password ? "error" : ""} help={errors.password?.message}>
            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <InputPassword
                  {...field}
                  prefix={<Lock className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder={t("Temporary password")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate(presetSchoolId ? `/platform/schools/${presetSchoolId}` : "/platform/schools")}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={loading}>
              {t("Register Admin")}
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
