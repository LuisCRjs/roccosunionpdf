namespace DocumentManager.WinUI.Services.Interfaces;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickPdfsAsync(
        bool allowMultiple,
        CancellationToken cancellationToken = default);
}
