using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentManager.Core.Models;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.WinUI.Helpers;
using DocumentManager.WinUI.Services.Interfaces;

namespace DocumentManager.WinUI.ViewModels;

public sealed partial class HistoryViewModel(
    IRecordService recordService,
    ISystemLauncherService launcherService) : ObservableObject
{
    public ObservableCollection<HistoryItemViewModel> Records { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public Task InitializeAsync() => SearchAsync();

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await recordService.InitializeAsync();
            var records = await recordService.SearchAsync(SearchText);
            Records.Clear();
            foreach (var record in records)
            {
                Records.Add(new HistoryItemViewModel(record, OpenRecordAsync));
            }
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

    private async Task OpenRecordAsync(ServiceRecord record)
    {
        try
        {
            await launcherService.OpenFileAsync(record.FinalPdfPath);
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
    }
}

public sealed partial class HistoryItemViewModel(
    ServiceRecord record,
    Func<ServiceRecord, Task> open) : ObservableObject
{
    public string InternalFolio => record.InternalFolio;

    public string ServiceOrderFolio => record.ServiceOrderFolio;

    public DateTime Date => record.Date;

    [RelayCommand]
    private Task OpenAsync() => open(record);
}
