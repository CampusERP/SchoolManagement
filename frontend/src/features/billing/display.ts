import { localize, t } from "@/i18n";
import type { SubscriptionPlan } from "./api";

type LocalizedBenefit = {
  descriptionEn?: string | null;
  descriptionAr?: string | null;
};

export const localizedPlanDescription = (
  plan: Pick<SubscriptionPlan, "descriptionEn" | "descriptionAr">
): string => localize(plan.descriptionEn, plan.descriptionAr);

export const localizedBenefitDescriptions = (
  benefits?: LocalizedBenefit[] | null
): string[] => {
  const descriptions = (benefits ?? [])
    .map((benefit) => localize(benefit.descriptionEn, benefit.descriptionAr))
    .filter(Boolean);
  return [...new Set(descriptions)];
};

export const planFeatureLabels = (plan: SubscriptionPlan): string[] => {
  const features = [
    t("{count} students", { count: plan.maxStudents.toLocaleString() }),
    t("{count} teachers", { count: plan.maxTeachers.toLocaleString() }),
  ];
  if (plan.hasParentPortal) features.push(t("Parent portal"));
  if (plan.hasExamModule) features.push(t("Exam & results module"));
  if (plan.hasAnalytics) features.push(t("Advanced analytics"));
  return features;
};

/**
 * Marketing bullets shown on public plan cards. Benefits replace the generic
 * feature labels when they exist, while limits are always shown.
 */
export const planHighlightLabels = (plan: SubscriptionPlan): string[] => {
  const benefits = localizedBenefitDescriptions(plan.benefits);
  const limits = planFeatureLabels(plan).slice(0, 2);
  return benefits.length > 0 ? [...benefits, ...limits] : planFeatureLabels(plan);
};
