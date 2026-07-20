import { FileSpreadsheet, Download, Upload } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import {
  studentImportTemplate,
  teacherImportTemplate,
  parentImportTemplate,
  type ImportFieldTemplate,
} from "@/features/importExport/importTemplates";
import { exportToExcel } from "@/lib/excel";
import { useAuthStore } from "@/store/authStore";

const sectionTabs = [
  { key: "students", label: "Students", template: studentImportTemplate },
  { key: "teachers", label: "Teachers", template: teacherImportTemplate },
  { key: "parents", label: "Parents", template: parentImportTemplate },
];

export default function ImportDocsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const downloadTemplate = (template: ImportFieldTemplate) => {
    exportToExcel(
      `${template.key}-template`,
      template.key,
      template.columns.map((c) => ({ header: c.name, accessor: () => c.example })),
      [{}]
    );
  };

  return (
    <DashboardTemplate
      title="Import Guide"
      subtitle="Excel column formats for bulk importing people"
    >
      <div className="space-y-8">
        <section className="rounded-[var(--border-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-6">
          <h3 className="text-base font-semibold text-[var(--color-text-primary)]">
            How to import
          </h3>
          <ol className="mt-3 list-decimal space-y-2 pl-5 text-sm text-[var(--color-text-secondary)]">
            <li>Download the template for the type you want to import.</li>
            <li>Fill in the rows. Keep the header row exactly as provided (column names are case-sensitive).</li>
            <li>
              Required columns must have a value in every row. Rows with missing required
              values or invalid data are skipped and reported.
            </li>
            <li>On the relevant page (Students, Teachers, or Parents), click <strong>Import</strong> and choose your file.</li>
            <li>Supported formats: .xlsx, .xls, .csv. Only the first sheet is read.</li>
          </ol>
          <div className="mt-4 flex flex-wrap gap-3">
            {sectionTabs.map((t) => (
              <Button
                key={t.key}
                variant="secondary"
                icon={<Download className="h-4 w-4" />}
                onClick={() => downloadTemplate(t.template)}
              >
                {t.label} template
              </Button>
            ))}
          </div>
        </section>

        {sectionTabs.map((tab) => (
          <section
            key={tab.key}
            className="rounded-[var(--border-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-6"
          >
            <div className="mb-4 flex items-center gap-2">
              {tab.key === "students" ? (
                <Upload className="h-5 w-5 text-[var(--color-primary)]" />
              ) : (
                <FileSpreadsheet className="h-5 w-5 text-[var(--color-primary)]" />
              )}
              <h3 className="text-base font-semibold text-[var(--color-text-primary)]">
                {tab.label} columns
              </h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-[var(--color-border)] text-[var(--color-text-muted)]">
                  <tr>
                    <th className="py-2 pr-4 font-medium">Column</th>
                    <th className="py-2 pr-4 font-medium">Required</th>
                    <th className="py-2 pr-4 font-medium">Description</th>
                    <th className="py-2 pr-4 font-medium">Example</th>
                  </tr>
                </thead>
                <tbody>
                  {tab.template.columns.map((col) => (
                    <tr key={col.name} className="border-b border-[var(--color-border)]/60">
                      <td className="py-2 pr-4">
                        <code className="rounded bg-[var(--color-surface)] px-1.5 py-0.5 text-xs font-mono text-[var(--color-primary)]">
                          {col.name}
                        </code>
                      </td>
                      <td className="py-2 pr-4">
                        {col.required ? (
                          <span className="text-[var(--color-danger)]">Yes</span>
                        ) : (
                          <span className="text-[var(--color-text-muted)]">No</span>
                        )}
                      </td>
                      <td className="py-2 pr-4 text-[var(--color-text-secondary)]">{col.description}</td>
                      <td className="py-2 pr-4">
                        <span className="font-mono text-xs text-[var(--color-text-secondary)]">
                          {col.example}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        ))}
      </div>
    </DashboardTemplate>
  );
}
