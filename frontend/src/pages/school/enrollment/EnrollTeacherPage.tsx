import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Form, Select } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { useTeachers } from "@/features/teachers/hooks";
import { useClassrooms, useAcademicYears } from "@/features/academics/hooks";
import { useEnrollTeacher } from "@/features/enrollment/hooks";
import { useAuthStore } from "@/store/authStore";
import type { EnrollTeacherCommand } from "@/types/enrollment.types";

const schema = z.object({
  teacherId: z.string().min(1, "Teacher is required"),
  academicYearId: z.string().min(1, "Academic year is required"),
  classRoomId: z.string().min(1, "Classroom is required"),
  termId: z.string().min(1, "Term is required"),
});

type FormData = z.infer<typeof schema>;

export default function EnrollTeacherPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [teacherSearch, setTeacherSearch] = useState("");
  const { data: teachers } = useTeachers({ searchTerm: teacherSearch, page: 1, pageSize: 50 });
  const { data: years } = useAcademicYears();
  const enrollTeacher = useEnrollTeacher();

  const {
    control,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { teacherId: "", academicYearId: "", classRoomId: "", termId: "" },
  });

  const selectedYear = watch("academicYearId");
  const { data: classrooms } = useClassrooms({ academicYearId: selectedYear || undefined });

  const selectedYearData = years?.find((y) => y.id === selectedYear);
  const termOptions = (selectedYearData?.terms ?? []).map((t) => ({
    value: t.id,
    label: t.name,
  }));

  const onSubmit = async (values: FormData) => {
    try {
      const payload: EnrollTeacherCommand = {
        schoolId,
        teacherId: values.teacherId,
        classRoomId: values.classRoomId,
        termId: values.termId,
      };
      await enrollTeacher.mutateAsync(payload);
      toast.success("Teacher enrolled successfully");
      navigate(`/people/teachers/${values.teacherId}`);
    } catch {
      toast.error("Failed to enroll teacher");
    }
  };

  const teacherOptions = (teachers?.items ?? []).map((t) => ({
    value: t.id,
    label: `${t.firstName} ${t.lastName} (${t.employeeCode})`,
  }));
  const classroomOptions = (classrooms ?? []).map((c) => ({ value: c.id, label: c.name }));
  const yearOptions = (years ?? []).map((y) => ({ value: y.id, label: y.name }));

  return (
    <DashboardTemplate title="Enroll Teacher" subtitle="Enroll a teacher in a classroom for a term">
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-card)]">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="Teacher" validateStatus={errors.teacherId ? "error" : ""} help={errors.teacherId?.message}>
            <Controller
              name="teacherId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  placeholder="Search and select a teacher"
                  filterOption={false}
                  onSearch={setTeacherSearch}
                  options={teacherOptions}
                  size="large"
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Academic Year" validateStatus={errors.academicYearId ? "error" : ""} help={errors.academicYearId?.message}>
            <Controller
              name="academicYearId"
              control={control}
              render={({ field }) => (
                <Select {...field} placeholder="Select academic year" options={yearOptions} size="large" />
              )}
            />
          </Form.Item>

          <Form.Item label="Term" validateStatus={errors.termId ? "error" : ""} help={errors.termId?.message}>
            <Controller
              name="termId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder="Select term"
                  options={termOptions}
                  size="large"
                  disabled={!selectedYear}
                  notFoundContent={selectedYear ? "No terms found" : "Select an academic year first"}
                />
              )}
            />
          </Form.Item>

          <Form.Item label="Classroom" validateStatus={errors.classRoomId ? "error" : ""} help={errors.classRoomId?.message}>
            <Controller
              name="classRoomId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  placeholder="Select classroom"
                  options={classroomOptions}
                  size="large"
                  disabled={!selectedYear}
                  notFoundContent={selectedYear ? "No classrooms found" : "Select an academic year first"}
                />
              )}
            />
          </Form.Item>

          <div className="mt-6 flex items-center justify-end gap-3">
            <Button variant="ghost" onClick={() => navigate("/enrollment")}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={enrollTeacher.isPending}>
              Enroll
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
