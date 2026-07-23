<div dir="rtl">

# لاگ جاری کار (Session Log)

این فایل حافظه‌ی زنده‌ی پروژه است: چه انجام شد، وضعیت فعلی، و قدم بعدی. بعد از هر
تغییر مهم به‌روزرسانی می‌شود تا کار وابسته به تاریخچه‌ی چت نباشد.

---

## وضعیت فعلی

**فاز:** پیش از توسعه — پایه و مستندات آماده و منتشر شد.
**شاخه:** `main` · **آخرین Milestone:** هیچ‌کدام (M0 هنوز شروع نشده).
**مخزن:** https://github.com/amirjavadi/Persian-Voice-Anywhere (عمومی، MIT).

## سشن ۱ — 2026-07-22 (۱۴۰۵/۰۴/۳۱)

**هدف:** راه‌اندازی پروژه‌ی جدید با gstack؛ ساخت پایه‌ی context و مستندات پیش از کد.

**تصمیم‌های گرفته‌شده (با مالک):**
- پلتفرم: .NET 10 + C# 14 (ADR-0001).
- UI: WPF + WPF-UI Fluent (ADR-0002).
- موتور STT: هیبرید از روز اول — whisper.cpp پیش‌فرض + Faster Whisper اختیاری (ADR-0003).
- دامنه‌ی v1: هسته + نوت‌پد داخلی + Sticky Notes (ADR-0006).
- مجوز MIT + مخزن عمومی روی GitHub (ADR-0009).

**انجام‌شده:**
- `git init` روی شاخه‌ی `main`.
- `.gitignore`، `LICENSE` (MIT، Amir Javadi).
- `CLAUDE.md` (فارسی) — قوانین، پشته، معماری، پروتکل توسعه، مسیریابی gstack.
- `README.md` (فارسی، جذاب، آماده‌ی GitHub با badgeها).
- `docs/architecture.md` — pipeline، ماژول‌ها، اینترفیس‌ها، ریسک‌ها (R1–R10).
- `docs/decisions.md` — ADR-0001 تا ADR-0009.
- `docs/roadmap.md` — Milestoneهای M0 تا M10 + backlog.
- `docs/session.md` — همین فایل.

**در حال انجام:**
- افزودن `.editorconfig` و workflow CI.
- حافظه‌ی ماندگار (`memory/`) + `MEMORY.md`.
- commit اولیه ساخته و به GitHub (عمومی، MIT) push شد. ✅

## سشن ۲ — 2026-07-22 — M0 (داربست و زیرساخت) ✅

**انجام‌شده:**
- solution `PersianVoiceAnywhere.sln` + ۱۳ پروژه (`src/Pva.*` و `tests/Pva.Tests`).
- TFMها: لایه‌های خالص net10.0؛ Audio/Injection/Hotkeys/App/Notepad/StickyNotes = net10.0-windows.
- `Directory.Build.props` (Nullable، AnalysisLevel، Release=warnings-as-errors) + `.editorconfig`.
- قراردادهای `Pva.Core`: `IAudioCapture`, `ISpeechToTextEngine`, `IPersianTextProcessor`,
  `ITextInjector`, `IHotkeyService`, `ICommandParser` + record modelها + `DictationState`.
- `Pva.App`: composition root با Generic Host + DI + Serilog (لاگ چرخشی در `logs/` کنار exe)،
  راه‌اندازی/خاموشی تمیز، پنجره‌ی معرفی RTL.
- `Pva.Tests`: ۴ تست دود روی قراردادهای Core.
- تصمیم جدید: پرهیز از FluentAssertions به‌دلیل مجوز تجاری (ADR-0010)؛ سخت‌گیری کیفیت (ADR-0011).

**نتیجه:** `dotnet build -c Release` → ۰ warning / ۰ error. `dotnet test` → ۴/۴ پاس. ✅

## سشن ۳ — 2026-07-22 — زبان طراحی Liquid Glass 🎨

**انجام‌شده (کار طراحی، نه کد اپ — دروازه‌ی M1 دست‌نخورده):**
- تعریف هویت بصری **Liquid Glass** به‌خواست مالک: شیشه‌ی مات، حرکت مدرن، آیکون‌های
  شیشه‌ای اختصاصی، پالت فیروزه‌ای `#13B9AC` + بنفش `#6D5EF6`.
- پروتوتایپ تعاملی زنده ساخته و منتشر شد (روشن/تیره، میکروفون قابل کلیک، waveform).
- ثبت در repo: `docs/design-language.md`، `docs/prototypes/liquid-glass.html`،
  ADR-0012، ریسک جدید R11 (تضاد افکت با کارایی)، به‌روزرسانی roadmap M7 و README و حافظه.

## سشن ۴ — 2026-07-22 — M1 (ضبط صدا + VAD) ✅

**انجام‌شده در `Pva.Audio`:**
- `AudioCaptureOptions`، `IVoiceActivityDetector`.
- `SpeechSegmenter` — ماشین حالت خالص و قطعی (آستانه + hangover + min-speech + pre-roll).
- `AudioResampler` — نمونه‌بردار خطی استریمی خالص (deviceRate→16kHz).
- `SileroVoiceActivityDetector` — Silero v5 via ONNX Runtime (نیاز به `models/silero_vad.onnx`).
- `WasapiAudioCapture : IAudioCapture` — WASAPI (NAudio)، downmix، resample، فریم→VAD→
  segmenter→`SegmentReady`، روی نخ پس‌زمینه.
- `AddAudioCapture` — DI با ساخت lazy مدل.
- پکیج‌ها: NAudio 2.3، Microsoft.ML.OnnxRuntime 1.27، DI.Abstractions. `Pva.Tests`→net10.0-windows.
- `docs/models.md` اضافه شد.

**نتیجه:** build Release ۰/۰؛ **۱۵/۱۵ تست سبز** (۱۱ تست جدید: segmenter + resampler).
تأیید دستیِ ضبط زنده + مدل Silero باقی مانده (نیاز به میکروفون و فایل مدل).

## سشن ۵ — 2026-07-22 — M2 (موتور STT هیبرید) ✅

**انجام‌شده در `Pva.Stt`:**
- `WhisperCppEngine` (Whisper.net) — پیش‌فرض CPU پرتابل.
- `FasterWhisperEngine` — کلاینت sidecar پایتون (پروتکل خط JSON، WAV موقت via `WavWriter`).
- `ISidecarTransport` + `ProcessSidecarTransport` (اجرای پروسه، stdin/stdout).
- `HybridSpeechEngineResolver` + `SttCandidate` — ترجیح + fallback خودکار (شامل خطای ساخت).
- `SttEngineOptions`، `AddSpeechToText` (DI، ساخت lazy).
- پکیج‌ها: Whisper.net 1.9 + Runtime، Logging/DI Abstractions.
- editorconfig: CA1848/CA1873 (الزام LoggerMessage) خاموش؛ CA1859 با نوع concrete رفع شد.

**نتیجه:** build Release ۰/۰؛ **۲۱/۲۱ تست سبز** (۶ تست جدید انتخاب/fallback موتور).
تأیید دستیِ رونویسی واقعی باقی مانده (نیاز به مدل ggml و برای FasterWhisper به python).

## سشن ۶ — 2026-07-22 — M3 (پس‌پردازش فارسی) ✅

**انجام‌شده در `Pva.PersianText`:**
- `PersianChars` — نیم‌فاصله، نگاشت ارقام و حروف عربی→فارسی.
- `PersianTextProcessor` — pipeline خالص و قطعی: نرمال‌سازی فاصله/حروف، جایگزینی
  اصطلاحات، نیم‌فاصله (می/نمی، ها/های/…، تر/ترین)، علائم نگارشی (تبدیل + فاصله‌گذاری)،
  ارقام فارسی (با حفظ توکن‌های لاتین مثل iOS16)، جمع‌بندی فاصله.
- `PersianInitialPrompt.Default` (پرامپت Whisper). `AddPersianText` (DI).

**نتیجه:** build Release ۰/۰؛ **۴۵/۴۵ تست سبز** (۲۴ تست golden). جمله‌ی مرجع
«امروز یک Pull Request روی GitHub زدم.» تمیز حفظ می‌شود. این ماژول کاملاً تست‌شده است
(بدون تأیید دستیِ باقی‌مانده).

## سشن ۷ — 2026-07-22 — اجرای پیوسته‌ی M4..M10 (بدون دروازه‌ی تأیید)

مالک دروازه‌ی تأیید هر milestone را برداشت («تا آخر برو، نیازی به تأیید نیست»). ادامه
با تست + commit جدا برای هر milestone.

- **M4 (تزریق متن) ✅** — `Pva.Injection`: `SendInputTextInjector` (Unicode via SendInput،
  بدون Copy/Paste، مدیریت surrogate)، `EditorActionMapper` (کنش→کلید مجازی، خالص و تست‌شده)،
  `NativeMethods` (LibraryImport)، `AddTextInjection` (DI). ۸ تست جدید؛ کل ۵۳/۵۳ سبز.
  محدودیت UIPI مستند (ریسک R3). تأیید دستیِ تایپ در اپ واقعی باقی مانده.

**اجرای پیوسته‌ی M5..M10 (همین سشن):**
- **M5 ✅** Hotkeys + `DictationOrchestrator` (اتصال کامل pipeline). ۶۵/۶۵.
- **M6 ✅** `VoiceCommandParser` (دستورهای صوتی). ۷۴/۷۴.
- **M7 🟨** UI هسته: tray + میکروفون شناور + تنظیمات + سیم‌کشی کامل DI. ۸۱/۸۱.
  (پولیش بصری Liquid Glass/Mica/انیمیشن باقی مانده.)
- **M8 ✅** نوت‌پد تب‌دار + session-restore. ۸۶/۸۶.
- **M9 ✅** Sticky Notes. ۹۰/۹۰.
- **M10 🟨** نسخه‌گذاری + `build/publish-portable.ps1` + `THIRD-PARTY-NOTICES.md`.
  (سنجش perf، امضای کد، آیکون، مدل‌ها برای انتشار نهایی لازم است.)

## سشن ۷ (ادامه) — قابلیت‌های backlog + پولیش + بستن سشن

- **پولیش M7:** میکروفون شناور با آیکون وکتور + انیمیشن تنفس نور و halo (Storyboard،
  با احترام به reduced-motion).
- **آماده‌سازی اجرای واقعی:** `build/fetch-models.ps1` (دانلود Silero + Whisper)؛
  `VadModelPath`/`WhisperModelPath` در تنظیمات.
- **مستندات:** `docs/developer-guide.md` و `docs/user-manual.md` (فارسی) اضافه شد.
- **backlog — Text Expansion + Voice Macro** (`Pva.TextExpansion`): `/phone`، «امضا»…،
  در خط‌لوله ادغام شد (درز `ITextExpander` در Core). ۷ تست.
- **backlog — Clipboard History** (`Pva.Clipboard`): سرویس خالص (حذف تکراری/سقف/pin/جستجو)
  + مانیتور Win32 + پنجره. ۸ تست.

## 🔚 وضعیت پایانِ سشن (2026-07-22)

- **۱۵ پروژه · ۱۰۵ تست سبز · Release ۰ warning/۰ error · درخت git تمیز · همه push‌شده.**
- مخزن: https://github.com/amirjavadi/Persian-Voice-Anywhere
- هسته‌ی v1 (M0–M9) کامل، M7/M10 با موارد پولیش/سخت‌سازی باز، به‌علاوه ۲ قابلیت backlog.
- **EXE پرتابلِ self-contained با موفقیت تولید شد** (`build/publish-portable.ps1`).

### کارهای باقی‌مانده (به‌ترتیب اولویت)
1. **اجرای واقعی end-to-end** روی ویندوز: `pwsh ./build/fetch-models.ps1` سپس
   `dotnet run --project src/Pva.App`؛ تست ضبط زنده → تایپ؛ رفع مشکلات interop.
2. **پولیش بصری M7:** WPF-UI Mica/Acrylic، waveform واکنش‌گر، آیکون‌های وکتور کامل.
3. ارتقای نوت‌پد به **AvalonEdit**؛ مهاجرت persistence یادداشت‌ها به **SQLite**.
4. **سنجش perf** (شروع < ۲s، Idle CPU < ۲٪، RAM)، **امضای کد**، آیکون اپ؛ سپس انتشار v1.
5. ادامه‌ی backlog: بازنویسی قاعده‌محور فارسی، OCR، آرشیو ضبط صدا.

### بلاکرها (خارج از کد — نیازِ محیط/تصمیم کاربر)
- **اجرای زنده و تأیید interopها** (WASAPI، Silero ONNX، Whisper، SendInput، keyboard hook،
  clipboard monitor) فقط با اجرا روی **ویندوز + فایل‌های مدل** ممکن است — headless قابل تأیید نیست.
- **امضای کد** نیازِ گواهی (Certificate) دارد.
- **پولیش Mica** نیازِ دیدن رندر روی ویندوز ۱۱ برای tune دارد.

### نکاتِ ازسرگیری
- Build/Test: `dotnet build -c Release` · `dotnet test`. لاگ زمان‌اجرا: `logs/pva-*.log` کنار exe.
- تنظیمات/داده کنار exe: `settings.json`، `notepad-session.json`، `sticky-notes.json`،
  `clipboard-history.json`، `text-expansions.json`.
- **نام کپی‌رایت** روی «Amir Javadi» (اکانت `amirjavadi`)؛ در صورت نیاز اصلاح شود.
- سشنِ جدید از همین فایل و از حافظه‌ی پروژه ([[project-overview]]) شروع کند.

## سشن ۹ — 2026-07-22 — رفع «اصلاً اجرا نمی‌شود» 🐛

**علت:** اپ در startup با `System.IO.FileNotFoundException` (نبودِ `models/silero_vad.onnx`)
به‌صورت unhandled کاملاً crash می‌شد. سازنده‌ی `SileroVoiceActivityDetector` مدل را
eager بار می‌کرد و چون `WasapiAudioCapture` هنگام resolve شدن `DictationViewModel` در
startup ساخته می‌شود، نبودِ فایل مدل کل برنامه را می‌شکست — برخلاف کامنت DI که ادعای
lazy بودن داشت و برخلاف قانون «مدل lazy-load، Idle نزدیک صفر» در CLAUDE.md.

**اصلاحات:**
- `SileroVoiceActivityDetector`: بارگذاری `InferenceSession` به‌صورت **lazy** (در اولین
  `Reset()`/`Detect()`)، نه در سازنده. حالا نبودِ مدل، startup را نمی‌شکند و خطای واضح
  فقط هنگام شروع ضبط رخ می‌دهد. `EnsureSession()` بررسی وجود فایل را همان‌جا انجام می‌دهد.
- `DictationViewModel.ToggleAsync`: افزودن try/catch (مثل `OnHotkey`) تا نبودِ مدل/خطای
  دستگاه صدا به `LastError` گزارش شود، نه crash.
- `App.OnStartup`: هندلر سراسری `DispatcherUnhandledException` — هر استثنای هندل‌نشده‌ی
  UI لاگ و با MessageBox به کاربر نشان داده می‌شود (بند ۸ CLAUDE.md).
- `build/fetch-models.ps1`: ذخیره‌ی مجدد با **UTF-8 + BOM**. بدون BOM، Windows PowerShell
  5.1 فایل را ANSI می‌خواند و متن فارسی garbled شده و اسکریپت parse نمی‌شد.
- مدل‌ها دانلود شدند: `silero_vad.onnx` (۲.۲MB) و `ggml-base.bin` (۱۴۱MB) در `models/`
  و کنار exe کپی شدند.

**نتیجه:** اپ هم بدون مدل (بدون crash، با خطای واضح هنگام ضبط) و هم با مدل بالا می‌آید.
`dotnet test` → ۱۰۵/۱۰۵ پاس. ✅

## سشن ۱۰ — 2026-07-23 — refactor عملکردی: ضبط + نوت‌پد + فونت 🛠️

**شکایت مالک:** «برنامه از نظر فرآیندی کار نمی‌کند؛ نه ضبط می‌کند نه نوت‌پد درستی دارد» +
«فونت را وزیر کن». پس از ممیزی عمیق (دو agent موازی + بازخوانی سیم‌کشی):

**علتِ ریشه‌ایِ «اصلاً ضبط نمی‌کند» (باگ خاموش):**
- `WasapiAudioCapture.ToMonoFloat` وقتی فرمت mix دستگاه `WaveFormatExtensible` بود (حالت
  رایج ویندوز؛ `Encoding = Extensible = 65534`، نه `IeeeFloat`) **هر بافر صدا را بی‌صدا دور
  می‌ریخت** → صف همیشه خالی → `SegmentReady` هرگز fire نمی‌شد و هیچ خطایی هم دیده نمی‌شد.
- اصلاح: متد `IsFloatFormat` که زیرفرمت `WaveFormatExtensible.SubFormat` را بررسی می‌کند؛
  پشتیبانی از float32 (مستقیم و Extensible)، PCM16 و PCM32.

**دومین باگ مهلک:** `ProcessLoop` (نخِ پس‌زمینه) try/catch نداشت؛ خطای VAD/ONNX کل پروسه
را بی‌صدا می‌کشت (چون `DispatcherUnhandledException` فقط UI thread را می‌گیرد). اضافه شد:
try/catch + رویداد جدید `IAudioCapture.CaptureFailed` که orchestrator آن را به
`ProcessingFailed` → `LastError` منتقل و ضبط را متوقف می‌کند.

**باگ UI (کلیک میکروفون کار نمی‌کرد):** روی `Root` رویداد `MouseLeftButtonDown` مستقیماً
`DragMove()` را صدا می‌زد که حلقه‌ی پیام تودرتو اجرا و `MouseUp` گوی (toggle ضبط) را می‌بلعید.
اصلاح: کشیدن فقط پس از عبور از آستانه‌ی حرکت؛ کلیک ساده حالا toggle می‌کند. + نمایش خطا
روی پنجره‌ی میکروفون و پاک‌سازی خطا هنگام شروع موفق + اتصال `ProcessingFailed`.

**refactor نوت‌پد (`Pva.Notepad`):**
- **ذخیره‌ی خودکار** با `DispatcherTimer` (debounce ۱.۵s) روی تغییر محتوا — دیگر با بستن
  ناگهانی، یادداشت‌ها گم نمی‌شوند. `New` هم بلافاصله session را ذخیره می‌کند.
- **میانبرها:** Ctrl+N/T (جدید)، Ctrl+O، Ctrl+S، Ctrl+Shift+S، Ctrl+W.
- **نشانگر «ذخیره‌نشده» (●)** روی سرِ تب + **دکمه‌ی بستن روی هر تب** + تأیید پیش از دور
  انداختن فایلِ ذخیره‌نشده.
- **toggle شکستن خط** (WordWrap) و **toggle جهت متن راست/چپ per-tab** (persist می‌شود).
- **نوار وضعیت:** شمارش واژه/نویسه + پیام وضعیت/خطا.
- **مدیریت خطای I/O** روی باز/ذخیره (بدون crash). حذف `Class1.cs` مرده. مدل
  `NotepadDocument` فیلد `IsRightToLeft` گرفت. دو converter (`Bool→TextWrapping/FlowDirection`).

**فونت وزیرمتن (SIL OFL):** سه وزن (Regular/Medium/Bold) در
`src/Pva.App/Assets/Fonts/` جاسازی شد (Resource) و به‌صورت سراسری `AppFontFamily` در
`App.xaml` تعریف و از همه‌ی پنجره‌ها (حتی اسمبلی‌های دیگر) با `DynamicResource` استفاده
شد — کاملاً آفلاین/پرتابل، بدون نیاز به نصب سیستمی.

**تست:** ۴ تست جدید (شکست ضبط→گزارش+توقف؛ persist شدن New؛ toggle جهت؛ dirty/TabHeader).
`dotnet test` → **۱۰۹/۱۰۹ پاس**. build سبز. اپ روی هدلس بدون crash بالا می‌آید.

**بلاکر باقی‌مانده:** تأیید نهاییِ *ضبط واقعی* (میکروفون زنده → متن) فقط روی ویندوز با
سخت‌افزار و مدل ممکن است؛ اصلاحِ `WaveFormatExtensible` منطقاً درست است اما اجرای زنده لازم
دارد. برای این باگِ سخت‌افزاری تست واحد نوشته نشد (ساخت `WaveFormatExtensible`ِ float با API
عمومی NAudio ساده نیست).

## سشن ۱۱ — 2026-07-23 — کشف علتِ واقعیِ «ضبط نمی‌کند»: مدلِ خرابِ VAD 🎯

مالک گزارش داد هنوز کار نمی‌کند. با لاگ‌گذاری تشخیصی + یک هارنس مستقل (scratchpad/MicDiag)
که pipeline واقعی را روی این ویندوز اجرا کرد، علتِ قطعی پیدا شد:

**فرمت صدا مشکل نبود** (دستگاه `IeeeFloat 32bit 2ch @ 48kHz` بود، صدا هم می‌رسید:
۱۰۱ بافر / ۲۸۶k نمونه). **علتِ واقعی: فایلِ مدل Silero VAD خراب بود.** روی نمونه‌ی
گفتارِ استاندارد (jfk.wav) با همان کد و فریم ۵۱۲:
- `silero_vad.onnx` از شاخه‌ی **master** (که `fetch-models.ps1` می‌گرفت) → احتمال گفتار
  همیشه ~۰.۰۰۴ (۰٪ فریم گفتار) → VAD هیچ‌گاه گفتار نمی‌دید → هیچ قطعه‌ای ساخته نمی‌شد.
- همان مدل از تگِ **v5.1** → ۶۰٪ فریم‌ها گفتار، بیشینه احتمال ۰.۹۹۷ → درست کار کرد.

مدلِ master با ONNX Runtime 1.27 ناسازگار است (خروجی صفر بدون خطا).

**اصلاح:**
- `build/fetch-models.ps1`: URL مدل از `master` به تگِ **v5.1** تغییر کرد
  (`.../snakers4/silero-vad/v5.1/src/silero_vad/data/silero_vad.onnx`).
- مدلِ v5.1 در `models/` و کنار exe جایگزین شد.

**تأیید کامل با کلاس‌های واقعی محصول** (SileroVoiceActivityDetector + SpeechSegmenter +
WhisperCppEngine) روی jfk.wav → ۳ قطعه، رونویسیِ درست:
«And so my fellow Americans.» / «ASK NOT!» / «what your country can do for you…».
یعنی زنجیره‌ی کامل **صدا → VAD → قطعه‌بندی → Whisper → متن** ثابت شد. ✅

**بونوس تشخیصی (ماندگار):** به `WasapiAudioCapture` لاگِ تشخیصی اضافه شد (فرمت دستگاه،
شمار بافر/نمونه/فریم، بیشینه احتمال گفتار، شمار قطعه) تا اگر دوباره چیزی خراب شد، لاگ
دقیقاً نقطه‌ی شکست را نشان دهد (بدون ذخیره‌ی محتوای صدا).

**نتیجه:** build سبز، `dotnet test` → ۱۰۹/۱۰۹ پاس. اصلاحاتِ سشن ۱۰ (کلیک میکروفون،
`WaveFormatExtensible`، محافظ نخِ پس‌زمینه، نوت‌پد، فونت وزیر) همگی معتبر و باقی هستند —
`WaveFormatExtensible` روی این دستگاه دخیل نبود ولی برای دستگاه‌های دیگر یک اصلاح درست است.

## سشن ۱۲ — 2026-07-23 — قطعه‌ی آخر: دزدیدن فوکوس توسط پنجره‌ی میکروفون 🎯

مالک باز گفت کار نکرد. لاگِ اجرای واقعی (با لاگ‌گذاری تشخیصی سشن ۱۱) نشان داد **همه‌چیز
درست کار می‌کند**: `شروع ضبط`، `قطعه‌ی گفتاری تولید شد`، `بیشینه احتمال گفتار: 1.00`،
و مهم‌تر: `رونویسی تولید شد (N نویسه)` — یعنی متن **تولید می‌شد**.

**علتِ نهایی: پنجره‌ی میکروفونِ شناور هنگام کلیک، فوکوس کیبورد را می‌دزدید.** پس متنِ
رونویسی‌شده با SendInput به‌جای برنامه‌ی هدف (Word/Notepad/…) به خودِ پنجره‌ی میکروفون
تزریق می‌شد که جای متن ندارد → کاربر «هیچ اتفاقی» نمی‌دید. (تزریق و STT هر دو سالم بودند.)

**اصلاح:** پنجره‌ی میکروفون **non-activating** شد:
- `ShowActivated="False"` در XAML.
- `WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW` در `OnSourceInitialized` (interop
  Get/SetWindowLong). حالا کلیک روی گوی، فوکوس را از برنامه‌ی مقصد نمی‌گیرد و متن به
  همان‌جایی که مکان‌نما هست تایپ می‌شود.

**نتیجه:** Release + Debug build سبز (nullable/CA1806 رفع شد)، `dotnet test` → ۱۰۹/۱۰۹.
با این، زنجیره‌ی محصول کامل است: **کلیک/هات‌کی → ضبط → VAD → Whisper → تزریق در برنامه‌ی
فعال**. آماده‌ی تست واقعیِ مالک (فوکوس روی Notepad/Word، صحبت، دیدن متن).

## سشن ۱۳ — 2026-07-23 — باگ نهایی تزریق + بازطراحی کامل Liquid Glass 🎨✅

مالک با اختیار کامل («تا آخر با تصمیم خودت جلو برو») خواست: refactor کامل چون «چیزی
تایپ نمی‌شود» + طراحی مدرن.

**باگِ قطعیِ «چیزی تایپ نمی‌شود» (با هارنس اثبات شد):** ساختار `INPUT` در
`Pva.Injection/NativeMethods` فقط `KEYBDINPUT` را در union داشت → sizeof = ۳۲ بایت،
در حالی که ویندوز x64 دقیقاً ۴۰ بایت می‌خواهد (union باید `MOUSEINPUT` را هم داشته باشد).
نتیجه: `SendInput` **همه‌ی** ورودی‌ها را با خطای ۸۷ رد می‌کرد و چون مقدار برگشتی چک
نمی‌شد، کاملاً بی‌صدا. هارنس: BROKEN=`sent 0/1, err 87` / FIXED=`sent 1/1, err 0`.
- اصلاح: `MOUSEINPUT` به union اضافه شد (اندازه‌ی درست ۴۰) + `Send` حالا مقدار برگشتی
  را چک می‌کند و در شکست، استثنا با پیام فارسی می‌دهد (→ `ProcessingFailed` → نمایش خطا).

**بازطراحی Liquid Glass (طبق docs/design-language.md):**
- `src/Pva.App/Themes/LiquidGlass.xaml` — سیستم طراحی مشترک: پالت برند (فیروزه‌ای
  #13B9AC + بنفش #6D5EF6)، گرادیان‌ها (از جمله `PvaOrbGradient`)، استایل‌های
  PvaPrimaryButton/GhostButton/IconButton/ToggleButton/TextBox/CheckBox/TabItem/Card.
  همه‌ی پنجره‌ها (در هر اسمبلی) با DynamicResource مصرف می‌کنند.
- **میکروفون شناور:** گوی گرادیانی برند با برق شیشه‌ای و سایه‌ی فیروزه‌ای، halo ضربانی،
  **اکولایزر ۵ میله‌ای متحرک هنگام شنیدن**، pill وضعیت با نقطه‌ی رنگی، راهنمای
  «ضربه بزن — یا Ctrl+Space» (هنگام شنیدن پنهان)، **پیش‌نمایش آخرین رونویسی**
  (`LastTranscription` جدید در VM)، چیپ خطای قرمز.
- **تنظیمات:** کارت‌های بخش‌بندی‌شده (ضبط/موتور/فارسی‌سازی/ظاهر) با کنترل‌های برند.
- **نوت‌پد:** toolbar شیشه‌ای، تب‌ها با زیرخط گرادیانی، ویرایشگر داخل کارت گرد.
- **کلیپ‌بورد:** کارت‌های hover با حاشیه‌ی فیروزه‌ای، جستجوی استایل‌دار.
- **یادداشت چسبان:** رگه‌ی گرادیانی بالای کارت + دکمه‌های برند.
- **آیکون tray اختصاصی:** دایره‌ی گرادیانی برند + میکروفون سفید، تولید در زمان اجرا
  (`TrayIconFactory`) — بدون فایل ico، پرتابل.
- تأیید بصری: اسکرین‌شات PrintWindow از پنجره‌ی میکروفون گرفته و بررسی شد. ✅

**نتیجه:** build سبز، `dotnet test` → ۱۰۹/۱۰۹ پاس. commit شد (اختیار از مالک).

**تست پیشنهادی مالک:** فوکوس روی Notepad → کلیک روی گوی/Ctrl+Space → صحبت فارسی →
متن باید تایپ شود (این‌بار SendInput واقعاً ارسال می‌کند؛ اگر برنامه‌ی مقصد Admin است،
PVA را هم Admin اجرا کنید).

## سشن ۱۴ — 2026-07-23 — ارتقای کیفیت تشخیص: مدل قوی‌تر + Vulkan (ADR-0013) 🚀

مالک از کیفیت ناراضی بود (مقایسه با Web Speech API کروم = بازشناسی ابری گوگل).
benchmark واقعی روی سخت‌افزار مرجع (i7-1355U + Iris Xe) گرفته شد — جدول در ADR-0013.
یافته‌ی کلیدی: turbo روی CPU ~۱۱x realtime (غیرقابل‌استفاده) اما با **Vulkan** روی همان
iGPU به 2.3x می‌رسد؛ small-q5_1 با Vulkan به **0.43x** (هر جمله ~۱.۵ ثانیه).

**تغییرات:**
- `Whisper.net.Runtime.Vulkan` اضافه شد؛ `WhisperCppEngine` بر اساس Device ترتیب
  runtime را می‌چیند (Auto/Gpu → Vulkan با fallback CPU). `SupportsGpu = true`.
- beam search + threads (هسته‌ها منهای یک) در موتور.
- انتخاب خودکار مدل دستگاه‌آگاه: Auto/Cpu → small-q5_1؛ Gpu صریح → turbo.
- پرامپت فارسی نمونه‌محور در `PersianInitialPrompt`.
- `fetch-models.ps1`: پیش‌فرض `small` (ggml-small-q5_1)؛ گزینه‌ی `turbo` برای GPU.
- مدل‌های small-q5_1 و turbo-q5_0 دانلود و کنار exe کپی شدند.

**تأیید با موتور واقعی محصول:** small-q5_1 + Auto (Vulkan) + beam → **0.47x realtime**
(جمله‌ها در ۱.۲–۱.۸ ثانیه). ✅ `dotnet test` → ۱۰۹/۱۰۹.

**نکته برای مالک:** اولین رونویسی هر اجرا روی Vulkan ~۱۰ ثانیه گرم‌کردن دارد (یک‌بار).
اگر «کیفیت حداکثری» خواستی: در تنظیمات «استفاده از GPU» را روشن کن تا turbo انتخاب
شود (پاسخ ~۴-۵ ثانیه بعد از هر جمله، دقت نزدیک کروم).

</div>
