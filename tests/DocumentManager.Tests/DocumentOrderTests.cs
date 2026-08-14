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
}

