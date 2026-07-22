using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pva.Core;
using Pva.PersianText;
using Pva.Stt;

namespace Pva.App.ViewModels;

/// <summary>
/// ViewModel هسته‌ی دیکته: چرخه‌ی حیات خط‌لوله را بر پایه‌ی کلید میانبر مدیریت و وضعیت
/// را برای میکروفون شناور و tray منتشر می‌کند. موتور STT به‌صورت lazy در اولین ضبط
/// resolve می‌شود تا نبودِ مدل، راه‌اندازی برنامه را نشکند.
/// </summary>
public partial class DictationViewModel : ObservableObject
{
    private readonly ISpeechEngineResolver _resolver;
    private readonly IAudioCapture _audio;
    private readonly ICommandParser _parser;
    private readonly IPersianTextProcessor _persian;
    private readonly ITextInjector _injector;
    private readonly IHotkeyService _hotkeys;
    private readonly ITextExpander _expander;
    private readonly Storage.AppSettings _settings;

    private DictationOrchestrator? _orchestrator;

    [ObservableProperty]
    private string _stateText = DictationStateText.ToPersian(DictationState.Idle);

    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private string? _lastError;

    public ObservableCollection<string> RecentTranscriptions { get; } = new();

    public DictationViewModel(
        ISpeechEngineResolver resolver,
        IAudioCapture audio,
        ICommandParser parser,
        IPersianTextProcessor persian,
        ITextInjector injector,
        IHotkeyService hotkeys,
        ITextExpander expander,
        Storage.AppSettings settings)
    {
        _resolver = resolver;
        _audio = audio;
        _parser = parser;
        _persian = persian;
        _injector = injector;
        _hotkeys = hotkeys;
        _expander = expander;
        _settings = settings;

        _hotkeys.Triggered += OnHotkey;
        _hotkeys.Register(new HotkeyBinding
        {
            Name = "dictation",
            Gesture = settings.HotkeyGesture,
            Mode = settings.HotkeyMode == "Toggle" ? HotkeyMode.Toggle : HotkeyMode.PushToTalk,
        });
    }

    [RelayCommand]
    private async Task ToggleAsync()
    {
        if (IsListening)
        {
            await StopAsync();
        }
        else
        {
            await StartAsync();
        }
    }

    private async void OnHotkey(object? sender, HotkeyTriggeredEventArgs e)
    {
        try
        {
            if (e.Binding.Mode == HotkeyMode.PushToTalk)
            {
                await (e.IsPressed ? StartAsync() : StopAsync());
            }
            else if (IsListening)
            {
                await StopAsync();
            }
            else
            {
                await StartAsync();
            }
        }
        catch (Exception ex)
        {
            OnUi(() => LastError = ex.Message);
        }
    }

    private async Task StartAsync()
    {
        await EnsureOrchestratorAsync();
        await _orchestrator!.StartAsync();
        OnUi(() => IsListening = true);
    }

    private async Task StopAsync()
    {
        if (_orchestrator is not null)
        {
            await _orchestrator.StopAsync();
        }

        OnUi(() => IsListening = false);
    }

    private async Task EnsureOrchestratorAsync()
    {
        if (_orchestrator is not null)
        {
            return;
        }

        var engine = await _resolver.ResolveAsync();
        var options = new DictationOptions
        {
            Stt = new SttOptions
            {
                Language = _settings.Language,
                InitialPrompt = _settings.SmartCorrection ? PersianInitialPrompt.Default : null,
            },
            PersianText = new PersianTextOptions
            {
                NormalizeArabicLetters = true,
                ApplyZeroWidthNonJoiner = _settings.SmartCorrection,
                FixPunctuationSpacing = _settings.SmartCorrection,
                UsePersianDigits = _settings.UsePersianDigits,
            },
            Commands = new CommandOptions { CommandModeEnabled = _settings.CommandModeEnabled },
        };

        var orchestrator = new DictationOrchestrator(_audio, engine, _parser, _persian, _injector, options, _expander);
        orchestrator.StateChanged += (_, state) => OnUi(() => StateText = DictationStateText.ToPersian(state));
        orchestrator.TranscriptionProduced += (_, text) => OnUi(() => AddTranscription(text));
        _orchestrator = orchestrator;
    }

    private void AddTranscription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        RecentTranscriptions.Insert(0, text);
        while (RecentTranscriptions.Count > 20)
        {
            RecentTranscriptions.RemoveAt(RecentTranscriptions.Count - 1);
        }
    }

    private static void OnUi(Action action)
        => Application.Current?.Dispatcher.Invoke(action);
}
