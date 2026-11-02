import { Table, type TableProps } from "antd";
import dayjs from "dayjs";
import StatusBadge from "@/components/molecules/StatusBadge";
import { t } from "@/i18n";

interface StudentRecord {
  id: string;
  name: string;
  code: string;
  gradeLevel: string;
  enrolledAt: string;
  status: string;
}

interface RecentStudentsTableProps {
  data: StudentRecord[];
  loading?: boolean;
}

export default function RecentStudentsTable({
  data,
  loading,
}: RecentStudentsTableProps) {
  const columns: TableProps<StudentRecord>["columns"] = [
    {
      title: t("Name"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => {
        const initials = name
          .split(" ")
          .map((n) => n[0])
          .join("")
          .toUpperCase()
          .slice(0, 2);
        return (
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-[var(--color-primary-light)] text-[var(--color-primary)] text-xs font-semibold">
              {initials}
            </div>
            <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
          </div>
        );
      },
    },
    {
      title: t("Code"),
      dataIndex: "code",
      key: "code",
    },
    {
      title: t("Grade Level"),
      dataIndex: "gradeLevel",
      key: "gradeLevel",
    },
    {
      title: t("Enrolled"),
      dataIndex: "enrolledAt",
      key: "enrolledAt",
      render: (date: string) => dayjs(date).format("MMM D, YYYY"),
    },
    {
      title: t("Status"),
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
  ];

  return (
    <Table<StudentRecord>
      columns={columns}
      dataSource={data}
      rowKey="id"
      loading={loading}
      scroll={{ x: "max-content" }}
      pagination={{ pageSize: 10, showSizeChanger: false }}
    />
  );
}
