import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form } from "antd";
import { Building2, Hash } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import { SchoolsApi } from "@/features/schools/api";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

const schema = z.object({
  name: z.string().min(1, t("School name is required")).max(200),
  subdomainCode: z
    .string()
    .min(1, t("Subdomain code is required"))
    .max(100)
    .regex(/^[a-z0-9-]+$/, t("Lowercase letters, numbers, and hyphens only")),
});

type FormData = z.infer<typeof schema>;

export default function CreateSchoolPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [loading, setLoading] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    setLoading(true);
    try {
      await SchoolsApi.create(data);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["schools"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard", "platform"] }),
      ]);
      toast.success(t("School created successfully"));
      navigate("/platform/schools");
    } catch (err: unknown) {
      const message =
        err && typeof err === "object" && "response" in err
          ? ((err as { response?: { data?: { error?: string; message?: string } } }).response?.data?.error ||
             (err as { response?: { data?: { message?: string } } }).response?.data?.message)
          : err instanceof Error ? err.message : t("Failed to create school.");
      toast.error(translateApiMessage(message ?? t("Failed to create school.")));
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardTemplate title={t("Create School")} subtitle={t("Add a new school to the platform")}>
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)] sm:p-8">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label={t("School Name")} validateStatus={errors.name ? "error" : ""} help={errors.name?.message}>
            <Controller
              name="name"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<Building2 className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder={t("e.g. Greenfield Academy")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label={t("Subdomain Code")} validateStatus={errors.subdomainCode ? "error" : ""} help={errors.subdomainCode?.message}>
            <Controller
              name="subdomainCode"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<Hash className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder={t("e.g. greenfield")}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate("/platform/schools")}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={loading}>
              {t("Create School")}
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
