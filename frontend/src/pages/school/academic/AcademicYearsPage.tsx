import { useState } from "react";
import { Plus } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, DatePicker, Checkbox, InputNumber, type TableProps } from "antd";
import dayjs from "dayjs";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import EmptyState from "@/components/molecules/EmptyState";
import StatusBadge from "@/components/molecules/StatusBadge";
import { useAuthStore } from "@/store/authStore";
import {
  useAcademicYears,
  useCreateAcademicYear,
  useCreateTerm,
} from "@/features/academics/hooks";
import type { AcademicYearDto, TermDto } from "@/types/academic.types";

const yearSchema = z.object({
  name: z.string().min(1, "Name is required"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
  setAsCurrent: z.boolean(),
});

type YearFormData = z.infer<typeof yearSchema>;

const termSchema = z.object({
  name: z.string().min(1, "Name is required"),
  sequence: z.number({ invalid_type_error: "Sequence is required" }).int().min(1, "Sequence must be at least 1"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
});

type TermFormData = z.infer<typeof termSchema>;

export default function AcademicYearsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const { data, isLoading, isError } = useAcademicYears();
  const createYear = useCreateAcademicYear();
  const createTerm = useCreateTerm();

  const [yearModalOpen, setYearModalOpen] = useState(false);
  const [termModalYearId, setTermModalYearId] = useState<string | null>(null);

  const yearForm = useForm<YearFormData>({
    resolver: zodResolver(yearSchema),
    defaultValues: { name: "", startDate: "", endDate: "", setAsCurrent: false },
  });

  const termForm = useForm<TermFormData>({
    resolver: zodResolver(termSchema),
    defaultValues: { name: "", sequence: 1, startDate: "", endDate: "" },
  });

  const openYearModal = () => {
    yearForm.reset({ name: "", startDate: "", endDate: "", setAsCurrent: false });
    setYearModalOpen(true);
  };

  const openTermModal = (yearId: string) => {
    termForm.reset({ name: "", sequence: 1, startDate: "", endDate: "" });
    setTermModalYearId(yearId);
  };

  const onCreateYear = async (values: YearFormData) => {
    try {
      await createYear.mutateAsync({ schoolId, ...values });
      toast.success("Academic year created");
      setYearModalOpen(false);
    } catch {
      toast.error("Failed to create academic year");
    }
  };

  const onCreateTerm = async (values: TermFormData) => {
    if (!termModalYearId) return;
    try {
      await createTerm.mutateAsync({
        schoolId,
        academicYearId: termModalYearId,
        ...values,
      });
      toast.success("Term created");
      setTermModalYearId(null);
    } catch {
      toast.error("Failed to create term");
    }
  };

  const columns: TableProps<AcademicYearDto>["columns"] = [
    {
      title: "Name",
      dataIndex: "name",
      key: "name",
      render: (name: string, record) => (
        <span className="font-medium text-[var(--color-text-primary)]">
          {name}
          {record.isCurrent && (
            <span className="ml-2 rounded-full bg-[var(--color-primary-light)] px-2 py-0.5 text-xs font-medium text-[var(--color-primary)]">
              Current
            </span>
          )}
        </span>
      ),
    },
    {
      title: "Start Date",
      dataIndex: "startDate",
      key: "startDate",
      render: (d: string) => dayjs(d).format("MMM D, YYYY"),
    },
    {
      title: "End Date",
      dataIndex: "endDate",
      key: "endDate",
      render: (d: string) => dayjs(d).format("MMM D, YYYY"),
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status: string | undefined) =>
        status ? <StatusBadge status={status} /> : "—",
    },
    {
      title: "Terms",
      key: "terms",
      align: "center",
      render: (_, record) => record.terms?.length ?? 0,
    },
  ];

  const termColumns: TableProps<TermDto>["columns"] = [
    { title: "Name", dataIndex: "name", key: "name" },
    { title: "Sequence", dataIndex: "sequence", key: "sequence", align: "center" },
    {
      title: "Start Date",
      dataIndex: "startDate",
      key: "startDate",
      render: (d: string) => dayjs(d).format("MMM D, YYYY"),
    },
    {
      title: "End Date",
      dataIndex: "endDate",
      key: "endDate",
      render: (d: string) => dayjs(d).format("MMM D, YYYY"),
    },
  ];

  return (
    <DashboardTemplate
      title="Academic Years"
      subtitle="Manage academic years and terms"
      loading={isLoading}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openYearModal}>
          Create Academic Year
        </Button>
      }
    >
      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load academic years.</p>
        </div>
      ) : (data?.length ?? 0) === 0 ? (
        <EmptyState
          title="No academic years yet"
          description="Create your first academic year to get started."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openYearModal}>
              Create Academic Year
            </Button>
          }
        />
      ) : (
        <Table<AcademicYearDto>
          columns={columns}
          dataSource={data ?? []}
          rowKey="id"
          pagination={false}
          expandable={{
            expandedRowRender: (record) => (
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Terms</h4>
                  <Button
                    variant="secondary"
                    size="small"
                    icon={<Plus className="h-4 w-4" />}
                    onClick={() => openTermModal(record.id)}
                  >
                    Add Term
                  </Button>
                </div>
                {(record.terms?.length ?? 0) === 0 ? (
                  <p className="text-sm text-[var(--color-text-muted)]">No terms defined.</p>
                ) : (
                  <Table<TermDto>
                    columns={termColumns}
                    dataSource={record.terms}
                    rowKey="id"
                    pagination={false}
                    size="small"
                  />
                )}
              </div>
            ),
          }}
        />
      )}

      {/* Create Academic Year Modal */}
      <Modal
        title="Create Academic Year"
        open={yearModalOpen}
        onCancel={() => setYearModalOpen(false)}
        footer={null}
        destroyOnHidden
      >
        <Form layout="vertical" onFinish={yearForm.handleSubmit(onCreateYear)} requiredMark={false}>
          <Form.Item
            label="Name"
            validateStatus={yearForm.formState.errors.name ? "error" : ""}
            help={yearForm.formState.errors.name?.message}
          >
            <Controller
              name="name"
              control={yearForm.control}
              render={({ field }) => <Input {...field} placeholder="e.g. 2025-2026" />}
            />
          </Form.Item>
          <Form.Item
            label="Start Date"
            validateStatus={yearForm.formState.errors.startDate ? "error" : ""}
            help={yearForm.formState.errors.startDate?.message}
          >
            <Controller
              name="startDate"
              control={yearForm.control}
              render={({ field }) => (
                <DatePicker
                  className="w-full"
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label="End Date"
            validateStatus={yearForm.formState.errors.endDate ? "error" : ""}
            help={yearForm.formState.errors.endDate?.message}
          >
            <Controller
              name="endDate"
              control={yearForm.control}
              render={({ field }) => (
                <DatePicker
                  className="w-full"
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item>
            <Controller
              name="setAsCurrent"
              control={yearForm.control}
              render={({ field }) => (
                <Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)}>
                  Set as current academic year
                </Checkbox>
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setYearModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={createYear.isPending}>
              Create
            </Button>
          </div>
        </Form>
      </Modal>

      {/* Create Term Modal */}
      <Modal
        title="Add Term"
        open={!!termModalYearId}
        onCancel={() => setTermModalYearId(null)}
        footer={null}
        destroyOnHidden
      >
        <Form layout="vertical" onFinish={termForm.handleSubmit(onCreateTerm)} requiredMark={false}>
          <Form.Item
            label="Name"
            validateStatus={termForm.formState.errors.name ? "error" : ""}
            help={termForm.formState.errors.name?.message}
          >
            <Controller
              name="name"
              control={termForm.control}
              render={({ field }) => <Input {...field} placeholder="e.g. Term 1" />}
            />
          </Form.Item>
          <Form.Item
            label="Sequence"
            validateStatus={termForm.formState.errors.sequence ? "error" : ""}
            help={termForm.formState.errors.sequence?.message}
          >
            <Controller
              name="sequence"
              control={termForm.control}
              render={({ field }) => (
                <InputNumber
                  className="w-full"
                  min={1}
                  value={field.value}
                  onChange={(v) => field.onChange(v ?? 1)}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label="Start Date"
            validateStatus={termForm.formState.errors.startDate ? "error" : ""}
            help={termForm.formState.errors.startDate?.message}
          >
            <Controller
              name="startDate"
              control={termForm.control}
              render={({ field }) => (
                <DatePicker
                  className="w-full"
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <Form.Item
            label="End Date"
            validateStatus={termForm.formState.errors.endDate ? "error" : ""}
            help={termForm.formState.errors.endDate?.message}
          >
            <Controller
              name="endDate"
              control={termForm.control}
              render={({ field }) => (
                <DatePicker
                  className="w-full"
                  value={field.value ? dayjs(field.value) : null}
                  onChange={(d) => field.onChange(d ? d.toISOString() : "")}
                />
              )}
            />
          </Form.Item>
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setTermModalYearId(null)}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={createTerm.isPending}>
              Add Term
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
