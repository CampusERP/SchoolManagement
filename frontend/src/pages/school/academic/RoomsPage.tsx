import { useState } from "react";
import { Plus, DoorOpen } from "lucide-react";
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
import { useRooms, useCreateRoom, useUpdateRoom } from "@/features/academics/hooks";
import type { RoomDto } from "@/types/academic.types";
import { useCurrentSchool } from "@/features/schools/hooks";
import { t } from "@/i18n";

const schema = z.object({
  name: z.string().min(1, t("Name is required")),
  campusId: z.string().optional(),
  capacity: z
    .number({ invalid_type_error: t("Capacity is required") })
    .int()
    .min(1, t("Capacity must be at least 1")),
});

type FormData = z.infer<typeof schema>;

export default function RoomsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const { data, isLoading, isError } = useRooms();
  const school = useCurrentSchool();
  const campusOptions = (school.data?.campuses ?? []).map((campus) => ({ value: campus.id, label: campus.name }));
  const createRoom = useCreateRoom();
  const updateRoom = useUpdateRoom();

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<RoomDto | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", capacity: 30, campusId: "" },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", capacity: 30, campusId: "" });
    setModalOpen(true);
  };

  const openEdit = (record: RoomDto) => {
    setEditing(record);
    reset({ name: record.name, capacity: record.capacity, campusId: record.campusId ?? "" });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (campusOptions.length && !values.campusId) {
      toast.error(t("Select a campus for this room"));
      return;
    }
    try {
      if (editing) {
        await updateRoom.mutateAsync({ id: editing.id, data: { schoolId, ...values } });
        toast.success(t("Room updated"));
      } else {
        await createRoom.mutateAsync({ schoolId, ...values });
        toast.success(t("Room created"));
      }
      setModalOpen(false);
    } catch {
      toast.error(editing ? t("Failed to update room") : t("Failed to create room"));
    }
  };

  const columns: TableProps<RoomDto>["columns"] = [
    {
      title: t("Name"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    { title: t("Capacity"), dataIndex: "capacity", key: "capacity", align: "center" },
    { title: t("Campus"), key: "campus", render: (_, record) => school.data?.campuses.find((campus) => campus.id === record.campusId)?.name ?? "—" },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) => (
        <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openEdit(record)}>
          {t("Edit")}
        </Button>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Rooms")}
      subtitle={t("Manage physical rooms and facilities")}
      loading={isLoading}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          {t("Add Room")}
        </Button>
      }
    >
      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Failed to load rooms.")}</p>
        </div>
      ) : (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<DoorOpen className="h-6 w-6" />}
          title={t("No rooms yet")}
          description={t("Add your first room.")}
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              {t("Add Room")}
            </Button>
          }
        />
      ) : (
        <Table<RoomDto> columns={columns} dataSource={data ?? []} rowKey="id" pagination={false} />
      )}

      <Modal
        title={editing ? t("Edit Room") : t("Add Room")}
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
              render={({ field }) => <Input {...field} placeholder={t("e.g. Room 101")} />}
            />
          </Form.Item>
          <Form.Item label={t("Campus")}>
            <Controller name="campusId" control={control} render={({ field }) => <Select {...field} allowClear className="w-full" placeholder={t("Select campus")} options={campusOptions} disabled={!campusOptions.length} />} />
          </Form.Item>
          <Form.Item label={t("Capacity")} validateStatus={errors.capacity ? "error" : ""} help={errors.capacity?.message}>
            <Controller
              name="capacity"
              control={control}
              render={({ field }) => (
                <InputNumber className="w-full" min={1} value={field.value} onChange={(v) => field.onChange(v ?? 1)} />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              {t("Cancel")}
            </Button>
            <Button variant="primary" htmlType="submit" loading={createRoom.isPending || updateRoom.isPending}>
              {editing ? t("Save") : t("Create")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
