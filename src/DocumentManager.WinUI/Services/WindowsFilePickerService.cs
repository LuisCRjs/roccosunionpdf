using DocumentManager.WinUI.Services.Interfaces;
using Windows.Storage.Pickers;

namespace DocumentManager.WinUI.Services;

public sealed class WindowsFilePickerService : IFilePickerService
{
    public async Task<string?> PickPdfAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List,
        };
        picker.FileTypeFilter.Add(".pdf");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return file?.Path;
    }
}

