import { z } from "zod";
import { t } from "@/i18n";

/**
 * Translates the validation messages Zod generates when a schema does not
 * provide its own message (for example an untouched field reported as
 * "Required"). Schema-specific messages are left untouched by Zod, so they
 * keep using the app dictionary directly.
 */
export function initZodErrorMap(): void {
  z.setErrorMap((issue) => {
    switch (issue.code) {
      case z.ZodIssueCode.invalid_type:
        return {
          message:
            issue.received === "undefined" || issue.received === "null"
              ? t("Required")
              : t("Invalid value"),
        };
      case z.ZodIssueCode.too_small:
        if (issue.type === "string") {
          return { message: t("Must be at least {min} characters", { min: String(issue.minimum) }) };
        }
        if (issue.type === "array") {
          return { message: t("Must contain at least {min} item(s)", { min: String(issue.minimum) }) };
        }
        if (issue.type === "date") {
          return { message: t("Date must be after {min}", { min: String(issue.minimum) }) };
        }
        return { message: t("Must be at least {min}", { min: String(issue.minimum) }) };
      case z.ZodIssueCode.too_big:
        if (issue.type === "string") {
          return { message: t("Must be at most {max} characters", { max: String(issue.maximum) }) };
        }
        if (issue.type === "array") {
          return { message: t("Must contain at most {max} item(s)", { max: String(issue.maximum) }) };
        }
        if (issue.type === "date") {
          return { message: t("Date must be before {max}", { max: String(issue.maximum) }) };
        }
        return { message: t("Must be at most {max}", { max: String(issue.maximum) }) };
      case z.ZodIssueCode.invalid_string:
        if (issue.validation === "email") return { message: t("Invalid email address") };
        if (issue.validation === "url") return { message: t("Invalid URL") };
        if (issue.validation === "uuid") return { message: t("Invalid ID") };
        return { message: t("Invalid format") };
      case z.ZodIssueCode.invalid_enum_value:
        return { message: t("Invalid option") };
      case z.ZodIssueCode.invalid_date:
        return { message: t("Invalid date") };
      case z.ZodIssueCode.unrecognized_keys:
      case z.ZodIssueCode.invalid_union:
      case z.ZodIssueCode.invalid_arguments:
      case z.ZodIssueCode.invalid_return_type:
      case z.ZodIssueCode.not_multiple_of:
      case z.ZodIssueCode.not_finite:
        return { message: t("Invalid input") };
      default:
        return { message: t("Invalid value") };
    }
  });
}
