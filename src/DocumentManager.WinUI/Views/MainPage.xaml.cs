using DocumentManager.WinUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocumentManager.WinUI.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MainViewModel>();
        DataContext = ViewModel;
    }

    public MainViewModel ViewModel { get; }

    private async void Page_Loaded(object sender, RoutedEventArgs e) =>
        await ViewModel.InitializeAsync();
}

