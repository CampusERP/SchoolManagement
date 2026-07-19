import { useRef, useState } from "react";
import { Upload, Download, FileSpreadsheet } from "lucide-react";
import { Modal, Table, type TableProps } from "antd";
import { toast } from "sonner";
import Button from "@/components/atoms/Button";
import { importPeopleFromExcel } from "@/features/importExport/importRunner";
import {
  studentImportTemplate,
  teacherImportTemplate,
  parentImportTemplate,
  type ImportSummary,
} from "@/features/importExport/importTemplates";
import { exportToExcel } from "@/lib/excel";
import type { StudentListDto } from "@/types/student.types";
import type { TeacherListDto } from "@/types/teacher.types";
import type { ParentListDto } from "@/types/parent.types";

type Kind = "students" | "teachers" | "parents";

interface Props {
  kind: Kind;
  schoolId: string;
  createFn: (data: any) => Promise<unknown>;
  exportColumns: { header: string; accessor: (r: any) => string | number | null | undefined }[];
  exportRecords: any[];
  exportFileName: string;
  exportSheetName: string;
}

const templates = {
  students: studentImportTemplate,
  teachers: teacherImportTemplate,
  parents: parentImportTemplate,
};

export default function ImportExportButtons({
  kind,
  schoolId,
  createFn,
  exportColumns,
  exportRecords,
  exportFileName,
  exportSheetName,
}: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [importing, setImporting] = useState(false);
  const [summary, setSummary] = useState<ImportSummary | null>(null);

  const onFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;

    setImporting(true);
    try {
      const result = await importPeopleFromExcel(file, kind, schoolId, createFn);
      setSummary(result);
      if (result.failed === 0) toast.success(`Imported ${result.succeeded} ${kind}.`);
      else toast.warning(`Imported ${result.succeeded}, ${result.failed} failed.`);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to import file.");
    } finally {
      setImporting(false);
    }
  };

  const onExport = () => {
    if (!exportRecords.length) {
      toast.error("No rows to export.");
      return;
    }
    exportToExcel(exportFileName, exportSheetName, exportColumns, exportRecords);
  };

  const onTemplate = () => {
    const tpl = templates[kind];
    exportToExcel(
      `${kind}-template`,
      kind,
      tpl.columns.map((c) => ({ header: c.name, accessor: () => c.example })),
      [{}]
    );
  };

  const resultColumns: TableProps<ImportSummary["results"][number]>["columns"] = [
    { title: "Row", dataIndex: "rowNumber", key: "rowNumber", width: 80 },
    {
      title: "Status",
      key: "status",
      render: (_, r) => (r.ok ? "OK" : "Failed"),
    },
    {
      title: "Error",
      dataIndex: "error",
      key: "error",
      render: (err: string) => err ?? "—",
    },
  ];

  return (
    <>
      <input
        ref={inputRef}
        type="file"
        accept=".xlsx,.xls,.csv"
        className="hidden"
        onChange={onFile}
      />
      <Button variant="secondary" icon={<Upload className="h-4 w-4" />} loading={importing} onClick={() => inputRef.current?.click()}>
        Import
      </Button>
      <Button variant="secondary" icon={<Download className="h-4 w-4" />} onClick={onExport}>
        Export
      </Button>
      <Button variant="ghost" icon={<FileSpreadsheet className="h-4 w-4" />} onClick={onTemplate}>
        Template
      </Button>

      <Modal
        title="Import results"
        open={!!summary}
        onCancel={() => setSummary(null)}
        footer={
          <Button variant="ghost" onClick={() => setSummary(null)}>
            Close
          </Button>
        }
      >
        {summary && (
          <div className="space-y-3">
            <p className="text-sm text-[var(--color-text-secondary)]">
              {summary.succeeded} succeeded, {summary.failed} failed out of {summary.total}.
            </p>
            <Table
              size="small"
              rowKey="rowNumber"
              pagination={false}
              columns={resultColumns}
              dataSource={summary.results.filter((r) => !r.ok)}
            />
          </div>
        )}
      </Modal>
    </>
  );
}
