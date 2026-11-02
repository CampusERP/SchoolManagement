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
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

type GuardianForm = { studentId: string; parentId: string; relationshipType: string; isPrimaryContact: boolean; canViewGrades: boolean; canViewBilling: boolean };

export default function StudentGuardiansPage() {
  const [studentSearch, setStudentSearch] = useState("");
  const [parentSearch, setParentSearch] = useState("");
  const { data: students, isLoading: loadingStudents } = useStudents({
    searchTerm: studentSearch,
    page: 1,
    pageSize: 50,
  });
  const { data: parents, isLoading: loadingParents } = useParents({
    searchTerm: parentSearch,
    page: 1,
    pageSize: 50,
  });
  const linkGuardian = useLinkStudentGuardian();
  const [form] = Form.useForm<GuardianForm>();

  const onFinish = async (values: GuardianForm) => {
    try {
      await linkGuardian.mutateAsync(values);
      toast.success(t("Guardian linked to student"));
      form.resetFields();
    } catch (error: any) {
      toast.error(translateApiMessage(error?.response?.data?.message ?? t("Could not link guardian")));
    }
  };

  const noRecords = !studentSearch && !parentSearch && !loadingStudents && !loadingParents && (!students?.items.length || !parents?.items.length);
  return <DashboardTemplate title={t("Student Guardians")} subtitle={t("Link parent and guardian accounts to student records")}>
    {noRecords ? <EmptyState icon={<Users className="h-6 w-6" />} title={t("Students and parents are required")} description={t("Create at least one student and one parent before creating a guardian relationship.")} /> :
      <div className="max-w-2xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ relationshipType: "Guardian", canViewGrades: true, canViewBilling: false, isPrimaryContact: false }} requiredMark={false}>
          <Form.Item label={t("Student")} name="studentId" rules={[{ required: true, message: t("Select a student") }]}><Select loading={loadingStudents} showSearch filterOption={false} onSearch={setStudentSearch} placeholder={t("Search and select a student")} options={(students?.items ?? []).map(s => ({ value: s.id, label: `${s.firstName} ${s.lastName} (${s.studentCode})` }))} /></Form.Item>
          <Form.Item label={t("Parent / Guardian")} name="parentId" rules={[{ required: true, message: t("Select a parent or guardian") }]}><Select loading={loadingParents} showSearch filterOption={false} onSearch={setParentSearch} placeholder={t("Search and select a parent or guardian")} options={(parents?.items ?? []).map(p => ({ value: p.id, label: `${p.firstName} ${p.lastName}` }))} /></Form.Item>
          <Form.Item label={t("Relationship")} name="relationshipType" rules={[{ required: true }]}><Select options={["Mother", "Father", "Guardian", "Other"].map(value => ({ value, label: t(value) }))} /></Form.Item>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <Form.Item label={t("Primary contact")} name="isPrimaryContact" valuePropName="checked"><Switch /></Form.Item>
            <Form.Item label={t("Can view grades")} name="canViewGrades" valuePropName="checked"><Switch /></Form.Item>
            <Form.Item label={t("Can view billing")} name="canViewBilling" valuePropName="checked"><Switch /></Form.Item>
          </div>
          <Button variant="primary" htmlType="submit" icon={<Link2 className="h-4 w-4" />} loading={linkGuardian.isPending}>{t("Link Guardian")}</Button>
        </Form>
      </div>}
  </DashboardTemplate>;
}
