using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Common.Services;

/// <summary>One payment row shown on a generated invoice PDF.</summary>
public sealed record InvoicePdfPaymentLine(
    DateTime PaidAtUtc,
    decimal Amount,
    string Method,
    string? Reference);

/// <summary>All data required to render an invoice PDF.</summary>
public sealed record InvoicePdfModel(
    string SchoolName,
    string PlanName,
    string InvoiceNumber,
    decimal Amount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime DueDate,
    DateTime IssuedAt,
    string Status,
    string? Notes,
    IReadOnlyList<InvoicePdfPaymentLine>? Payments = null);

/// <summary>
/// Generates a professional A4 invoice PDF with QuestPDF. The document is built
/// from data already stored in the platform database; nothing is read from or
/// written to disk, so the API can stream it straight to the browser.
/// </summary>
public static class InvoicePdfGenerator
{
    private static readonly CultureInfo MoneyCulture = CultureInfo.GetCultureInfo("en-US");

    static InvoicePdfGenerator()
    {
        // This project uses QuestPDF under its Community licence. Change this if
        // the deployment does not meet the Community licence eligibility rules.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Backwards-compatible overload used when an invoice is generated during
    /// school registration (no payment history exists yet).
    /// </summary>
    public static byte[] Create(
        string school,
        string plan,
        string invoiceNumber,
        decimal amount,
        DateTime dueDate,
        decimal paidAmount = 0,
        decimal? balanceDue = null,
        string? status = null,
        string? notes = null,
        DateTime? issuedAt = null) =>
        Create(new InvoicePdfModel(
            school,
            plan,
            invoiceNumber,
            amount,
            paidAmount,
            balanceDue ?? amount - paidAmount,
            dueDate,
            issuedAt ?? DateTime.UtcNow,
            status ?? "Issued",
            notes));

    public static byte[] Create(InvoicePdfModel model) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(text => text
                    .FontSize(10)
                    .FontColor("#334155"));

                page.Header().Element(header => ComposeHeader(header, model));
                page.Content().PaddingVertical(24).Column(column =>
                {
                    ComposeParties(column, model);
                    ComposeCharges(column, model);
                    ComposeTotals(column, model);
                    ComposePayments(column, model);
                    ComposeNotes(column, model);
                });
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();

    private static void ComposeHeader(IContainer container, InvoicePdfModel model)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(brand =>
                {
                    brand.Item().Text("CAMPUS ERP").FontSize(20).Bold().FontColor("#2563eb");
                    brand.Item().PaddingTop(2).Text("School management platform")
                        .FontSize(9).FontColor("#64748b");
                });

                row.ConstantItem(120).AlignRight().Column(right =>
                {
                    right.Item().Text("INVOICE").FontSize(26).Bold().FontColor("#0f172a");
                    right.Item().PaddingTop(4).Element(status => StatusBadge(status, model.Status));
                });
            });

            column.Item().PaddingTop(18).LineHorizontal(2).LineColor("#2563eb");
        });
    }

    private static void ComposeParties(ColumnDescriptor column, InvoicePdfModel model)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Column(billTo =>
            {
                billTo.Item().Text("BILLED TO").FontSize(8).Bold().FontColor("#64748b");
                billTo.Item().PaddingTop(4).Text(model.SchoolName).FontSize(14).SemiBold()
                    .FontColor("#0f172a");
                billTo.Item().PaddingTop(2).Text($"Plan: {model.PlanName}")
                    .FontColor("#475569");
            });

            row.ConstantItem(270).Column(meta =>
            {
                AddMeta(meta, "Invoice number", model.InvoiceNumber);
                AddMeta(meta, "Issued", model.IssuedAt.ToLocalTime().ToString("MMM d, yyyy"));
                AddMeta(meta, "Due date", model.DueDate.ToLocalTime().ToString("MMM d, yyyy"));
            });
        });
    }

    private static void ComposeCharges(ColumnDescriptor column, InvoicePdfModel model)
    {
        column.Item().PaddingTop(28).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeader).Text("DESCRIPTION").FontSize(8).Bold()
                    .FontColor("#475569");
                header.Cell().Element(TableHeader).AlignRight().Text("AMOUNT").FontSize(8).Bold()
                    .FontColor("#475569");
            });

            table.Cell().Element(TableCell).Text($"{model.PlanName} subscription");
            table.Cell().Element(TableCell).AlignRight().Text(Money(model.Amount));
        });
    }

    private static void ComposeTotals(ColumnDescriptor column, InvoicePdfModel model)
    {
        column.Item().PaddingTop(14).AlignRight().Width(240).Column(totals =>
        {
            TotalRow(totals, "Invoice total", model.Amount);
            TotalRow(totals, "Paid", model.PaidAmount);
            TotalRow(totals, "Balance due", model.BalanceDue, emphasized: true);
        });
    }

    private static void ComposePayments(ColumnDescriptor column, InvoicePdfModel model)
    {
        if (model.Payments is not { Count: > 0 })
            return;

        column.Item().PaddingTop(28).Text("PAYMENTS").FontSize(8).Bold().FontColor("#475569");

        column.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeader).Text("DATE").FontSize(8).Bold()
                    .FontColor("#475569");
                header.Cell().Element(TableHeader).Text("METHOD").FontSize(8).Bold()
                    .FontColor("#475569");
                header.Cell().Element(TableHeader).Text("REFERENCE").FontSize(8).Bold()
                    .FontColor("#475569");
                header.Cell().Element(TableHeader).AlignRight().Text("AMOUNT").FontSize(8).Bold()
                    .FontColor("#475569");
            });

            foreach (var payment in model.Payments)
            {
                table.Cell().Element(TableCell)
                    .Text(payment.PaidAtUtc.ToLocalTime().ToString("MMM d, yyyy"));
                table.Cell().Element(TableCell).Text(payment.Method);
                table.Cell().Element(TableCell).Text(string.IsNullOrWhiteSpace(payment.Reference)
                    ? "—"
                    : payment.Reference);
                table.Cell().Element(TableCell).AlignRight().Text(Money(payment.Amount));
            }
        });
    }

    private static void ComposeNotes(ColumnDescriptor column, InvoicePdfModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Notes))
            return;

        column.Item().PaddingTop(28).Column(notes =>
        {
            notes.Item().Text("NOTES").FontSize(8).Bold().FontColor("#475569");
            notes.Item().PaddingTop(5).Text(model.Notes).FontColor("#475569");
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor("#e2e8f0").PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Text("Thank you for your business.")
                .FontSize(9).FontColor("#64748b");

            row.ConstantItem(130).AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(9).FontColor("#64748b"));
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    private static void AddMeta(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(80).Text(label).FontSize(9).FontColor("#64748b");
            row.RelativeItem().AlignRight().Text(value).FontSize(8.5f).SemiBold().FontColor("#0f172a");
        });
    }

    private static void TotalRow(
        ColumnDescriptor column,
        string label,
        decimal amount,
        bool emphasized = false)
    {
        column.Item().PaddingVertical(3).Row(row =>
        {
            var labelText = row.RelativeItem().Text(label);
            var amountText = row.ConstantItem(100).AlignRight().Text(Money(amount));

            if (emphasized)
            {
                labelText.SemiBold().FontColor("#0f172a");
                amountText.SemiBold().FontColor("#0f172a");
            }
        });
    }

    private static void StatusBadge(IContainer container, string status)
    {
        var background = status switch
        {
            "Paid" => "#16a34a",
            "Overdue" => "#dc2626",
            "Cancelled" => "#64748b",
            "Draft" => "#94a3b8",
            "Issued" => "#2563eb",
            _ => "#2563eb",
        };

        container.Background(background).PaddingHorizontal(9).PaddingVertical(4)
            .Text(status.ToUpperInvariant()).FontSize(8).Bold().FontColor(Colors.White);
    }

    private static IContainer TableHeader(IContainer container) =>
        container.Background("#f1f5f9").PaddingHorizontal(8).PaddingVertical(7);

    private static IContainer TableCell(IContainer container) =>
        container.BorderBottom(1).BorderColor("#e2e8f0").PaddingHorizontal(8).PaddingVertical(8);

    private static string Money(decimal value) =>
        value.ToString("C", MoneyCulture);
}
