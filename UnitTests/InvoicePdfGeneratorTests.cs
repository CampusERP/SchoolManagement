using System.Text;
using Application.Common.Services;

namespace UnitTests;

public class InvoicePdfGeneratorTests
{
    [Fact]
    public void Create_GeneratesAValidPdfDocument()
    {
        var model = new InvoicePdfModel(
            "Demo School",
            "Standard",
            "528ffa9d-ab7b-4ef5-92c6-eaaa263e27ea",
            49m,
            49m,
            0m,
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow,
            "Paid",
            "Paid in full.",
            new[]
            {
                new InvoicePdfPaymentLine(
                    DateTime.UtcNow,
                    49m,
                    "BankTransfer",
                    "TX-1001"),
            });

        var pdf = InvoicePdfGenerator.Create(model);

        Assert.True(pdf.Length > 1_000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
