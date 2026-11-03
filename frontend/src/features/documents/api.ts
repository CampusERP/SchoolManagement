import api from "@/lib/axios";

export interface DocumentDto {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  createdAtUtc: string;
}

export const DocumentsApi = {
  delete: (schoolId: string, documentId: string) =>
    api.delete(`/documents/${documentId}`, { params: { schoolId } }).then((r) => r.data),
  download: (schoolId: string, documentId: string) =>
    api.get<Blob>(`/documents/${documentId}`, { params: { schoolId }, responseType: "blob" }).then((r) => r.data),
};

export async function openDocument(schoolId: string, documentId: string) {
  const tab = window.open("about:blank", "_blank");
  try {
    const blob = await DocumentsApi.download(schoolId, documentId);
    const url = URL.createObjectURL(blob);
    if (tab) tab.location.href = url;
    else window.location.href = url;
    window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
  } catch (error) {
    tab?.close();
    throw error;
  }
}
