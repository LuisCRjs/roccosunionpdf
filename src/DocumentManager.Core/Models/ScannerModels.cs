namespace DocumentManager.Core.Models;

public sealed record ScannerDevice(
    string Id,
    string Name,
    bool SupportsFlatbed,
    bool SupportsFeeder);

public enum ScannerSource
{
    Default,
    Flatbed,
    Feeder,
}

public sealed record ScanRequest(
    string DeviceId,
    ScannerSource Source,
    string DestinationDirectory);

public sealed record ScanResult(
    IReadOnlyList<string> PagePaths,
    bool WasCancelled = false);

