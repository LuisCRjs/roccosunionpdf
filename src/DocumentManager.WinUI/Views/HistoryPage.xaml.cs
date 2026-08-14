using DocumentManager.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocumentManager.WinUI.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<HistoryViewModel>();
        DataContext = ViewModel;
    }

    public HistoryViewModel ViewModel { get; }

    private async void Page_Loaded(object sender, RoutedEventArgs e) =>
        await ViewModel.InitializeAsync();
}

