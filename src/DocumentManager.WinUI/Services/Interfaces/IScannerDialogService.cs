using DocumentManager.Core.Models;

namespace DocumentManager.WinUI.Services.Interfaces;

public sealed record ScannerSelection(string DeviceId, ScannerSource Source);

public interface IScannerDialogService
{
    Task<ScannerSelection?> SelectAsync(
        IReadOnlyList<ScannerDevice> devices,
        string? preferredDeviceId,
        CancellationToken cancellationToken = default);
}

