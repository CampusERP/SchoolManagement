import { Table, type TableProps } from "antd";
import { t } from "@/i18n";

interface ClassRecord {
  id?: string;
  classRoomId?: string;
  name: string;
  subject: string;
  studentCount: number;
  gradeLevel: string;
}

interface MyClassesTableProps {
  data: ClassRecord[];
  loading?: boolean;
}

export default function MyClassesTable({
  data,
  loading,
}: MyClassesTableProps) {
  const columns: TableProps<ClassRecord>["columns"] = [
    {
      title: t("Class Name"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: t("Subject"),
      dataIndex: "subject",
      key: "subject",
    },
    {
      title: t("Grade Level"),
      dataIndex: "gradeLevel",
      key: "gradeLevel",
    },
    {
      title: t("Students"),
      dataIndex: "studentCount",
      key: "studentCount",
      align: "center",
    },
  ];

  return (
    <Table<ClassRecord>
      columns={columns}
      dataSource={data}
      rowKey={(r) => r.classRoomId ?? r.id ?? r.name}
      loading={loading}
      pagination={{ pageSize: 10, showSizeChanger: false }}
    />
  );
}
