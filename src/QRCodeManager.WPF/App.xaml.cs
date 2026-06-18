using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QRCodeManager.Infrastructure;
using QRCodeManager.WPF.Helpers;
using QRCodeManager.WPF.Services;
using QRCodeManager.WPF.ViewModels;

namespace QRCodeManager.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IServiceScope? _scope;

    protected override async void OnStartup(StartupEventArgs e)
    {
        StartupErrorReporter.Register();

        try
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services.AddInfrastructure();

                    services.AddSingleton<INavigationMessenger, NavigationMessenger>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<GenerateQrViewModel>();
                    services.AddTransient<ReadQrViewModel>();
                    services.AddTransient<HistoryViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            await _host.Services.InitializeDatabaseAsync();

            _scope = _host.Services.CreateScope();
            var mainWindow = _scope.ServiceProvider.GetRequiredService<MainWindow>();
            var settingsService = _scope.ServiceProvider.GetRequiredService<Application.Interfaces.ISettingsService>();

            if (mainWindow.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.SettingsViewModel.ThemeChanged += ThemeHelper.ApplyTheme;
            }

            ThemeHelper.ApplyTheme(settingsService.GetSettings().Theme);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            StartupErrorReporter.ShowFatal("QR Manager başlatılamadı", ex, shutdown: true);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _scope?.Dispose();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
