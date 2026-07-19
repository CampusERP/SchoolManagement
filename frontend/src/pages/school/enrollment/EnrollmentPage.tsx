import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Form, Select } from "antd";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { useStudents } from "@/features/students/hooks";
import { useClassrooms, useAcademicYears } from "@/features/academics/hooks";
import { useEnrollStudent } from "@/features/enrollment/hooks";
import { useAuthStore } from "@/store/authStore";

const schema = z.object({
  studentId: z.string().min(1, "Student is required"),
  academicYearId: z.string().min(1, "Academic year is required"),
  classRoomId: z.string().min(1, "Classroom is required"),
});

type FormData = z.infer<typeof schema>;

export default function EnrollmentPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;
  const navigate = useNavigate();

  const [studentSearch, setStudentSearch] = useState("");
  const { data: students } = useStudents({ searchTerm: studentSearch, page: 1, pageSize: 50 });
  const { data: years } = useAcademicYears();
  const enrollStudent = useEnrollStudent();

  const {
    control,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { studentId: "", academicYearId: "", classRoomId: "" },
  });

  const selectedYear = watch("academicYearId");
  const { data: classrooms } = useClassrooms({ academicYearId: selectedYear || undefined });

  const onSubmit = async (values: FormData) => {
    try {
      await enrollStudent.mutateAsync({ schoolId, ...values });
      toast.success("Student enrolled successfully");
      navigate(`/people/students/${values.studentId}`);
    } catch {
      toast.error("Failed to enroll student");
    }
  };

  const studentOptions = (students?.items ?? []).map((s) => ({
    value: s.id,
    label: `${s.firstName} ${s.lastName} (${s.studentCode})`,
  }));
  const yearOptions = (years ?? []).map((y) => ({ value: y.id, label: y.name }));
  const classroomOptions = (classrooms ?? []).map((c) => ({ value: c.id, label: c.name }));

  return (
    <DashboardTemplate title="Student Enrollment" subtitle="Enroll a student into a classroom">
      <div className="mx-auto max-w-xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-8 shadow-[var(--shadow-card)]">
        <Form layout="vertical" onFinish={handleSubmit(onSubmit)} requiredMark={false}>
          <Form.Item label="Student" validateStatus={errors.studentId ? "error" : ""} help={errors.studentId?.message}>
            <Controller
              name="studentId"
              control={control}
              render={({ field }) => (
                <Select
                  {...field}
                  showSearch
                  placeholder="Search and select a student"
                  filterOption={false}
                  onSearch={setStudentSearch}
                  options={studentOptions}
                  size="large"
                />
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
                <Select {...field} placeholder="Select academic year" options={yearOptions} size="large" />
              )}
            />
          </Form.Item>

          <Form.Item
            label="Classroom"
            validateStatus={errors.classRoomId ? "error" : ""}
            help={errors.classRoomId?.message}
          >
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
            <Button variant="ghost" onClick={() => navigate("/school")}>
              Cancel
            </Button>
            <Button variant="primary" htmlType="submit" loading={enrollStudent.isPending}>
              Enroll
            </Button>
          </div>
        </Form>
      </div>
    </DashboardTemplate>
  );
}
