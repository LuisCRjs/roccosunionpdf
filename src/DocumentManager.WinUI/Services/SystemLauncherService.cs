using System.Diagnostics;
using DocumentManager.WinUI.Services.Interfaces;

namespace DocumentManager.WinUI.Services;

public sealed class SystemLauncherService : ISystemLauncherService
{
    public Task OpenFileAsync(string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("El PDF ya no se encuentra en la ruta registrada.", path);
        }

        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}

