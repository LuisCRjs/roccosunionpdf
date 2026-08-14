namespace DocumentManager.WinUI.Services.Interfaces;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync(CancellationToken cancellationToken = default);
}

