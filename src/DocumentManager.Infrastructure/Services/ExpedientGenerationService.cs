using DocumentManager.Core.Models;
using DocumentManager.Core.Services;
using DocumentManager.Core.Services.Interfaces;

namespace DocumentManager.Infrastructure.Services;

public sealed class ExpedientGenerationService(
    IPdfService pdfService,
    IRecordService recordService,
    IFileService fileService,
    ExpedientValidator validator,
    FinalPdfNameBuilder fileNameBuilder) : IExpedientGenerationService
{
    public async Task<ExpedientGenerationResult> GenerateAsync(
        ExpedientGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = validator.Validate(
            request.ServiceOrderFolio,
            request.EconomicNumber,
            request.Documents);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Errors));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.DestinationDirectory);

        var orderedDocuments = DocumentOrder.Sort(request.Documents);
        var normalizedPdfs = new List<string>(orderedDocuments.Count);
        var generatedTemporaryPdfs = new List<string>();

        foreach (var document in orderedDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(Path.GetExtension(document.SourcePath), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                await pdfService.ValidatePdfAsync(document.SourcePath, cancellationToken);
                normalizedPdfs.Add(document.SourcePath);
                continue;
            }

            var normalizedPath = Path.Combine(
                fileService.TempDirectory,
                $"document-{(int)document.Type}-{Guid.NewGuid():N}.pdf");
            await pdfService.ConvertImagesToPdfAsync([document.SourcePath], normalizedPath, cancellationToken);
            generatedTemporaryPdfs.Add(normalizedPath);
            normalizedPdfs.Add(normalizedPath);
        }

        var finalFileName = fileNameBuilder.Build(
            request.EconomicNumber.Trim(),
            request.ServiceOrderFolio.Trim());
        var finalPath = Path.Combine(request.DestinationDirectory, finalFileName);

        await pdfService.MergeAsync(normalizedPdfs, finalPath, cancellationToken);

        var record = await recordService.CreateWithNextInternalFolioAsync(
            request.Date,
            request.ServiceOrderFolio,
            finalPath,
            cancellationToken);

        var ownedTemporaryFiles = orderedDocuments
            .Where(document => document.IsTemporary)
            .Select(document => document.SourcePath)
            .Concat(generatedTemporaryPdfs);
        await fileService.DeleteTemporaryFilesAsync(ownedTemporaryFiles, cancellationToken);

        return new ExpedientGenerationResult(record, finalPath);
    }
}
