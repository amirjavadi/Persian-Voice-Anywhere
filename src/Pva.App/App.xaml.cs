using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pva.App.ViewModels;
using Pva.App.Views;
using Pva.Audio;
using Pva.Clipboard;
using Pva.Commands;
using Pva.Core;
using Pva.Hotkeys;
using Pva.Injection;
using Pva.Notepad;
using Pva.PersianText;
using Pva.StickyNotes;
using Pva.Storage;
using Pva.Stt;
using Pva.TextExpansion;
using Serilog;

namespace Pva.App;

/// <summary>
/// نقطه‌ی ورود و composition root. کل خط‌لوله را در DI می‌بندد، برنامه را در System Tray
/// اجرا و میکروفون شناور را نمایش می‌دهد. برنامه پرتابل است؛ لاگ و تنظیمات کنار exe.
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private TaskbarIcon? _tray;
    private FloatingMicWindow? _mic;
    private NotepadWindow? _notepad;
    private ClipboardMonitor? _clipboardMonitor;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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

        var settings = _host.Services.GetRequiredService<AppSettings>();
        var viewModel = _host.Services.GetRequiredService<DictationViewModel>();

        _mic = new FloatingMicWindow(viewModel, settings.MicOpacity, settings.MicAlwaysOnTop);
        _mic.Show();

        _tray = BuildTray(viewModel);

        // بازیابی یادداشت‌های چسبان از session قبلی.
        _host.Services.GetRequiredService<StickyNotesManager>().ShowAll();

        // شنود کلیپ‌بورد و ثبت در تاریخچه.
        var clipboard = _host.Services.GetRequiredService<ClipboardHistoryService>();
        clipboard.Load();
        _clipboardMonitor = _host.Services.GetRequiredService<ClipboardMonitor>();
        _clipboardMonitor.TextCopied += text => clipboard.Add(text, DateTime.UtcNow.Ticks);
        _clipboardMonitor.Start();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // تنظیمات را زودتر بارگذاری می‌کنیم تا گزینه‌های موتور/صدا از آن ساخته شوند.
        var settings = new JsonSettingsStore().Load();

        var engineKind = settings.PreferredEngine == "FasterWhisper"
            ? SpeechEngineKind.FasterWhisper
            : SpeechEngineKind.WhisperCpp;

        var device = settings.Device switch
        {
            "Gpu" => ComputeDevice.Gpu,
            "Cpu" => ComputeDevice.Cpu,
            _ => ComputeDevice.Auto,
        };

        services.AddSettings();
        services.AddPersianText();
        services.AddVoiceCommands();
        services.AddTextExpansion();
        services.AddTextInjection();
        services.AddHotkeys();
        services.AddAudioCapture(new AudioCaptureOptions { VadModelPath = settings.VadModelPath });
        services.AddSpeechToText(new SttEngineOptions
        {
            Preferred = engineKind,
            Device = device,
            WhisperModelPath = settings.WhisperModelPath,
        });

        services.AddNotepad();
        services.AddStickyNotes();
        services.AddClipboardHistory();
        services.AddSingleton<DictationViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private TaskbarIcon BuildTray(DictationViewModel viewModel)
    {
        var menu = new ContextMenu { FlowDirection = FlowDirection.RightToLeft };

        var toggle = new MenuItem { Header = "شروع / توقف ضبط" };
        toggle.Click += (_, _) => viewModel.ToggleCommand.Execute(null);

        var showMic = new MenuItem { Header = "نمایش میکروفون" };
        showMic.Click += (_, _) => _mic?.Show();

        var notepad = new MenuItem { Header = "نوت‌پد داخلی" };
        notepad.Click += (_, _) => ShowNotepad();

        var newNote = new MenuItem { Header = "یادداشت چسبان جدید" };
        newNote.Click += (_, _) => _host!.Services.GetRequiredService<StickyNotesManager>().CreateNew();

        var clipboard = new MenuItem { Header = "تاریخچه‌ی کلیپ‌بورد" };
        clipboard.Click += (_, _) => ShowClipboardHistory();

        var settings = new MenuItem { Header = "تنظیمات" };
        settings.Click += (_, _) => ShowSettings();

        var exit = new MenuItem { Header = "خروج" };
        exit.Click += (_, _) => ExitApp();

        menu.Items.Add(toggle);
        menu.Items.Add(showMic);
        menu.Items.Add(notepad);
        menu.Items.Add(newNote);
        menu.Items.Add(clipboard);
        menu.Items.Add(settings);
        menu.Items.Add(new Separator());
        menu.Items.Add(exit);

        return new TaskbarIcon
        {
            ToolTipText = "Persian Voice Anywhere",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenu = menu,
        };
    }

    private void ShowNotepad()
    {
        if (_notepad is null)
        {
            _notepad = new NotepadWindow(_host!.Services.GetRequiredService<NotepadViewModel>());
            _notepad.Closed += (_, _) => _notepad = null;
        }

        _notepad.Show();
        _notepad.Activate();
    }

    private void ShowClipboardHistory()
    {
        var viewModel = _host!.Services.GetRequiredService<ClipboardHistoryViewModel>();
        var window = new ClipboardHistoryWindow(viewModel);
        window.Show();
    }

    private void ShowSettings()
    {
        var viewModel = _host!.Services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(viewModel);
        window.Show();
    }

    private void ExitApp()
    {
        _clipboardMonitor?.Dispose();
        _tray?.Dispose();

        if (_host is not null)
        {
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }

        Log.Information("Persian Voice Anywhere stopped");
        Log.CloseAndFlush();
        Shutdown();
    }
}
