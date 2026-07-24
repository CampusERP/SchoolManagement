import { useState } from "react";
import { Table, type TableProps, Select, Button, Modal, InputNumber, Input, message, Tag } from "antd";
import { ArrowLeft, Award } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { useAuthStore } from "@/store/authStore";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import { useClassAssignments, useGradeSubmission } from "@/features/teacher/hooks";
import type { AssignmentSummary } from "@/features/teacher/api";

export default function TeacherGradesPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const { data: dashboard, isLoading: dashLoading } = useTeacherDashboard();
  const [selectedTaId, setSelectedTaId] = useState<string | null>(null);
  const [selectedAssignment, setSelectedAssignment] = useState<AssignmentSummary | null>(null);

  const { data: assignmentsData, isLoading: assignLoading } = useClassAssignments(selectedTaId, schoolId);

  const classes = dashboard?.myClasses ?? [];
  const assignments = assignmentsData?.items ?? [];

  const assignmentColumns: TableProps<AssignmentSummary>["columns"] = [
    {
      title: "Title",
      dataIndex: "title",
      key: "title",
      render: (title: string) => (
        <span className="font-medium text-[var(--color-text-primary)]">{title}</span>
      ),
    },
    {
      title: "Due Date",
      dataIndex: "dueDate",
      key: "dueDate",
      render: (d: string) => new Date(d).toLocaleDateString(),
    },
    {
      title: "Submissions",
      key: "subs",
      render: (_: unknown, r: AssignmentSummary) => (
        <div className="flex gap-1">
          <Tag color="green">{r.gradedSubmissions} graded</Tag>
          <Tag color="orange">{r.pendingSubmissions} pending</Tag>
        </div>
      ),
    },
    {
      title: "",
      key: "action",
      width: 100,
      render: (_: unknown, record: AssignmentSummary) => (
        <button
          onClick={() => setSelectedAssignment(record)}
          className="rounded-lg bg-[var(--color-primary)] px-3 py-1.5 text-xs font-medium text-white hover:opacity-90"
        >
          View details
        </button>
      ),
    },
  ];

  if (selectedAssignment) {
    return (
      <GradesDetail
        assignment={selectedAssignment}
        schoolId={schoolId}
        onBack={() => setSelectedAssignment(null)}
      />
    );
  }

  return (
    <DashboardTemplate
      title="Enter Grades"
      subtitle="Grade student assignments"
      loading={dashLoading}
    >
      <div className="space-y-4">
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            Select a class
          </label>
          <Select
            className="w-full max-w-md"
            placeholder="Choose a class..."
            value={selectedTaId}
            onChange={setSelectedTaId}
            options={classes.map((c) => ({
              value: c.teachingAssignmentId,
              label: `${c.subject} - ${c.name}`,
            }))}
            loading={dashLoading}
            disabled={classes.length === 0}
          />
          {classes.length === 0 && !dashLoading && (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              No classes assigned. Contact your administrator.
            </p>
          )}
        </div>

        {selectedTaId && (
          <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-1">
            <Table<AssignmentSummary>
              columns={assignmentColumns}
              dataSource={assignments}
              rowKey="id"
              loading={assignLoading}
              pagination={{ pageSize: 10, showSizeChanger: false }}
            />
          </div>
        )}
      </div>
    </DashboardTemplate>
  );
}

function GradesDetail({
  assignment,
  schoolId,
  onBack,
}: {
  assignment: AssignmentSummary;
  schoolId: string;
  onBack: () => void;
}) {
  const gradeMutation = useGradeSubmission();
  const [modalOpen, setModalOpen] = useState(false);
  const [currentSubmission, setCurrentSubmission] = useState<{
    submissionId: string;
    studentName: string;
  } | null>(null);
  const [gradeValue, setGradeValue] = useState<number>(0);
  const [feedback, setFeedback] = useState("");

  const handleGrade = async () => {
    if (!currentSubmission) return;
    try {
      await gradeMutation.mutateAsync({
        schoolId,
        assignmentId: assignment.id,
        submissionId: currentSubmission.submissionId,
        grade: gradeValue,
        feedback: feedback || undefined,
      });
      message.success("Grade submitted");
      setModalOpen(false);
      setCurrentSubmission(null);
      setGradeValue(0);
      setFeedback("");
    } catch {
      message.error("Failed to submit grade");
    }
  };

  return (
    <DashboardTemplate
      title={assignment.title}
      subtitle={`Due: ${new Date(assignment.dueDate).toLocaleDateString()} · Max score: ${assignment.maxScore ?? "N/A"}`}
      actions={
        <button
          onClick={onBack}
          className="flex items-center gap-2 rounded-lg border border-[var(--color-border)] px-3 py-2 text-sm text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]"
        >
          <ArrowLeft className="h-4 w-4" /> Back
        </button>
      }
    >
      <div className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4 text-center">
          <p className="text-sm text-[var(--color-text-secondary)]">Total Submissions</p>
          <p className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">{assignment.totalSubmissions}</p>
        </div>
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4 text-center">
          <p className="text-sm text-[var(--color-text-secondary)]">Graded</p>
          <p className="mt-1 text-2xl font-semibold text-green-600">{assignment.gradedSubmissions}</p>
        </div>
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4 text-center">
          <p className="text-sm text-[var(--color-text-secondary)]">Pending</p>
          <p className="mt-1 text-2xl font-semibold text-amber-600">{assignment.pendingSubmissions}</p>
        </div>
      </div>

      <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-4">
        <p className="mb-3 text-sm font-medium text-[var(--color-text-primary)]">Grading</p>
        {assignment.pendingSubmissions > 0 ? (
          <p className="text-sm text-[var(--color-text-secondary)]">
            {assignment.pendingSubmissions} submission{assignment.pendingSubmissions !== 1 ? "s" : ""} awaiting your review.
            Student submissions are managed through the assignment system.
          </p>
        ) : (
          <div className="flex items-center gap-2 text-green-600">
            <Award className="h-5 w-5" />
            <span className="text-sm font-medium">All submissions have been graded!</span>
          </div>
        )}
      </div>

      <Modal
        title={`Grade: ${currentSubmission?.studentName}`}
        open={modalOpen}
        onOk={handleGrade}
        onCancel={() => setModalOpen(false)}
        confirmLoading={gradeMutation.isPending}
        okText="Submit Grade"
      >
        <div className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium">Grade</label>
            <InputNumber
              className="w-full"
              min={0}
              max={assignment.maxScore ?? 100}
              value={gradeValue}
              onChange={(v) => setGradeValue(v ?? 0)}
              placeholder={`0 - ${assignment.maxScore ?? 100}`}
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium">Feedback (optional)</label>
            <Input.TextArea
              rows={3}
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              placeholder="Add feedback for the student..."
            />
          </div>
        </div>
      </Modal>
    </DashboardTemplate>
  );
}
