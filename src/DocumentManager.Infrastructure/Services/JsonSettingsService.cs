using System.Text.Json;
using DocumentManager.Core.Models;
using DocumentManager.Core.Services.Interfaces;

namespace DocumentManager.Infrastructure.Services;

public sealed class JsonSettingsService(IFileService fileService) : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private string SettingsPath => Path.Combine(fileService.StorageRoot, "settings.json");

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await fileService.EnsureDirectoriesAsync(cancellationToken);
        if (!File.Exists(SettingsPath))
        {
            return CreateDefaults();
        }

        await using var stream = File.OpenRead(SettingsPath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(
            stream,
            SerializerOptions,
            cancellationToken);

        settings ??= CreateDefaults();
        if (string.IsNullOrWhiteSpace(settings.ExpedientsDirectory))
        {
            settings.ExpedientsDirectory = fileService.ExpedientsDirectory;
        }

        return settings;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        await fileService.EnsureDirectoriesAsync(cancellationToken);

        var partialPath = $"{SettingsPath}.{Guid.NewGuid():N}.partial";
        try
        {
            await using (var stream = new FileStream(
                partialPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
            }

            File.Move(partialPath, SettingsPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(partialPath))
            {
                File.Delete(partialPath);
            }
        }
    }

    private AppSettings CreateDefaults() => new()
    {
        ExpedientsDirectory = fileService.ExpedientsDirectory,
    };
}

