import { useState } from "react";
import { Form, Select, Switch } from "antd";
import { Link2, Users } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import EmptyState from "@/components/molecules/EmptyState";
import { useStudents } from "@/features/students/hooks";
import { useParents } from "@/features/parents/hooks";
import { useLinkStudentGuardian } from "@/features/students/hooks";

type GuardianForm = { studentId: string; parentId: string; relationshipType: string; isPrimaryContact: boolean; canViewGrades: boolean; canViewBilling: boolean };

export default function StudentGuardiansPage() {
  const { data: students, isLoading: loadingStudents } = useStudents({ page: 1, pageSize: 100 });
  const { data: parents, isLoading: loadingParents } = useParents({ page: 1, pageSize: 100 });
  const linkGuardian = useLinkStudentGuardian();
  const [form] = Form.useForm<GuardianForm>();

  const onFinish = async (values: GuardianForm) => {
    try {
      await linkGuardian.mutateAsync(values);
      toast.success("Guardian linked to student");
      form.resetFields();
    } catch (error: any) {
      toast.error(error?.response?.data?.message ?? "Could not link guardian");
    }
  };

  const noRecords = !loadingStudents && !loadingParents && (!students?.items.length || !parents?.items.length);
  return <DashboardTemplate title="Student Guardians" subtitle="Link parent and guardian accounts to student records">
    {noRecords ? <EmptyState icon={<Users className="h-6 w-6" />} title="Students and parents are required" description="Create at least one student and one parent before creating a guardian relationship." /> :
      <div className="max-w-2xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)]">
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ relationshipType: "Guardian", canViewGrades: true, canViewBilling: false, isPrimaryContact: false }} requiredMark={false}>
          <Form.Item label="Student" name="studentId" rules={[{ required: true, message: "Select a student" }]}><Select loading={loadingStudents} showSearch optionFilterProp="label" options={(students?.items ?? []).map(s => ({ value: s.id, label: `${s.firstName} ${s.lastName} (${s.studentCode})` }))} /></Form.Item>
          <Form.Item label="Parent / Guardian" name="parentId" rules={[{ required: true, message: "Select a parent or guardian" }]}><Select loading={loadingParents} showSearch optionFilterProp="label" options={(parents?.items ?? []).map(p => ({ value: p.id, label: `${p.firstName} ${p.lastName}` }))} /></Form.Item>
          <Form.Item label="Relationship" name="relationshipType" rules={[{ required: true }]}><Select options={["Mother", "Father", "Guardian", "Other"].map(value => ({ value, label: value }))} /></Form.Item>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <Form.Item label="Primary contact" name="isPrimaryContact" valuePropName="checked"><Switch /></Form.Item>
            <Form.Item label="Can view grades" name="canViewGrades" valuePropName="checked"><Switch /></Form.Item>
            <Form.Item label="Can view billing" name="canViewBilling" valuePropName="checked"><Switch /></Form.Item>
          </div>
          <Button variant="primary" htmlType="submit" icon={<Link2 className="h-4 w-4" />} loading={linkGuardian.isPending}>Link Guardian</Button>
        </Form>
      </div>}
  </DashboardTemplate>;
}
