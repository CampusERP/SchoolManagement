import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Form } from "antd";
import { Building2 } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import { SchoolsApi } from "@/features/schools/api";

const schema = z.object({
  name: z.string().min(1, "School name is required").max(200),
});

type FormData = z.infer<typeof schema>;

export default function EditSchoolPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
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
      .then((data) => reset({ name: data.name }))
      .catch(() => toast.error("Failed to load school."))
      .finally(() => setInitialLoading(false));
  }, [id, reset]);

  const onSubmit = async (data: FormData) => {
    if (!id) return;
    setLoading(true);
    try {
      await SchoolsApi.update(id, { name: data.name });
      toast.success("School updated successfully");
      navigate(`/platform/schools/${id}`);
    } catch (err: unknown) {
      const message =
        err && typeof err === "object" && "response" in err
          ? ((err as { response?: { data?: { error?: string; message?: string } } }).response?.data?.error ||
             (err as { response?: { data?: { message?: string } } }).response?.data?.message)
          : err instanceof Error ? err.message : "Failed to update school.";
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) {
    return (
      <DashboardTemplate title="Edit School" loading>
        <div />
      </DashboardTemplate>
    );
  }

  return (
    <DashboardTemplate title="Edit School" subtitle="Update school information">
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-card)]">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="School Name" validateStatus={errors.name ? "error" : ""} help={errors.name?.message}>
            <Controller
              name="name"
              control={control}
              render={({ field }) => (
                <Input
                  {...field}
                  prefix={<Building2 className="h-4 w-4 text-[var(--color-text-muted)]" />}
                  placeholder="e.g. Greenfield Academy"
                  size="large"
                />
              )}
            />
          </Form.Item>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate(`/platform/schools/${id}`)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={loading}>
              Save Changes
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
