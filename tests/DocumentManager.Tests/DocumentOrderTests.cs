using DocumentManager.Core.Models;
using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class DocumentOrderTests
{
    [Fact]
    public void Sort_AlwaysUsesBusinessOrder()
    {
        DocumentInput[] shuffled =
        [
            new(DocumentType.Quote, "quote.pdf"),
            new(DocumentType.MaintenanceReport, "report.pdf"),
            new(DocumentType.ServiceOrder, "service.pdf"),
            new(DocumentType.WorkOrder, "work.pdf"),
        ];

        var result = DocumentOrder.Sort(shuffled);

        Assert.Collection(
            result,
            item => Assert.Equal(DocumentType.ServiceOrder, item.Type),
            item => Assert.Equal(DocumentType.WorkOrder, item.Type),
            item => Assert.Equal(DocumentType.Quote, item.Type),
            item => Assert.Equal(DocumentType.MaintenanceReport, item.Type));
    }

    [Fact]
    public void Sort_RejectsMissingDocuments()
    {
        DocumentInput[] incomplete = [new(DocumentType.ServiceOrder, "service.pdf")];
        Assert.Throws<ArgumentException>(() => DocumentOrder.Sort(incomplete));
    }

    [Fact]
    public void Sort_AllowsMultipleQuotesAndKeepsThemTogetherInSelectionOrder()
    {
        DocumentInput[] documents =
        [
            new(DocumentType.Quote, "quote-b.pdf"),
            new(DocumentType.MaintenanceReport, "report.pdf"),
            new(DocumentType.ServiceOrder, "service.pdf"),
            new(DocumentType.Quote, "quote-a.pdf"),
            new(DocumentType.WorkOrder, "work.pdf"),
        ];

        var result = DocumentOrder.Sort(documents);

        Assert.Equal(
            ["service.pdf", "work.pdf", "quote-b.pdf", "quote-a.pdf", "report.pdf"],
            result.Select(document => document.SourcePath));
    }

    [Fact]
    public void Sort_RejectsDuplicatesOutsideQuotes()
    {
        DocumentInput[] documents =
        [
            new(DocumentType.ServiceOrder, "service-a.pdf"),
            new(DocumentType.ServiceOrder, "service-b.pdf"),
            new(DocumentType.WorkOrder, "work.pdf"),
            new(DocumentType.Quote, "quote.pdf"),
            new(DocumentType.MaintenanceReport, "report.pdf"),
        ];

        Assert.Throws<ArgumentException>(() => DocumentOrder.Sort(documents));
    }
}
