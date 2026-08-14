using DocumentManager.Core.Models;

namespace DocumentManager.Core.Services.Interfaces;

public interface IScannerService
{
    Task<IReadOnlyList<ScannerDevice>> GetAvailableScannersAsync(
        CancellationToken cancellationToken = default);

    Task<ScanResult> ScanAsync(
        ScanRequest request,
        CancellationToken cancellationToken = default);
}

