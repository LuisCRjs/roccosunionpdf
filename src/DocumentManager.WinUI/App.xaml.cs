using DocumentManager.Core.Services;
using DocumentManager.Core.Services.Interfaces;
using DocumentManager.Infrastructure.Data;
using DocumentManager.Infrastructure.Services;
using DocumentManager.WinUI.Services;
using DocumentManager.WinUI.Services.Interfaces;
using DocumentManager.WinUI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace DocumentManager.WinUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
        UnhandledException += OnUnhandledException;
    }

    public static Window MainWindow { get; private set; } = null!;

    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);

    public static IServiceProvider Services { get; private set; } = null!;

    public static T GetService<T>() where T : notnull => Services.GetRequiredService<T>();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IFilePickerService, WindowsFilePickerService>();
        services.AddSingleton<IFolderPickerService, WindowsFolderPickerService>();
        services.AddSingleton<ISystemLauncherService, SystemLauncherService>();
        services.AddSingleton<IScannerService, WindowsScannerService>();
        services.AddSingleton<IScannerDialogService, ScannerDialogService>();
        services.AddSingleton<ExpedientValidator>();
        services.AddSingleton<FinalPdfNameBuilder>();

        services.AddDbContextFactory<AppDbContext>((provider, options) =>
        {
            var fileService = provider.GetRequiredService<IFileService>();
            var databasePath = Path.Combine(fileService.DatabaseDirectory, "app.db");
            options.UseSqlite($"Data Source={databasePath};Cache=Shared;Foreign Keys=True");
        });

        services.AddSingleton<IRecordService, RecordService>();
        services.AddSingleton<IExpedientGenerationService, ExpedientGenerationService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine(args.Exception);
        args.Handled = true;
    }
}

