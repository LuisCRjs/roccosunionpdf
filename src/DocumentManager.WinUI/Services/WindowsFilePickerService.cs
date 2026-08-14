using DocumentManager.WinUI.Services.Interfaces;
using Windows.Storage.Pickers;

namespace DocumentManager.WinUI.Services;

public sealed class WindowsFilePickerService : IFilePickerService
{
    public async Task<IReadOnlyList<string>> PickPdfsAsync(
        bool allowMultiple,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List,
        };
        picker.FileTypeFilter.Add(".pdf");
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
