using DocumentManager.Infrastructure.Services;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace DocumentManager.Tests;

public sealed class PdfServiceTests : IDisposable
{
    private readonly string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public PdfServiceTests() => Directory.CreateDirectory(temporaryDirectory);

    [Fact]
    public async Task MergeAsync_PreservesEveryPageAndInputOrder()
    {
        var first = CreatePdf("first.pdf", 101, 102);
        var second = CreatePdf("second.pdf", 201);
        var destination = Path.Combine(temporaryDirectory, "merged.pdf");
        var sut = new PdfService();

        await sut.MergeAsync([first, second], destination);

        using var result = PdfReader.Open(destination, PdfDocumentOpenMode.Import);
        Assert.Equal(3, result.PageCount);
        Assert.Equal(101, result.Pages[0].Width.Point, precision: 3);
        Assert.Equal(102, result.Pages[1].Width.Point, precision: 3);
        Assert.Equal(201, result.Pages[2].Width.Point, precision: 3);
    }

    [Fact]
    public async Task ValidatePdfAsync_RejectsCorruptFile()
    {
        var corruptPath = Path.Combine(temporaryDirectory, "corrupt.pdf");
        await File.WriteAllTextAsync(corruptPath, "not-a-pdf");

        await Assert.ThrowsAnyAsync<Exception>(() => new PdfService().ValidatePdfAsync(corruptPath));
    }

    [Fact]
    public async Task ConvertImagesToPdfAsync_CreatesOnePagePerImage()
    {
        const string onePixelPng =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";
        var first = Path.Combine(temporaryDirectory, "page-1.png");
        var second = Path.Combine(temporaryDirectory, "page-2.png");
        await File.WriteAllBytesAsync(first, Convert.FromBase64String(onePixelPng));
        await File.WriteAllBytesAsync(second, Convert.FromBase64String(onePixelPng));
        var destination = Path.Combine(temporaryDirectory, "images.pdf");

        await new PdfService().ConvertImagesToPdfAsync([first, second], destination);

        using var result = PdfReader.Open(destination, PdfDocumentOpenMode.Import);
        Assert.Equal(2, result.PageCount);
    }

    private string CreatePdf(string fileName, params double[] pageWidths)
    {
        var path = Path.Combine(temporaryDirectory, fileName);
        using var document = new PdfDocument();
        foreach (var width in pageWidths)
        {
            var page = document.AddPage();
            page.Width = XUnit.FromPoint(width);
            page.Height = XUnit.FromPoint(300);
        }

        document.Save(path);
        return path;
    }

    public void Dispose() => Directory.Delete(temporaryDirectory, recursive: true);
}
