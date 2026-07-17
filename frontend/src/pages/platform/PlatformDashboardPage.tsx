import { useNavigate } from "react-router-dom";
import {
  Building2,
  Users,
  GraduationCap,
  DollarSign,
  CreditCard,
  Activity,
  PlusCircle,
  UserPlus,
  HeartPulse,
} from "lucide-react";
import { usePlatformDashboard } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import AnalyticsCardsGrid from "@/components/organisms/AnalyticsCardsGrid";
import RecentSchoolsTable from "@/components/organisms/RecentSchoolsTable";
import TimelinePanel from "@/components/organisms/TimelinePanel";
import ChartsPlaceholder from "@/components/organisms/ChartsPlaceholder";
import QuickActionCard from "@/components/molecules/QuickActionCard";
import StatusBadge from "@/components/molecules/StatusBadge";
import Spinner from "@/components/atoms/Spinner";

export default function PlatformDashboardPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError } = usePlatformDashboard();

  if (isLoading) {
    return (
      <DashboardTemplate title="Platform Dashboard" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (isError || !data) {
    return (
      <DashboardTemplate title="Platform Dashboard">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">Failed to load dashboard data.</p>
        </div>
      </DashboardTemplate>
    );
  }

  const statCards = [
    { title: "Total Schools", value: data.totalSchools, icon: <Building2 className="h-5 w-5" /> },
    { title: "Active Schools", value: data.activeSchools, icon: <Activity className="h-5 w-5" /> },
    { title: "Total Students", value: data.totalStudents, icon: <GraduationCap className="h-5 w-5" /> },
    { title: "Total Teachers", value: data.totalTeachers, icon: <Users className="h-5 w-5" /> },
    { title: "Total Revenue", value: `$${data.totalRevenue.toLocaleString()}`, icon: <DollarSign className="h-5 w-5" /> },
    { title: "Active Subscriptions", value: data.activeSubscriptions, icon: <CreditCard className="h-5 w-5" /> },
  ];

  return (
    <DashboardTemplate title="Platform Dashboard" subtitle="Overview of all schools and system performance">
      <AnalyticsCardsGrid cards={statCards} />

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <QuickActionCard
          icon={<PlusCircle className="h-6 w-6" />}
          label="Create School"
          onClick={() => navigate("/platform/schools/new")}
        />
        <QuickActionCard
          icon={<UserPlus className="h-6 w-6" />}
          label="Register Admin"
          onClick={() => navigate("/platform/admins/new")}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <RecentSchoolsTable data={data.recentSchools} />
        </div>
        <div className="space-y-6">
          <TimelinePanel activities={data.recentActivity} />

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-5 shadow-[var(--shadow-card)]">
            <div className="mb-4 flex items-center gap-2">
              <HeartPulse className="h-5 w-5 text-[var(--color-primary)]" />
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
                System Health
              </h3>
            </div>
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-[var(--color-text-secondary)]">API Status</span>
                <StatusBadge status={data.systemHealth.apiStatus.charAt(0).toUpperCase() + data.systemHealth.apiStatus.slice(1)} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-[var(--color-text-secondary)]">Database</span>
                <StatusBadge status={data.systemHealth.dbStatus.charAt(0).toUpperCase() + data.systemHealth.dbStatus.slice(1)} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-[var(--color-text-secondary)]">Uptime</span>
                <span className="text-sm font-medium text-[var(--color-text-primary)]">{data.systemHealth.uptime}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <ChartsPlaceholder title="Revenue & Growth Analytics" />
    </DashboardTemplate>
  );
}
