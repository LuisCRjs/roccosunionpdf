using DocumentManager.WinUI.Services.Interfaces;
using Windows.Storage.Pickers;

namespace DocumentManager.WinUI.Services;

public sealed class WindowsFilePickerService : IFilePickerService
{
    public async Task<IReadOnlyList<string>> PickFilesAsync(
        bool allowMultiple,
        IReadOnlyCollection<string> allowedExtensions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allowedExtensions);
        if (allowedExtensions.Count == 0)
        {
            throw new ArgumentException("Se requiere al menos un formato permitido.", nameof(allowedExtensions));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List,
        };
        foreach (var extension in allowedExtensions)
        {
            picker.FileTypeFilter.Add(extension);
        }
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);

        if (allowMultiple)
        {
            var files = await picker.PickMultipleFilesAsync();
            cancellationToken.ThrowIfCancellationRequested();
            return files.Select(file => file.Path).ToArray();
        }

        var file = await picker.PickSingleFileAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return file is null ? [] : [file.Path];
    }
}
