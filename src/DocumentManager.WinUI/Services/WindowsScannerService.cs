using DocumentManager.Core.Models;
using DocumentManager.Core.Services.Interfaces;
using System.Runtime.InteropServices;
using Windows.Devices.Enumeration;
using Windows.Devices.Scanners;
using Windows.Storage;

namespace DocumentManager.WinUI.Services;

public sealed class WindowsScannerService : IScannerService
{
    public async Task<IReadOnlyList<ScannerDevice>> GetAvailableScannersAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var devices = await DeviceInformation.FindAllAsync(ImageScanner.GetDeviceSelector());
        var result = new List<ScannerDevice>(devices.Count);

        foreach (var device in devices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var scanner = await ImageScanner.FromIdAsync(device.Id);
                if (scanner is null)
                {
                    continue;
                }

                result.Add(new ScannerDevice(
                    device.Id,
                    device.Name,
                    scanner.IsScanSourceSupported(ImageScannerScanSource.Flatbed),
                    scanner.IsScanSourceSupported(ImageScannerScanSource.Feeder)));
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or COMException)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        return result.OrderBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public async Task<ScanResult> ScanAsync(
        ScanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(request.DestinationDirectory);

        var scanner = await ImageScanner.FromIdAsync(request.DeviceId);
        if (scanner is null)
        {
            throw new InvalidOperationException("El escáner seleccionado ya no está disponible.");
        }

        var source = ResolveSource(scanner, request.Source);
        if (source != ImageScannerScanSource.Default && !scanner.IsScanSourceSupported(source))
        {
            throw new InvalidOperationException("El escáner no admite la fuente seleccionada.");
        }

        ConfigureBestFormat(scanner, source);
        var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetFullPath(request.DestinationDirectory));

        try
        {
            var result = await scanner
                .ScanFilesToFolderAsync(source, folder)
                .AsTask(cancellationToken);

            if (!result.ScannedFiles.Any())
            {
                return new ScanResult([], WasCancelled: true);
            }

            return new ScanResult(result.ScannedFiles.Select(file => file.Path).ToArray());
        }
        catch (TaskCanceledException)
        {
            return new ScanResult([], WasCancelled: true);
        }
    }

    private static ImageScannerScanSource ResolveSource(ImageScanner scanner, ScannerSource source) => source switch
    {
        ScannerSource.Flatbed => ImageScannerScanSource.Flatbed,
        ScannerSource.Feeder => ImageScannerScanSource.Feeder,
        _ => scanner.DefaultScanSource,
    };

    private static void ConfigureBestFormat(ImageScanner scanner, ImageScannerScanSource source)
    {
        IImageScannerFormatConfiguration? configuration = source switch
        {
            ImageScannerScanSource.Flatbed => scanner.FlatbedConfiguration,
            ImageScannerScanSource.Feeder => scanner.FeederConfiguration,
            ImageScannerScanSource.AutoConfigured => scanner.AutoConfiguration,
            _ => null,
        };

        if (configuration is null)
        {
            return;
        }

        ImageScannerFormat[] preferredFormats =
        [
            ImageScannerFormat.Pdf,
            ImageScannerFormat.Png,
            ImageScannerFormat.Jpeg,
            ImageScannerFormat.DeviceIndependentBitmap,
        ];

        foreach (var format in preferredFormats)
        {
            if (!configuration.IsFormatSupported(format))
            {
                continue;
            }

            configuration.Format = format;
            return;
        }
    }
}
