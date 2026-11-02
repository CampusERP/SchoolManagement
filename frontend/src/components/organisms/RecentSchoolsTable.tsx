import { useState } from "react";
import { Table, type TableProps } from "antd";
import dayjs from "dayjs";
import { Eye } from "lucide-react";
import StatusBadge from "@/components/molecules/StatusBadge";
import SearchInput from "@/components/molecules/SearchInput";
import type { School } from "@/types/dashboard.types";
import { t } from "@/i18n";

interface RecentSchoolsTableProps {
  data: School[];
  loading?: boolean;
  onSearch?: (value: string) => void;
}

export default function RecentSchoolsTable({
  data,
  loading,
  onSearch,
}: RecentSchoolsTableProps) {
  const [searchValue, setSearchValue] = useState("");

  const handleSearchChange = (value: string) => {
    setSearchValue(value);
    onSearch?.(value);
  };

  const columns: TableProps<School>["columns"] = [
    {
      title: t("School"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: t("Subdomain"),
      dataIndex: "subdomainCode",
      key: "subdomainCode",
    },
    {
      title: t("Students"),
      dataIndex: "totalStudents",
      key: "totalStudents",
      align: "center",
    },
    {
      title: t("Teachers"),
      dataIndex: "totalTeachers",
      key: "totalTeachers",
      align: "center",
    },
    {
      title: t("Status"),
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: t("Created"),
      dataIndex: "createdAt",
      key: "createdAt",
      render: (date: string) => dayjs(date).format("MMM D, YYYY"),
    },
    {
      title: t("Actions"),
      key: "actions",
      align: "center",
      width: 60,
      render: () => (
        <button className="flex h-8 w-8 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-muted)] hover:bg-[var(--color-surface)] hover:text-[var(--color-primary)] transition-colors" disabled>
          <Eye className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      {onSearch && (
        <SearchInput placeholder={t("Search schools...")} value={searchValue} onChange={handleSearchChange} />
      )}
      <Table<School>
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={loading}
        scroll={{ x: "max-content" }}
        pagination={{ pageSize: 10, showSizeChanger: false }}
      />
    </div>
  );
}
