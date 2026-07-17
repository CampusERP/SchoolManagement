import { Table, type TableProps } from "antd";

interface ClassRecord {
  id: string;
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
      title: "Class Name",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: "Subject",
      dataIndex: "subject",
      key: "subject",
    },
    {
      title: "Grade Level",
      dataIndex: "gradeLevel",
      key: "gradeLevel",
    },
    {
      title: "Students",
      dataIndex: "studentCount",
      key: "studentCount",
      align: "center",
    },
  ];

  return (
    <Table<ClassRecord>
      columns={columns}
      dataSource={data}
      rowKey="id"
      loading={loading}
      pagination={{ pageSize: 10, showSizeChanger: false }}
    />
  );
}
