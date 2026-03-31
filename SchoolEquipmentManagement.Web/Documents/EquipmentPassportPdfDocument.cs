using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Documents;

public sealed class EquipmentPassportPdfDocument : IDocument
{
    private const string ProductTitle = "\u0423\u0447\u0435\u0442 IT-\u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u044F";
    private const string PassportTitle = "\u041F\u0430\u0441\u043F\u043E\u0440\u0442 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u044F";
    private const string InventoryNumberLabel = "\u0418\u043D\u0432\u0435\u043D\u0442\u0430\u0440\u043D\u044B\u0439 \u043D\u043E\u043C\u0435\u0440";
    private const string GeneratedLabel = "\u0421\u0444\u043E\u0440\u043C\u0438\u0440\u043E\u0432\u0430\u043D\u043E";
    private const string PageLabel = "\u0421\u0442\u0440\u0430\u043D\u0438\u0446\u0430";
    private const string PhotoLabel = "\u0424\u043E\u0442\u043E\u0433\u0440\u0430\u0444\u0438\u044F";
    private const string PhotoMissingLabel = "\u0424\u043E\u0442\u043E \u043D\u0435 \u0437\u0430\u0433\u0440\u0443\u0436\u0435\u043D\u043E";
    private const string QrLabel = "QR-\u043A\u043E\u0434 \u043E\u0431\u044A\u0435\u043A\u0442\u0430";
    private const string IdentitySectionTitle = "\u041F\u0430\u0441\u043F\u043E\u0440\u0442 \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u044F";
    private const string ServiceSectionTitle = "\u0421\u0435\u0440\u0432\u0438\u0441\u043D\u0430\u044F \u0438\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0438\u044F";
    private const string StatusSectionTitle = "\u0421\u043E\u0441\u0442\u043E\u044F\u043D\u0438\u0435 \u043E\u0431\u044A\u0435\u043A\u0442\u0430";
    private const string MovementSectionTitle = "\u0414\u0432\u0438\u0436\u0435\u043D\u0438\u0435 \u043E\u0431\u044A\u0435\u043A\u0442\u0430";
    private const string NotesSectionTitle = "\u041F\u0440\u0438\u043C\u0435\u0447\u0430\u043D\u0438\u0435";
    private const string ServiceSummaryLabel = "\u0421\u0435\u0440\u0432\u0438\u0441\u043D\u0430\u044F \u0441\u0432\u043E\u0434\u043A\u0430";
    private const string StatusLabel = "\u0421\u0442\u0430\u0442\u0443\u0441";
    private const string LifecycleLabel = "\u0416\u0438\u0437\u043D\u0435\u043D\u043D\u044B\u0439 \u0446\u0438\u043A\u043B";
    private const string WarrantyStatusLabel = "\u0413\u0430\u0440\u0430\u043D\u0442\u0438\u0439\u043D\u044B\u0439 \u0441\u0442\u0430\u0442\u0443\u0441";
    private const string LastChangedLabel = "\u041F\u043E\u0441\u043B\u0435\u0434\u043D\u0435\u0435 \u0438\u0437\u043C\u0435\u043D\u0435\u043D\u0438\u0435";
    private const string LocationLabel = "\u041C\u0435\u0441\u0442\u043E\u043F\u043E\u043B\u043E\u0436\u0435\u043D\u0438\u0435";
    private const string ResponsibleLabel = "\u041E\u0442\u0432\u0435\u0442\u0441\u0442\u0432\u0435\u043D\u043D\u043E\u0435 \u043B\u0438\u0446\u043E";
    private const string ManufacturerLabel = "\u041F\u0440\u043E\u0438\u0437\u0432\u043E\u0434\u0438\u0442\u0435\u043B\u044C";
    private const string ModelLabel = "\u041C\u043E\u0434\u0435\u043B\u044C";
    private const string SerialLabel = "\u0421\u0435\u0440\u0438\u0439\u043D\u044B\u0439 \u043D\u043E\u043C\u0435\u0440";
    private const string TypeLabel = "\u0422\u0438\u043F \u043E\u0431\u043E\u0440\u0443\u0434\u043E\u0432\u0430\u043D\u0438\u044F";
    private const string PurchaseDateLabel = "\u0414\u0430\u0442\u0430 \u043F\u043E\u043A\u0443\u043F\u043A\u0438";
    private const string CommissioningDateLabel = "\u0412\u0432\u043E\u0434 \u0432 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u044E";
    private const string WarrantyUntilLabel = "\u0413\u0430\u0440\u0430\u043D\u0442\u0438\u044F \u0434\u043E";
    private const string OwnershipLabel = "\u041E\u0442\u0432\u0435\u0442\u0441\u0442\u0432\u0435\u043D\u043D\u043E\u0441\u0442\u044C \u0438 \u0440\u0430\u0437\u043C\u0435\u0449\u0435\u043D\u0438\u0435";
    private const string LastInventoryLabel = "\u041F\u043E\u0441\u043B\u0435\u0434\u043D\u044F\u044F \u0438\u043D\u0432\u0435\u043D\u0442\u0430\u0440\u0438\u0437\u0430\u0446\u0438\u044F";
    private const string MovementEmptyLabel = "\u0421\u043E\u0431\u044B\u0442\u0438\u044F \u0434\u0432\u0438\u0436\u0435\u043D\u0438\u044F \u043F\u043E\u043A\u0430 \u043D\u0435 \u0437\u0430\u0444\u0438\u043A\u0441\u0438\u0440\u043E\u0432\u0430\u043D\u044B.";
    private const string ExecutorLabel = "\u0418\u0441\u043F\u043E\u043B\u043D\u0438\u0442\u0435\u043B\u044C";
    private const string NotSpecifiedLabel = "\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E";

    private readonly EquipmentDetailsViewModel _model;
    private readonly byte[]? _photoBytes;
    private readonly byte[] _qrBytes;

    public EquipmentPassportPdfDocument(EquipmentDetailsViewModel model, byte[]? photoBytes, byte[] qrBytes)
    {
        _model = model;
        _photoBytes = photoBytes;
        _qrBytes = qrBytes;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(18);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9.5f).FontColor(Colors.Grey.Darken4));

            page.Content().Column(column =>
            {
                column.Spacing(10);
                column.Item().Element(ComposeHeader);
                column.Item().Element(ComposeIdentitySection);
                column.Item().Row(row =>
                {
                    row.Spacing(10);
                    row.RelativeItem().Element(ComposeServiceSection);
                    row.RelativeItem().Element(ComposeStatusSection);
                });
                column.Item().Element(ComposeMovementSection);
                column.Item().Element(ComposeNotesSection);
            });

            page.Footer().AlignRight().Text(text =>
            {
                text.Span(PageLabel + " ");
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
                column.Spacing(4);
                column.Item().Text(ProductTitle)
                    .FontColor(Colors.Blue.Darken3)
                    .FontSize(8)
                    .SemiBold();
                column.Item().Text(PassportTitle)
                    .FontSize(21)
                    .Bold()
                    .FontColor(Colors.Grey.Darken4);
                column.Item().Text(DisplayText(_model.Name))
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2);
            });

            row.ConstantItem(170).Element(ComposeInventoryCard);
        });
    }

    private void ComposeInventoryCard(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .CornerRadius(10)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(3);
                column.Item().Text(InventoryNumberLabel)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column.Item().Text(DisplayText(_model.InventoryNumber))
                    .FontSize(14)
                    .Bold();
                column.Item().Text($"{GeneratedLabel} {DateTime.Now:dd.MM.yyyy HH:mm}")
                    .FontSize(7)
                    .FontColor(Colors.Grey.Darken1);
            });
    }

    private void ComposeIdentitySection(IContainer container)
    {
        ComposeSection(container, IdentitySectionTitle, section =>
        {
            section.Row(row =>
            {
                row.Spacing(10);

                row.ConstantItem(160).Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Element(ComposePhotoCard);
                    column.Item().Element(ComposeQrCard);
                });

                row.RelativeItem().Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Element(c => ComposeFieldRow(c, StatusLabel, DisplayText(_model.Status), LifecycleLabel, DisplayText(_model.LifecycleStage)));
                    column.Item().Element(c => ComposeFieldRow(c, WarrantyStatusLabel, DisplayText(_model.WarrantyStatus), LastChangedLabel, DisplayDate(_model.LastChangedAt)));
                    column.Item().Element(c => ComposeFieldRow(c, LocationLabel, DisplayText(_model.Location), ResponsibleLabel, DisplayText(_model.ResponsiblePerson)));
                    column.Item().Element(c => ComposeFieldRow(c, ManufacturerLabel, DisplayText(_model.Manufacturer), ModelLabel, DisplayText(_model.Model)));
                    column.Item().Element(c => ComposeFieldRow(c, SerialLabel, DisplayText(_model.SerialNumber), TypeLabel, DisplayText(_model.EquipmentType)));
                    column.Item().Element(c => ComposeSummaryBox(c, ServiceSummaryLabel, DisplayText(_model.ServiceSummary)));
                });
            });
        });
    }

    private void ComposePhotoCard(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.White)
            .CornerRadius(10)
            .Padding(8)
            .Column(column =>
            {
                column.Spacing(6);
                column.Item().Text(PhotoLabel)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column.Item().Height(135)
                    .AlignCenter()
                    .AlignMiddle()
                    .Element(c =>
                    {
                        if (_photoBytes is { Length: > 0 })
                            c.Image(_photoBytes).FitArea();
                        else
                            c.AlignCenter().AlignMiddle().Text(PhotoMissingLabel).FontColor(Colors.Grey.Darken1);
                    });
            });
    }

    private void ComposeQrCard(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.White)
            .CornerRadius(10)
            .Padding(8)
            .Column(column =>
            {
                column.Spacing(6);
                column.Item().Text(QrLabel)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column.Item()
                    .AlignCenter()
                    .Width(96)
                    .Height(96)
                    .Element(c => c.Image(_qrBytes).FitArea());
                column.Item().AlignCenter().Text(DisplayText(_model.InventoryNumber))
                    .FontSize(7)
                    .FontColor(Colors.Grey.Darken1);
            });
    }

    private void ComposeServiceSection(IContainer container)
    {
        ComposeSection(container, ServiceSectionTitle, section =>
        {
            section.Column(column =>
            {
                column.Spacing(8);
                column.Item().Element(c => ComposeFieldRow(c, PurchaseDateLabel, DisplayDate(_model.PurchaseDate), CommissioningDateLabel, DisplayDate(_model.CommissioningDate)));
                column.Item().Element(c => ComposeFieldRow(c, WarrantyUntilLabel, DisplayDate(_model.WarrantyEndDate), ManufacturerLabel, DisplayText(_model.Manufacturer)));
                column.Item().Element(c => ComposeFieldRow(c, ModelLabel, DisplayText(_model.Model), SerialLabel, DisplayText(_model.SerialNumber)));
            });
        });
    }

    private void ComposeStatusSection(IContainer container)
    {
        ComposeSection(container, StatusSectionTitle, section =>
        {
            section.Column(column =>
            {
                column.Spacing(8);
                column.Item().Element(c => ComposeSummaryBox(c, LifecycleLabel, DisplayText(_model.LifecycleSummary)));
                column.Item().Element(c => ComposeSummaryBox(c, WarrantyStatusLabel, DisplayText(_model.WarrantySummary)));
                column.Item().Element(c => ComposeSummaryBox(c, OwnershipLabel, DisplayText(_model.OwnershipSummary)));
                column.Item().Element(c => ComposeSummaryBox(c, LastInventoryLabel, DisplayText(_model.LastInventorySummary)));
            });
        });
    }

    private void ComposeMovementSection(IContainer container)
    {
        ComposeSection(container, MovementSectionTitle, section =>
        {
            if (!_model.Movements.Any())
            {
                section.Text(MovementEmptyLabel)
                    .FontColor(Colors.Grey.Darken1);
                return;
            }

            section.Column(column =>
            {
                column.Spacing(6);

                foreach (var movement in _model.Movements.Take(6))
                {
                    column.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Background(Colors.Grey.Lighten5)
                        .CornerRadius(10)
                        .Padding(9)
                        .Column(item =>
                        {
                            item.Spacing(4);
                            item.Item().Row(row =>
                            {
                                row.RelativeItem().Text(DisplayText(movement.EventName))
                                    .SemiBold()
                                    .FontColor(Colors.Grey.Darken4);
                                row.ConstantItem(96).AlignRight().Text(movement.OccurredAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"))
                                    .FontSize(7)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            item.Item().Text(DisplayText(movement.Summary))
                                .FontSize(8.5f)
                                .FontColor(Colors.Grey.Darken3);

                            if (!string.IsNullOrWhiteSpace(movement.Details))
                            {
                                item.Item().Text(movement.Details)
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            }

                            if (!string.IsNullOrWhiteSpace(movement.ChangedBy))
                            {
                                item.Item().Text($"{ExecutorLabel}: {movement.ChangedBy}")
                                    .FontSize(7)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });
                }
            });
        });
    }

    private void ComposeNotesSection(IContainer container)
    {
        ComposeSection(container, NotesSectionTitle, section =>
        {
            section.Text(DisplayText(_model.Notes))
                .FontColor(Colors.Grey.Darken2);
        });
    }

    private static void ComposeSection(IContainer container, string title, Action<IContainer> content)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.White)
            .CornerRadius(12)
            .Padding(12)
            .Column(column =>
            {
                column.Spacing(8);
                column.Item().Text(title)
                    .FontSize(11)
                    .Bold()
                    .FontColor(Colors.Grey.Darken4);
                column.Item().Element(content);
            });
    }

    private static void ComposeSummaryBox(IContainer container, string title, string body)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .CornerRadius(10)
            .Padding(8)
            .Column(column =>
            {
                column.Spacing(4);
                column.Item().Text(title)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column.Item().Text(body)
                    .FontSize(8.5f)
                    .FontColor(Colors.Grey.Darken3);
            });
    }

    private static void ComposeFieldRow(IContainer container, string leftLabel, string leftValue, string rightLabel, string rightValue)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(c => ComposeField(c, leftLabel, leftValue));
            row.RelativeItem().Element(c => ComposeField(c, rightLabel, rightValue));
        });
    }

    private static void ComposeField(IContainer container, string label, string value)
    {
        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().Text(label)
                .FontSize(7)
                .FontColor(Colors.Grey.Darken1);
            column.Item().Text(value)
                .FontSize(8.5f)
                .SemiBold()
                .FontColor(Colors.Grey.Darken4);
        });
    }

    private static string DisplayText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? NotSpecifiedLabel : value;

    private static string DisplayDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("dd.MM.yyyy") : NotSpecifiedLabel;
}
