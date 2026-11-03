import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { BookMarked, Plus, Pencil, Trash2, Upload, FileText } from "lucide-react";
import { Form, Input as AntInput, Modal, Select, Table, type TableProps } from "antd";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import EmptyState from "@/components/molecules/EmptyState";
import { getApiErrorMessage } from "@/lib/errors";
import { useGradeLevels, useGradeCurriculum, useCreateSubjectBook, useDeleteSubjectBook, useUpdateSubjectBook, useUploadBookPdf } from "@/features/academics/hooks";
import { useDeleteDocument } from "@/features/documents/hooks";
import { openDocument } from "@/features/documents/api";
import { useAuthStore } from "@/store/authStore";
import type { CurriculumBookDto, SubjectBookInput } from "@/types/academic.types";
import { t } from "@/i18n";

export default function BooksPage() {
  const [searchParams] = useSearchParams();
  const { data: grades, isLoading: gradesLoading } = useGradeLevels();
  const [gradeId, setGradeId] = useState<string | undefined>(searchParams.get("gradeLevelId") ?? undefined);
  const curriculum = useGradeCurriculum(gradeId);
  const [curriculumSubjectId, setCurriculumSubjectId] = useState<string | undefined>(searchParams.get("curriculumSubjectId") ?? undefined);
  const selectedSubject = curriculum.data?.find((subject) => subject.curriculumSubjectId === curriculumSubjectId);
  const createBook = useCreateSubjectBook();
  const updateBook = useUpdateSubjectBook();
  const deleteBook = useDeleteSubjectBook();
  const uploadPdf = useUploadBookPdf();
  const deleteDocument = useDeleteDocument();
  const schoolId = useAuthStore((s) => s.user?.schoolId)!;

  const [modalOpen, setModalOpen] = useState(false);
  const [editingBook, setEditingBook] = useState<CurriculumBookDto | null>(null);
  const [form] = Form.useForm<SubjectBookInput>();

  useEffect(() => {
    if (!gradeId && grades?.length) setGradeId(grades[0].id);
  }, [gradeId, grades]);

  useEffect(() => {
    if (curriculum.isLoading) return;
    if (!curriculum.data?.length) {
      setCurriculumSubjectId(undefined);
      return;
    }
    if (!curriculumSubjectId || !curriculum.data.some((item) => item.curriculumSubjectId === curriculumSubjectId))
      setCurriculumSubjectId(curriculum.data[0].curriculumSubjectId);
  }, [curriculum.data, curriculum.isLoading, curriculumSubjectId]);

  const openCreate = () => {
    setEditingBook(null);
    form.resetFields();
    setModalOpen(true);
  };

  const openEdit = (book: CurriculumBookDto) => {
    setEditingBook(book);
    form.setFieldsValue({ title: book.title, author: book.author ?? "", publisher: book.publisher ?? "", isbn: book.isbn ?? "", edition: book.edition ?? "", notes: book.notes ?? "" });
    setModalOpen(true);
  };

  const saveBook = async (values: SubjectBookInput) => {
    if (!selectedSubject) return;
    const data = Object.fromEntries(Object.entries(values).map(([key, value]) => [key, value?.trim() || undefined])) as SubjectBookInput;
    try {
      if (editingBook) {
        await updateBook.mutateAsync({ curriculumSubjectId: selectedSubject.curriculumSubjectId, bookId: editingBook.id, data });
        toast.success(t("Book updated"));
      } else {
        await createBook.mutateAsync({ curriculumSubjectId: selectedSubject.curriculumSubjectId, data });
        toast.success(t("Book added"));
      }
      setModalOpen(false);
    } catch (error) {
      toast.error(getApiErrorMessage(error, editingBook ? t("Failed to update book") : t("Failed to add book")));
    }
  };

  const removeBook = (book: CurriculumBookDto) => Modal.confirm({
    title: t("Delete book?"), content: book.title, okText: t("Delete"), okType: "danger",
    onOk: async () => {
      if (!selectedSubject) return;
      try {
        await deleteBook.mutateAsync({ curriculumSubjectId: selectedSubject.curriculumSubjectId, bookId: book.id });
        toast.success(t("Book deleted"));
      } catch { toast.error(t("Failed to delete book")); }
    },
  });

  const columns: TableProps<CurriculumBookDto>["columns"] = [
    { title: t("Title"), dataIndex: "title", key: "title" },
    { title: t("Author"), dataIndex: "author", key: "author", render: (value?: string) => value || "—" },
    { title: t("Publisher"), dataIndex: "publisher", key: "publisher", responsive: ["md"], render: (value?: string) => value || "—" },
    { title: t("ISBN"), dataIndex: "isbn", key: "isbn", responsive: ["lg"], render: (value?: string) => value || "—" },
    { title: t("Edition"), dataIndex: "edition", key: "edition", responsive: ["md"], render: (value?: string) => value || "—" },
    { title: t("PDF Documents"), key: "documents", render: (_, book) => <div className="flex min-w-48 flex-col gap-2">{(book.documents ?? []).map((doc) => <div key={doc.id} className="flex items-center gap-2"><button className="flex min-w-0 items-center gap-1 text-sm text-[var(--color-primary)] hover:underline" onClick={async () => { try { await openDocument(schoolId, doc.id); } catch { toast.error(t("Failed to open document")); } }}><FileText className="h-4 w-4 shrink-0" /><span className="max-w-40 truncate">{doc.fileName}</span></button><Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-3.5 w-3.5" />} aria-label={t("Delete document")} onClick={() => Modal.confirm({ title: t("Delete document?"), content: doc.fileName, okType: "danger", onOk: async () => { try { await deleteDocument.mutateAsync(doc.id); toast.success(t("Document deleted")); } catch { toast.error(t("Failed to delete document")); } } })} /></div>)}<label className="inline-flex w-fit cursor-pointer items-center gap-1 text-xs text-[var(--color-primary)]"><Upload className="h-3.5 w-3.5" />{uploadPdf.isPending ? t("Uploading…") : t("Upload PDF")}<input type="file" accept=".pdf,application/pdf" className="hidden" disabled={uploadPdf.isPending} onChange={async (event) => { const file = event.target.files?.[0]; event.target.value = ""; if (!file || !selectedSubject) return; try { await uploadPdf.mutateAsync({ curriculumSubjectId: selectedSubject.curriculumSubjectId, bookId: book.id, file }); toast.success(t("Book PDF uploaded")); } catch (error) { toast.error(getApiErrorMessage(error, t("Failed to upload PDF"))); } }} /></label></div> },
    { title: t("Actions"), key: "actions", align: "right", render: (_, book) => <div className="flex justify-end gap-2"><Button variant="ghost" size="small" icon={<Pencil className="h-4 w-4" />} onClick={() => openEdit(book)} aria-label={t("Edit book")} /><Button variant="ghost" size="small" className="text-[var(--color-danger)]" icon={<Trash2 className="h-4 w-4" />} onClick={() => removeBook(book)} aria-label={t("Delete book")} /></div> },
  ];

  return <DashboardTemplate title={t("Books")} subtitle={t("Manage books for each grade subject")} loading={gradesLoading}
    actions={<Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={openCreate} disabled={!selectedSubject}>{t("Add book")}</Button>}>
    {!grades?.length ? <EmptyState icon={<BookMarked className="h-6 w-6" />} title={t("Create a grade first")} description={t("Books are managed under a subject assigned to a grade.")} /> : <>
      <div className="mb-5 flex flex-col gap-3 rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 sm:flex-row">
        <Select value={gradeId} onChange={(value) => { setGradeId(value); setCurriculumSubjectId(undefined); }} options={(grades ?? []).map((grade) => ({ value: grade.id, label: grade.name }))} placeholder={t("Select a grade")} className="w-full sm:max-w-sm" />
        <Select value={curriculumSubjectId} onChange={setCurriculumSubjectId} options={(curriculum.data ?? []).map((subject) => ({ value: subject.curriculumSubjectId, label: `${subject.name} (${subject.code})` }))} loading={curriculum.isLoading} placeholder={t("Select a subject")} className="w-full sm:flex-1" disabled={!curriculum.data?.length} />
      </div>
      {!selectedSubject ? <EmptyState icon={<BookMarked className="h-6 w-6" />} title={t("No subject selected")} description={t("Assign a subject to this grade from the Subjects page first.")} /> : <>
        <h2 className="mb-3 text-lg font-semibold text-[var(--color-text-primary)]">{selectedSubject.name} · {grades?.find((grade) => grade.id === gradeId)?.name}</h2>
        <Table<CurriculumBookDto> rowKey="id" columns={columns} dataSource={selectedSubject.books} pagination={false} scroll={{ x: "max-content" }} locale={{ emptyText: t("No books added for this subject yet.") }} />
      </>}
    </>}

    <Modal title={editingBook ? t("Edit book") : t("Add book")} open={modalOpen} onCancel={() => setModalOpen(false)} footer={null} destroyOnHidden>
      <Form form={form} layout="vertical" onFinish={saveBook} requiredMark={false}>
        <Form.Item name="title" label={t("Title")} rules={[{ required: true }, { max: 200 }]}><AntInput maxLength={200} /></Form.Item>
        <div className="grid grid-cols-1 gap-x-4 sm:grid-cols-2">
          <Form.Item name="author" label={t("Author")} rules={[{ max: 150 }]}><AntInput maxLength={150} /></Form.Item>
          <Form.Item name="publisher" label={t("Publisher")} rules={[{ max: 150 }]}><AntInput maxLength={150} /></Form.Item>
          <Form.Item name="isbn" label={t("ISBN")} rules={[{ max: 30 }]}><AntInput maxLength={30} /></Form.Item>
          <Form.Item name="edition" label={t("Edition")} rules={[{ max: 50 }]}><AntInput maxLength={50} /></Form.Item>
        </div>
        <Form.Item name="notes" label={t("Notes")} rules={[{ max: 1000 }]}><AntInput.TextArea rows={3} maxLength={1000} /></Form.Item>
        <div className="flex justify-end gap-2"><Button variant="ghost" onClick={() => setModalOpen(false)}>{t("Cancel")}</Button><Button variant="primary" htmlType="submit" loading={createBook.isPending || updateBook.isPending}>{editingBook ? t("Save") : t("Add book")}</Button></div>
      </Form>
    </Modal>
  </DashboardTemplate>;
}
