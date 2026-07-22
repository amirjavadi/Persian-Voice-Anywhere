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

## وضعیت نهایی v1

هسته‌ی محصول به‌صورت end-to-end سیم‌کشی شده و کل solution با ۹۰ تست سبز build می‌شود
(Release، ۰ warning). آنچه برای **اجرای واقعی** لازم است (تأیید دستی):
- قرار دادن مدل‌ها در `models/` (`silero_vad.onnx`، `ggml-base.bin` — `docs/models.md`).
- اجرای برنامه روی ویندوز و تست ضبط زنده → تایپ در اپ‌های واقعی.
- سنجش perf و امضای کد پیش از انتشار عمومی.

## قدم‌های بعدی (پیشنهادی)

1. تأمین مدل‌ها و اجرای دستی end-to-end؛ رفع مشکلات احتمالی interop (WASAPI/Silero/SendInput/hook).
2. پولیش بصری M7 (WPF-UI Mica + انیمیشن‌های spring + آیکون‌های وکتور).
3. ارتقای نوت‌پد به AvalonEdit؛ مهاجرت persistence به SQLite.
4. سنجش perf + امضای کد + آیکون اپ؛ سپس انتشار v1.
5. شروع backlog (OCR، Clipboard History، Voice Macro، بازنویسی هوشمند…).

## نکات باز / بلاکرها

- **نام کپی‌رایت:** روی «Amir Javadi» (بر اساس اکانت `amirjavadi`) تنظیم شد؛ در صورت
  نیاز اصلاح شود.
- **اجرای GUI:** صحت اسکلت با build تأیید شد؛ اجرای واقعی پنجره‌ی `Pva.App` روی دسکتاپ
  به‌صورت دستی قابل بررسی است (`dotnet run --project src/Pva.App`).
- **مدل‌های Whisper:** جدا از مخزن توزیع می‌شوند؛ استراتژی دانلود در اولین اجرا در M2.

</div>
