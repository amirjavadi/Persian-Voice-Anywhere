using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Pva.App;

/// <summary>
/// نقطه‌ی ورود اپلیکیشن و composition root. یک Generic Host مسئول DI و logging است.
/// در Milestone M0 فقط اسکلت راه‌اندازی/خاموشی تمیز پیاده شده؛ سرویس‌های ماژول‌ها در
/// milestoneهای بعدی ثبت می‌شوند.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // داده‌ی زمان‌اجرا کنار فایل اجرایی نگه‌داری می‌شود (پرتابل، بدون رجیستری).
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logDirectory, "pva-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true,
                formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(ConfigureServices)
            .Build();

        _host.Start();

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
        Log.Information("Persian Voice Anywhere starting (v{Version})", version);

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        // ثبت سرویس‌های ماژول‌ها (Audio, Stt, Injection, …) در milestoneهای مربوطه.
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }

        Log.Information("Persian Voice Anywhere stopped");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
