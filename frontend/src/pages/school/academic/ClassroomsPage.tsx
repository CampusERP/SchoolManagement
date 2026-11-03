import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, LayoutGrid, Eye } from "lucide-react";
import { toast } from "sonner";
import { Table, Modal, Form, Select, type TableProps } from "antd";
import { useForm, Controller, useWatch } from "react-hook-form";
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
  useRooms,
} from "@/features/academics/hooks";
import type { ClassRoomDetailDto } from "@/types/academic.types";
import { useCurrentSchool } from "@/features/schools/hooks";
import { t } from "@/i18n";

const schema = z.object({
  name: z.string().min(1, t("Name is required")),
  gradeLevelId: z.string().optional(),
  academicYearId: z.string().optional(),
  roomId: z.string().optional(),
  campusId: z.string().optional(),
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
  const { data: rooms } = useRooms();
  const school = useCurrentSchool();
  const campusOptions = (school.data?.campuses ?? []).map((campus) => ({ value: campus.id, label: campus.name }));
  const createClassroom = useCreateClassroom();
  const updateClassroom = useUpdateClassroom();

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ClassRoomDetailDto | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", gradeLevelId: "", academicYearId: "", roomId: "", campusId: "" },
  });
  const selectedCampusId = useWatch({ control, name: "campusId" });

  const openCreate = () => {
    setEditing(null);
    reset({ name: "", gradeLevelId: "", academicYearId: academicYearId ?? "", roomId: "", campusId: "" });
    setModalOpen(true);
  };

  const openEdit = (record: ClassRoomDetailDto) => {
    setEditing(record);
    reset({ name: record.name, gradeLevelId: "", academicYearId: "", roomId: record.roomId ?? "", campusId: record.campusId ?? "" });
    setModalOpen(true);
  };

  const onSubmit = async (values: FormData) => {
    if (campusOptions.length && !values.campusId) {
      toast.error(t("Select a campus for this classroom"));
      return;
    }
    try {
      if (editing) {
        await updateClassroom.mutateAsync({
          id: editing.id,
          data: { schoolId, name: values.name, roomId: values.roomId || undefined, campusId: values.campusId || undefined },
        });
        toast.success(t("Classroom updated"));
      } else {
        if (!values.gradeLevelId || !values.academicYearId) {
          toast.error(t("Please select a grade level and academic year"));
          return;
        }
        await createClassroom.mutateAsync({ schoolId, gradeLevelId: values.gradeLevelId, academicYearId: values.academicYearId, name: values.name, roomId: values.roomId || undefined, campusId: values.campusId || undefined });
        toast.success(t("Classroom created"));
      }
      setModalOpen(false);
    } catch {
      toast.error(editing ? t("Failed to update classroom") : t("Failed to create classroom"));
    }
  };

  const yearOptions = (years ?? []).map((y) => ({ value: y.id, label: y.name }));
  const gradeOptions = (grades ?? []).map((g) => ({
    value: g.id,
    label: g.educationStage ? `${g.name} - ${g.educationStage}` : g.name,
  }));
  const roomOptions = (rooms ?? []).filter((room) => !selectedCampusId || room.campusId === selectedCampusId).map((room) => ({ value: room.id, label: `${room.name} (${room.capacity})` }));

  const columns: TableProps<ClassRoomDetailDto>["columns"] = [
    {
      title: t("Name"),
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
      title: t("Grade Level"),
      key: "gradeLevel",
      render: (_, record) => record.gradeLevel ?? record.gradeLevelName ?? "—",
    },
    {
      title: t("Academic Year"),
      key: "academicYear",
      render: (_, record) => record.academicYear ?? record.academicYearName ?? "—",
    },
    {
      title: t("Room"),
      dataIndex: "roomName",
      key: "room",
      render: (roomName?: string | null) => roomName || "—",
    },
    { title: t("Campus"), key: "campus", responsive: ["md"], render: (_, record) => school.data?.campuses.find((campus) => campus.id === record.campusId)?.name ?? "—" },
    {
      title: t("Student Count"),
      key: "studentCount",
      align: "center",
      render: (_, record) => record.enrolledStudents ?? record.studentCount ?? 0,
    },
    {
      title: t("Actions"),
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
            {t("View")}
          </Button>
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => openEdit(record)}>
            {t("Edit")}
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Classrooms")}
      subtitle={t("Manage classrooms and assignments")}
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
          {t("Create Classroom")}
        </Button>
      }
    >
      <div className="mb-6 flex flex-wrap items-center gap-4">
        <Select
          allowClear
          placeholder={t("Filter by academic year")}
          className="w-full min-w-0 sm:w-auto sm:min-w-56"
          value={academicYearId}
          onChange={(v) => setAcademicYearId(v)}
          options={yearOptions}
        />
        <Select
          allowClear
          placeholder={t("Filter by grade level")}
          className="w-full min-w-0 sm:w-auto sm:min-w-56"
          value={gradeLevelId}
          onChange={(v) => setGradeLevelId(v)}
          options={gradeOptions}
        />
      </div>

      {isError ? (
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">{t("Failed to load classrooms.")}</p>
        </div>
      ) : !isLoading && (data?.length ?? 0) === 0 ? (
        <EmptyState
          icon={<LayoutGrid className="h-6 w-6" />}
          title={t("No classrooms found")}
          description={t("Create a classroom or adjust your filters.")}
          action={
            <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate}>
              {t("Create Classroom")}
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
          scroll={{ x: "max-content" }}
        />
      )}

      <Modal
        title={editing ? t("Edit Classroom") : t("Create Classroom")}
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
              render={({ field }) => <Input {...field} placeholder={t("e.g. Grade 10-A")} />}
            />
          </Form.Item>
          <Form.Item label={t("Campus")}>
            <Controller name="campusId" control={control} render={({ field }) => <Select {...field} allowClear className="w-full" placeholder={t("Select campus")} options={campusOptions} disabled={!campusOptions.length} onChange={(value) => { field.onChange(value); setValue("roomId", ""); }} />} />
          </Form.Item>
          {!editing && (
            <>
              <Form.Item
                label={t("Grade Level")}
                validateStatus={errors.gradeLevelId ? "error" : ""}
                help={errors.gradeLevelId?.message}
              >
                <Controller
                  name="gradeLevelId"
                  control={control}
                  render={({ field }) => (
                    <Select {...field} placeholder={t("Select grade level")} options={gradeOptions} />
                  )}
                />
              </Form.Item>
              <Form.Item
                label={t("Academic Year")}
                validateStatus={errors.academicYearId ? "error" : ""}
                help={errors.academicYearId?.message}
              >
                <Controller
                  name="academicYearId"
                  control={control}
                  render={({ field }) => (
                    <Select {...field} placeholder={t("Select academic year")} options={yearOptions} />
                  )}
                />
              </Form.Item>
            </>
          )}
          <Form.Item label={t("Room")} help={t("Optionally assign a physical room to this classroom.")}>
            <Controller
              name="roomId"
              control={control}
              render={({ field }) => (
                <Select
                  allowClear
                  value={field.value || undefined}
                  onChange={(value) => field.onChange(value ?? "")}
                  placeholder={t("Select a room (optional)")}
                  options={roomOptions}
                  className="w-full"
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
              loading={createClassroom.isPending || updateClassroom.isPending}
            >
              {editing ? t("Save") : t("Create")}
            </Button>
          </div>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}
