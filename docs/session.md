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

## قدم بعدی

1. **دریافت تأیید مالک برای شروع M1** (ضبط صدا + VAD در `Pva.Audio`). طبق پروتکل،
   بدون تأیید وارد milestone بعد نمی‌شویم. زبان طراحی در M7 پیاده می‌شود.

## نکات باز / بلاکرها

- **نام کپی‌رایت:** روی «Amir Javadi» (بر اساس اکانت `amirjavadi`) تنظیم شد؛ در صورت
  نیاز اصلاح شود.
- **اجرای GUI:** صحت اسکلت با build تأیید شد؛ اجرای واقعی پنجره‌ی `Pva.App` روی دسکتاپ
  به‌صورت دستی قابل بررسی است (`dotnet run --project src/Pva.App`).
- **مدل‌های Whisper:** جدا از مخزن توزیع می‌شوند؛ استراتژی دانلود در اولین اجرا در M2.

</div>
