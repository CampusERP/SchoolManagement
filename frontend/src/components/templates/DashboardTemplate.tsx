import type { ReactNode } from "react";
import DashboardHeader from "@/components/organisms/DashboardHeader";
import Spinner from "@/components/atoms/Spinner";

interface DashboardTemplateProps {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  children: ReactNode;
  loading?: boolean;
}

export default function DashboardTemplate({
  title,
  subtitle,
  actions,
  children,
  loading = false,
}: DashboardTemplateProps) {
  return (
    <div className="space-y-6">
      <DashboardHeader title={title} subtitle={subtitle} actions={actions} />

      {loading ? (
        <div className="flex items-center justify-center py-24">
          <Spinner size="lg" />
        </div>
      ) : (
        children
      )}
    </div>
  );
}
