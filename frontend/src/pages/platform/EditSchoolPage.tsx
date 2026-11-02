import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form, Select } from "antd";
import { Building2, Phone, Mail, MapPin } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, TextArea } from "@/components/atoms/Input";
import { SchoolsApi } from "@/features/schools/api";
import { getApiErrorMessage } from "@/lib/errors";
import { t } from "@/i18n";

const schema = z.object({
  name: z.string().min(1, t("School name is required")).max(200),
  address: z.string().max(500, t("Address must be 500 characters or fewer")).optional(),
  phone: z.string().max(50, t("Phone must be 50 characters or fewer")).optional(),
  email: z
    .union([z.string().email(t("Invalid email address")).max(256), z.literal("")])
    .optional(),
  status: z.enum(["Active", "Suspended"]),
});

type FormData = z.infer<typeof schema>;

export default function EditSchoolPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  useEffect(() => {
    if (!id) return;
    SchoolsApi.getById(id)
      .then((data) =>
        reset({
          name: data.name,
          address: data.address ?? "",
          phone: data.phone ?? "",
          email: data.email ?? "",
          status: data.status === "Suspended" ? "Suspended" : "Active",
        })
      )
      .catch(() => toast.error(t("Failed to load school.")))
      .finally(() => setInitialLoading(false));
  }, [id, reset]);

  const onSubmit = async (data: FormData) => {
    if (!id) return;
    setLoading(true);
    try {
      await SchoolsApi.update(id, {
        name: data.name.trim(),
        address: data.address?.trim() || null,
        phone: data.phone?.trim() || null,
        email: data.email?.trim() || null,
        status: data.status,
      });
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["schools"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard", "platform"] }),
      ]);
      toast.success(t("School updated successfully"));
      navigate(`/platform/schools/${id}`);
    } catch (err: unknown) {
      toast.error(getApiErrorMessage(err, t("Failed to update school.")));
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return (
      <DashboardTemplate title={t("Edit School")} loading>
        <div />
      </DashboardTemplate>
    );
  }

  return (
    <DashboardTemplate
      title={t("Edit School")}
      subtitle={t("Update the school's details and contact information")}
    >
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)] sm:p-8">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label={t("School Name")}
            validateStatus={errors.name ? "error" : ""}
            help={errors.name?.message}
          >
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

          <Form.Item
            label={t("Address")}
            validateStatus={errors.address ? "error" : ""}
            help={errors.address?.message}
          >
            <Controller
              name="address"
              control={control}
              render={({ field }) => (
                <TextArea
                  {...field}
                  placeholder={t("Street, city, postal code")}
                  autoSize={{ minRows: 2, maxRows: 4 }}
                />
              )}
            />
          </Form.Item>

          <div className="grid gap-x-4 sm:grid-cols-2">
            <Form.Item
              label={t("Phone")}
              validateStatus={errors.phone ? "error" : ""}
              help={errors.phone?.message}
            >
              <Controller
                name="phone"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Phone className="h-4 w-4 text-[var(--color-text-muted)]" />}
                    placeholder="+1 555 000 0000"
                    size="large"
                  />
                )}
              />
            </Form.Item>

            <Form.Item
              label={t("Email")}
              validateStatus={errors.email ? "error" : ""}
              help={errors.email?.message}
            >
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <Input
                    {...field}
                    prefix={<Mail className="h-4 w-4 text-[var(--color-text-muted)]" />}
                    placeholder={t("contact@school.com")}
                    size="large"
                  />
                )}
              />
            </Form.Item>
          </div>

          <Form.Item
            label={t("Status")}
            help={t("Suspended schools are counted as inactive in platform analytics.")}
          >
            <Controller
              name="status"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  size="large"
                  className="w-full"
                  options={[
                    { value: "Active", label: t("Active") },
                    { value: "Suspended", label: t("Suspended") },
                  ]}
                />
              )}
            />
          </Form.Item>

          <div className="mt-2 flex items-start gap-2 rounded-[var(--border-radius)] bg-[var(--color-neutral-soft)] p-3 text-xs text-[var(--color-text-secondary)]">
            <MapPin className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-text-muted)]" />
            <span>
              {t("Campuses are managed from the school's detail page.")}
            </span>
          </div>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate(`/platform/schools/${id}`)}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={loading}>
              {t("Save Changes")}
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
