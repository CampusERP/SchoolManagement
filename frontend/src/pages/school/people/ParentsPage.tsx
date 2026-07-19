import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Users } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import SearchInput from "@/components/molecules/SearchInput";
import EmptyState from "@/components/molecules/EmptyState";
import { useAuthStore } from "@/store/authStore";
import { useParents, useCreateParent } from "@/features/parents/hooks";
import ImportExportButtons from "@/components/importExport/ImportExportButtons";
import type { ParentListDto } from "@/types/parent.types";

const schema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().min(1, "Email is required").email("Invalid email"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

type FormData = z.infer<typeof schema>;

export default function ParentsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const pageSize = 20;

  const { data, isLoading, isError } = useParents({ searchTerm: search, page, pageSize });
  const createParent = useCreateParent();

  const [modalOpen, setModalOpen] = useState(false);
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { firstName: "", lastName: "", email: "", password: "" },
  });

  const openCreate = () => {
    reset({ firstName: "", lastName: "", email: "", password: "" });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      await createParent.mutateAsync({ schoolId, ...values });
      toast.success("Parent created");
      setModalOpen(false);
    } catch {
      toast.error("Failed to create parent");
    }
  };

  const columns: TableProps<ParentListDto>["columns"] = [
    {
      title: "Name",
      key: "name",
      render: (_, record) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {record.firstName} {record.lastName}
        </span>
      ),
    },
    {
      title: "Children",
      key: "children",
      align: "center",
      render: (_, record) => record.childrenCount ?? 0,
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 120,
      render: (_, record) => (
        <Button
          variant="ghost"
          type="link"
          size="small"
          className="p-0"
          onClick={() => navigate(`/people/parents/${record.id}`)}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Parents"
      subtitle="Manage parent records"
      actions={
        <>
          <ImportExportButtons
            kind="parents"
            schoolId={schoolId}
            createFn={(d: any) => createParent.mutateAsync(d)}
            exportFileName="parents"
            exportSheetName="parents"
            exportRecords={data?.items ?? []}
            exportColumns={[
              { header: "firstName", accessor: (r: ParentListDto) => r.firstName },
              { header: "lastName", accessor: (r: ParentListDto) => r.lastName },
              { header: "childrenCount", accessor: (r: ParentListDto) => r.childrenCount ?? 0 },
            ]}
          />
          <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
            Add Parent
          </Button>
        </>
      }
    >
      <div className="mb-6">
        <SearchInput
          value={search}
          onChange={(v) => {
            setSearch(v);
            setPage(1);
          }}
          placeholder="Search parents..."
          className="max-w-md"
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load parents.</p>
        </div>
      ) : !isLoading && (data?.items.length ?? 0) === 0 ? (
        <EmptyState
          icon={<Users className="h-6 w-6" />}
          title="No parents found"
          description="Add your first parent to get started."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              Add Parent
            </Button>
          }
        />
      ) : (
        <Table<ParentListDto>
          columns={columns}
          dataSource={data?.items ?? []}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: page,
            pageSize,
            total: data?.totalCount ?? 0,
            onChange: setPage,
            showSizeChanger: false,
            showTotal: (total) => `Total ${total} parents`,
          }}
        />
      )}

      <Modal title="Add Parent" open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <div className="grid grid-cols-2 gap-4">
            <Form.Item
              label="First Name"
              validateStatus={errors.firstName ? "error" : ""}
              help={errors.firstName?.message}
            >
              <Controller name="firstName" control={control} render={({ field }) => <Input {...field} placeholder="First name" />} />
            </Form.Item>
            <Form.Item
              label="Last Name"
              validateStatus={errors.lastName ? "error" : ""}
              help={errors.lastName?.message}
            >
              <Controller name="lastName" control={control} render={({ field }) => <Input {...field} placeholder="Last name" />} />
            </Form.Item>
          </div>
          <Form.Item label="Email" validateStatus={errors.email ? "error" : ""} help={errors.email?.message}>
            <Controller name="email" control={control} render={({ field }) => <Input {...field} placeholder="parent@example.com" />} />
          </Form.Item>
          <Form.Item label="Password" validateStatus={errors.password ? "error" : ""} help={errors.password?.message}>
            <Controller
              name="password"
              control={control}
              render={({ field }) => <InputPassword {...field} placeholder="Temporary password" />}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={createParent.isPending}>
              Create
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
