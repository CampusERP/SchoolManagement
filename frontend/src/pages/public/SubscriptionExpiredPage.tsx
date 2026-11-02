import { useNavigate } from "react-router-dom";
import { CreditCard, LogOut, Mail } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { useAuthStore } from "@/store/authStore";
import { ROLE_HOME } from "@/lib/constants";
import { t } from "@/i18n";

export default function SubscriptionExpiredPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);

  const handleSignOut = () => {
    clearAuth();
    navigate("/auth/login", { replace: true });
  };

  return (
    <DashboardTemplate
      title={t("Subscription required")}
      subtitle={t("Your school's subscription is not active right now.")}
    >
      <div className="mx-auto max-w-2xl rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)] sm:p-8">
        <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-[var(--color-warning-soft)] text-[var(--color-warning)]">
          <CreditCard className="h-6 w-6" />
        </div>
        <h2 className="mb-2 text-lg font-semibold text-[var(--color-text-primary)]">
          {t("Access is paused until the subscription is renewed")}
        </h2>
        <p className="mb-6 text-sm text-[var(--color-text-secondary)]">
          {t("Your school's paid period has ended or is awaiting payment. Once the outstanding invoice is settled, access is restored automatically. Contact the platform team if you need help choosing or renewing a plan.")}
        </p>

        <div className="flex flex-wrap gap-3">
          {user?.role === "school_admin" && (
            <Button
              variant="primary"
              icon={<CreditCard className="h-4 w-4" />}
              onClick={() => navigate("/billing")}
            >
              {t("View billing")}
            </Button>
          )}
          <Button
            variant="secondary"
            icon={<Mail className="h-4 w-4" />}
            href="mailto:sales@campuserp.com"
          >
            {t("Contact us for payment")}
          </Button>
          {user?.role !== "school_admin" && user && (
            <Button
              variant="secondary"
              onClick={() => navigate(ROLE_HOME[user.role])}
            >
              {t("Back to dashboard")}
            </Button>
          )}
          <Button variant="ghost" icon={<LogOut className="h-4 w-4" />} onClick={handleSignOut}>
            {t("Sign out")}
          </Button>
        </div>
      </div>
    </DashboardTemplate>
  );
}
