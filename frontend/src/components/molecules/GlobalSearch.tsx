import { useEffect, useMemo, useRef, useState, type KeyboardEvent, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Building2, GraduationCap, UserRound, Users } from "lucide-react";
import SearchInput from "@/components/molecules/SearchInput";
import { useAuthStore } from "@/store/authStore";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { DashboardApi } from "@/features/dashboard/api";
import { StudentsApi } from "@/features/students/api";
import { TeachersApi } from "@/features/teachers/api";
import { ParentsApi } from "@/features/parents/api";
import type { School } from "@/types/dashboard.types";
import { t } from "@/i18n";

type SearchResult = {
  id: string;
  label: string;
  meta?: string;
  href: string;
};

type SearchGroup = {
  key: string;
  label: string;
  icon: ReactNode;
  items: SearchResult[];
};

export default function GlobalSearch() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const schoolId = user?.schoolId;
  const isPlatformAdmin = user?.role === "platform_admin";
  const isSchoolAdmin = user?.role === "school_admin";

  const [value, setValue] = useState("");
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  const debounced = useDebouncedValue(value.trim(), 300);
  const isQueryReady = debounced.length >= 2;

  const schoolsQuery = useQuery({
    queryKey: ["global-search", "schools", debounced],
    queryFn: () =>
      DashboardApi.getSchools({ page: 1, pageSize: 5, search: debounced }),
    enabled: isPlatformAdmin && isQueryReady,
    staleTime: 30_000,
  });

  const studentsQuery = useQuery({
    queryKey: ["global-search", "students", schoolId, debounced],
    queryFn: () =>
      StudentsApi.getStudents({
        schoolId: schoolId!,
        searchTerm: debounced,
        page: 1,
        pageSize: 5,
      }),
    enabled: isSchoolAdmin && !!schoolId && isQueryReady,
    staleTime: 30_000,
  });

  const teachersQuery = useQuery({
    queryKey: ["global-search", "teachers", schoolId, debounced],
    queryFn: () =>
      TeachersApi.getTeachers({
        schoolId: schoolId!,
        searchTerm: debounced,
        page: 1,
        pageSize: 5,
      }),
    enabled: isSchoolAdmin && !!schoolId && isQueryReady,
    staleTime: 30_000,
  });

  const parentsQuery = useQuery({
    queryKey: ["global-search", "parents", schoolId, debounced],
    queryFn: () =>
      ParentsApi.getParents({
        schoolId: schoolId!,
        searchTerm: debounced,
        page: 1,
        pageSize: 5,
      }),
    enabled: isSchoolAdmin && !!schoolId && isQueryReady,
    staleTime: 30_000,
  });

  useEffect(() => {
    const handlePointerDown = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    document.addEventListener("mousedown", handlePointerDown);
    return () => document.removeEventListener("mousedown", handlePointerDown);
  }, []);

  const groups = useMemo<SearchGroup[]>(() => {
    const next: SearchGroup[] = [];

    if (isPlatformAdmin && schoolsQuery.data?.items.length) {
      next.push({
        key: "schools",
        label: t("Schools"),
        icon: <Building2 className="h-4 w-4" />,
        items: schoolsQuery.data.items.map((school: School) => ({
          id: school.id,
          label: school.name,
          meta: school.subdomainCode,
          href: `/platform/schools/${school.id}`,
        })),
      });
    }

    if (isSchoolAdmin && studentsQuery.data?.items.length) {
      next.push({
        key: "students",
        label: t("Students"),
        icon: <GraduationCap className="h-4 w-4" />,
        items: studentsQuery.data.items.map((student) => ({
          id: student.id,
          label: `${student.firstName} ${student.lastName}`,
          meta: student.studentCode,
          href: `/people/students/${student.id}`,
        })),
      });
    }

    if (isSchoolAdmin && teachersQuery.data?.items.length) {
      next.push({
        key: "teachers",
        label: t("Teachers"),
        icon: <UserRound className="h-4 w-4" />,
        items: teachersQuery.data.items.map((teacher) => ({
          id: teacher.id,
          label: `${teacher.firstName} ${teacher.lastName}`,
          meta: teacher.employeeCode,
          href: `/people/teachers/${teacher.id}`,
        })),
      });
    }

    if (isSchoolAdmin && parentsQuery.data?.items.length) {
      next.push({
        key: "parents",
        label: t("Parents"),
        icon: <Users className="h-4 w-4" />,
        items: parentsQuery.data.items.map((parent) => ({
          id: parent.id,
          label: `${parent.firstName} ${parent.lastName}`,
          href: `/people/parents/${parent.id}`,
        })),
      });
    }

    return next;
  }, [
    isPlatformAdmin,
    isSchoolAdmin,
    schoolsQuery.data,
    studentsQuery.data,
    teachersQuery.data,
    parentsQuery.data,
  ]);

  const allResults = groups.flatMap((group) => group.items);
  const isLoading =
    schoolsQuery.isFetching ||
    studentsQuery.isFetching ||
    teachersQuery.isFetching ||
    parentsQuery.isFetching;

  const close = () => {
    setOpen(false);
    setValue("");
  };

  const selectResult = (result: SearchResult) => {
    close();
    navigate(result.href);
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Escape") {
      close();
    } else if (event.key === "Enter" && allResults.length > 0) {
      event.preventDefault();
      selectResult(allResults[0]);
    }
  };

  const showDropdown = open && value.trim().length > 0;

  return (
    <div ref={containerRef} className="relative w-full">
      <SearchInput
        value={value}
        onChange={(next) => {
          setValue(next);
          setOpen(true);
        }}
        onFocus={() => setOpen(true)}
        onKeyDown={handleKeyDown}
        placeholder={t("Search...")}
        ariaLabel={t("Search")}
      />

      {showDropdown && (
        <div className="absolute inset-x-0 top-full z-50 mt-2 overflow-hidden rounded-[var(--card-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] shadow-[var(--shadow-card)]">
          {!isQueryReady ? (
            <p className="px-4 py-3 text-sm text-[var(--color-text-muted)]">
              {t("Type at least 2 characters")}
            </p>
          ) : isLoading && allResults.length === 0 ? (
            <p className="px-4 py-3 text-sm text-[var(--color-text-muted)]">
              {t("Searching...")}
            </p>
          ) : allResults.length === 0 ? (
            <p className="px-4 py-3 text-sm text-[var(--color-text-muted)]">
              {t("No results found")}
            </p>
          ) : (
            <div className="max-h-80 overflow-y-auto py-1">
              {groups.map((group) => (
                <div key={group.key}>
                  <div className="flex items-center gap-2 px-4 pb-1 pt-3 text-[11px] font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                    {group.icon}
                    {group.label}
                  </div>
                  {group.items.map((item) => (
                    <button
                      key={`${group.key}-${item.id}`}
                      type="button"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => selectResult(item)}
                      className="flex w-full items-center justify-between gap-3 px-4 py-2 text-start text-sm text-[var(--color-text-primary)] transition-colors hover:bg-[var(--color-surface)]"
                    >
                      <span className="truncate font-medium">{item.label}</span>
                      {item.meta && (
                        <span className="shrink-0 text-xs text-[var(--color-text-muted)]">
                          {item.meta}
                        </span>
                      )}
                    </button>
                  ))}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
