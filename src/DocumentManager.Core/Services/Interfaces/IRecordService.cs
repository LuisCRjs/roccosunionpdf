using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services.Interfaces;

public interface IRecordService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<string> ReserveNextInternalFolioAsync(CancellationToken cancellationToken = default);

    Task<ServiceRecord> CreateAsync(ServiceRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceRecord>> SearchAsync(
        string? searchText,
        CancellationToken cancellationToken = default);
}

