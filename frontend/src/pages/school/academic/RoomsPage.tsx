import { useState } from "react";
import { Plus, DoorOpen } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, InputNumber, type TableProps } from "antd";
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

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  capacity: z
    .number({ invalid_type_error: "Capacity is required" })
    .int()
    .min(1, "Capacity must be at least 1"),
});

type FormData = z.infer<typeof schema>;

export default function RoomsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const { data, isLoading, isError } = useRooms();
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
    defaultValues: { name: "", capacity: 30 },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", capacity: 30 });
    setModalOpen(true);
  };

  const openEdit = (record: RoomDto) => {
    setEditing(record);
    reset({ name: record.name, capacity: record.capacity });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      if (editing) {
        await updateRoom.mutateAsync({ id: editing.id, data: { schoolId, ...values } });
        toast.success("Room updated");
      } else {
        await createRoom.mutateAsync({ schoolId, ...values });
        toast.success("Room created");
      }
      setModalOpen(false);
    } catch {
      toast.error(editing ? "Failed to update room" : "Failed to create room");
    }
  };

  const columns: TableProps<RoomDto>["columns"] = [
    {
      title: "Name",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    { title: "Capacity", dataIndex: "capacity", key: "capacity", align: "center" },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) => (
        <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openEdit(record)}>
          Edit
        </Button>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Rooms"
      subtitle="Manage physical rooms and facilities"
      loading={isLoading}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          Add Room
        </Button>
      }
    >
      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load rooms.</p>
        </div>
      ) : (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<DoorOpen className="h-6 w-6" />}
          title="No rooms yet"
          description="Add your first room."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              Add Room
            </Button>
          }
        />
      ) : (
        <Table<RoomDto> columns={columns} dataSource={data ?? []} rowKey="id" pagination={false} />
      )}

      <Modal
        title={editing ? "Edit Room" : "Add Room"}
        open={modalOpen}
        onCancel={() => setModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="Name" validateStatus={errors.name ? "error" : ""} help={errors.name?.message}>
            <Controller
              name="name"
              control={control}
              render={({ field }) => <Input {...field} placeholder="e.g. Room 101" />}
            />
          </Form.Item>
          <Form.Item label="Capacity" validateStatus={errors.capacity ? "error" : ""} help={errors.capacity?.message}>
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
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={createRoom.isPending || updateRoom.isPending}>
              {editing ? "Save" : "Create"}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
