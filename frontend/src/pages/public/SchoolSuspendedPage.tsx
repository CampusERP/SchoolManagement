import { useNavigate } from "react-router-dom";
import { LogOut, Mail, ShieldAlert } from "lucide-react";
import Button from "@/components/atoms/Button";
import Logo from "@/components/atoms/Logo";
import { useAuthStore } from "@/store/authStore";
import { t } from "@/i18n";

/**
 * Shown when the platform administrator suspends a school. The suspension blocks
 * every account in the school (admins, teachers, students, and parents), so this
 * page intentionally avoids the normal app layout and its authenticated requests.
 */
export default function SchoolSuspendedPage() {
  const navigate = useNavigate();
  const clearAuth = useAuthStore((s) => s.clearAuth);

  const handleSignOut = () => {
    clearAuth();
    navigate("/auth/login", { replace: true });
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--color-surface)] p-4">
      <div className="w-full max-w-lg rounded-2xl bg-[var(--color-surface-card)] p-8 text-center shadow-[var(--shadow-elevated)]">
        <Logo className="mx-auto mb-6 h-12 w-12" />
        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-[var(--color-danger-soft)] text-[var(--color-danger)]">
          <ShieldAlert className="h-7 w-7" />
        </div>

        <h1 className="mb-2 text-2xl font-semibold text-[var(--color-text-primary)]">
          {t("School access suspended")}
        </h1>
        <p className="mb-2 text-sm text-[var(--color-text-secondary)]">
          {t("Your school has been suspended by the platform administrator. All accounts in the school are temporarily disabled.")}
        </p>
        <p className="mb-6 text-sm text-[var(--color-text-secondary)]">
          {t("If you believe this is a mistake, or you need to restore access, please contact the platform team.")}
        </p>

        <div className="flex flex-wrap justify-center gap-3">
          <Button
            variant="primary"
            icon={<Mail className="h-4 w-4" />}
            href="mailto:support@campuserp.com"
          >
            {t("Contact support")}
          </Button>
          <Button
            variant="ghost"
            icon={<LogOut className="h-4 w-4" />}
            onClick={handleSignOut}
          >
            {t("Sign out")}
          </Button>
        </div>
      </div>
    </div>
  );
}
