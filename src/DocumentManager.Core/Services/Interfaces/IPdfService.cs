namespace DocumentManager.Core.Services.Interfaces;

public interface IPdfService
{
    Task ValidatePdfAsync(string path, CancellationToken cancellationToken = default);

    Task ConvertImagesToPdfAsync(
        IReadOnlyList<string> imagePaths,
        string destinationPdfPath,
        CancellationToken cancellationToken = default);

    Task MergeAsync(
        IReadOnlyList<string> sourcePdfPaths,
        string destinationPdfPath,
        CancellationToken cancellationToken = default);
}

