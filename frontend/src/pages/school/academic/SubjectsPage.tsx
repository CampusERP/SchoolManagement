import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { BookOpen, Plus, Pencil, Trash2 } from "lucide-react";
import { Form, Input as AntInput, Modal, Select, Table, type TableProps } from "antd";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import EmptyState from "@/components/molecules/EmptyState";
import { getApiErrorMessage } from "@/lib/errors";
import { useGradeLevels } from "@/features/academics/hooks";
import {
  useAddCurriculumSubject,
  useCreateSchoolSubject,
  useDeleteSchoolSubject,
  useGradeCurriculum,
  useRemoveCurriculumSubject,
  useSubjects,
  useUpdateSchoolSubject,
} from "@/features/academics/hooks";
import type { SubjectInput } from "@/features/academics/api";
import type { CurriculumSubjectDto } from "@/types/academic.types";
import { t } from "@/i18n";

export default function SubjectsPage() {
  const navigate = useNavigate();
  const { data: grades, isLoading: gradesLoading } = useGradeLevels();
  const [gradeId, setGradeId] = useState<string>();
  const curriculum = useGradeCurriculum(gradeId);
  const subjects = useSubjects();
  const addSubject = useAddCurriculumSubject();
  const createSubject = useCreateSchoolSubject();
  const updateSubject = useUpdateSchoolSubject();
  const removeSubject = useRemoveCurriculumSubject();
  const deleteSubject = useDeleteSchoolSubject();

  const [selectedSubjectId, setSelectedSubjectId] = useState<string>();
  const [subjectModalOpen, setSubjectModalOpen] = useState(false);
  const [editingSubject, setEditingSubject] = useState<CurriculumSubjectDto | null>(null);
  const [form] = Form.useForm<SubjectInput>();

  useEffect(() => {
    if (!gradeId && grades?.length) setGradeId(grades[0].id);
  }, [gradeId, grades]);

  const assigned = curriculum.data ?? [];
  const assignedIds = useMemo(() => new Set(assigned.map((subject) => subject.subjectId)), [assigned]);
  const availableSubjects = (subjects.data ?? [])
    .filter((subject) => !assignedIds.has(subject.id))
    .map((subject) => ({ value: subject.id, label: `${subject.name} (${subject.code})` }));

  const openForm = (subject?: CurriculumSubjectDto) => {
    setEditingSubject(subject ?? null);
    if (subject) form.setFieldsValue({ code: subject.code, name: subject.name, description: subject.description ?? "" });
    else form.resetFields();
    setSubjectModalOpen(true);
  };

  const saveSubject = async (values: SubjectInput) => {
    if (!gradeId) return;
    const data = Object.fromEntries(Object.entries(values).map(([key, value]) => [key, value?.trim() || undefined])) as SubjectInput;
    try {
      if (editingSubject) {
        await updateSubject.mutateAsync({ curriculumSubjectId: editingSubject.curriculumSubjectId, data });
        toast.success(t("Subject updated"));
      } else {
        await createSubject.mutateAsync({ gradeLevelId: gradeId, data });
        toast.success(t("Subject created and assigned to grade"));
      }
      setSubjectModalOpen(false);
    } catch (error) {
      toast.error(getApiErrorMessage(error, editingSubject ? t("Failed to update subject") : t("Failed to create subject")));
    }
  };

  const assignSelected = async () => {
    if (!gradeId || !selectedSubjectId) return;
    try {
      await addSubject.mutateAsync({ gradeLevelId: gradeId, subjectId: selectedSubjectId });
      setSelectedSubjectId(undefined);
      toast.success(t("Subject assigned to grade"));
    } catch {
      toast.error(t("Failed to assign subject to grade"));
    }
  };

  const deleteRow = (subject: CurriculumSubjectDto) => {
    const schoolOwned = subject.isSchoolSpecific;
    Modal.confirm({
      title: schoolOwned ? t("Delete subject from this school?") : t("Remove subject from this grade?"),
      content: schoolOwned
        ? t("The subject and its books will be removed from every grade. Deletion is blocked if it is already used for teaching or academic records.")
        : t("This only removes the subject from this grade and its grade-specific books."),
      okText: schoolOwned ? t("Delete subject") : t("Remove from grade"),
      okType: "danger",
      onOk: async () => {
        try {
          if (schoolOwned) await deleteSubject.mutateAsync(subject.subjectId);
          else await removeSubject.mutateAsync(subject.curriculumSubjectId);
          toast.success(schoolOwned ? t("Subject deleted") : t("Subject removed from grade"));
        } catch (error) {
          toast.error(getApiErrorMessage(error, schoolOwned
            ? t("Subject cannot be deleted while teaching assignments or academic records use it.")
            : t("Failed to remove subject from grade")));
        }
      },
    });
  };

  const columns: TableProps<CurriculumSubjectDto>["columns"] = [
    { title: t("Code"), dataIndex: "code", key: "code", width: 130 },
    { title: t("Subject"), dataIndex: "name", key: "name", render: (name: string, row) => <div><p className="font-medium">{name}</p><p className="text-xs text-[var(--color-text-muted)]">{row.isSchoolSpecific ? t("School subject") : t("Shared catalog")}</p></div> },
    { title: t("Books"), key: "books", align: "center", render: (_, row) => row.books.length },
    { title: t("Actions"), key: "actions", align: "right", render: (_, row) => <div className="flex flex-wrap justify-end gap-2">
      <Button variant="secondary" size="small" icon={<BookOpen className="h-4 w-4" />} onClick={() => navigate(`/academics/books?gradeLevelId=${gradeId}&curriculumSubjectId=${row.curriculumSubjectId}`)}>{t("Manage books")}</Button>
      <Button variant="ghost" size="small" icon={<Pencil className="h-4 w-4" />} onClick={() => openForm(row)} aria-label={t("Edit subject")} />
      <Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-4 w-4" />} onClick={() => deleteRow(row)} aria-label={row.isSchoolSpecific ? t("Delete subject") : t("Remove subject from grade")} />
    </div> },
  ];

  return <DashboardTemplate title={t("Subjects")} subtitle={t("Manage subjects assigned to each grade")}
    loading={gradesLoading}
    actions={<Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={() => openForm()} disabled={!gradeId}>{t("Create subject")}</Button>}>
    <div className="mb-5 flex flex-col gap-3 rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 sm:flex-row">
      <Select value={gradeId} onChange={(value) => setGradeId(value)} options={(grades ?? []).map((grade) => ({ value: grade.id, label: grade.name }))} placeholder={t("Select a grade")} className="w-full sm:max-w-sm" />
      <Select showSearch optionFilterProp="label" value={selectedSubjectId} onChange={setSelectedSubjectId} options={availableSubjects} loading={subjects.isLoading} placeholder={t("Add an existing subject")} className="w-full sm:flex-1" disabled={!gradeId || !availableSubjects.length} />
      <Button variant="secondary" onClick={assignSelected} loading={addSubject.isPending} disabled={!selectedSubjectId}>{t("Assign to grade")}</Button>
    </div>
    {!grades?.length ? <EmptyState icon={<BookOpen className="h-6 w-6" />} title={t("Create a grade first")} description={t("Subjects are managed within a grade.")} /> :
      <Table<CurriculumSubjectDto> rowKey="curriculumSubjectId" columns={columns} dataSource={assigned} loading={curriculum.isLoading} pagination={false} scroll={{ x: "max-content" }} locale={{ emptyText: t("No subjects assigned to this grade yet.") }} />}

    <Modal title={editingSubject ? t("Edit subject") : t("Create subject")} open={subjectModalOpen} onCancel={() => setSubjectModalOpen(false)} footer={null} destroyOnHidden>
      {editingSubject && !editingSubject.isSchoolSpecific && <p className="mb-3 text-sm text-[var(--color-text-muted)]">{t("Saving creates a school-specific copy. The shared catalog subject is unchanged.")}</p>}
      {editingSubject?.isSchoolSpecific && <p className="mb-3 text-sm text-[var(--color-text-muted)]">{t("Changes apply wherever this school uses the subject.")}</p>}
      <Form form={form} layout="vertical" onFinish={saveSubject} requiredMark={false}>
        <Form.Item name="code" label={t("Subject code")} rules={[{ required: true }, { max: 20 }, { pattern: /^[A-Za-z0-9]+$/, message: t("Use letters and numbers only") }]}><AntInput maxLength={20} /></Form.Item>
        <Form.Item name="name" label={t("Subject name")} rules={[{ required: true }, { max: 150 }]}><AntInput maxLength={150} /></Form.Item>
        <Form.Item name="description" label={t("Description")} rules={[{ max: 500 }]}><AntInput.TextArea rows={3} maxLength={500} /></Form.Item>
        <div className="flex justify-end gap-2"><Button variant="ghost" onClick={() => setSubjectModalOpen(false)}>{t("Cancel")}</Button><Button variant="primary" htmlType="submit" loading={createSubject.isPending || updateSubject.isPending}>{editingSubject ? t("Save") : t("Create subject")}</Button></div>
      </Form>
    </Modal>
  </DashboardTemplate>;
}
