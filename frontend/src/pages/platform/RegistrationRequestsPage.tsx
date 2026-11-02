import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Eye, Search, Filter, CheckCircle, XCircle, Trash2 } from "lucide-react";
import { Table, Select, Modal, Input as AntdInput } from "antd";
import type { ColumnsType } from "antd/es/table";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import Input from "@/components/atoms/Input";
import StatusBadge from "@/components/molecules/StatusBadge";
import dayjs from "dayjs";
import {
  useSchoolRegistrationRequests,
  useApproveSchoolRegistrationRequest,
  useRejectSchoolRegistrationRequest,
  useDeleteSchoolRegistrationRequest,
} from "@/features/schoolRegistrationRequests/hooks";
import type { SchoolRegistrationRequest } from "@/features/schoolRegistrationRequests/api";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

const STATUS_OPTIONS = [
  { value: "", label: t("All Statuses") },
  { value: "Pending", label: t("Pending") },
  { value: "Approved", label: t("Approved") },
  { value: "Rejected", label: t("Rejected") },
];

export default function RegistrationRequestsPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState<string>("");

  const { data, isLoading } = useSchoolRegistrationRequests({
    page,
    pageSize,
    search: search || undefined,
    status: status || undefined,
  });

  const approveMutation = useApproveSchoolRegistrationRequest();
  const rejectMutation = useRejectSchoolRegistrationRequest();
  const deleteMutation = useDeleteSchoolRegistrationRequest();

  const [approveModalOpen, setApproveModalOpen] = useState(false);
  const [rejectModalOpen, setRejectModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState<SchoolRegistrationRequest | null>(null);
  const [notes, setNotes] = useState("");
  const [reason, setReason] = useState("");

  const handleApprove = (request: SchoolRegistrationRequest) => {
    setSelectedRequest(request);
    setNotes("");
    setApproveModalOpen(true);
  };

  const confirmApprove = async () => {
    if (!selectedRequest) return;
    try {
      await approveMutation.mutateAsync({ id: selectedRequest.id, notes: notes || undefined });
      toast.success(t("Registration request approved. School and admin account created."));
      setApproveModalOpen(false);
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to approve request.");
      toast.error(translateApiMessage(message));
    }
  };

  const handleReject = (request: SchoolRegistrationRequest) => {
    setSelectedRequest(request);
    setReason("");
    setRejectModalOpen(true);
  };

  const confirmReject = async () => {
    if (!selectedRequest) return;
    try {
      await rejectMutation.mutateAsync({ id: selectedRequest.id, reason: reason || undefined });
      toast.success(t("Registration request rejected."));
      setRejectModalOpen(false);
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to reject request.");
      toast.error(translateApiMessage(message));
    }
  };

  const handleDelete = (request: SchoolRegistrationRequest) => {
    setSelectedRequest(request);
    setDeleteModalOpen(true);
  };

  const confirmDelete = async () => {
    if (!selectedRequest) return;
    try {
      await deleteMutation.mutateAsync(selectedRequest.id);
      toast.success(t("Registration request deleted."));
      setDeleteModalOpen(false);
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to delete request.");
      toast.error(translateApiMessage(message));
    }
  };

  const columns: ColumnsType<SchoolRegistrationRequest> = [
    {
      title: t("School"),
      dataIndex: "schoolName",
      key: "school",
      render: (name, record) => (
        <div>
          <div className="font-medium text-[var(--color-text-primary)]">{name}</div>
          <div className="text-sm text-[var(--color-text-secondary)]">
            {record.subdomainCode}.app
          </div>
        </div>
      ),
    },
    {
      title: t("Admin"),
      key: "admin",
      render: (_, record) => (
        <div>
          <div className="font-medium text-[var(--color-text-primary)]">
            {record.adminFirstName} {record.adminLastName}
          </div>
          <div className="text-sm text-[var(--color-text-secondary)]">
            {record.adminEmail}
          </div>
        </div>
      ),
    },
    {
      title: t("Status"),
      dataIndex: "status",
      key: "status",
      render: (status: string) => <StatusBadge status={status} />,
    },
    {
      title: t("School Email"),
      dataIndex: "schoolEmail",
      key: "schoolEmail",
      render: (email: string | undefined) => email || "-",
    },
    {
      title: t("Submitted"),
      dataIndex: "createdAtUtc",
      key: "createdAtUtc",
      render: (date: string) => dayjs(date).format("MMM DD, YYYY hh:mm A"),
    },
    {
      title: t("Reviewed"),
      key: "reviewedAtUtc",
      render: (_, record) =>
        record.reviewedAtUtc ? dayjs(record.reviewedAtUtc).format("MMM DD, YYYY hh:mm A") : "-",
    },
    {
      title: t("Actions"),
      key: "actions",
      render: (_, record) => (
        <div className="flex gap-2">
          <Button
            variant="ghost"
            size="small"
            onClick={() => navigate(`/platform/registration-requests/${record.id}`)}
            className="p-2"
          >
            <Eye className="h-4 w-4" />
          </Button>
          {record.status === "Pending" && (
            <>
              <Button
                variant="ghost"
                size="small"
                onClick={() => handleApprove(record)}
                className="p-2 text-emerald-600 hover:bg-[var(--color-success-soft)] hover:text-emerald-700"
                loading={approveMutation.isPending && selectedRequest?.id === record.id}
              >
                <CheckCircle className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="small"
                onClick={() => handleReject(record)}
                className="p-2 text-amber-600 hover:bg-[var(--color-warning-soft)] hover:text-amber-700"
                loading={rejectMutation.isPending && selectedRequest?.id === record.id}
              >
                <XCircle className="h-4 w-4" />
              </Button>
            </>
          )}
          <Button
            variant="ghost"
            size="small"
            onClick={() => handleDelete(record)}
            className="p-2 text-red-600 hover:bg-[var(--color-danger-soft)] hover:text-red-700"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DashboardTemplate
      title={t("School Registration Requests")}
      subtitle={t("Review and approve new school onboarding requests")}
    >
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex w-full flex-1 flex-col gap-3 sm:flex-row sm:items-center">
          <Input
            placeholder={t("Search by school, admin, or email...")}
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            prefix={<Search className="h-4 w-4 text-[var(--color-text-secondary)]" />}
            className="w-full sm:max-w-md"
          />
          <div className="flex items-center gap-2">
            <Filter className="h-4 w-4 shrink-0 text-[var(--color-text-secondary)]" />
            <Select
              value={status}
              onChange={(value) => {
                setStatus(value);
                setPage(1);
              }}
              options={STATUS_OPTIONS}
              className="w-full sm:w-40"
            />
          </div>
        </div>
      </div>

      <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] shadow-sm">
        <Table
          columns={columns}
          dataSource={data?.items || []}
          rowKey="id"
          loading={isLoading}
          scroll={{ x: "max-content" }}
          pagination={{
            current: data?.page || page,
            pageSize: data?.pageSize || pageSize,
            total: data?.totalCount || 0,
            showSizeChanger: false,
            onChange: (p) => setPage(p),
          }}
        />
      </div>

      <Modal
        title={t("Approve Registration Request")}
        open={approveModalOpen}
        onOk={confirmApprove}
        onCancel={() => setApproveModalOpen(false)}
        confirmLoading={approveMutation.isPending}
        okText={t("Approve & Create School")}
        okButtonProps={{ className: "bg-emerald-600 hover:bg-emerald-700" }}
      >
        <p className="mb-4 text-sm text-[var(--color-text-secondary)]">
          {t("Approving will create the school")} <strong>{selectedRequest?.schoolName}</strong> {t("with subdomain")} <strong>{selectedRequest?.subdomainCode}</strong> {t("and create the school admin account")} <strong>{selectedRequest?.adminEmail}</strong>.
        </p>
        <div className="mb-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            {t("Notes (optional)")}
          </label>
          <AntdInput.TextArea
            value={notes}
            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNotes(e.target.value)}
            placeholder={t("Add any approval notes...")}
            rows={3}
          />
        </div>
      </Modal>

      <Modal
        title={t("Reject Registration Request")}
        open={rejectModalOpen}
        onOk={confirmReject}
        onCancel={() => setRejectModalOpen(false)}
        confirmLoading={rejectMutation.isPending}
        okText={t("Reject Request")}
        okButtonProps={{ className: "bg-amber-600 hover:bg-amber-700" }}
      >
        <p className="mb-4 text-sm text-[var(--color-text-secondary)]">
          {t("Rejecting the request for")} <strong>{selectedRequest?.schoolName}</strong>.
        </p>
        <div className="mb-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            {t("Reason (optional)")}
          </label>
          <AntdInput.TextArea
            value={reason}
            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setReason(e.target.value)}
            placeholder={t("Add rejection reason...")}
            rows={3}
          />
        </div>
      </Modal>

      <Modal
        title={t("Delete Registration Request")}
        open={deleteModalOpen}
        onOk={confirmDelete}
        onCancel={() => setDeleteModalOpen(false)}
        confirmLoading={deleteMutation.isPending}
        okText={t("Delete")}
        okButtonProps={{ className: "bg-red-600 hover:bg-red-700" }}
      >
        <p className="text-sm text-[var(--color-text-secondary)]">
          {t("Are you sure you want to delete the registration request for")}{" "}
          <strong>{selectedRequest?.schoolName}</strong>{t("? This action cannot be undone.")}
        </p>
      </Modal>
    </DashboardTemplate>
  );
}
