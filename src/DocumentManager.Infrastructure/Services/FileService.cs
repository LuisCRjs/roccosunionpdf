using DocumentManager.Core.Services.Interfaces;

namespace DocumentManager.Infrastructure.Services;

public sealed class FileService : IFileService
{
    public FileService(string? storageRoot = null)
    {
        StorageRoot = storageRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GestorExpedientes");
    }

    public string StorageRoot { get; }

    public string DatabaseDirectory => Path.Combine(StorageRoot, "Database");

    public string TempDirectory => Path.Combine(StorageRoot, "Temp");

    public string ExpedientsDirectory => Path.Combine(StorageRoot, "Expedientes");

    public Task EnsureDirectoriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(DatabaseDirectory);
        Directory.CreateDirectory(TempDirectory);
        Directory.CreateDirectory(ExpedientsDirectory);
        return Task.CompletedTask;
    }

    public Task DeleteTemporaryFilesAsync(
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsInsideDirectory(path, TempDirectory))
            {
                continue;
            }

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // La limpieza es de mejor esfuerzo y nunca debe invalidar un expediente generado.
            }
            catch (UnauthorizedAccessException)
            {
                // La aplicación reintentará indirectamente al limpiar el archivo en una operación futura.
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsInsideDirectory(string candidatePath, string directoryPath)
    {
        var candidate = Path.GetFullPath(candidatePath);
        var directory = Path.GetFullPath(directoryPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return candidate.StartsWith(directory, StringComparison.OrdinalIgnoreCase);
    }
}

