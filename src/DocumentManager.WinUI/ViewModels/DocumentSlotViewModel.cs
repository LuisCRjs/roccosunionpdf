using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentManager.Core.Models;

namespace DocumentManager.WinUI.ViewModels;

public sealed partial class DocumentSlotViewModel : ObservableObject
{
    private readonly List<DocumentSlotFile> files = [];
    private readonly Func<DocumentSlotViewModel, Task> selectFile;
    private readonly Func<DocumentSlotViewModel, Task> scan;
    private readonly Func<DocumentSlotViewModel, Task> open;
    private readonly Func<DocumentSlotViewModel, Task> remove;

    public DocumentSlotViewModel(
        int position,
        DocumentType type,
        string title,
        Func<DocumentSlotViewModel, Task> selectFile,
        Func<DocumentSlotViewModel, Task> scan,
        Func<DocumentSlotViewModel, Task> open,
        Func<DocumentSlotViewModel, Task> remove)
    {
        Position = position;
        Type = type;
        Title = title;
        this.selectFile = selectFile;
        this.scan = scan;
        this.open = open;
        this.remove = remove;
    }

    public int Position { get; }

    public DocumentType Type { get; }

    public string Title { get; }

    public bool AllowsMultipleFiles => Type == DocumentType.Quote;

    public string SelectButtonText => AllowsMultipleFiles ? "Seleccionar archivos" : "Seleccionar archivo";

    public string ReadySelectButtonText => AllowsMultipleFiles ? "Agregar archivos" : "Cambiar archivo";

    public bool CanAddScannedFile => AllowsMultipleFiles && IsReady;

    public IReadOnlyList<DocumentSlotFile> Files => files;

    public bool IsReady => files.Count > 0;

    public string FileName => files.Count switch
    {
        0 => string.Empty,
        1 => Path.GetFileName(files[0].Path),
        _ => $"{files.Count} cotizaciones: {string.Join(", ", files.Select(file => Path.GetFileName(file.Path)))}",
    };

    public string StatusText => IsBusy
        ? "Procesando..."
        : files.Count > 1
            ? $"{files.Count} documentos listos"
            : IsReady
                ? "Documento listo"
                : "Documento pendiente";

    public bool CanInteract => !IsBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(CanInteract))]
    public partial bool IsBusy { get; set; }

    public void SetFiles(IEnumerable<DocumentSlotFile> selectedFiles)
    {
        ArgumentNullException.ThrowIfNull(selectedFiles);
        files.Clear();
        files.AddRange(selectedFiles);
        NotifyFilesChanged();
    }

    public void AddFile(string path, bool temporary)
    {
        files.Add(new DocumentSlotFile(path, temporary));
        NotifyFilesChanged();
    }

    public void Clear()
    {
        files.Clear();
        NotifyFilesChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        SelectFileCommand.NotifyCanExecuteChanged();
        ScanCommand.NotifyCanExecuteChanged();
        OpenCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
    }

    private void NotifyFilesChanged()
    {
        OnPropertyChanged(nameof(Files));
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(CanAddScannedFile));
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(StatusText));
        OpenCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private Task SelectFileAsync() => selectFile(this);

    [RelayCommand(CanExecute = nameof(CanInteract))]
    private Task ScanAsync() => scan(this);

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private Task OpenAsync() => open(this);

    private bool CanOpen() => CanInteract && IsReady;

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private Task RemoveAsync() => remove(this);

    private bool CanRemove() => CanInteract && IsReady;
}

public sealed record DocumentSlotFile(string Path, bool IsTemporary);
