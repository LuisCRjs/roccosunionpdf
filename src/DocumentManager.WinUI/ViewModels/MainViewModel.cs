using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentManager.Core.Models;
using DocumentManager.Core.Services;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.WinUI.Helpers;
using DocumentManager.WinUI.Services.Interfaces;

namespace DocumentManager.WinUI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IRecordService recordService;
    private readonly IExpedientGenerationService generationService;
    private readonly IPdfService pdfService;
    private readonly IScannerService scannerService;
    private readonly ISettingsService settingsService;
    private readonly IFileService fileService;
    private readonly IFilePickerService filePickerService;
    private readonly IScannerDialogService scannerDialogService;
    private readonly ISystemLauncherService launcherService;
    private readonly HashSet<string> ownedTemporaryFiles = new(StringComparer.OrdinalIgnoreCase);
    private bool initialized;

    public MainViewModel(
        IRecordService recordService,
        IExpedientGenerationService generationService,
        IPdfService pdfService,
        IScannerService scannerService,
        ISettingsService settingsService,
        IFileService fileService,
        IFilePickerService filePickerService,
        IScannerDialogService scannerDialogService,
        ISystemLauncherService launcherService)
    {
        this.recordService = recordService;
        this.generationService = generationService;
        this.pdfService = pdfService;
        this.scannerService = scannerService;
        this.settingsService = settingsService;
        this.fileService = fileService;
        this.filePickerService = filePickerService;
        this.scannerDialogService = scannerDialogService;
        this.launcherService = launcherService;

        Documents = new ObservableCollection<DocumentSlotViewModel>(DocumentOrder.Required.Select((type, index) =>
        {
            var slot = new DocumentSlotViewModel(
                index + 1,
                type,
                ExpedientValidator.GetDisplayName(type),
                SelectFileAsync,
                ScanAsync,
                OpenSlotAsync,
                RemoveSlot);
            slot.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(DocumentSlotViewModel.IsReady) or nameof(DocumentSlotViewModel.IsBusy))
                {
                    GenerateCommand.NotifyCanExecuteChanged();
                }
            };
            return slot;
        }));
    }

    public ObservableCollection<DocumentSlotViewModel> Documents { get; }

    public string DateText => Date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

    public bool CanGenerate =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(ServiceOrderFolio) &&
        !string.IsNullOrWhiteSpace(EconomicNumber) &&
        Documents.All(document => document.IsReady && !document.IsBusy);

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    [ObservableProperty]
    public partial DateTime Date { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string ServiceOrderFolio { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EconomicNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InternalFolio { get; set; } = "Preparando...";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial string? GeneratedPdfPath { get; set; }

    public async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        await StartNewAsync();
    }

    partial void OnServiceOrderFolioChanged(string value) => GenerateCommand.NotifyCanExecuteChanged();

    partial void OnEconomicNumberChanged(string value) => GenerateCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value) => GenerateCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync()
    {
        ClearFeedback();
        IsBusy = true;
        BusyText = "Generando expediente...";
        try
        {
            var settings = await settingsService.LoadAsync();
            var destination = settings.ExpedientsDirectory ?? fileService.ExpedientsDirectory;
            var inputs = Documents
                .SelectMany(slot => slot.Files.Select(file => new DocumentInput(
                    slot.Type,
                    file.Path,
                    file.IsTemporary)))
                .ToArray();

            var result = await generationService.GenerateAsync(new ExpedientGenerationRequest(
                Date,
                ServiceOrderFolio,
                EconomicNumber,
                inputs,
                destination));

            InternalFolio = result.Record.InternalFolio;
            GeneratedPdfPath = result.FinalPdfPath;
            ownedTemporaryFiles.Clear();
            IsCompleted = true;
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task StartNewAsync()
    {
        ClearFeedback();
        await fileService.DeleteTemporaryFilesAsync(ownedTemporaryFiles);
        ownedTemporaryFiles.Clear();
        IsCompleted = false;
        GeneratedPdfPath = null;
        ServiceOrderFolio = string.Empty;
        EconomicNumber = string.Empty;
        Date = DateTime.Today;
        OnPropertyChanged(nameof(DateText));
        foreach (var slot in Documents)
        {
            slot.Clear();
        }

        IsBusy = true;
        BusyText = "Preparando expediente...";
        try
        {
            await fileService.EnsureDirectoriesAsync();
            await recordService.InitializeAsync();
            InternalFolio = await recordService.GetNextInternalFolioAsync();
        }
        catch (Exception exception)
        {
            InternalFolio = "No disponible";
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task OpenGeneratedAsync()
    {
        if (GeneratedPdfPath is null)
        {
            return;
        }

        await TryOpenAsync(GeneratedPdfPath);
    }

    private async Task SelectFileAsync(DocumentSlotViewModel slot)
    {
        ClearFeedback();
        slot.IsBusy = true;
        try
        {
            var paths = await filePickerService.PickPdfsAsync(slot.AllowsMultipleFiles);
            if (paths.Count == 0)
            {
                return;
            }

            foreach (var path in paths)
            {
                await pdfService.ValidatePdfAsync(path);
            }

            if (slot.AllowsMultipleFiles)
            {
                var combinedFiles = slot.Files
                    .Concat(paths.Select(path => new DocumentSlotFile(path, IsTemporary: false)))
                    .DistinctBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                await ReplaceSlotFilesAsync(slot, combinedFiles);
            }
            else
            {
                await ReplaceSlotFilesAsync(slot, [new DocumentSlotFile(paths[0], IsTemporary: false)]);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            slot.IsBusy = false;
        }
    }

    private async Task ScanAsync(DocumentSlotViewModel slot)
    {
        ClearFeedback();
        slot.IsBusy = true;
        try
        {
            var devices = await scannerService.GetAvailableScannersAsync();
            if (devices.Count == 0)
            {
                ErrorMessage = "No se encontró ningún escáner disponible. Comprueba que esté encendido y que tenga instalado un controlador WIA en Windows.";
                return;
            }

            var settings = await settingsService.LoadAsync();
            var selection = await scannerDialogService.SelectAsync(devices, settings.DefaultScannerId);
            if (selection is null)
            {
                return;
            }

            var scanDirectory = Path.Combine(fileService.TempDirectory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scanDirectory);
            var scan = await scannerService.ScanAsync(new ScanRequest(
                selection.DeviceId,
                selection.Source,
                scanDirectory));
            if (scan.WasCancelled || scan.PagePaths.Count == 0)
            {
                return;
            }

            var normalizedPath = Path.Combine(
                fileService.TempDirectory,
                $"scan-{slot.Type}-{Guid.NewGuid():N}.pdf");
            if (scan.PagePaths.All(path => string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase)))
            {
                if (scan.PagePaths.Count == 1)
                {
                    normalizedPath = scan.PagePaths[0];
                }
                else
                {
                    await pdfService.MergeAsync(scan.PagePaths, normalizedPath);
                    await fileService.DeleteTemporaryFilesAsync(scan.PagePaths);
                }
            }
            else
            {
                await pdfService.ConvertImagesToPdfAsync(scan.PagePaths, normalizedPath);
                await fileService.DeleteTemporaryFilesAsync(scan.PagePaths);
            }

            await pdfService.ValidatePdfAsync(normalizedPath);
            if (slot.AllowsMultipleFiles)
            {
                slot.AddFile(normalizedPath, temporary: true);
                ownedTemporaryFiles.Add(normalizedPath);
            }
            else
            {
                await ReplaceSlotFilesAsync(
                    slot,
                    [new DocumentSlotFile(normalizedPath, IsTemporary: true)]);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
        finally
        {
            slot.IsBusy = false;
        }
    }

    private async Task OpenSlotAsync(DocumentSlotViewModel slot)
    {
        foreach (var file in slot.Files)
        {
            await TryOpenAsync(file.Path);
        }
    }

    private async Task TryOpenAsync(string path)
    {
        ClearFeedback();
        try
        {
            await launcherService.OpenFileAsync(path);
        }
        catch (Exception exception)
        {
            ErrorMessage = UserMessageMapper.FromException(exception);
        }
    }

    private async Task RemoveSlot(DocumentSlotViewModel slot)
    {
        ClearFeedback();
        var temporaryPaths = slot.Files
            .Where(file => file.IsTemporary)
            .Select(file => file.Path)
            .ToArray();
        await fileService.DeleteTemporaryFilesAsync(temporaryPaths);
        ownedTemporaryFiles.ExceptWith(temporaryPaths);

        slot.Clear();
    }

    private async Task ReplaceSlotFilesAsync(
        DocumentSlotViewModel slot,
        IReadOnlyCollection<DocumentSlotFile> replacementFiles)
    {
        var replacementPaths = replacementFiles
            .Select(file => file.Path)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var discardedTemporaryPaths = slot.Files
            .Where(file => file.IsTemporary && !replacementPaths.Contains(file.Path))
            .Select(file => file.Path)
            .ToArray();

        await fileService.DeleteTemporaryFilesAsync(discardedTemporaryPaths);
        ownedTemporaryFiles.ExceptWith(discardedTemporaryPaths);

        slot.SetFiles(replacementFiles);
        foreach (var file in replacementFiles.Where(file => file.IsTemporary))
        {
            ownedTemporaryFiles.Add(file.Path);
        }
    }

    private void ClearFeedback() => ErrorMessage = null;
}
