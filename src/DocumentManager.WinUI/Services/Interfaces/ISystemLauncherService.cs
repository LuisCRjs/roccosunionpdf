namespace DocumentManager.WinUI.Services.Interfaces;

public interface ISystemLauncherService
{
    Task OpenFileAsync(string path, CancellationToken cancellationToken = default);
}

