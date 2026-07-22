<div dir="rtl">

# نقشه‌ی راه — Persian Voice Anywhere

کار به Milestoneهای کوچک و قابل‌تحویل شکسته شده است. **هر Milestone پس از اتمام
تست می‌شود و بدون تأیید مالک، وارد Milestone بعد نمی‌شویم** (طبق `CLAUDE.md` §۵).

وضعیت‌ها: ⬜ شروع‌نشده · 🟨 در حال انجام · ✅ کامل

---

## دامنه‌ی v1 (قابل‌فروش)

هسته‌ی دیکته + نوت‌پد داخلی + Sticky Notes. مطابق ADR-0006.

| Milestone | عنوان | خروجی | وضعیت |
|-----------|-------|-------|-------|
| M0 | داربست و زیرساخت | solution، DI، logging، CI، تست نمونه | ✅ |
| M1 | ضبط صدا + VAD | `Pva.Audio` با WASAPI و Silero VAD | ✅ |
| M2 | موتور STT هیبرید | whisper.cpp کارکردی + adapter Faster Whisper | ✅ |
| M3 | پس‌پردازش فارسی | `Pva.PersianText` + تست‌های golden | ✅ |
| M4 | تزریق متن | `Pva.Injection` با SendInput در اپ واقعی | ✅ |
| M5 | Hotkey + orchestrator | pipeline کامل صحبت→تایپ (اولین دموی واقعی) | ✅ |
| M6 | دستورات صوتی | «خط بعد»، «ویرگول»… به‌صورت کنش | ✅ |
| M7 | UI: Tray + میکروفون شناور + تنظیمات | تجربه‌ی کاربری کامل هسته | 🟨 |
| M8 | نوت‌پد داخلی | ویرایشگر تب‌دار + session-restore | ✅ |
| M9 | Sticky Notes | یادداشت‌های پین‌شونده | ⬜ |
| M10 | بسته‌بندی پرتابل + سخت‌سازی | ZIP/EXE، perf، پایداری، انتشار v1 | ⬜ |

---

## جزئیات Milestoneها

### M0 — داربست و زیرساخت ✅
- ✅ solution و ۱۳ پروژه طبق نقشه‌ی `architecture.md` §۴ (`src/` و `tests/`).
- ✅ DI (Microsoft.Extensions.Hosting) + Serilog (فایل چرخشی کنار exe) در `Pva.App`.
- ✅ `.editorconfig`، `Directory.Build.props` (Release = warnings-as-errors).
- ✅ قراردادهای `Pva.Core` (اینترفیس‌های همه‌ی درزها + record modelها).
- ✅ `Pva.Tests` با ۴ تست دود؛ **`dotnet test` سبز (۴/۴)**.
- ✅ CI (GitHub Actions): build + test روی هر push/PR.
- **نتیجه‌ی تست:** Build حالت Release با ۰ warning/۰ error؛ ۴ تست پاس.
- **دروازه:** ✅ انجام شد — منتظر تأیید مالک برای M1.

### M1 — ضبط صدا + VAD ✅
- ✅ `WasapiAudioCapture : IAudioCapture` با WASAPI (NAudio)، downmix→mono، resample→16kHz.
- ✅ `SpeechSegmenter` (ماشین حالت خالص: آستانه + hangover + min-speech + pre-roll).
- ✅ `AudioResampler` (خطی، استریمی، خالص).
- ✅ `SileroVoiceActivityDetector` (Silero v5 via ONNX Runtime).
- ✅ `AddAudioCapture` (DI؛ ساخت lazy مدل تا نبودِ فایل، اجرا را نشکند).
- ✅ **تست:** ۱۱ unit جدید (۶ segmenter + ۵ resampler)؛ کل ۱۵/۱۵ سبز، build Release ۰/۰.
- **تأیید دستی باقی‌مانده:** ضبط زنده‌ی میکروفون + مدل Silero (نیاز به `models/silero_vad.onnx`
  و دستگاه صوتی) — در اجرای واقعی بررسی می‌شود. راهنما: `docs/models.md`.

### M2 — موتور STT هیبرید ✅
- ✅ `WhisperCppEngine` (Whisper.net) روی CPU — پیش‌فرض پرتابل.
- ✅ `FasterWhisperEngine` (sidecar پایتون + IPC خط JSON، WAV موقت) پشت همان اینترفیس.
- ✅ `HybridSpeechEngineResolver` — ترجیح + fallback خودکار به whisper.cpp (حتی اگر
  engine pack نصب نباشد یا سازنده استثنا بدهد).
- ✅ `ISidecarTransport` + `ProcessSidecarTransport`؛ `WavWriter`؛ `AddSpeechToText` (DI، lazy).
- ✅ **تست:** ۶ تست انتخاب/fallback با موتور جعلی؛ کل ۲۱/۲۱ سبز، build Release ۰/۰.
- **تأیید دستی باقی‌مانده:** رونویسی واقعی نیاز به مدل ggml (whisper.cpp) و برای Faster
  Whisper به python + مدل CTranslate2 دارد. راهنما: `docs/models.md`.

### M3 — پس‌پردازش فارسی ✅
- ✅ `PersianTextProcessor : IPersianTextProcessor` — pipeline قوانین: نرمال‌سازی فاصله/حروف
  (ي/ك/ة→ی/ک/ه، حذف حرکات/تطویل)، جایگزینی اصطلاحات، **نیم‌فاصله (می/نمی، ها/های، تر/ترین)**،
  علائم نگارشی (تبدیل لاتین→فارسی + فاصله‌گذاری)، ارقام فارسی (با حفظ توکن‌های لاتین)،
  جمع‌بندی فاصله.
- ✅ `PersianInitialPrompt.Default` (پرامپت فارسی+فنی برای Whisper).
- ✅ `AddPersianText` (DI با دیکشنری اصطلاحات اختیاری). ماژول خالص و قطعی.
- ✅ **تست:** ۲۴ تست golden شامل «امروز یک Pull Request روی GitHub زدم.»؛ کل ۴۵/۴۵ سبز، build Release ۰/۰.

### M4 — تزریق متن ✅
- ✅ `SendInputTextInjector : ITextInjector` با SendInput (Unicode، surrogate-safe).
- ✅ `EditorActionMapper` (کنش→کلید مجازی، خالص و تست‌شده)؛ `NativeMethods` (LibraryImport).
- ✅ `AddTextInjection` (DI). محدودیت UIPI مستند (R3).
- ✅ **تست:** ۸ تست نگاشت کنش. تأیید دستیِ تایپ در اپ واقعی باقی مانده.

### M5 — Hotkey + Orchestrator ✅
- ✅ `HotkeyGesture.Parse` (خالص و تست‌شده)؛ `LowLevelKeyboardHook` (WH_KEYBOARD_LL،
  نخ اختصاصی + message loop)؛ `GlobalHotkeyService : IHotkeyService` (Push-to-Talk،
  Toggle، Combo، Single-Key، Double-Ctrl)؛ `AddHotkeys` (DI).
- ✅ `DictationOrchestrator` (در Core): اتصال کامل ضبط→STT→دستور→فارسی→تزریق، وضعیت
  و رویدادها، پردازش سریالی قطعه‌ها.
- ✅ **تست:** ۸ تست gesture + ۴ تست orchestrator (با fake + پس‌پردازش فارسی واقعی)؛
  کل ۶۵/۶۵ سبز. تأیید دستیِ hook زنده باقی مانده.

### M6 — دستورات صوتی ✅
- ✅ `VoiceCommandParser : ICommandParser` — تطبیق عبارت (تا ۳ کلمه، طولانی‌ترین ابتدا)؛
  کنش‌ها (خط بعد/پاراگراف/حذف کلمه/undo/redo) متن را می‌شکنند، علائم (ویرگول/نقطه/پرانتز…)
  درون متن درج می‌شوند؛ `CommandModeEnabled` برای خاموش‌کردن تفسیر (R6). `AddVoiceCommands` (DI).
- ✅ **تست:** ۹ تست (چندکلمه‌ای، علائم، پرانتز تمیز، حالت خاموش، نرمال‌سازی عربی)؛ کل ۷۴/۷۴ سبز.

### M7 — UI هسته 🟨 (کارکرد کامل؛ پولیش بصری باقی)
- ✅ `DictationViewModel` (چرخه‌ی حیات + کلید میانبر + وضعیت + تاریخچه، resolve موتور lazy).
- ✅ `SettingsViewModel` + پنجره‌ی تنظیمات (کلید میانبر، موتور، GPU، اصلاح فارسی، شفافیت).
- ✅ `FloatingMicWindow` — بدون‌قاب، Topmost، draggable، شفافیت، سطح شیشه‌ی مونوکروم.
- ✅ System Tray (Hardcodet) با منوی شروع/توقف/نمایش/تنظیمات/خروج؛ `ShutdownMode=OnExplicitShutdown`.
- ✅ `App.xaml.cs` کل خط‌لوله را در DI می‌بندد (Settings/Persian/Commands/Injection/Hotkeys/
  Audio/Stt + orchestrator). `DictationStateText` (خالص، تست‌شده).
- ✅ **تست:** ۳ تست settings round-trip + ۴ تست state-text؛ کل ۸۱/۸۱ سبز، build Release ۰/۰.
- **باقی‌مانده (پولیش بصری، بعد از سیم‌کشی هسته):** جایگزینی گلَس دست‌ساز با WPF-UI
  Mica/Acrylic، انیمیشن‌های spring (تنفس/waveform)، آیکون‌های وکتور اختصاصی، تم Light/Dark
  پویا. تأیید بصری دستی. — طبق `docs/design-language.md` و پروتوتایپ.

### M8 — نوت‌پد داخلی ⬜
- ویرایشگر تب‌دار (AvalonEdit): جستجو/جایگزینی، Word Wrap، RTL، Undo/Redo،
  Session Restore، Dark/Light، ذخیره‌ی خودکار، Drag & Drop.
- **تست:** تست ذخیره/بازیابی session و repositoryها.

### M9 — Sticky Notes ⬜
- یادداشت‌های کوچک topmost و pin‌شونده روی دسکتاپ؛ Markdown؛ ذخیره‌ی خودکار (SQLite).
- **تست:** persistence و رفتار pin.

### M10 — بسته‌بندی و سخت‌سازی ⬜
- انتشار ZIP پرتابل + EXE تک‌فایل؛ تنظیمات کنار exe.
- perf (شروع < ۲ ثانیه، Idle CPU < ۲٪، RAM)، تست نشت حافظه، پایداری اجرای طولانی.
- امضای کد (در صورت فراهم‌بودن گواهی)، NOTICE کامپوننت‌ها.
- **خروجی:** انتشار v1.

---

## پس از v1 (Backlog)

ویرایش هوشمند متن · تاریخچه‌ی کلیپ‌بورد (جستجو/دسته‌بندی) · OCR (اسکرین‌شات → متن
فارسی) · اسکرین‌شات و حاشیه‌نویسی (هایلایت/رسم/Blur/Export) · آرشیو ضبط صدا
(جستجو/پخش/خروجی) · Voice Macro · Text Expansion (مثل `/phone`) · بازنویسی هوشمند
(رسمی/دوستانه/کوتاه/بلند، آفلاین‌محور و سپس LLM opt-in) · marketplace افزونه‌ها ·
MSIX + به‌روزرسانی خودکار · مستندسازی خودکار (Developer Guide، API Docs، User Manual).

</div>
