import * as XLSX from "xlsx";
import { saveAs } from "file-saver";

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

    reader.onerror = () => reject(new Error("Could not read the file."));
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: "array" });

        if (workbook.SheetNames.length === 0)
          throw new Error("The workbook contains no sheets.");

        const sheetName = workbook.SheetNames[0];
        const sheet = workbook.Sheets[sheetName];

        // Header row mapped to existing column headers (trimmed).
        const rawRows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, {
          defval: "",
          raw: false,
        }) as Array<Record<string, unknown>>;

        if (rawRows.length === 0)
          throw new Error("The first sheet is empty.");

        const headers = Object.keys(rawRows[0]).map((h) => h.trim());

        const rows: ParsedRow[] = rawRows.map((raw, i) => {
          const values: Record<string, string> = {};
          for (const key of Object.keys(raw)) {
            values[key.trim()] = normalize(raw[key]);
          }
          return { rowNumber: i + 2, values };
        });

        resolve({ headers, rows, sheetName });
      } catch (err) {
        reject(err instanceof Error ? err : new Error("Failed to parse Excel file."));
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
