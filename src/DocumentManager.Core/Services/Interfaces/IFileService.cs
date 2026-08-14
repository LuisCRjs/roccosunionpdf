namespace DocumentManager.Core.Services.Interfaces;

public interface IFileService
{
    string StorageRoot { get; }

    string DatabaseDirectory { get; }

    string TempDirectory { get; }

    string ExpedientsDirectory { get; }

    Task EnsureDirectoriesAsync(CancellationToken cancellationToken = default);

    Task DeleteTemporaryFilesAsync(
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default);
}

