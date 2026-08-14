using DocumentManager.Core.Models;
using DocumentManager.Core.Services;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.Infrastructure.Services;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DocumentManager.Tests;

public sealed class ExpedientGenerationServiceTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public ExpedientGenerationServiceTests() => Directory.CreateDirectory(temporaryDirectory);

    [Fact]
    public async Task GenerateAsync_UsesBusinessOrderAndCreatesRecord()
    {
        var inputDirectory = Path.Combine(temporaryDirectory, "input");
        var outputDirectory = Path.Combine(temporaryDirectory, "output");
        var storageDirectory = Path.Combine(temporaryDirectory, "storage");
        Directory.CreateDirectory(inputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var documents = new[]
        {
            CreateDocument(DocumentType.Quote, 300, inputDirectory),
            CreateDocument(DocumentType.ServiceOrder, 100, inputDirectory),
            CreateDocument(DocumentType.MaintenanceReport, 400, inputDirectory),
            CreateDocument(DocumentType.WorkOrder, 200, inputDirectory),
        };
        var records = new InMemoryRecordService();
        var fileService = new FileService(storageDirectory);
        await fileService.EnsureDirectoriesAsync();
        var sut = new ExpedientGenerationService(
            new PdfService(),
            records,
            fileService,
            new ExpedientValidator(),
            new FinalPdfNameBuilder());

        var result = await sut.GenerateAsync(new ExpedientGenerationRequest(
            new DateTime(2026, 8, 14, 18, 30, 0),
            "OS:5812",
            "EXP-000123",
            documents,
            outputDirectory));

        Assert.Equal("EXP-000123_OS_5812.pdf", Path.GetFileName(result.FinalPdfPath));
        Assert.Single(records.Records);
        Assert.Equal(new DateTime(2026, 8, 14), records.Records[0].Date);

        using var merged = PdfSharp.Pdf.IO.PdfReader.Open(
            result.FinalPdfPath,
            PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
        var pageWidths = Enumerable.Range(0, merged.PageCount)
            .Select(index => merged.Pages[index].Width.Point)
            .ToArray();
        Assert.Equal([100d, 200d, 300d, 400d], pageWidths);
    }

    private static DocumentInput CreateDocument(DocumentType type, double pageWidth, string directory)
    {
        var path = Path.Combine(directory, $"{type}.pdf");
        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Width = XUnit.FromPoint(pageWidth);
        page.Height = XUnit.FromPoint(300);
        document.Save(path);
        return new DocumentInput(type, path);
    }

    public void Dispose() => Directory.Delete(temporaryDirectory, recursive: true);

    private sealed class InMemoryRecordService : IRecordService
    {
        public List<ServiceRecord> Records { get; } = [];

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string> ReserveNextInternalFolioAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult("EXP-000001");

        public Task<ServiceRecord> CreateAsync(ServiceRecord record, CancellationToken cancellationToken = default)
        {
            record.Id = Records.Count + 1;
            Records.Add(record);
            return Task.FromResult(record);
        }

        public Task<IReadOnlyList<ServiceRecord>> SearchAsync(
            string? searchText,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ServiceRecord>>(Records);
    }
}
