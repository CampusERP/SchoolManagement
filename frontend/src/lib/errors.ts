import { isAxiosError } from "axios";
import { getLanguage, t } from "@/i18n";

const ENTITY_LABELS: Record<string, string> = {
  School: "المدرسة",
  Student: "الطالب",
  Teacher: "المعلم",
  Parent: "ولي الأمر",
  ClassRoom: "الفصل",
  Classroom: "الفصل",
  GradeLevel: "المستوى الدراسي",
  AcademicYear: "السنة الدراسية",
  Subject: "المادة",
  Room: "الغرفة",
  Term: "الفصل الدراسي",
  EducationStage: "المرحلة التعليمية",
  AttendanceSession: "جلسة الحضور",
  TeachingAssignment: "التكليف التدريسي",
  Enrollment: "التسجيل",
  User: "المستخدم",
  SchoolRegistrationRequest: "طلب التسجيل",
};

function entityLabel(name: string): string {
  return getLanguage() === "ar" ? (ENTITY_LABELS[name] ?? name) : name;
}

/**
 * Translates the API error messages that the backend returns as plain
 * English strings. Unknown messages are returned unchanged.
 */
export function translateApiMessage(message: string): string {
  const patterns: Array<[RegExp, (m: RegExpMatchArray) => string]> = [
    [
      /^A student with code '(.+)' already exists in this school\.$/,
      (m) => t("A student with code '{code}' already exists in this school.", { code: m[1] }),
    ],
    [
      /^A teacher with employee code '(.+)' already exists in this school\.$/,
      (m) =>
        t("A teacher with employee code '{code}' already exists in this school.", { code: m[1] }),
    ],
    [
      /^A user with email '(.+)' already exists\.$/,
      (m) => t("A user with email '{email}' already exists.", { email: m[1] }),
    ],
    [
      /^Subdomain '(.+)' is already taken\.$/,
      (m) => t("Subdomain '{code}' is already taken.", { code: m[1] }),
    ],
    [
      /^The subdomain '(.+)' is already taken\.$/,
      (m) => t("The subdomain '{code}' is already taken.", { code: m[1] }),
    ],
    [
      /^Subject code '(.+)' already exists\.$/,
      (m) => t("Subject code '{code}' already exists.", { code: m[1] }),
    ],
    [
      /^An education stage named '(.+)' already exists\.$/,
      (m) => t("An education stage named '{name}' already exists.", { name: m[1] }),
    ],
    [
      /^A room with name '(.+)' already exists in this school\.$/,
      (m) => t("A room with name '{name}' already exists in this school.", { name: m[1] }),
    ],
    [
      /^A term with sequence (\d+) already exists\.$/,
      (m) => t("A term with sequence {sequence} already exists.", { sequence: m[1] }),
    ],
    [
      /^(Teacher|ClassRoom|Classroom|Student|Parent|GradeLevel|AcademicYear) with ID '(.+)' was not found in this school\.$/,
      (m) =>
        t("The {entity} with ID '{id}' was not found in this school.", {
          entity: entityLabel(m[1]),
          id: m[2],
        }),
    ],
    [
      /^(Teacher|ClassRoom|Classroom|Student|Parent|GradeLevel|AcademicYear) with ID '(.+)' was not found\.$/,
      (m) =>
        t("The {entity} with ID '{id}' was not found.", {
          entity: entityLabel(m[1]),
          id: m[2],
        }),
    ],
    [
      /^(School|Student|Teacher|Parent|ClassRoom|Classroom|GradeLevel|AcademicYear|Subject|Room|Term|EducationStage|AttendanceSession|TeachingAssignment|Enrollment|User|SchoolRegistrationRequest) with key '(.+)' was not found\.$/,
      (m) =>
        t("The {entity} with key '{key}' was not found.", {
          entity: entityLabel(m[1]),
          key: m[2],
        }),
    ],
    [
      /^Request failed with status code (\d+)$/,
      (m) => t("Request failed with status code {status}", { status: m[1] }),
    ],
  ];

  for (const [pattern, format] of patterns) {
    const match = message.match(pattern);
    if (match) return format(match);
  }

  return t(message);
}

/**
 * Pulls a human-readable message out of an API error.
 * The backend returns `{ error: string }` for Result failures and
 * `{ message: string }` for problem-details responses.
 */
export function getApiErrorMessage(
  err: unknown,
  fallback = t("Something went wrong. Please try again.")
): string {
  if (isAxiosError(err)) {
    const data = err.response?.data as
      | { error?: string; message?: string; title?: string }
      | undefined;
    return translateApiMessage(
      data?.error || data?.message || data?.title || err.message || fallback
    );
  }
  if (err instanceof Error) return translateApiMessage(err.message);
  return fallback;
}
