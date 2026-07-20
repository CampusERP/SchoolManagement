import { parseExcelFile } from "@/lib/excel";
import type { ParsedRow } from "@/lib/excel";
import {
  mapStudentRow,
  mapTeacherRow,
  mapParentRow,
  type ImportSummary,
  type ImportRowResult,
} from "./importTemplates";

type EntityKind = "students" | "teachers" | "parents";

interface Mapper {
  map: (values: Record<string, string>) => {
    data?: Record<string, unknown>;
    error?: string;
  };
  create: (data: Record<string, unknown>) => Promise<unknown>;
  label: string;
}

function buildMapper(
  kind: EntityKind,
  schoolId: string,
  create: (data: any) => Promise<unknown>
): Mapper {
  const withSchool = (data?: unknown) =>
    data ? ({ ...(data as Record<string, unknown>), schoolId } as Record<string, unknown>) : undefined;

  if (kind === "students") {
    return {
      label: "student",
      create,
      map: (values) => {
        const r = mapStudentRow(values);
        return { data: withSchool(r.data), error: r.error };
      },
    };
  }
  if (kind === "teachers") {
    return {
      label: "teacher",
      create,
      map: (values) => {
        const r = mapTeacherRow(values);
        return { data: withSchool(r.data), error: r.error };
      },
    };
  }
  return {
    label: "parent",
    create,
    map: (values) => {
      const r = mapParentRow(values);
      return { data: withSchool(r.data), error: r.error };
    },
  };
}

export async function importPeopleFromExcel(
  file: File,
  kind: EntityKind,
  schoolId: string,
  create: (data: any) => Promise<unknown>
): Promise<ImportSummary> {
  const parsed = await parseExcelFile(file);
  const mapper = buildMapper(kind, schoolId, create);

  const results: ImportRowResult[] = [];
  let succeeded = 0;

  for (const row of parsed.rows as ParsedRow[]) {
    const { data, error } = mapper.map(row.values);

    if (error || !data) {
      results.push({ rowNumber: row.rowNumber, ok: false, error: error ?? "Invalid row" });
      continue;
    }

    try {
      await mapper.create(data);
      succeeded += 1;
      results.push({ rowNumber: row.rowNumber, ok: true });
    } catch {
      results.push({
        rowNumber: row.rowNumber,
        ok: false,
        error: `Failed to create ${mapper.label}`,
      });
    }
  }

  return {
    total: parsed.rows.length,
    succeeded,
    failed: parsed.rows.length - succeeded,
    results,
  };
}
