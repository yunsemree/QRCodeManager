using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using QRCodeManager.Application.Constants;

namespace QRCodeManager.WPF.Helpers;

/// <summary>
/// Kurulum sonrası sessiz kapanmaları önlemek için başlatma hatalarını kullanıcıya gösterir.
/// </summary>
internal static class StartupErrorReporter
{
    public static void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                ShowFatal("Beklenmeyen hata", exception, shutdown: true);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            args.SetObserved();
            ShowFatal("Arka plan görevi hatası", args.Exception, shutdown: true);
        };

        if (System.Windows.Application.Current is not null)
        {
            System.Windows.Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        args.Handled = true;
        ShowFatal("Uygulama hatası", args.Exception, shutdown: true);
    }

    public static void ShowFatal(string title, Exception exception, bool shutdown)
    {
        var logPath = WriteCrashLog(exception);

        try
        {
            MessageBox.Show(
                $"{title}:\n\n{exception.Message}\n\nAyrıntılar kaydedildi:\n{logPath}",
                ApplicationConstants.ApplicationName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            // Son çare: en azından log dosyası oluşmuş olmalı.
        }

        if (shutdown && System.Windows.Application.Current is not null)
        {
            System.Windows.Application.Current.Shutdown(-1);
        }
    }

    private static string WriteCrashLog(Exception exception)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var content = new StringBuilder()
            .AppendLine($"[{timestamp}] FATAL")
            .AppendLine(exception.ToString())
            .ToString();

        var candidates = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationConstants.ApplicationName,
                ApplicationConstants.LogsDirectoryName,
                "startup-crash.log"),
            Path.Combine(AppContext.BaseDirectory, "startup-crash.log")
        };

        foreach (var path in candidates)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(path, content);
                return path;
            }
            catch
            {
                // Bir sonraki konumu dene.
            }
        }

        return "startup-crash.log (yazılamadı)";
    }
}
