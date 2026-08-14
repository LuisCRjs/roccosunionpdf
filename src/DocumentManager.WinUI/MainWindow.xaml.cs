using DocumentManager.WinUI.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocumentManager.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Gestor de expedientes 1.02";
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1180, 820));
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ContentFrame.Navigate(typeof(MainPage));
    }

    private void Navigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is not NavigationViewItem item)
        {
            return;
        }

        Navigate(item.Tag?.ToString() switch
        {
            "new" => typeof(MainPage),
            "history" => typeof(HistoryPage),
            _ => typeof(MainPage),
        });
    }

    private void Navigate(Type pageType)
    {
        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
