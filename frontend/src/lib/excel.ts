import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import { t } from "@/i18n";

export interface ParsedRow {
  rowNumber: number;
  values: Record<string, string>;
}

export interface ParseResult {
  headers: string[];
  rows: ParsedRow[];
  sheetName: string;
}

function normalize(value: unknown): string {
  if (value === null || value === undefined) return "";
  if (typeof value === "string") return value.trim();
  if (typeof value === "number" || typeof value === "boolean")
    return String(value);
  if (value instanceof Date) return value.toISOString().split("T")[0];
  return "";
}

export function parseExcelFile(file: File): Promise<ParseResult> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onerror = () => reject(new Error(t("Could not read the file.")));
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: "array" });

        if (workbook.SheetNames.length === 0)
          throw new Error(t("The workbook contains no sheets."));

        const sheetName = workbook.SheetNames.find((name) => name.trim().toLowerCase() === "data") ?? workbook.SheetNames[0];
        const sheet = workbook.Sheets[sheetName];

        // Header row mapped to existing column headers (trimmed).
        const rawRows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, {
          defval: "",
          raw: false,
        }) as Array<Record<string, unknown>>;

        if (rawRows.length === 0)
          throw new Error(t("The first sheet is empty."));

        const headers = Object.keys(rawRows[0]).map((h) => h.trim());

        const rows: ParsedRow[] = rawRows.map((raw, i) => {
          const values: Record<string, string> = {};
          for (const key of Object.keys(raw)) {
            values[key.trim()] = normalize(raw[key]);
          }
          return { rowNumber: i + 2, values };
        }).filter((row) => Object.values(row.values).some(Boolean));

        if (rows.length === 0)
          throw new Error(t("No data rows were found in the Data sheet."));

        resolve({ headers, rows, sheetName });
      } catch (err) {
        reject(err instanceof Error ? err : new Error(t("Failed to parse Excel file.")));
      }
    };

    reader.readAsArrayBuffer(file);
  });
}

export interface ExportColumn {
  header: string;
  accessor: (record: any) => string | number | null | undefined;
}

export function exportToExcel<T>(
  fileName: string,
  sheetName: string,
  columns: ExportColumn[],
  records: T[]
): void {
  const data = records.map((record) => {
    const row: Record<string, string | number> = {};
    for (const col of columns) {
      const value = col.accessor(record);
      row[col.header] = value === null || value === undefined ? "" : value;
    }
    return row;
  });

  const worksheet = XLSX.utils.json_to_sheet(data, {
    header: columns.map((c) => c.header),
  });
  worksheet["!cols"] = columns.map((column) => ({ wch: Math.max(14, Math.min(36, column.header.length + 4)) }));
  worksheet["!freeze"] = { xSplit: 0, ySplit: 1, topLeftCell: "A2", activePane: "bottomLeft", state: "frozen" };
  if (columns.length > 0 && records.length > 0)
    worksheet["!autofilter"] = { ref: XLSX.utils.encode_range({ s: { r: 0, c: 0 }, e: { r: records.length, c: columns.length - 1 } }) };
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, sheetName.slice(0, 31));

  const out = XLSX.write(workbook, { bookType: "xlsx", type: "array" });
  saveAs(
    new Blob([out], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    }),
    fileName.endsWith(".xlsx") ? fileName : `${fileName}.xlsx`
  );
}

export interface ImportTemplateColumn {
  name: string;
  required: boolean;
  description: string;
  example: string;
}

export function exportImportTemplate(fileName: string, columns: ImportTemplateColumn[]): void {
  const workbook = XLSX.utils.book_new();
  const isStudentTemplate = columns.some((column) => column.name === "studentCode");
  const hasLoginField = columns.some((column) => column.name === "hasLogin");
  const dataSheet = XLSX.utils.aoa_to_sheet([columns.map((column) => column.name), []]);
  dataSheet["!cols"] = columns.map((column) => ({ wch: Math.max(16, Math.min(34, column.name.length + 6)) }));
  dataSheet["!freeze"] = { xSplit: 0, ySplit: 1, topLeftCell: "A2", activePane: "bottomLeft", state: "frozen" };
  dataSheet["!autofilter"] = { ref: XLSX.utils.encode_range({ s: { r: 0, c: 0 }, e: { r: 1, c: columns.length - 1 } }) };
  XLSX.utils.book_append_sheet(workbook, dataSheet, "Data");

  const instructions = [
    [t("Import instructions")],
    [t("Enter one person per row on the Data sheet. Do not rename the column headers.")],
    [t("Required fields are marked Yes. Keep optional fields blank when they do not apply.")],
    ...(isStudentTemplate ? [[t("Student codes are generated automatically when studentCode is blank.")]] : []),
    ...(isStudentTemplate ? [[t("For dateOfBirth, use YYYY-MM-DD.")]] : []),
    ...(hasLoginField ? [[t("Use Yes/No for hasLogin.")]] : []),
    [t("Imports create new records; they do not update existing people.")],
    [t("Passwords are never exported. To create a login, provide a new temporary password.")],
    [],
    [t("Column"), t("Required"), t("Description"), t("Example")],
    ...columns.map((column) => [column.name, column.required ? t("Yes") : t("No"), t(column.description), column.example]),
  ];
  const instructionSheet = XLSX.utils.aoa_to_sheet(instructions);
  instructionSheet["!cols"] = [{ wch: 28 }, { wch: 14 }, { wch: 70 }, { wch: 34 }];
  instructionSheet["!freeze"] = { xSplit: 0, ySplit: 7, topLeftCell: "A8", activePane: "bottomLeft", state: "frozen" };
  XLSX.utils.book_append_sheet(workbook, instructionSheet, "Instructions");

  const out = XLSX.write(workbook, { bookType: "xlsx", type: "array" });
  saveAs(new Blob([out], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }),
    fileName.endsWith(".xlsx") ? fileName : `${fileName}.xlsx`);
}
