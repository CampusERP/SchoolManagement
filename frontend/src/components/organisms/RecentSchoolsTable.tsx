import { useState } from "react";
import { Table, type TableProps } from "antd";
import dayjs from "dayjs";
import { Eye } from "lucide-react";
import StatusBadge from "@/components/molecules/StatusBadge";
import SearchInput from "@/components/molecules/SearchInput";

interface SchoolRecord {
  id: string;
  name: string;
  ownerName: string;
  totalStudents: number;
  totalTeachers: number;
  subscriptionStatus: string;
  createdAt: string;
  status: string;
}

interface RecentSchoolsTableProps {
  data: SchoolRecord[];
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

  const columns: TableProps<SchoolRecord>["columns"] = [
    {
      title: "School",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: "Owner",
      dataIndex: "ownerName",
      key: "ownerName",
    },
    {
      title: "Students",
      dataIndex: "totalStudents",
      key: "totalStudents",
      align: "center",
    },
    {
      title: "Teachers",
      dataIndex: "totalTeachers",
      key: "totalTeachers",
      align: "center",
    },
    {
      title: "Subscription",
      dataIndex: "subscriptionStatus",
      key: "subscriptionStatus",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: "Created",
      dataIndex: "createdAt",
      key: "createdAt",
      render: (date: string) => dayjs(date).format("MMM D, YYYY"),
    },
    {
      title: "Actions",
      key: "actions",
      align: "center",
      width: 60,
      render: () => (
        <button className="flex h-8 w-8 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-muted)] hover:bg-[var(--color-surface)] hover:text-[var(--color-primary)] transition-colors">
          <Eye className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      {onSearch && (
        <SearchInput placeholder="Search schools..." value={searchValue} onChange={handleSearchChange} />
      )}
      <Table<SchoolRecord>
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={loading}
        pagination={{ pageSize: 10, showSizeChanger: false }}
      />
    </div>
  );
}
