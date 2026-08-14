using DocumentManager.Core.Models;
using DocumentManager.WinUI.Services.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocumentManager.WinUI.Services;

public sealed class ScannerDialogService : IScannerDialogService
{
    public async Task<ScannerSelection?> SelectAsync(
        IReadOnlyList<ScannerDevice> devices,
        string? preferredDeviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(devices);
        cancellationToken.ThrowIfCancellationRequested();
        if (devices.Count == 0)
        {
            return null;
        }

        var devicePicker = new ComboBox
        {
            Header = "Escáner",
            DisplayMemberPath = nameof(ScannerDevice.Name),
            ItemsSource = devices,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        devicePicker.SelectedItem = devices.FirstOrDefault(device => device.Id == preferredDeviceId) ?? devices[0];

        var sourcePicker = new ComboBox
        {
            Header = "Origen",
            DisplayMemberPath = nameof(SourceOption.Name),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        void UpdateSources()
        {
            if (devicePicker.SelectedItem is not ScannerDevice device)
            {
                return;
            }

            var sources = new List<SourceOption> { new("Automático", ScannerSource.Default) };
            if (device.SupportsFlatbed)
            {
                sources.Add(new SourceOption("Cama plana", ScannerSource.Flatbed));
            }

            if (device.SupportsFeeder)
            {
                sources.Add(new SourceOption("Alimentador automático", ScannerSource.Feeder));
            }

            sourcePicker.ItemsSource = sources;
            sourcePicker.SelectedIndex = 0;
        }

        devicePicker.SelectionChanged += (_, _) => UpdateSources();
        UpdateSources();

        var content = new StackPanel { Spacing = 16 };
        content.Children.Add(new TextBlock
        {
            Text = "Selecciona el equipo y dónde colocaste el documento.",
            TextWrapping = TextWrapping.Wrap,
        });
        content.Children.Add(devicePicker);
        content.Children.Add(sourcePicker);

        var dialog = new ContentDialog
        {
            Title = "Escanear documento",
            Content = content,
            PrimaryButtonText = "Escanear",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = ((FrameworkElement)App.MainWindow.Content).XamlRoot,
        };

        var result = await dialog.ShowAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (result != ContentDialogResult.Primary ||
            devicePicker.SelectedItem is not ScannerDevice selectedDevice ||
            sourcePicker.SelectedItem is not SourceOption selectedSource)
        {
            return null;
        }

        return new ScannerSelection(selectedDevice.Id, selectedSource.Source);
    }

    private sealed record SourceOption(string Name, ScannerSource Source);
}

