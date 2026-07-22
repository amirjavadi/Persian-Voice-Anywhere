# Third-Party Notices — Persian Voice Anywhere

This product bundles or depends on the following third-party components. Each is
distributed under its own license; all are compatible with commercial use.

| Component | Purpose | License |
|-----------|---------|---------|
| [Whisper.net](https://github.com/sandrohanea/whisper.net) | whisper.cpp bindings for .NET | MIT |
| [whisper.cpp](https://github.com/ggerganov/whisper.cpp) | native speech recognition | MIT |
| [Silero VAD](https://github.com/snakers4/silero-vad) | voice-activity detection model | MIT |
| [ONNX Runtime](https://github.com/microsoft/onnxruntime) | ML inference runtime | MIT |
| [CTranslate2](https://github.com/OpenNMT/CTranslate2) (Faster Whisper engine pack) | inference engine | MIT |
| [NAudio](https://github.com/naudio/NAudio) | audio capture (WASAPI) | MIT |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | MVVM | MIT |
| [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) | system tray icon | CPOL / MIT-compatible |
| [Serilog](https://github.com/serilog/serilog) | logging | Apache-2.0 |
| .NET 10 runtime & WPF | platform | MIT |

Whisper model weights (ggml / CTranslate2) and the Silero VAD model are distributed
separately and are not part of this repository. See `docs/models.md`.

> Before each public release, verify versions and licenses of the actually shipped
> binaries and update this file accordingly.
