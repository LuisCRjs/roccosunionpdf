using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentManager.Core.Models;

namespace DocumentManager.WinUI.ViewModels;

public sealed partial class DocumentSlotViewModel : ObservableObject
{
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

    public bool IsReady => !string.IsNullOrWhiteSpace(FilePath);

    public string FileName => IsReady ? Path.GetFileName(FilePath!) : string.Empty;

    public string StatusText => IsBusy ? "Procesando..." : IsReady ? "Documento listo" : "Documento pendiente";

    public bool CanInteract => !IsBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    [NotifyPropertyChangedFor(nameof(FileName))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    public partial string? FilePath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(CanInteract))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsTemporary { get; set; }

    public void SetFile(string path, bool temporary)
    {
        FilePath = path;
        IsTemporary = temporary;
    }

    public void Clear()
    {
        FilePath = null;
        IsTemporary = false;
    }

    partial void OnIsBusyChanged(bool value)
    {
        SelectFileCommand.NotifyCanExecuteChanged();
        ScanCommand.NotifyCanExecuteChanged();
        OpenCommand.NotifyCanExecuteChanged();
        RemoveCommand.NotifyCanExecuteChanged();
    }

    partial void OnFilePathChanged(string? value)
    {
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
