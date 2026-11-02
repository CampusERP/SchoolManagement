import { useNavigate } from "react-router-dom";
import { Activity, Building2, GraduationCap, Heart, PlusCircle, ShieldCheck, UserPlus, Users } from "lucide-react";
import { usePlatformDashboard, useSchools } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import AnalyticsCardsGrid from "@/components/organisms/AnalyticsCardsGrid";
import RecentSchoolsTable from "@/components/organisms/RecentSchoolsTable";
import QuickActionCard from "@/components/molecules/QuickActionCard";
import { t } from "@/i18n";
import { usePlatformBillingOverview } from "@/features/billing/hooks";
import { BadgeDollarSign, ReceiptText } from "lucide-react";

export default function PlatformDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError } = usePlatformDashboard();
  const { data: schools, isLoading: schoolsLoading } = useSchools({ page: 1, pageSize: 10 });
  const billing = usePlatformBillingOverview();

  if (isLoading) return <DashboardTemplate title={t("Platform Dashboard")} loading><div /></DashboardTemplate>;
  if (isError || !data) return <DashboardTemplate title={t("Platform Dashboard")}><div className="flex items-center justify-center py-24"><p className="text-[var(--color-text-secondary)]">{t("Failed to load platform statistics.")}</p></div></DashboardTemplate>;

  const statCards = [
    { title: t("Total Schools"), value: data.totalSchools, icon: <Building2 className="h-5 w-5" /> },
    { title: t("Active Schools"), value: data.activeSchools, icon: <Activity className="h-5 w-5" /> },
    { title: t("Inactive Schools"), value: data.suspendedSchools, icon: <Building2 className="h-5 w-5" /> },
    { title: t("Students"), value: data.totalStudents, icon: <Users className="h-5 w-5" /> },
    { title: t("Parents & Guardians"), value: data.totalParents, icon: <Heart className="h-5 w-5" /> },
    { title: t("Teachers"), value: data.totalTeachers, icon: <GraduationCap className="h-5 w-5" /> },
    { title: t("School Admins"), value: data.totalSchoolAdmins, icon: <ShieldCheck className="h-5 w-5" /> },
    { title: t("Active Subscriptions"), value: billing.data?.activeSubscriptions ?? "—", icon: <BadgeDollarSign className="h-5 w-5" /> },
    { title: t("Awaiting Payment"), value: billing.data?.awaitingPayment ?? "—", icon: <ReceiptText className="h-5 w-5" /> },
    { title: t("Outstanding Invoices"), value: billing.data ? `$${billing.data.outstandingBalance.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : "—", icon: <ReceiptText className="h-5 w-5" /> },
  ];

  return <DashboardTemplate title={t("Platform Dashboard")} subtitle={t("SaaS-wide schools, accounts, and adoption metrics")}>
    <AnalyticsCardsGrid cards={statCards} />
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
      <QuickActionCard icon={<PlusCircle className="h-6 w-6" />} label={t("Create School")} onClick={() => navigate("/platform/schools/new")} />
      <QuickActionCard icon={<UserPlus className="h-6 w-6" />} label={t("Register Admin")} onClick={() => navigate("/platform/admins/new")} />
    </div>
    <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-4 shadow-[var(--shadow-card)] sm:p-6">
      <h2 className="mb-4 text-base font-semibold text-[var(--color-text-primary)]">{t("Schools on the platform")}</h2>
      <RecentSchoolsTable data={schools?.items ?? []} loading={schoolsLoading} />
    </div>
  </DashboardTemplate>;
}
