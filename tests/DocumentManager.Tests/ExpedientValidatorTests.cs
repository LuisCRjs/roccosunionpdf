using DocumentManager.Core.Models;
using DocumentManager.Core.Services;

namespace DocumentManager.Tests;

public sealed class ExpedientValidatorTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public ExpedientValidatorTests() => Directory.CreateDirectory(temporaryDirectory);

    [Fact]
    public void Validate_AcceptsCompleteExpedient()
    {
        var documents = CreateDocuments();
        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsEmptyFolioAndMissingDocuments()
    {
        var result = new ExpedientValidator().Validate(" ", " ", []);
        Assert.False(result.IsValid);
        Assert.Equal(6, result.Errors.Count);
    }

    [Fact]
    public void Validate_AcceptsMultipleQuoteFiles()
    {
        var documents = CreateDocuments().ToList();
        var additionalQuote = Path.Combine(temporaryDirectory, "Quote-2.pdf");
        File.WriteAllText(additionalQuote, "test");
        documents.Add(new DocumentInput(DocumentType.Quote, additionalQuote));

        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_AcceptsMultiplePdfAndImageFilesForOrders()
    {
        var documents = CreateDocuments().ToList();
        var serviceImage = CreateFile("service-extra.PNG");
        var workImage = CreateFile("work-extra.JpEg");
        documents.Add(new DocumentInput(DocumentType.ServiceOrder, serviceImage));
        documents.Add(new DocumentInput(DocumentType.WorkOrder, workImage));

        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsImagesForQuoteAndMaintenanceReport()
    {
        var documents = CreateDocuments().ToList();
        documents.RemoveAll(document => document.Type is DocumentType.Quote or DocumentType.MaintenanceReport);
        documents.Add(new DocumentInput(DocumentType.Quote, CreateFile("quote.png")));
        documents.Add(new DocumentInput(DocumentType.MaintenanceReport, CreateFile("report.jpg")));

        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_RejectsMultipleMaintenanceReports()
    {
        var documents = CreateDocuments().ToList();
        documents.Add(new DocumentInput(DocumentType.MaintenanceReport, CreateFile("report-2.pdf")));

        var result = new ExpedientValidator().Validate("OS-5812", "123", documents);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("duplicado", StringComparison.OrdinalIgnoreCase));
    }

    private string CreateFile(string fileName)
    {
        var path = Path.Combine(temporaryDirectory, fileName);
        File.WriteAllText(path, "test");
        return path;
    }

    private IReadOnlyList<DocumentInput> CreateDocuments() =>
        DocumentOrder.Required.Select(type =>
        {
            var path = Path.Combine(temporaryDirectory, $"{type}.pdf");
            File.WriteAllText(path, "test");
            return new DocumentInput(type, path);
        }).ToArray();

    public void Dispose() => Directory.Delete(temporaryDirectory, recursive: true);
}
