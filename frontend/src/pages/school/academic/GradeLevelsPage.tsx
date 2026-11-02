import { useState } from "react";
import { Plus, GraduationCap } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, InputNumber, Select, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import EmptyState from "@/components/molecules/EmptyState";
import { useAuthStore } from "@/store/authStore";
import {
  useGradeLevels,
  useCreateGradeLevel,
  useUpdateGradeLevel,
} from "@/features/academics/hooks";
import { useEducationStages } from "@/features/educationStages/hooks";
import type { GradeLevelDto } from "@/types/academic.types";
import { t } from "@/i18n";

const schema = z.object({
  name: z.string().min(1, t("Name is required")),
  sequence: z
    .number({ invalid_type_error: t("Sequence is required") })
    .int()
    .min(1, t("Sequence must be at least 1")),
  educationStageId: z.string().min(1, t("Education stage is required")),
});

type FormData = z.infer<typeof schema>;

export default function GradeLevelsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const { data, isLoading, isError } = useGradeLevels();
  const { data: stages } = useEducationStages();
  const createGradeLevel = useCreateGradeLevel();
  const updateGradeLevel = useUpdateGradeLevel();

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<GradeLevelDto | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      sequence: 1,
      educationStageId: stages?.[0]?.id ?? "",
    },
  });

  const openCreate = () => {
    setEditing(null);
    reset({
      name: "",
      sequence: 1,
      educationStageId: stages?.[0]?.id ?? "",
    });
    setModalOpen(true);
  };

  const openEdit = (record: GradeLevelDto) => {
    setEditing(record);
    reset({
      name: record.name,
      sequence: record.sequence,
      educationStageId: record.educationStageId,
    });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      if (editing) {
        await updateGradeLevel.mutateAsync({
          id: editing.id,
          data: { schoolId, name: values.name, sequence: values.sequence },
        });
        toast.success(t("Grade level updated"));
      } else {
        const nextSequence = Math.max(0, ...(data ?? [])
          .filter((grade) => grade.educationStageId === values.educationStageId)
          .map((grade) => grade.sequence)) + 1;
        await createGradeLevel.mutateAsync({
          schoolId,
          educationStageId: values.educationStageId,
          name: values.name,
          sequence: nextSequence,
        });
        toast.success(t("Grade level created"));
      }
      setModalOpen(false);
    } catch {
      toast.error(editing ? t("Failed to update grade level") : t("Failed to create grade level"));
    }
  };

  const stageOptions = (stages ?? []).map((s) => ({
    value: s.id,
    label: s.name,
  }));

  const stageName = (id: string) =>
    stages?.find((s) => s.id === id)?.name ?? "—";

  const columns: TableProps<GradeLevelDto>["columns"] = [
    {
      title: t("Name"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: t("Education Stage"),
      key: "educationStage",
      render: (_, record) =>
        record.educationStage ?? stageName(record.educationStageId),
    },
    { title: t("Sequence"), dataIndex: "sequence", key: "sequence", align: "center" },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) => (
        <div className="flex justify-center gap-2">
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openEdit(record)}>
            {t("Edit")}
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Grade Levels")}
      subtitle={t("Configure grade levels")}
      loading={isLoading}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          {t("Add Grade Level")}
        </Button>
      }
    >
      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Failed to load grade levels.")}</p>
        </div>
      ) : (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<GraduationCap className="h-6 w-6" />}
          title={t("No grade levels yet")}
          description={t("Add your first grade level.")}
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              {t("Add Grade Level")}
            </Button>
          }
        />
      ) : (
        <Table<GradeLevelDto> columns={columns} dataSource={data ?? []} rowKey="id" pagination={false} />
      )}

      <Modal
        title={editing ? t("Edit Grade Level") : t("Add Grade Level")}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label={t("Name")} validateStatus={errors.name ? "error" : ""} help={errors.name?.message}>
            <Controller
              name="name"
              control={control}
              render={({ field }) => <Input {...field} placeholder={t("e.g. Grade 1")} />}
            />
          </Form.Item>
          <Form.Item label={t("Sequence")} validateStatus={errors.sequence ? "error" : ""} help={errors.sequence?.message}>
            <Controller
              name="sequence"
              control={control}
              render={({ field }) => (
                <InputNumber className="w-full" min={1} value={field.value} onChange={(v) => field.onChange(v ?? 1)} />
              )}
            />
          </Form.Item>
          <Form.Item
            label={t("Education Stage")}
            validateStatus={errors.educationStageId ? "error" : ""}
            help={errors.educationStageId?.message ?? t("The education stage this grade level belongs to.")}
          >
            <Controller
              name="educationStageId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder={t("Select education stage")}
                  options={stageOptions}
                />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button
              variant="primary"
              htmlType="submit"
              loading={createGradeLevel.isPending || updateGradeLevel.isPending}
            >
              {editing ? t("Save") : t("Create")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
