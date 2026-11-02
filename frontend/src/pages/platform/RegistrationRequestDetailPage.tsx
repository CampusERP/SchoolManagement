import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, CheckCircle, XCircle, Trash2, Building2, User, Mail, Phone, MapPin, Hash } from "lucide-react";
import { Modal, Input as AntdInput, Spin } from "antd";
import { toast } from "sonner";
import DashboardTemplate from "@/components/templates/DashboardTemplate";
import Button from "@/components/atoms/Button";
import { Input, InputPassword } from "@/components/atoms/Input";
import StatusBadge from "@/components/molecules/StatusBadge";
import dayjs from "dayjs";
import {
  useSchoolRegistrationRequest,
  useApproveSchoolRegistrationRequest,
  useRejectSchoolRegistrationRequest,
  useDeleteSchoolRegistrationRequest,
  useUpdateSchoolRegistrationRequest,
} from "@/features/schoolRegistrationRequests/hooks";
import { t } from "@/i18n";
import { translateApiMessage } from "@/lib/errors";

export default function RegistrationRequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: request, isLoading } = useSchoolRegistrationRequest(id);

  const approveMutation = useApproveSchoolRegistrationRequest();
  const rejectMutation = useRejectSchoolRegistrationRequest();
  const deleteMutation = useDeleteSchoolRegistrationRequest();
  const updateMutation = useUpdateSchoolRegistrationRequest();

  const [approveModalOpen, setApproveModalOpen] = useState(false);
  const [rejectModalOpen, setRejectModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [notes, setNotes] = useState("");
  const [reason, setReason] = useState("");

  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({
    schoolName: request?.schoolName || "",
    subdomainCode: request?.subdomainCode || "",
    address: request?.address || "",
    phone: request?.phone || "",
    schoolEmail: request?.schoolEmail || "",
    adminFirstName: request?.adminFirstName || "",
    adminLastName: request?.adminLastName || "",
    adminEmail: request?.adminEmail || "",
    adminPassword: "",
    reviewNotes: request?.reviewNotes || "",
  });

  const enterEditMode = () => {
    if (!request) return;
    setEditForm({
      schoolName: request.schoolName,
      subdomainCode: request.subdomainCode,
      address: request.address || "",
      phone: request.phone || "",
      schoolEmail: request.schoolEmail || "",
      adminFirstName: request.adminFirstName,
      adminLastName: request.adminLastName,
      adminEmail: request.adminEmail,
      adminPassword: "",
      reviewNotes: request.reviewNotes || "",
    });
    setIsEditing(true);
  };

  const handleApprove = () => {
    setNotes("");
    setApproveModalOpen(true);
  };

  const confirmApprove = async () => {
    if (!id) return;
    try {
      await approveMutation.mutateAsync({ id, notes: notes || undefined });
      toast.success(t("Registration request approved. School and admin account created."));
      setApproveModalOpen(false);
      navigate("/platform/registration-requests");
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to approve request.");
      toast.error(translateApiMessage(message));
    }
  };

  const handleReject = () => {
    setReason("");
    setRejectModalOpen(true);
  };

  const confirmReject = async () => {
    if (!id) return;
    try {
      await rejectMutation.mutateAsync({ id, reason: reason || undefined });
      toast.success(t("Registration request rejected."));
      setRejectModalOpen(false);
      navigate("/platform/registration-requests");
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to reject request.");
      toast.error(translateApiMessage(message));
    }
  };

  const handleDelete = () => {
    setDeleteModalOpen(true);
  };

  const confirmDelete = async () => {
    if (!id) return;
    try {
      await deleteMutation.mutateAsync(id);
      toast.success(t("Registration request deleted."));
      setDeleteModalOpen(false);
      navigate("/platform/registration-requests");
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to delete request.");
      toast.error(translateApiMessage(message));
    }
  };

  const handleEditSubmit = async () => {
    if (!id || !request) return;
    try {
      await updateMutation.mutateAsync({
        id,
        data: {
          schoolName: editForm.schoolName,
          subdomainCode: editForm.subdomainCode,
          address: editForm.address || undefined,
          phone: editForm.phone || undefined,
          schoolEmail: editForm.schoolEmail || undefined,
          adminFirstName: editForm.adminFirstName,
          adminLastName: editForm.adminLastName,
          adminEmail: editForm.adminEmail,
          adminPassword: editForm.adminPassword || undefined,
          reviewNotes: editForm.reviewNotes || undefined,
        },
      });
      toast.success(t("Registration request updated."));
      setIsEditing(false);
    } catch (err: any) {
      const message = err?.response?.data?.error || err?.message || t("Failed to update request.");
      toast.error(translateApiMessage(message));
    }
  };

  if (isLoading || !request) {
    return (
      <DashboardTemplate title={t("Registration Request Details")}>
        <div className="flex justify-center py-12">
          <Spin size="large" />
        </div>
      </DashboardTemplate>
    );
  }

  return (
    <DashboardTemplate
      title={t("Registration Request Details")}
      subtitle={t("Review details for {school}", { school: request.schoolName })}
    >
      <div className="mb-6">
        <button
          onClick={() => navigate("/platform/registration-requests")}
          className="group mb-4 inline-flex items-center gap-2 text-sm font-medium text-[var(--color-text-secondary)] transition-colors hover:text-indigo-600"
        >
          <ArrowLeft className="h-4 w-4 transition-transform group-hover:-translate-x-1" />
          {t("Back to Requests")}
        </button>

        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">
              {request.schoolName}
            </h1>
            <StatusBadge status={request.status} />
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="ghost" onClick={() => navigate("/platform/registration-requests")}>
              {t("Cancel")}
            </Button>
            {request.status === "Pending" && (
              <>
                {!isEditing && (
                  <Button variant="secondary" onClick={enterEditMode}>
                    {t("Edit")}
                  </Button>
                )}
                {isEditing && (
                  <>
                    <Button variant="ghost" onClick={() => setIsEditing(false)}>
                      {t("Discard")}
                    </Button>
                    <Button
                      variant="primary"
                      onClick={handleEditSubmit}
                      loading={updateMutation.isPending}
                    >
                      {t("Save Changes")}
                    </Button>
                  </>
                )}
                <Button
                  variant="primary"
                  onClick={handleApprove}
                  className="bg-emerald-600 hover:bg-emerald-700"
                  loading={approveMutation.isPending}
                >
                  <CheckCircle className="mr-2 h-4 w-4" />
                  {t("Approve")}
                </Button>
                <Button
                  variant="primary"
                  onClick={handleReject}
                  className="bg-amber-600 hover:bg-amber-700"
                  loading={rejectMutation.isPending}
                >
                  <XCircle className="mr-2 h-4 w-4" />
                  {t("Reject")}
                </Button>
              </>
            )}
            <Button
              variant="primary"
              onClick={handleDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              {t("Delete")}
            </Button>
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* School Information */}
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm sm:p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-[var(--color-text-primary)]">
            <Building2 className="h-5 w-5 text-indigo-500" />
            {t("School Information")}
          </h2>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("School Name")}
              </label>
              {isEditing ? (
                <Input
                  value={editForm.schoolName}
                  onChange={(e) => setEditForm({ ...editForm, schoolName: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.schoolName}</p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Subdomain Code")}
              </label>
              {isEditing ? (
                <Input
                  prefix={<Hash className="h-4 w-4 text-slate-400" />}
                  value={editForm.subdomainCode}
                  onChange={(e) => setEditForm({ ...editForm, subdomainCode: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.subdomainCode}</p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("School Email")}
              </label>
              {isEditing ? (
                <Input
                  prefix={<Mail className="h-4 w-4 text-slate-400" />}
                  value={editForm.schoolEmail}
                  onChange={(e) => setEditForm({ ...editForm, schoolEmail: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">
                  {request.schoolEmail || "-"}
                </p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Phone")}
              </label>
              {isEditing ? (
                <Input
                  prefix={<Phone className="h-4 w-4 text-slate-400" />}
                  value={editForm.phone}
                  onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.phone || "-"}</p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Address")}
              </label>
              {isEditing ? (
                <Input
                  prefix={<MapPin className="h-4 w-4 text-slate-400" />}
                  value={editForm.address}
                  onChange={(e) => setEditForm({ ...editForm, address: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.address || "-"}</p>
              )}
            </div>
          </div>
        </div>

        {/* Admin Information */}
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm sm:p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-[var(--color-text-primary)]">
            <User className="h-5 w-5 text-indigo-500" />
            {t("Administrator Information")}
          </h2>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("First Name")}
              </label>
              {isEditing ? (
                <Input
                  value={editForm.adminFirstName}
                  onChange={(e) => setEditForm({ ...editForm, adminFirstName: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.adminFirstName}</p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Last Name")}
              </label>
              {isEditing ? (
                <Input
                  value={editForm.adminLastName}
                  onChange={(e) => setEditForm({ ...editForm, adminLastName: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.adminLastName}</p>
              )}
            </div>
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Admin Email")}
              </label>
              {isEditing ? (
                <Input
                  prefix={<Mail className="h-4 w-4 text-slate-400" />}
                  value={editForm.adminEmail}
                  onChange={(e) => setEditForm({ ...editForm, adminEmail: e.target.value })}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-[var(--color-text-primary)]">{request.adminEmail}</p>
              )}
            </div>
            {isEditing && request.status === "Pending" && (
              <div>
                <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                  {t("New Admin Password (optional)")}
                </label>
                <InputPassword
                  value={editForm.adminPassword}
                  onChange={(e) => setEditForm({ ...editForm, adminPassword: e.target.value })}
                  placeholder={t("Leave empty to keep current")}
                  className="mt-1"
                />
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="mt-6 rounded-xl border border-[var(--color-border)] bg-[var(--color-surface)] p-4 shadow-sm sm:p-6">
        <h2 className="mb-4 text-lg font-semibold text-[var(--color-text-primary)]">
          {t("Status & Review")}
        </h2>
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium text-[var(--color-text-secondary)]">
              {t("Submitted At")}
            </label>
            <p className="mt-1 text-[var(--color-text-primary)]">
              {dayjs(request.createdAtUtc).format("MMMM DD, YYYY hh:mm A")}
            </p>
          </div>
          {request.reviewedAtUtc && (
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Reviewed At")}
              </label>
              <p className="mt-1 text-[var(--color-text-primary)]">
                {dayjs(request.reviewedAtUtc).format("MMMM DD, YYYY hh:mm A")}
              </p>
            </div>
          )}
          {request.createdSchoolId && (
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Created School ID")}
              </label>
              <p className="mt-1 text-[var(--color-text-primary)] font-mono text-sm">
                {request.createdSchoolId}
              </p>
            </div>
          )}
          {request.createdAdminUserId && (
            <div>
              <label className="text-sm font-medium text-[var(--color-text-secondary)]">
                {t("Created Admin User ID")}
              </label>
              <p className="mt-1 text-[var(--color-text-primary)] font-mono text-sm">
                {request.createdAdminUserId}
              </p>
            </div>
          )}
          <div>
            <label className="text-sm font-medium text-[var(--color-text-secondary)]">
              {t("Review Notes")}
            </label>
            {isEditing ? (
              <AntdInput.TextArea
                value={editForm.reviewNotes}
                onChange={(e) => setEditForm({ ...editForm, reviewNotes: e.target.value })}
                rows={3}
                className="mt-1"
                placeholder={t("Add review notes...")}
              />
            ) : (
              <p className="mt-1 whitespace-pre-wrap text-[var(--color-text-primary)]">
                {request.reviewNotes || t("No notes")}
              </p>
            )}
          </div>
        </div>
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
          {t("Approving will create the school")} <strong>{request.schoolName}</strong> {t("with subdomain")}{" "}
          <strong>{request.subdomainCode}</strong> {t("and create the school admin account")}{" "}
          <strong>{request.adminEmail}</strong>.
        </p>
        <div className="mb-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            {t("Notes (optional)")}
          </label>
          <AntdInput.TextArea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
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
          {t("Rejecting the request for")} <strong>{request.schoolName}</strong>.
        </p>
        <div className="mb-4">
          <label className="mb-2 block text-sm font-medium text-[var(--color-text-primary)]">
            {t("Reason (optional)")}
          </label>
          <AntdInput.TextArea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
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
          <strong>{request.schoolName}</strong>{t("? This action cannot be undone.")}
        </p>
      </Modal>
    </DashboardTemplate>
  );
}
