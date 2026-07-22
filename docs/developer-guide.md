<div dir="rtl">

# راهنمای توسعه‌دهنده — Persian Voice Anywhere

این سند برای توسعه‌دهندگانی است که می‌خواهند کد را بسازند، بفهمند و گسترش دهند.
قوانین کامل در [CLAUDE.md](../CLAUDE.md) و طراحی در
[architecture.md](architecture.md) است.

## پیش‌نیازها

- **.NET 10 SDK** (نسخه‌ی ۱۰.۰.۲۰۲ یا بالاتر).
- ویندوز ۱۰/۱۱ (اپ به Win32/WPF وابسته است).
- برای اجرای واقعی: مدل‌ها در `models/` (به [models.md](models.md)).

## ساخت، تست، اجرا

```bash
dotnet build                       # ساخت کل solution
dotnet test                        # اجرای همه‌ی تست‌ها
dotnet build -c Release            # ساخت سخت‌گیرانه (warnings-as-errors)
dotnet run --project src/Pva.App   # اجرای اپ
pwsh ./build/publish-portable.ps1  # انتشار نسخه‌ی پرتابل (ZIP)
```

## ساختار پروژه

```
src/
  Pva.Core         مدل‌ها، اینترفیس‌های همه‌ی درزها، DictationOrchestrator (مستقل از UI)
  Pva.Audio        ضبط WASAPI + VAD (Silero) + segmenter + resampler
  Pva.Stt          موتور هیبرید STT (whisper.cpp + Faster Whisper) + resolver
  Pva.PersianText  پس‌پردازش فارسی (خالص، تست‌محور)
  Pva.Commands     تفسیر دستورهای صوتی
  Pva.Injection    تزریق متن با SendInput
  Pva.Hotkeys      کلید میانبر سراسری (low-level hook)
  Pva.Storage      تنظیمات JSON کنار exe
  Pva.Notepad      نوت‌پد تب‌دار + session
  Pva.StickyNotes  یادداشت‌های چسبان
  Pva.Plugins      SDK افزونه (اسکلت)
  Pva.App          میزبان WPF: composition root، tray، میکروفون شناور، تنظیمات
tests/
  Pva.Tests        xUnit (۹۰ تست)
```

جهت وابستگی همیشه به‌سمت `Pva.Core` است؛ Core به هیچ ماژول دیگری وابسته نیست.

## خط‌لوله‌ی دیکته

`DictationOrchestrator` (در Core) اجزا را به هم می‌بندد:

```
Hotkey → IAudioCapture.SegmentReady → ISpeechToTextEngine.TranscribeAsync
       → ICommandParser.Parse → IPersianTextProcessor.Process → ITextInjector
```

هر درز یک اینترفیس در Core دارد و در ماژول خودش پیاده و در `Pva.App` با DI ثبت
می‌شود. برای تست، پیاده‌سازی‌های جعلی جای واقعی‌ها را می‌گیرند (نمونه:
`DictationOrchestratorTests`).

## قراردادهای تست

- منطق **خالص و پرریسک** (segmenter، resampler، resolver، پس‌پردازش فارسی، parser،
  نگاشت کنش، gesture) کاملاً unit-test می‌شود.
- اجزای وابسته به سخت‌افزار/مدل (WASAPI، ONNX، Whisper، SendInput، hook) پشت
  اینترفیس‌اند و **تأیید دستی** روی ویندوز نیاز دارند.
- نام تست: `Method_Scenario_Expectation`. از `Assert` خالص xUnit استفاده کن
  (FluentAssertions به‌دلیل مجوز ممنوع — ADR-0010).

## استانداردهای کد

C# 14، nullable فعال، Release = warnings-as-errors، اعمال `.editorconfig`.
Async تا انتها روی I/O؛ `CancellationToken` روی عملیات طولانی. Logging با Serilog
بدون ثبت صدا/متن کاربر. جزئیات در [CLAUDE.md](../CLAUDE.md) §۸.

## افزودن یک ماژول/قابلیت

۱. اینترفیس درز را (در صورت نیاز) در `Pva.Core` تعریف کن.
۲. پیاده‌سازی را در ماژول جداگانه بنویس؛ منطق خالص را جدا و تست‌پذیر نگه دار.
۳. یک `Add<Feature>` extension برای DI فراهم کن.
۴. تست بنویس؛ `dotnet build -c Release` و `dotnet test` سبز باشد.
۵. مستندات (`architecture.md`، `roadmap.md`، `session.md`) و در صورت لزوم ADR را
   به‌روزرسانی کن.

## نقاط توسعه (افزونه)

`Pva.Plugins` اسکلت SDK افزونه است. در نسخه‌های بعدی: نقاط مشارکت برای دستورهای
صوتی، post-processorها و providerهای بازنویسی، با بارگذاری از پوشه‌ی `plugins/`.

## عیب‌یابی

- **تایپ در اپ elevated کار نمی‌کند:** محدودیت UIPI ویندوز؛ برنامه را با دسترسی
  Administrator اجرا کن (ریسک R3).
- **آنتی‌ویروس هشدار می‌دهد:** به‌خاطر keyboard hook + SendInput؛ برای انتشار،
  Code Signing لازم است (R4).
- **مدل یافت نشد:** فایل‌های `models/` را طبق [models.md](models.md) قرار بده.

</div>
