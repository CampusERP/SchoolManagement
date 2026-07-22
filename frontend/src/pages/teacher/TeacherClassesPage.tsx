import { useState } from "react";
import { Table, type TableProps } from "antd";
import { ArrowLeft, Users } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { useAuthStore } from "@/store/authStore";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import { useTeacherRoster } from "@/features/teacher/hooks";
import type { RosterStudent } from "@/features/teacher/api";

export default function TeacherClassesPage() {
  const { data: dashboard, isLoading: dashLoading } = useTeacherDashboard();
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const [selectedClassRoomId, setSelectedClassRoomId] = useState<string | null>(null);
  const [selectedClassName, setSelectedClassName] = useState<string>("");
  const { data: rosterData, isLoading: rosterLoading } = useTeacherRoster(selectedClassRoomId, schoolId);

  if (selectedClassRoomId) {
    return (
      <DashboardTemplate
        title={selectedClassName}
        subtitle="Student roster"
        loading={rosterLoading}
        actions={
          <button
            onClick={() => setSelectedClassRoomId(null)}
            className="flex items-center gap-2 rounded-lg border border-[var(--color-border)] px-3 py-2 text-sm text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]"
          >
            <ArrowLeft className="h-4 w-4" /> Back to classes
          </button>
        }
      >
        <RosterTable data={rosterData?.items ?? []} />
      </DashboardTemplate>
    );
  }

  const classColumns: TableProps<{
    classRoomId: string;
    name: string;
    subject: string;
    studentCount: number;
    gradeLevel: string;
  }>["columns"] = [
    {
      title: "Class Name",
      dataIndex: "name",
      key: "name",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    { title: "Subject", dataIndex: "subject", key: "subject" },
    { title: "Grade Level", dataIndex: "gradeLevel", key: "gradeLevel" },
    { title: "Students", dataIndex: "studentCount", key: "studentCount", align: "center" },
    {
      title: "",
      key: "action",
      width: 80,
      render: (_: unknown, record: { classRoomId: string; name: string }) => (
        <button
          onClick={() => {
            setSelectedClassRoomId(record.classRoomId);
            setSelectedClassName(record.name);
          }}
          className="rounded-lg bg-[var(--color-primary)] px-3 py-1.5 text-xs font-medium text-white hover:opacity-90"
        >
          View roster
        </button>
      ),
    },
  ];

  const classes = (dashboard?.myClasses ?? []).map((c) => ({
    classRoomId: c.classRoomId,
    name: c.name,
    subject: c.subject,
    studentCount: c.studentCount,
    gradeLevel: c.gradeLevel,
  }));

  return (
    <DashboardTemplate title="My Classes" subtitle="Classes you teach and their student rosters" loading={dashLoading}>
      <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-1">
        <Table
          columns={classColumns}
          dataSource={classes}
          rowKey="classRoomId"
          pagination={{ pageSize: 10, showSizeChanger: false }}
        />
      </div>
    </DashboardTemplate>
  );
}

function RosterTable({ data }: { data: RosterStudent[] }) {
  const columns: TableProps<RosterStudent>["columns"] = [
    {
      title: "Student Code",
      dataIndex: "studentCode",
      key: "studentCode",
      render: (code: string) => (
        <span className="font-mono text-xs text-[var(--color-text-muted)]">{code}</span>
      ),
    },
    {
      title: "First Name",
      dataIndex: "firstName",
      key: "firstName",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: "Last Name",
      dataIndex: "lastName",
      key: "lastName",
      render: (name: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{name}</span>
      ),
    },
    {
      title: "Status",
      dataIndex: "enrollmentStatus",
      key: "status",
      render: (status: string) => (
        <span
          className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
            status === "Active"
              ? "bg-green-50 text-green-700"
              : "bg-gray-100 text-gray-600"
          }`}
        >
          {status}
        </span>
      ),
    },
  ];

  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-1">
      {data.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <Users className="mb-3 h-8 w-8 text-[var(--color-text-muted)]" />
          <p className="text-sm text-[var(--color-text-secondary)]">No students enrolled in this class.</p>
        </div>
      ) : (
        <Table<RosterStudent>
          columns={columns}
          dataSource={data}
          rowKey="enrollmentId"
          pagination={{ pageSize: 20, showSizeChanger: false }}
        />
      )}
    </div>
  );
}
