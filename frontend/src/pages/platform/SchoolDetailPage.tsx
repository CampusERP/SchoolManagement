import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Building2, Hash, Pencil, UserPlus, MapPin } from "lucide-react";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import StatusBadge from "@/components/molecules/StatusBadge";
import Button from "@/components/atoms/Button";
import { SchoolsApi, type SchoolDetail } from "@/features/schools/api";

export default function SchoolDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [school, setSchool] = useState<SchoolDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    let active = true;
    setIsLoading(true);
    SchoolsApi.getById(id)
      .then((data) => {
        if (active) setSchool(data);
      })
      .catch(() => {
        if (active) toast.error("Failed to load school.");
      })
      .finally(() => {
        if (active) setIsLoading(false);
      });
    return () => {
      active = false;
    };
  }, [id]);

  if (isLoading) {
    return (
      <DashboardTemplate title="School Details" loading>
        <div />
      </DashboardTemplate>
    );
  }

  if (!school) {
    return (
      <DashboardTemplate title="School Details">
        <div className="flex items-center justify-center py-24">
          <p className="text-[var(--color-text-secondary)]">School not found.</p>
        </div>
      </DashboardTemplate>
    );
  }

  return (
    <DashboardTemplate
      title={school.name}
      subtitle={`Subdomain: ${school.subdomainCode}`}
      actions={
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            icon={<UserPlus className="h-4 w-4" />}
            onClick={() => navigate(`/platform/admins/new?schoolId=${school.id}`)}
          >
            Register Admin
          </Button>
          <Button
            variant="primary"
            icon={<Pencil className="h-4 w-4" />}
            onClick={() => navigate(`/platform/schools/${school.id}/edit`)}
          >
            Edit
          </Button>
        </div>
      }
    >
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)]">
            <h3 className="mb-4 text-sm font-semibold text-[var(--color-text-primary)]">
              General Information
            </h3>
            <dl className="space-y-3">
              <div className="flex items-center gap-3">
                <Building2 className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Name</dt>
                <dd className="text-sm font-medium text-[var(--color-text-primary)]">{school.name}</dd>
              </div>
              <div className="flex items-center gap-3">
                <Hash className="h-4 w-4 text-[var(--color-text-muted)]" />
                <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Subdomain</dt>
                <dd className="font-mono text-sm text-[var(--color-text-primary)]">{school.subdomainCode}</dd>
              </div>
              <div className="flex items-center gap-3">
                <dt className="w-32 text-sm text-[var(--color-text-secondary)]">Status</dt>
                <dd><StatusBadge status={school.status} /></dd>
              </div>
            </dl>
          </div>

          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)]">
            <h3 className="mb-4 text-sm font-semibold text-[var(--color-text-primary)]">
              Campuses
            </h3>
            {school.campuses.length === 0 ? (
              <p className="text-sm text-[var(--color-text-muted)]">No campuses registered.</p>
            ) : (
              <ul className="space-y-3">
                {school.campuses.map((campus) => (
                  <li key={campus.id} className="flex items-start gap-3">
                    <MapPin className="mt-0.5 h-4 w-4 text-[var(--color-text-muted)]" />
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-[var(--color-text-primary)]">{campus.name}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">{campus.address}</p>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>

        <div className="space-y-6">
          <div className="rounded-[var(--card-radius)] bg-[var(--color-surface-card)] p-6 shadow-[var(--shadow-card)]">
            <h3 className="mb-2 text-sm font-semibold text-[var(--color-text-primary)]">
              School Admin
            </h3>
            <p className="mb-4 text-sm text-[var(--color-text-secondary)]">
              Every school must have a school admin to manage its day-to-day operations.
            </p>
            <Button
              variant="primary"
              icon={<UserPlus className="h-4 w-4" />}
              onClick={() => navigate(`/platform/admins/new?schoolId=${school.id}`)}
            >
              Register School Admin
            </Button>
          </div>
        </div>
      </div>
    </DashboardTemplate>
  );
}
