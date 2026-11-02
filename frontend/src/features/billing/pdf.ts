import { saveAs } from "file-saver";
import { BillingApi } from "./api";

const fileName = (invoiceId: string) => `invoice-${invoiceId}.pdf`;

/**
 * Opens the generated invoice PDF in a new tab. The tab is opened before the
 * request so browsers do not treat it as a pop-up created outside a click.
 */
export async function openInvoicePdf(invoiceId: string) {
  const preview = window.open("about:blank", "_blank");

  try {
    const pdf = await BillingApi.getInvoicePdf(invoiceId);
    const pdfUrl = URL.createObjectURL(pdf);

    if (preview) {
      preview.location.href = pdfUrl;
    } else {
      saveAs(pdf, fileName(invoiceId));
    }

    window.setTimeout(() => URL.revokeObjectURL(pdfUrl), 60_000);
  } catch (error) {
    preview?.close();
    throw error;
  }
}

/** Downloads the generated invoice PDF instead of opening it in a tab. */
export async function downloadInvoicePdf(invoiceId: string) {
  const pdf = await BillingApi.getInvoicePdf(invoiceId);
  saveAs(pdf, fileName(invoiceId));
}
