namespace DocumentManager.WinUI.Services.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickPdfAsync(CancellationToken cancellationToken = default);
}

