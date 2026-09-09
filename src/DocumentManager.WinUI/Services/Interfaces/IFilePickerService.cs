namespace DocumentManager.WinUI.Services.Interfaces;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickFilesAsync(
        bool allowMultiple,
        IReadOnlyCollection<string> allowedExtensions,
        CancellationToken cancellationToken = default);
}
