using Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FileGeneratorApi.Utils;

public class DocumentGenerator(FileCreateEvent invoice, GetCompanyResponse seller, GetCompanyResponse bayer) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Spacing(5);
                column
                    .Item().Text($"Faktura VAT nr: {invoice.Number}")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                column.Item().Text(text =>
                {
                    text.Span("Data wystawienia: ").SemiBold().FontSize(10);
                    text.Span($"{DateTime.Now:d-M-yyyy}").FontSize(10);
                });

                column.Item().Text(text =>
                {
                    text.Span("Sposób zapłaty: ").SemiBold().FontSize(10);
                    text.Span($"{invoice.PaymentType.ToString()}").FontSize(10);
                });

                column.Item().Text(text =>
                {
                    var term = invoice.Status is "Paid"
                        ? "zapłacone"
                        : DateTime.Now.AddDays(invoice.TermsOfPayment).ToString("dd-MM-yyyy");

                    text.Span("Termin płatności: ").SemiBold().FontSize(10);
                    text.Span(term).FontSize(10);
                });

                if (invoice.Status is "Unpaid")
                {
                    column.Item().Text(text =>
                    {
                        text.Span("Dni do zapłaty: ").SemiBold().FontSize(10);
                        text.Span($"{invoice.TermsOfPayment} dni").FontSize(10);
                    });
                }
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(30).Column(column =>
        {
            column.Spacing(20);
            column.Item().Row(row =>
            {
                row.RelativeItem().Component(new AddressComponent("Sprzedawca", seller));
                row.ConstantItem(50);
                row.RelativeItem().Component(new AddressComponent("Nabywca", bayer));
            });
            column.Item().Element(ComposeTable);
            column.Item().Element(FinalTable);

            var result = invoice.Status is "Paid"
                ? $"Zapłacono: {invoice.TotalGrossPrice} PLN"
                : $"Termin Płatności: {DateTime.Now.AddDays(invoice.TermsOfPayment):dd-MM-yyyy}";

            column.Item().AlignRight().PaddingRight(10).Text(result).SemiBold();
            
            column.Item().PaddingTop(70).Row(row =>
            {
                row.RelativeItem().Component(new SignatureComponent("Wystawił(a)"));
                row.ConstantItem(150);
                row.RelativeItem().Component(new SignatureComponent("Odebrał(a)"));
            });
        });
    }

    private void ComposeTable(IContainer container)
    {
        var headerStyle = TextStyle.Default.SemiBold().FontSize(10);
        var itemsStyle = TextStyle.Default.Italic().FontSize(10);

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Text("I.p").Style(headerStyle);
                header.Cell().Text("Nazwa towaru lub usługi").Style(headerStyle);
                header.Cell().AlignRight().Text("Ilość").Style(headerStyle);
                header.Cell().AlignRight().Text("J.m.").Style(headerStyle);
                header.Cell().AlignRight().Text("Cena netto").Style(headerStyle);
                header.Cell().AlignRight().Text("W. netto").Style(headerStyle);
                header.Cell().AlignRight().Text("St. VAT").Style(headerStyle);
                header.Cell().AlignRight().Text("W. brutto").Style(headerStyle);

                header.Cell().ColumnSpan(8).PaddingTop(8).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var item in invoice.Items)
            {
                var items = invoice.Items.ToList();
                var index = items.IndexOf(item) + 1;

                var vat = item.Vat switch
                {
                    "Vat0" => "-",
                    "Vat5" => "5%",  
                    "Vat7" => "7%",
                    "Vat8" => "8%",
                    _ => "23%"
                };

                var unit = item.Unit switch
                {
                    "Hours" => "godz.",
                    "Days" => "dni",
                    "Kg" => "kg",
                    "Km" => "km",
                    _ => "szt."
                };

                table.Cell().Element(CellStyle).Text($"{index}").Style(itemsStyle);
                table.Cell().Element(CellStyle).Text(item.Name).Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Amount}").Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text(unit).Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.NetPrice}").Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SumNetPrice}").Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text(vat).Style(itemsStyle);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SumGrossPrice}").Style(itemsStyle);

                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        });
    }

    private void FinalTable(IContainer container)
    {
        container.AlignRight().Width(250).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().AlignCenter().Text("Wartość netto").SemiBold().FontSize(10);
                header.Cell().AlignCenter().Text("Wartość brutto").SemiBold().FontSize(10);

                header.Cell().ColumnSpan(2).PaddingTop(2).BorderBottom(1).BorderColor(Colors.Black)
                    .Background(Colors.Grey.Lighten3);
            });

            table.Cell().Element(CellStyle).AlignCenter().Text($"{invoice.TotalNetPrice} PLN");
            table.Cell().Element(CellStyle).AlignCenter().Text($"{invoice.TotalGrossPrice} PLN");

            static IContainer CellStyle(IContainer container) =>
                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }

    private class AddressComponent(string title, GetCompanyResponse company) : IComponent
    {
        public void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
            {
                column.Spacing(2);

                column.Item().Text(title).SemiBold();
                column.Item().PaddingBottom(5).LineHorizontal(1);

                column.Item().Text($"{company.Name}");
                column.Item().Text(company.Street);
                column.Item().Text($"{company.PostalCode}, {company.City}");
                column.Item().Text($"NIP: {company.TaxNumber}");
            });
        }
    }
    
    private class SignatureComponent(string title) : IComponent
    {
        public void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
            {
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().AlignCenter().Text(title).Italic().FontSize(10);
            });
        }
    }
}