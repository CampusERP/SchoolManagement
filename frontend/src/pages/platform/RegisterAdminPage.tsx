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

const schema = z.object({
  schoolId: z.string().min(1, "Please select a school"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().min(1, "Email is required").email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

type FormData = z.infer<typeof schema>;

export default function RegisterAdminPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const presetSchoolId = searchParams.get("schoolId") ?? undefined;
  const [loading, setLoading] = useState(false);
  const { data: schools } = useSchools({ page: 1, pageSize: 100 });

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ defaultValues: { schoolId: presetSchoolId }, resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    setLoading(true);
    try {
      await SchoolsApi.registerAdmin(data);
      toast.success("School admin registered successfully");
      navigate(presetSchoolId ? `/platform/schools/${presetSchoolId}` : "/platform/schools");
    } catch (err: unknown) {
      const message =
        err && typeof err === "object" && "response" in err
          ? ((err as { response?: { data?: { error?: string; message?: string } } }).response?.data?.error ||
             (err as { response?: { data?: { message?: string } } }).response?.data?.message)
          : err instanceof Error ? err.message : "Failed to register admin.";
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardTemplate title="Register School Admin" subtitle="Create an admin account for a school">
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-card)]">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="School" validateStatus={errors.schoolId ? "error" : ""} help={errors.schoolId?.message}>
            <Controller
              name="schoolId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  disabled={!!presetSchoolId}
                  placeholder="Select a school"
                  size="large"
                  options={(schools?.items ?? []).map((s: School) => ({ value: s.id, label: s.name }))}
                />
              )}
            />
          </Form.Item>

          <Form.Item label="First Name" validateStatus={errors.firstName ? "error" : ""} help={errors.firstName?.message}>
            <Controller
              name="firstName"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<User className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder="First name"
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Last Name" validateStatus={errors.lastName ? "error" : ""} help={errors.lastName?.message}>
            <Controller
              name="lastName"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  placeholder="Last name"
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Email" validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<Mail className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder="admin@school.com"
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Password" validateStatus={errors.password ? "error" : ""} help={errors.password?.message}>
            <Controller
              name="password"
              control={control}
              render={({ field }) => (
                <InputPassword
                  {...field}
                  prefix={<Lock className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder="Temporary password"
                  size="large"
                />
              )}
            />
          </Form.Item>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate(presetSchoolId ? `/platform/schools/${presetSchoolId}` : "/platform/schools")}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={loading}>
              Register Admin
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
