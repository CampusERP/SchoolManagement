import { useState, useEffect, useMemo } from "react";
import { Table, type TableProps, Button, Modal, Form, Input, InputNumber, Select, DatePicker, message, Tag } from "antd";
import { Plus, Eye, Lock, Calendar } from "lucide-react";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import { useAuthStore } from "@/store/authStore";
import { useTeacherDashboard } from "@/features/dashboard/hooks";
import {
  useTeacherExams,
  useCreateExam,
  useAddExamSchedule,
  useRecordExamResults,
  useExamSchedules,
  useSubjects,
  useRooms,
  useClassExamResults,
  useLockExam,
} from "@/features/teacher/hooks";
import type { ExamListItem, ExamScheduleItem } from "@/features/teacher/api";
import { useTeacherRoster } from "@/features/teacher/hooks";
import { t } from "@/i18n";

export default function TeacherExamsPage() {
  const schoolId = useAuthStore((s) => s.user?.schoolId) ?? "";
  const { data: dashboard, isLoading: dashLoading } = useTeacherDashboard();
  const termId = dashboard?.currentTermId ?? null;

  const { data: examsData, isLoading: examsLoading } = useTeacherExams(schoolId, termId);
  const createExam = useCreateExam();
  const addSchedule = useAddExamSchedule();
  const lockExam = useLockExam();

  const { data: subjects } = useSubjects();
  const { data: rooms } = useRooms(schoolId || null);

  const [createOpen, setCreateOpen] = useState(false);
  const [scheduleOpen, setScheduleOpen] = useState(false);
  const [selectedExamId, setSelectedExamId] = useState<string | null>(null);
  const [selectedExamName, setSelectedExamName] = useState<string>("");
  const [createForm] = Form.useForm();
  const [scheduleForm] = Form.useForm();

  const classes = dashboard?.myClasses ?? [];

  const assignedSubjectNames = useMemo(() => {
    return new Set((dashboard?.myClasses ?? []).map((c) => c.subject));
  }, [dashboard?.myClasses]);

  const filteredSubjects = useMemo(() => {
    return (subjects ?? []).filter((s) => assignedSubjectNames.has(s.name));
  }, [subjects, assignedSubjectNames]);

  const selectedExam = useMemo(
    () => (examsData?.items ?? []).find((e) => e.id === selectedExamId) ?? null,
    [selectedExamId, examsData]
  );

  const filteredClassesForSchedule = useMemo(() => {
    if (!selectedExam) return classes;
    return classes.filter((c) => c.subject === selectedExam.subjectName);
  }, [classes, selectedExam]);

  const examSubjects = useMemo(() => examsData?.items ?? [], [examsData]);

  const handleCreateExam = async () => {
    try {
      const values = await createForm.validateFields();
      await createExam.mutateAsync({
        schoolId,
        subjectId: values.subjectId,
        termId: termId!,
        name: values.name,
        maxScore: values.maxScore,
      });
      message.success(t("Exam created"));
      setCreateOpen(false);
      createForm.resetFields();
    } catch {
      message.error(t("Failed to create exam"));
    }
  };

  const handleAddSchedule = async () => {
    try {
      const values = await scheduleForm.validateFields();
      await addSchedule.mutateAsync({
        schoolId,
        examId: selectedExamId!,
        classRoomId: values.classRoomId,
        roomId: values.roomId,
        examDate: values.examDate.format("YYYY-MM-DDT00:00:00"),
      });
      message.success(t("Exam schedule added"));
      setScheduleOpen(false);
      scheduleForm.resetFields();
    } catch {
      message.error(t("Failed to add schedule"));
    }
  };

  const columns: TableProps<ExamListItem>["columns"] = [
    {
      title: t("Exam Name"),
      dataIndex: "name",
      key: "name",
      render: (name: string) => <span className="font-medium text-[var(--color-text-primary)]">{name}</span>,
    },
    { title: t("Subject"), dataIndex: "subjectName", key: "subjectName" },
    {
      title: t("Max Score"),
      dataIndex: "maxScore",
      key: "maxScore",
      align: "center",
      render: (v: number) => <span className="font-medium">{v}</span>,
    },
    {
      title: t("Schedules"),
      dataIndex: "scheduleCount",
      key: "scheduleCount",
      align: "center",
      render: (v: number) => <Tag color={v > 0 ? "green" : "default"}>{v}</Tag>,
    },
    {
      title: t("Status"),
      dataIndex: "isLocked",
      key: "isLocked",
      render: (locked: boolean) =>
        locked ? (
          <Tag icon={<Lock className="h-3 w-3" />} color="default">{t("Locked")}</Tag>
        ) : (
          <Tag color="processing">{t("Open")}</Tag>
        ),
    },
    {
      title: "",
      key: "actions",
      width: 200,
      render: (_: unknown, record: ExamListItem) => (
        <div className="flex gap-1">
          {!record.isLocked && (
            <>
              <button
                onClick={() => {
                  setSelectedExamId(record.id);
                  setSelectedExamName(record.name);
                  setScheduleOpen(true);
                }}
                className="rounded-lg border border-[var(--color-border)] px-2.5 py-1 text-xs text-[var(--color-text-secondary)] hover:bg-[var(--color-surface)]"
              >
                {t("Add schedule")}
              </button>
              <button
                onClick={async () => {
                  try {
                    await lockExam.mutateAsync({ examId: record.id, schoolId });
                    message.success(t("Exam locked"));
                  } catch {
                    message.error(t("Failed to lock exam. Make sure results are recorded."));
                  }
                }}
                className="rounded-lg border border-red-200 text-red-600 px-2.5 py-1 text-xs hover:bg-red-50"
              >
                {t("Lock")}
              </button>
            </>
          )}
          {record.scheduleCount > 0 && (
            <ExamResultsButton examId={record.id} examName={record.name} schoolId={schoolId} />
          )}
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("Exams")}
      subtitle={t("Create and manage exams for your classes")}
      loading={dashLoading}
      actions={
        <Button
          type="primary"
          icon={<Plus className="h-4 w-4" />}
          onClick={() => setCreateOpen(true)}
          disabled={!termId}
        >
          {t("Create Exam")}
        </Button>
      }
    >
      <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface-card)] p-1">
        <Table<ExamListItem>
          columns={columns}
          dataSource={examsData?.items ?? []}
          rowKey="id"
          loading={examsLoading}
          pagination={{ pageSize: 10, showSizeChanger: false }}
        />
      </div>

      <Modal
        title={t("Create Exam")}
        open={createOpen}
        onOk={handleCreateExam}
        onCancel={() => setCreateOpen(false)}
        confirmLoading={createExam.isPending}
        okText={t("Create")}
      >
        <Form form={createForm} layout="vertical">
          <Form.Item name="name" label={t("Exam Name")} rules={[{ required: true, message: t("Required") }]}>
            <Input placeholder={t("e.g., Midterm Examination")} />
          </Form.Item>
          <Form.Item name="subjectId" label={t("Subject")} rules={[{ required: true, message: t("Required") }]}>
            <Select placeholder={t("Select subject")} showSearch optionFilterProp="label">
              {filteredSubjects.map((s) => (
                <Select.Option key={s.id} value={s.id} label={s.name}>
                  {s.name} ({s.code})
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="maxScore" label={t("Max Score")} rules={[{ required: true, message: t("Required") }]}>
            <InputNumber min={1} max={1000} className="w-full" placeholder="100" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={t("Add Schedule — {exam}", { exam: selectedExamName })}
        open={scheduleOpen}
        onOk={handleAddSchedule}
        onCancel={() => setScheduleOpen(false)}
        confirmLoading={addSchedule.isPending}
        okText={t("Add")}
      >
        <Form form={scheduleForm} layout="vertical">
          <Form.Item name="classRoomId" label={t("Classroom")} rules={[{ required: true, message: t("Required") }]}>
            <Select placeholder={t("Select classroom")}>
              {filteredClassesForSchedule.map((c) => (
                <Select.Option key={c.classRoomId} value={c.classRoomId}>
                  {c.name} ({c.subject})
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="roomId" label={t("Room")} rules={[{ required: true, message: t("Required") }]}>
            <Select placeholder={t("Select room")} showSearch optionFilterProp="label">
              {(rooms ?? []).map((r) => (
                <Select.Option key={r.id} value={r.id} label={r.name}>
                  {r.name} {t("(cap:")} {r.capacity})
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="examDate" label={t("Exam Date")} rules={[{ required: true, message: t("Required") }]}>
            <DatePicker className="w-full" />
          </Form.Item>
        </Form>
      </Modal>
    </DashboardTemplate>
  );
}

function ExamResultsButton({
  examId,
  examName,
  schoolId,
}: {
  examId: string;
  examName: string;
  schoolId: string;
}) {
  const [open, setOpen] = useState(false);
  const recordResults = useRecordExamResults();

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="rounded-lg bg-[var(--color-primary)] px-2.5 py-1 text-xs font-medium text-white hover:opacity-90"
      >
        {t("Results")}
      </button>
      <ExamResultsModal
        examId={examId}
        examName={examName}
        schoolId={schoolId}
        open={open}
        onClose={() => setOpen(false)}
        recordResults={recordResults}
      />
    </>
  );
}

function ExamResultsModal({
  examId,
  examName,
  schoolId,
  open,
  onClose,
  recordResults,
}: {
  examId: string;
  examName: string;
  schoolId: string;
  open: boolean;
  onClose: () => void;
  recordResults: ReturnType<typeof useRecordExamResults>;
}) {
  const { data: dashboard } = useTeacherDashboard();
  const classes = dashboard?.myClasses ?? [];
  const [selectedClassRoomId, setSelectedClassRoomId] = useState<string | null>(null);
  const [scores, setScores] = useState<Record<string, number>>({});
  const [resolvedScheduleId, setResolvedScheduleId] = useState<string | null>(null);
  const { data: rosterData } = useTeacherRoster(selectedClassRoomId, schoolId);
  const { data: examSchedules } = useExamSchedules(open ? examId : null);
  const { data: classResults } = useClassExamResults(schoolId, resolvedScheduleId);

  useEffect(() => {
    if (!open) {
      setSelectedClassRoomId(null);
      setScores({});
      setResolvedScheduleId(null);
    }
  }, [open]);

  useEffect(() => {
    if (!selectedClassRoomId || !examSchedules) {
      setResolvedScheduleId(null);
      return;
    }
    const match = examSchedules.find((s) => s.classRoomId === selectedClassRoomId);
    setResolvedScheduleId(match?.id ?? null);
  }, [selectedClassRoomId, examSchedules]);

  useEffect(() => {
    if (!classResults) return;
    const map: Record<string, number> = {};
    for (const r of classResults) {
      map[r.enrollmentId] = Math.round(r.score);
    }
    setScores(map);
  }, [classResults]);

  const handleSave = async () => {
    if (!resolvedScheduleId) {
      message.warning(t("No exam schedule found for this classroom. Add a schedule first."));
      return;
    }
    const results = (rosterData?.items ?? [])
      .filter((s) => scores[s.enrollmentId] !== undefined)
      .map((s) => ({
        studentEnrollmentId: s.enrollmentId,
        score: scores[s.enrollmentId],
      }));

    if (results.length === 0) {
      message.warning(t("No scores to save"));
      return;
    }

    try {
      await recordResults.mutateAsync({
        schoolId,
        examId,
        examScheduleId: resolvedScheduleId,
        results,
      });
      message.success(t("Results recorded"));
      onClose();
      setScores({});
    } catch {
      message.error(t("Failed to record results"));
    }
  };

  return (
    <Modal
      title={t("{exam} — Results", { exam: examName })}
      open={open}
      onOk={handleSave}
      onCancel={onClose}
      confirmLoading={recordResults.isPending}
      okText={t("Save Results")}
      width={700}
    >
      <div className="space-y-4">
        <div>
          <label className="mb-1 block text-sm font-medium">{t("Select classroom")}</label>
          <Select
            className="w-full"
            placeholder={t("Choose a class...")}
            value={selectedClassRoomId}
            onChange={(val) => {
              setSelectedClassRoomId(val);
              setScores({});
            }}
          >
            {classes.map((c) => (
              <Select.Option key={c.classRoomId} value={c.classRoomId}>
                {c.name}
              </Select.Option>
            ))}
          </Select>
          {selectedClassRoomId && !resolvedScheduleId && (
            <p className="mt-1 text-xs text-amber-600">
              {t("No exam schedule exists for this classroom. Go back and add one first.")}
            </p>
          )}
        </div>

        {selectedClassRoomId && rosterData?.items && (
          <div className="max-h-80 overflow-y-auto">
            <table className="w-full text-sm">
              <thead className="sticky top-0 bg-[var(--color-surface-card)]">
                <tr>
                  <th className="border-b p-2 text-start">{t("Student")}</th>
                  <th className="border-b p-2 text-end">{t("Score")}</th>
                </tr>
              </thead>
              <tbody>
                {rosterData.items.map((s) => (
                  <tr key={s.enrollmentId}>
                    <td className="border-b p-2">{s.firstName} {s.lastName}</td>
                    <td className="border-b p-2 text-end">
                      <InputNumber
                        size="small"
                        min={0}
                        max={100}
                        value={scores[s.enrollmentId]}
                        onChange={(v) => setScores((prev) => {
                          const next = { ...prev };
                          if (v === null || v === undefined) {
                            delete next[s.enrollmentId];
                          } else {
                            next[s.enrollmentId] = v;
                          }
                          return next;
                        })}
                        className="w-20"
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </Modal>
  );
}
