import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Search } from "lucide-react";
import { useSchools } from "@/features/dashboard/hooks";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { Table, type TableProps } from "antd";
import dayjs from "dayjs";
import StatusBadge from "@/components/molecules/StatusBadge";
import Button from "@/components/atoms/Button";
import Input from "@/components/atoms/Input";
import type { School } from "@/types/dashboard.types";

export default function SchoolsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const pageSize = 20;
  const navigate = useNavigate();

  const { data, isLoading } = useSchools({ page, pageSize, search });

  const columns: TableProps<School>["columns"] = [
    {
      title: "School Name",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: "Subdomain",
      dataIndex: "subdomainCode",
      key: "subdomainCode",
      render: (code: string) => (
        <span className="font-mono text-sm text-[var(--color-text-secondary)]">{code}</span>
      ),
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: "Students",
      dataIndex: "totalStudents",
      key: "totalStudents",
      align: "center",
      render: (count: number) => (
        <span className="font-medium text-[var(--color-text-primary)]">{count}</span>
      ),
    },
    {
      title: "Teachers",
      dataIndex: "totalTeachers",
      key: "totalTeachers",
      align: "center",
      render: (count: number) => (
        <span className="font-medium text-[var(--color-text-primary)]">{count}</span>
      ),
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
      width: 120,
      render: (_: unknown, record: School) => (
        <div className="flex items-center justify-center gap-2">
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => navigate(`/platform/schools/${record.id}`)}>
            View
          </Button>
          <Button variant="ghost" type="link" size="small" className="p-0" onClick={() => navigate(`/platform/schools/${record.id}/edit`)}>
            Edit
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title="Schools"
      subtitle="Manage all schools in the platform"
      actions={
        <Button variant="primary" icon={<Plus className="h-4 w-4" />} onClick={() => navigate("/platform/schools/new")}>
          Create School
        </Button>
      }
    >
      <div className="mb-6 flex items-center gap-4">
        <Input
          placeholder="Search schools..."
          prefix={<Search className="h-4 w-4 text-[var(--color-text-muted)]" />}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-md"
          allowClear
        />
      </div>

      <Table<School>
        columns={columns}
        dataSource={data?.items || []}
        rowKey="id"
        loading={isLoading}
        pagination={{
          current: page,
          pageSize,
          total: data?.totalCount || 0,
          onChange: setPage,
          showSizeChanger: false,
          showTotal: (total) => `Total ${total} schools`,
        }}
      />
    </DashboardTemplate>
  );
}
