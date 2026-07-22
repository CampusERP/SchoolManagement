import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, LayoutGrid, Eye } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, type TableProps } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input } from "@/components/atoms/Input";
import EmptyState from "@/components/molecules/EmptyState";
import { useAuthStore } from "@/store/authStore";
import {
  useClassrooms,
  useCreateClassroom,
  useUpdateClassroom,
  useAcademicYears,
  useGradeLevels,
} from "@/features/academics/hooks";
import type { ClassRoomDetailDto } from "@/types/academic.types";

const schema = z.object({
  name: z.string().min(1, "Name is required"),
  gradeLevelId: z.string().optional(),
  academicYearId: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

export default function ClassroomsPage() {
  const navigate = useNavigate();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const [academicYearId, setAcademicYearId] = useState<string | undefined>();
  const [gradeLevelId, setGradeLevelId] = useState<string | undefined>();

  const { data, isLoading, isError } = useClassrooms({ academicYearId, gradeLevelId });
  const { data: years } = useAcademicYears();
  const { data: grades } = useGradeLevels();
  const createClassroom = useCreateClassroom();
  const updateClassroom = useUpdateClassroom();

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ClassRoomDetailDto | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", gradeLevelId: "", academicYearId: "" },
  });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", gradeLevelId: "", academicYearId: academicYearId ?? "" });
    setModalOpen(true);
  };

  const openEdit = (record: ClassRoomDetailDto) => {
    setEditing(record);
    reset({ name: record.name, gradeLevelId: "", academicYearId: "" });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    try {
      if (editing) {
        await updateClassroom.mutateAsync({
          id: editing.id,
          data: { schoolId, name: values.name },
        });
        toast.success("Classroom updated");
      } else {
        if (!values.gradeLevelId || !values.academicYearId) {
          toast.error("Please select a grade level and academic year");
          return;
        }
        await createClassroom.mutateAsync({ schoolId, gradeLevelId: values.gradeLevelId, academicYearId: values.academicYearId, name: values.name });
        toast.success("Classroom created");
      }
      setModalOpen(false);
    } catch {
      toast.error(editing ? "Failed to update classroom" : "Failed to create classroom");
    }
  };

  const yearOptions = (years ?? []).map((y) => ({ value: y.id, label: y.name }));
  const gradeOptions = (grades ?? []).map((g) => ({
    value: g.id,
    label: g.educationStage ? `${g.name} - ${g.educationStage}` : g.name,
  }));

  const columns: TableProps<ClassRoomDetailDto>["columns"] = [
    {
      title: "Name",
      dataIndex: "name",
      key: "name",
      render: (name: string, record) => (
        <Button
          variant="ghost"
          type="link"
          size="small"
          className="p-0 font-medium text-[var(--color-text-primary)]"
          onClick={() => navigate(`/academics/classrooms/${record.id}`)}
        >
          {name}
        </Button>
      ),
    },
    {
      title: "Grade Level",
      key: "gradeLevel",
      render: (_, record) => record.gradeLevel ?? record.gradeLevelName ?? "—",
    },
    {
      title: "Academic Year",
      key: "academicYear",
      render: (_, record) => record.academicYear ?? record.academicYearName ?? "—",
    },
    {
      title: "Student Count",
      key: "studentCount",
      align: "center",
      render: (_, record) => record.enrolledStudents ?? record.studentCount ?? 0,
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 160,
      render: (_, record) => (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="ghost"
            type="link"
            size="small"
            className="p-0"
            icon={<Eye className="h-3.5 w-3.5" />}
            onClick={() => navigate(`/academics/classrooms/${record.id}`)}
          >
            View
          </Button>
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openEdit(record)}>
            Edit
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Classrooms"
      subtitle="Manage classrooms and assignments"
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          Create Classroom
        </Button>
      }
    >
      <div className="mb-6 flex flex-wrap items-center gap-4">
        <Select
          allowClear
          placeholder="Filter by academic year"
          className="min-w-56"
          value={academicYearId}
          onChange={(v) => setAcademicYearId(v)}
          options={yearOptions}
        />
        <Select
          allowClear
          placeholder="Filter by grade level"
          className="min-w-56"
          value={gradeLevelId}
          onChange={(v) => setGradeLevelId(v)}
          options={gradeOptions}
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load classrooms.</p>
        </div>
      ) : !isLoading && (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<LayoutGrid className="h-6 w-6" />}
          title="No classrooms found"
          description="Create a classroom or adjust your filters."
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              Create Classroom
            </Button>
          }
        />
      ) : (
        <Table<ClassRoomDetailDto>
          columns={columns}
          dataSource={data ?? []}
          rowKey="id"
          loading={isLoading}
          pagination={false}
        />
      )}

      <Modal
        title={editing ? "Edit Classroom" : "Create Classroom"}
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
              render={({ field }) => <Input {...field} placeholder="e.g. Grade 10-A" />}
            />
          </Form.Item>
          {!editing && (
            <>
              <Form.Item
                label="Grade Level"
                validateStatus={errors.gradeLevelId ? "error" : ""}
                help={errors.gradeLevelId?.message}
              >
                <Controller
                  name="gradeLevelId"
                  control={control}
                  render={({ field }) => (
                    <Select {...field} placeholder="Select grade level" options={gradeOptions} />
                  )}
                />
              </Form.Item>
              <Form.Item
                label="Academic Year"
                validateStatus={errors.academicYearId ? "error" : ""}
                help={errors.academicYearId?.message}
              >
                <Controller
                  name="academicYearId"
                  control={control}
                  render={({ field }) => (
                    <Select {...field} placeholder="Select academic year" options={yearOptions} />
                  )}
                />
              </Form.Item>
            </>
          )}
          <div className="mt-4 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => setModalOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="primary"
              htmlType="submit"
              loading={createClassroom.isPending || updateClassroom.isPending}
            >
              {editing ? "Save" : "Create"}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
