using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pva.Storage;

namespace Pva.App.ViewModels;

/// <summary>ViewModel پنجره‌ی تنظیمات. تغییرات را در settings.json ذخیره می‌کند.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _store;

    [ObservableProperty]
    private string _hotkeyGesture;

    [ObservableProperty]
    private bool _pushToTalk;

    [ObservableProperty]
    private bool _preferFasterWhisper;

    [ObservableProperty]
    private bool _useGpu;

    [ObservableProperty]
    private bool _smartCorrection;

    [ObservableProperty]
    private bool _usePersianDigits;

    [ObservableProperty]
    private bool _commandModeEnabled;

    [ObservableProperty]
    private double _micOpacity;

    [ObservableProperty]
    private bool _saved;

    public SettingsViewModel(ISettingsStore store, AppSettings current)
    {
        _store = store;
        _hotkeyGesture = current.HotkeyGesture;
        _pushToTalk = current.HotkeyMode != "Toggle";
        _preferFasterWhisper = current.PreferredEngine == "FasterWhisper";
        _useGpu = current.Device == "Gpu";
        _smartCorrection = current.SmartCorrection;
        _usePersianDigits = current.UsePersianDigits;
        _commandModeEnabled = current.CommandModeEnabled;
        _micOpacity = current.MicOpacity;
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new AppSettings
        {
            HotkeyGesture = string.IsNullOrWhiteSpace(HotkeyGesture) ? "Ctrl+Space" : HotkeyGesture.Trim(),
            HotkeyMode = PushToTalk ? "PushToTalk" : "Toggle",
            PreferredEngine = PreferFasterWhisper ? "FasterWhisper" : "WhisperCpp",
            Device = UseGpu ? "Gpu" : "Auto",
            SmartCorrection = SmartCorrection,
            UsePersianDigits = UsePersianDigits,
            CommandModeEnabled = CommandModeEnabled,
            MicOpacity = MicOpacity,
        };

        _store.Save(settings);
        Saved = true;
    }
}
