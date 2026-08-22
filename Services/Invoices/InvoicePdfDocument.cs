using KejaHUnt_PropertiesAPI.Models.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KejaHUnt_PropertiesAPI.Services.Invoices
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly InvoiceDto _invoice;
        private readonly byte[]? _logoBytes;

        public InvoicePdfDocument(InvoiceDto invoice, byte[]? logoBytes)
        {
            _invoice = invoice;
            _logoBytes = logoBytes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    // --- Header: logo + status badge ---
                    column.Item().Row(row =>
                    {
                        if (_logoBytes != null)
                        {
                            row.ConstantItem(140).Height(40).Image(_logoBytes).FitArea();
                        }
                        else
                        {
                            row.ConstantItem(140).Text("kejaHUnT").FontSize(18).Bold().FontColor("#0c447c");
                        }

                        row.RelativeItem(); // spacer

                        var statusText = _invoice.Status.ToString()?.ToLower() ?? "pending";
                        if (statusText is "paid" or "partial" or "overpaid")
                        {
                            var (bg, fg) = statusText switch
                            {
                                "partial" => ("#fde68a", "#78350f"),
                                _ => ("#a7f3d0", "#064e3b") // paid / overpaid
                            };

                            row.ConstantItem(80).AlignRight().Background(bg).Padding(6)
                                .Text(statusText.ToUpper())
                                .FontSize(9).Bold().FontColor(fg);
                        }
                    });

                    // --- Invoice info box ---
                    column.Item().Background("#f2f2f2").Padding(14).Column(box =>
                    {
                        box.Item().Text($"Invoice #{_invoice.InvoiceNumber}").FontSize(15).Bold().FontColor(Colors.Black);
                        box.Item().PaddingTop(6).Text(text =>
                        {
                            text.Span("Invoice Date: ").FontColor(Colors.Grey.Darken1);
                            text.Span(_invoice.CreatedAt.ToString("d MMM yyyy")).FontColor(Colors.Black);
                        });
                        box.Item().Text(text =>
                        {
                            text.Span("Due Date: ").FontColor(Colors.Grey.Darken1);
                            text.Span(_invoice.DueDate.ToString("d MMM yyyy")).FontColor(Colors.Black);
                        });
                    });

                    // --- Billed to / Property ---
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("BILLED TO").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(_invoice.TenantName ?? "—").FontSize(11).Bold().FontColor(Colors.Black);
                            c.Item().Text(_invoice.DoorNumber ?? "—").FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text("PROPERTY").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().AlignRight().Text(_invoice.PropertyName ?? "—").FontSize(11).Bold().FontColor(Colors.Black);
                            c.Item().AlignRight().Text(_invoice.PropertyLocation ?? "").FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // --- Item table ---
                    column.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingBottom(4).Text("DESCRIPTION").FontSize(8).FontColor(Colors.Grey.Medium);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingBottom(4).AlignRight().Text("AMOUNT").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        table.Cell().PaddingVertical(5).Text("Rent").FontColor(Colors.Black);
                        table.Cell().PaddingVertical(5).AlignRight().Text($"KSh {_invoice.RentAmount:N2}").FontColor(Colors.Black);

                        table.Cell().PaddingVertical(5).Text("Water").FontColor(Colors.Black);
                        table.Cell().PaddingVertical(5).AlignRight().Text($"KSh {_invoice.WaterBillAmount:N2}").FontColor(Colors.Black);

                        table.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(6)
                            .Text("Total due").Bold().FontSize(11).FontColor(Colors.Black);
                        table.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(6).AlignRight()
                            .Text($"KSh {_invoice.TotalAmount:N2}").Bold().FontSize(11).FontColor(Colors.Black);
                    });

                    // --- Transactions + balance (only when present) ---
                    if (_invoice.Transactions.Count > 0)
                    {
                        column.Item().Column(t =>
                        {
                            t.Item().PaddingBottom(4).Text("TRANSACTIONS").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);

                            t.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingBottom(3).Text("DATE").FontSize(8).FontColor(Colors.Grey.Medium);
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingBottom(3).Text("REFERENCE").FontSize(8).FontColor(Colors.Grey.Medium);
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingBottom(3).AlignRight().Text("AMOUNT").FontSize(8).FontColor(Colors.Grey.Medium);
                                });

                                foreach (var tx in _invoice.Transactions)
                                {
                                    table.Cell().PaddingVertical(4).Text(tx.CreatedAt.ToString("d MMM yyyy")).FontColor(Colors.Black);
                                    table.Cell().PaddingVertical(4).Text(tx.MpesaCode ?? tx.Reference ?? "—").FontColor(Colors.Grey.Darken2);
                                    table.Cell().PaddingVertical(4).AlignRight().Text($"KSh {tx.Amount:N2}").FontColor(Colors.Black);
                                }
                            });

                            var balance = _invoice.TotalAmount - _invoice.Transactions.Sum(x => x.Amount);
                            t.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Text("Balance").Bold().FontColor(Colors.Black);
                                row.ConstantItem(100).AlignRight().Text($"KSh {balance:N2}").Bold().FontColor(Colors.Black);
                            });
                        });
                    }

                    // --- Footer ---
                    column.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(8).AlignCenter()
                        .Text("KEJAHUNT · NAIROBI, KENYA")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}