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
  type CampusImportOption,
} from "@/features/importExport/importTemplates";
import { exportImportTemplate, exportToExcel } from "@/lib/excel";
import { getApiErrorMessage } from "@/lib/errors";
import type { StudentListDto } from "@/types/student.types";
import type { TeacherListDto } from "@/types/teacher.types";
import type { ParentListDto } from "@/types/parent.types";
import { t } from "@/i18n";

type Kind = "students" | "teachers" | "parents";

interface Props {
  kind: Kind;
  schoolId: string;
  createFn: (data: any) => Promise<unknown>;
  exportColumns: { header: string; accessor: (r: any) => string | number | null | undefined }[];
  exportRecords: any[];
  exportFileName: string;
  exportSheetName: string;
  campuses?: CampusImportOption[];
  fetchAll?: () => Promise<any[]>;
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
  campuses = [],
  fetchAll,
}: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [importing, setImporting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [summary, setSummary] = useState<ImportSummary | null>(null);

  const onFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;

    setImporting(true);
    try {
      const result = await importPeopleFromExcel(file, kind, schoolId, createFn, campuses);
      setSummary(result);
      if (result.failed === 0) toast.success(t("Imported {count} records.", { count: result.succeeded }));
      else toast.warning(t("Imported {count} records, {failed} failed.", { count: result.succeeded, failed: result.failed }));
    } catch (err) {
      toast.error(getApiErrorMessage(err, t("Failed to import file.")));
    } finally {
      setImporting(false);
    }
  };

  const onExport = async () => {
    setExporting(true);
    try {
      const records = fetchAll ? await fetchAll() : exportRecords;
      if (!records.length) {
        toast.error(t("No rows to export."));
        return;
      }
      exportToExcel(exportFileName, exportSheetName, exportColumns, records);
    } catch (error) {
      toast.error(getApiErrorMessage(error, t("Failed to export records.")));
    } finally {
      setExporting(false);
    }
  };

  const onTemplate = () => {
    const tpl = templates[kind];
    const columns = tpl.columns.map((column) => {
      if (column.name !== "campus" && column.name !== "campuses") return column;
      const names = campuses.map((campus) => campus.name);
      return {
        ...column,
        required: names.length > 0,
        description: names.length ? `${column.description}. ${t("Available campuses")}: ${names.join(", ")}` : column.description,
        example: names.length ? (kind === "teachers" ? names.slice(0, 2).join("; ") : names[0]) : column.example,
      };
    });
    exportImportTemplate(`${kind}-template`, columns);
  };

  const resultColumns: TableProps<ImportSummary["results"][number]>["columns"] = [
    { title: t("Row"), dataIndex: "rowNumber", key: "rowNumber", width: 80 },
    {
      title: t("Status"),
      key: "status",
      render: (_, r) => (r.ok ? t("OK") : t("Failed")),
    },
    {
      title: t("Error"),
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
        {t("Import")}
      </Button>
      <Button variant="secondary" icon={<Download className="h-4 w-4" />} loading={exporting} onClick={onExport}>
        {t("Export")}
      </Button>
      <Button variant="ghost" icon={<FileSpreadsheet className="h-4 w-4" />} onClick={onTemplate}>
        {t("Template")}
      </Button>

      <Modal
        title={t("Import results")}
        open={!!summary}
        onCancel={() => setSummary(null)}
        footer={
          <Button variant="ghost" onClick={() => setSummary(null)}>
            {t("Close")}
          </Button>
        }
      >
        {summary && (
          <div className="space-y-3">
            <p className="text-sm text-[var(--color-text-secondary)]">
              {t("{succeeded} succeeded, {failed} failed out of {total}.", { succeeded: summary.succeeded, failed: summary.failed, total: summary.total })}
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
