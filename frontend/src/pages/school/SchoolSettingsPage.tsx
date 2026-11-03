import { useEffect, useState } from "react";
import { Building2, Pencil, Plus, Trash2 } from "lucide-react";
import { Form, Input as AntInput, Modal, Table, type TableProps } from "antd";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import InfoCard from "@/components/molecules/InfoCard";
import Button from "@/components/atoms/Button";
import { SchoolsApi, type CampusDto } from "@/features/schools/api";
import { useAuthStore } from "@/store/authStore";
import { getApiErrorMessage } from "@/lib/errors";
import { t } from "@/i18n";

type CampusForm = { name: string; address: string };

export default function SchoolSettingsPage() {
  const schoolId = useAuthStore((state) => state.user?.schoolId);
  const client = useQueryClient();
  const [schoolForm] = Form.useForm();
  const [campusForm] = Form.useForm<CampusForm>();
  const [campusModalOpen, setCampusModalOpen] = useState(false);
  const [editingCampus, setEditingCampus] = useState<CampusDto | null>(null);

  const schoolQuery = useQuery({
    queryKey: ["school-settings", schoolId],
    queryFn: () => SchoolsApi.getById(schoolId!),
    enabled: !!schoolId,
  });
  useEffect(() => {
    const school = schoolQuery.data;
    if (school) schoolForm.setFieldsValue({ name: school.name, address: school.address ?? "", phone: school.phone ?? "", email: school.email ?? "" });
  }, [schoolQuery.data, schoolForm]);
  const refresh = async () => {
    await Promise.all([
      client.invalidateQueries({ queryKey: ["school-settings", schoolId] }),
      client.invalidateQueries({ queryKey: ["schools"] }),
    ]);
  };
  const updateSchool = useMutation({ mutationFn: (data: { name: string; address?: string; phone?: string; email?: string }) => SchoolsApi.update(schoolId!, data), onSuccess: refresh });
  const saveCampus = useMutation({
    mutationFn: (data: CampusForm) => editingCampus
      ? SchoolsApi.updateCampus(schoolId!, editingCampus.id, data)
      : SchoolsApi.addCampus(schoolId!, data),
    onSuccess: async () => { await refresh(); setCampusModalOpen(false); toast.success(t(editingCampus ? "Campus updated" : "Campus added")); },
  });
  const deleteCampus = useMutation({ mutationFn: (campusId: string) => SchoolsApi.deleteCampus(schoolId!, campusId), onSuccess: async () => { await refresh(); toast.success(t("Campus deleted")); } });

  const openCampus = (campus?: CampusDto) => {
    setEditingCampus(campus ?? null);
    campusForm.setFieldsValue(campus ? { name: campus.name, address: campus.address } : { name: "", address: "" });
    setCampusModalOpen(true);
  };

  const campusColumns: TableProps<CampusDto>["columns"] = [
    { title: t("Campus Name"), dataIndex: "name", key: "name" },
    { title: t("Address"), dataIndex: "address", key: "address", responsive: ["sm"] },
    { title: t("Actions"), key: "actions", align: "right", render: (_, campus) => <div className="flex justify-end gap-1"><Button variant="ghost" size="small" icon={<Pencil className="h-4 w-4" />} aria-label={t("Edit campus")} onClick={() => openCampus(campus)} /><Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-4 w-4" />} aria-label={t("Delete campus")} onClick={() => Modal.confirm({ title: t("Delete campus?"), content: campus.name, okType: "danger", onOk: async () => { try { await deleteCampus.mutateAsync(campus.id); } catch (error) { toast.error(getApiErrorMessage(error, t("Failed to delete campus"))); } } })} /></div> },
  ];

  return <DashboardTemplate title={t("School Settings")} subtitle={t("Manage school details and campuses")} loading={schoolQuery.isLoading}>
    {schoolQuery.isError || !schoolQuery.data ? <p className="py-8 text-sm text-[var(--color-danger)]">{t("Failed to load school settings.")}</p> : <div className="space-y-6">
      <InfoCard title={t("School Details")}>
        <Form form={schoolForm} layout="vertical" requiredMark={false} onFinish={async (values) => { try { await updateSchool.mutateAsync({ name: values.name, address: values.address || undefined, phone: values.phone || undefined, email: values.email || undefined }); toast.success(t("School details updated")); } catch (error) { toast.error(getApiErrorMessage(error, t("Failed to update school details"))); } }}>
          <div className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
            <Form.Item name="name" label={t("School Name")} rules={[{ required: true }, { max: 200 }]}><AntInput maxLength={200} /></Form.Item>
            <Form.Item name="email" label={t("Email")} rules={[{ type: "email" }]}><AntInput /></Form.Item>
            <Form.Item name="phone" label={t("Phone")}><AntInput /></Form.Item>
            <Form.Item name="address" label={t("Address")}><AntInput /></Form.Item>
          </div>
          <div className="flex justify-end"><Button variant="primary" htmlType="submit" loading={updateSchool.isPending}>{t("Save Changes")}</Button></div>
        </Form>
      </InfoCard>
      <InfoCard title={t("Campuses")} extra={<Button variant="primary" size="small" icon={<Plus className="h-4 w-4" />} onClick={() => openCampus()}>{t("Add Campus")}</Button>}>
        {!schoolQuery.data.campuses.length ? <div className="py-8 text-center text-sm text-[var(--color-text-muted)]"><Building2 className="mx-auto mb-2 h-6 w-6" />{t("No campuses have been added yet.")}</div> : <Table<CampusDto> rowKey="id" columns={campusColumns} dataSource={schoolQuery.data.campuses} pagination={false} scroll={{ x: "max-content" }} />}
      </InfoCard>
    </div>}
    <Modal title={editingCampus ? t("Edit Campus") : t("Add Campus")} open={campusModalOpen} onCancel={() => setCampusModalOpen(false)} footer={null} destroyOnHidden>
      <Form form={campusForm} layout="vertical" requiredMark={false} onFinish={async (values) => { try { await saveCampus.mutateAsync(values); } catch (error) { toast.error(getApiErrorMessage(error, t(editingCampus ? "Failed to update campus" : "Failed to add campus"))); } }}>
        <Form.Item name="name" label={t("Campus Name")} rules={[{ required: true }, { max: 200 }]}><AntInput maxLength={200} /></Form.Item>
        <Form.Item name="address" label={t("Address")} rules={[{ required: true }, { max: 500 }]}><AntInput maxLength={500} /></Form.Item>
        <div className="flex justify-end gap-2"><Button variant="ghost" onClick={() => setCampusModalOpen(false)}>{t("Cancel")}</Button><Button variant="primary" htmlType="submit" loading={saveCampus.isPending}>{editingCampus ? t("Save") : t("Add Campus")}</Button></div>
      </Form>
    </Modal>
  </DashboardTemplate>;
}
