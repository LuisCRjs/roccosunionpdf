using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentManager.Core.Models;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.WinUI.Helpers;
using DocumentManager.WinUI.Services.Interfaces;

namespace DocumentManager.WinUI.ViewModels;

public sealed partial class SettingsViewModel(
    IScannerService scannerService,
    ISettingsService settingsService,
    IFileService fileService,
    IFolderPickerService folderPickerService) : ObservableObject
{
    public ObservableCollection<ScannerDevice> Scanners { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    [ObservableProperty]
    private ScannerDevice? selectedScanner;

    [ObservableProperty]
    private string expedientsDirectory = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string? successMessage;

    public async Task InitializeAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await fileService.EnsureDirectoriesAsync();
            var settings = await settingsService.LoadAsync();
            ExpedientsDirectory = settings.ExpedientsDirectory ?? fileService.ExpedientsDirectory;
            var devices = await scannerService.GetAvailableScannersAsync();
            Scanners.Clear();
            foreach (var device in devices)
            {
                Scanners.Add(device);
            }

            SelectedScanner = Scanners.FirstOrDefault(device => device.Id == settings.DefaultScannerId);
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BrowseDirectoryAsync()
    {
        var path = await folderPickerService.PickFolderAsync();
        if (path is not null)
        {
            ExpedientsDirectory = path;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        if (string.IsNullOrWhiteSpace(ExpedientsDirectory))
        {
            ErrorMessage = "Selecciona una carpeta para guardar los expedientes.";
            return;
        }

        IsBusy = true;
        try
        {
            Directory.CreateDirectory(ExpedientsDirectory);
            await settingsService.SaveAsync(new AppSettings
            {
                DefaultScannerId = SelectedScanner?.Id,
                DefaultScannerName = SelectedScanner?.Name,
                ExpedientsDirectory = ExpedientsDirectory,
            });
            SuccessMessage = "Configuración guardada.";
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
