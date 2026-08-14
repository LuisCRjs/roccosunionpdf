using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services.Interfaces;

public interface IRecordService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<string> GetNextInternalFolioAsync(CancellationToken cancellationToken = default);

    Task<ServiceRecord> CreateWithNextInternalFolioAsync(
        DateTime date,
        string serviceOrderFolio,
        string finalPdfPath,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceRecord>> SearchAsync(
        string? searchText,
        CancellationToken cancellationToken = default);
}
