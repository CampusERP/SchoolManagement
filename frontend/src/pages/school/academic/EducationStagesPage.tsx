import { useState } from "react";
import { Plus, Layers } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Popconfirm, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import EmptyState from "@/components/molecules/EmptyState";
import {
  useEducationStages,
  useCreateEducationStage,
  useUpdateEducationStage,
  useDeleteEducationStage,
} from "@/features/educationStages/hooks";
import type { EducationStageDto } from "@/types/academic.types";

const schema = z.object({
  name: z.string().min(1, "Name is required").max(100),
});

type FormData = z.infer<typeof schema>;

export default function EducationStagesPage() {
  const { data, isLoading, isError } = useEducationStages();
  const createStage = useCreateEducationStage();
  const updateStage = useUpdateEducationStage();
  const deleteStage = useDeleteEducationStage();

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<EducationStageDto | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "" },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "" });
    setModalOpen(true);
  };

  const openEdit = (record: EducationStageDto) => {
    setEditing(record);
    reset({ name: record.name });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      if (editing) {
        await updateStage.mutateAsync({
          id: editing.id,
          data: { name: values.name },
        });
        toast.success("Education stage updated");
      } else {
        await createStage.mutateAsync({ name: values.name });
        toast.success("Education stage created");
      }
      setModalOpen(false);
    } catch {
      toast.error(
        editing
          ? "Failed to update education stage"
          : "Failed to create education stage"
      );
    }
  };

  const onDelete = async (id: string) => {
    try {
      await deleteStage.mutateAsync(id);
      toast.success("Education stage deleted");
    } catch {
      toast.error("Failed to delete education stage");
    }
  };

  const columns: TableProps<EducationStageDto>["columns"] = [
    {
      title: "Name",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {name}
        </span>
      ),
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 160,
      render: (_, record) => (
        <div className="flex items-center justify-center gap-3">
          <Button
            variant="ghost"
            type="link"
            size="small"
            className="p-0"
            onClick={() => openEdit(record)}
          >
            Edit
          </Button>
          <Popconfirm
            title="Delete education stage?"
            description="Grade levels linked to this stage will keep their existing assignment."
            okText="Delete"
            okButtonProps={{ danger: true }}
            onConfirm={() => onDelete(record.id)}
          >
            <Button variant="ghost" type="link" size="small" className="p-0 text-red-500">
              Delete
            </Button>
          </Popconfirm>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Education Stages"
      subtitle="Define the stages of education (e.g. Primary, Middle, Secondary)"
      loading={isLoading}
      actions={
        <Button
          variant="primary"
          icon={<Plus className="h-4 w-4" />}
          onClick={openCreate}
        >
          Add Stage
        </Button>
      }
    >
      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">
            Failed to load education stages.
          </p>
        </div>
      ) : (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<Layers className="h-6 w-6" />}
          title="No education stages yet"
          description="Add your first education stage to group grade levels."
          action={
            <Button
              variant="primary"
              icon={<Plus className="h-4 w-4" />}
              onClick={openCreate}
            >
              Add Stage
            </Button>
          }
        />
      ) : (
        <Table<EducationStageDto>
          columns={columns}
          dataSource={data ?? []}
          rowKey="id"
          pagination={false}
        />
      )}

      <Modal
        title={editing ? "Edit Education Stage" : "Add Education Stage"}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item
            label="Name"
            validateStatus={errors.name ? "error" : ""}
            help={errors.name?.message}
          >
            <Controller
              name="name"
              control={control}
              render={({ field }) => (
                <Input {...field} placeholder="e.g. Primary" />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              htmlType="submit"
              loading={
                createStage.isPending ||
                updateStage.isPending
              }
            >
              {editing ? "Save" : "Create"}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
