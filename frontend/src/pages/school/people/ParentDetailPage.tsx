import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Pencil, User } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import { useAuthStore } from "@/store/authStore";
import { useParent, useUpdateParent } from "@/features/parents/hooks";
import type { ChildSummary } from "@/types/parent.types";

const schema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
});

type FormData = z.infer<typeof schema>;

export default function ParentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const { data: parent, isLoading, isError } = useParent(id);
  const updateParent = useUpdateParent();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const openEdit = () => {
    if (!parent) return;
    reset({ firstName: parent.firstName, lastName: parent.lastName });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (!id) return;
    try {
      await updateParent.mutateAsync({ parentId: id, data: { schoolId, ...values } });
      toast.success("Parent updated");
      setModalOpen(false);
    } catch {
      toast.error("Failed to update parent");
    }
  };

  if (isLoading) {
    return (
      <DashboardTemplate title="Parent" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !parent) {
    return (
      <DashboardTemplate title="Parent">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Parent not found.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const childColumns: TableProps<ChildSummary>["columns"] = [
    {
      title: "Student Code",
      dataIndex: "studentCode",
      key: "studentCode",
      render: (code: string) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">{code}</span>
      ),
    },
    {
      title: "Name",
      key: "name",
      render: (_, r) => `${r.firstName} ${r.lastName}`,
    },
  ];

  return (
    <DashboardTemplate
      title={`${parent.firstName} ${parent.lastName}`}
      subtitle="Parent details"
      actions={
        <div className="flex items-center gap-2">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />} onClick={() => navigate("/people/parents")}>
            Back
          </Button>
          <Button variant="primary" icon={<Pencil className="h-4 w-4" />} onClick={openEdit}>
            Edit
          </Button>
        </div>
      }
    >
      <div className="space-y-6">
        <InfoCard title="Parent Information">
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="flex items-center gap-3">
              <User className="h-4 w-4 text-[var(--color-text-muted)]" />
              <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Name</dt>
              <dd className="text-sm font-medium text-[var(--color-text-primary)]">
                {parent.firstName} {parent.lastName}
              </dd>
            </div>
          </dl>
        </InfoCard>

        <InfoCard title="Children">
          <Table<ChildSummary>
            columns={childColumns}
            dataSource={parent.children ?? []}
            rowKey={(r) => r.id ?? r.studentId ?? r.studentCode}
            pagination={false}
            size="small"
            locale={{ emptyText: "No children linked." }}
          />
        </InfoCard>
      </div>

      <Modal title="Edit Parent" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller name="lastName" control={control} render={({ field }) => <Input {...field} />} />
            </Form.Item>
          </div>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={updateParent.isPending}>
              Save
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
