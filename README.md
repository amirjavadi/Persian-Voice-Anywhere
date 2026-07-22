<div align="center">

# 🎙️ Persian Voice Anywhere

### در هر نرم‌افزار ویندوز فقط **صحبت کن** — متن دقیق **فارسی** همان‌جا تایپ می‌شود

کاملاً **آفلاین** · **پرتابل** · بدون نصب · بدون Copy/Paste

<br/>

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D6?logo=windows&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10%20(LTS)-512BD4?logo=dotnet&logoColor=white)
![UI](https://img.shields.io/badge/UI-WPF%20%2B%20Fluent-2D7D9A)
![Whisper](https://img.shields.io/badge/STT-Whisper%20(offline)-00A67E)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![Status](https://img.shields.io/badge/Status-Foundation%20Ready-orange)
![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)

<br/>

[امکانات](#-قابلیت‌های-نسخه‌ی-اول-v1) · [پشته‌ی فنی](#-پشته‌ی-فنی) · [ساخت و اجرا](#-ساخت-و-اجرا) · [نقشه‌ی راه](docs/roadmap.md) · [معماری](docs/architecture.md)

</div>

---

<div dir="rtl">

## ✨ در یک نگاه

**Persian Voice Anywhere** یک نرم‌افزار دیکته‌ی فارسیِ ویندوز است: آفلاین، پرتابل و
متن‌باز. در Chrome، Word، VS Code، Telegram، Notepad یا هر برنامه‌ای که تایپ دارد،
فقط صحبت کنید و متن دقیق فارسی در محل مکان‌نما نوشته می‌شود — دقیقاً مثل یک کیبورد
واقعی، بدون Copy/Paste و بدون اینترنت.

> 💬 «امروز یک Pull Request روی GitHub زدم.» ← دقیقاً همین‌طور تمیز تایپ می‌شود؛
> نیم‌فاصله، علائم نگارشی و ترکیب فارسی/انگلیسی حفظ می‌شود.

</div>

<div align="center">

| | |
|:--|:--|
| 🌐 **همه‌جا کار می‌کند** | تزریق Unicode با `SendInput` در هر اپ فوکوس‌دار |
| 🔒 **کاملاً آفلاین** | Whisper محلی؛ صدا هرگز به اینترنت نمی‌رود |
| 🎯 **کیفیت فارسی** | نیم‌فاصله، علائم، اعداد فارسی، ترکیب fa/en |
| 📦 **پرتابل** | اجرا از ZIP، بدون نصب، بدون رجیستری |
| ⚡ **سبک و سریع** | شروع < ۲ ثانیه، CPU در Idle < ۲٪ |
| 🔌 **قابل توسعه** | معماری ماژولار + افزونه (Plugin) |

</div>

<div dir="rtl">

## 🚀 قابلیت‌های نسخه‌ی اول (v1)

- 🌐 **دیکته در همه‌جا** — تزریق متن Unicode با `SendInput` ویندوز.
- 🧠 **Whisper محلی (هیبرید)** — whisper.cpp پیش‌فرض پرتابل + Faster Whisper
  اختیاری؛ در صورت وجود GPU از آن، وگرنه CPU.
- 🎯 **کیفیت متن فارسی** — پس‌پردازش اختصاصی: نیم‌فاصله (ZWNJ)، علائم نگارشی، اعداد
  فارسی، ترکیب فارسی/انگلیسی، حفظ اصطلاحات فنی.
- ⌨️ **کلیدهای میانبر قابل‌تنظیم** — Push-to-Talk، Toggle، Double-Ctrl، Caps Lock، دلخواه.
- 🗣️ **دستورات صوتی** — «خط بعد»، «پاراگراف جدید»، «ویرگول»، «حذف کلمه قبل»،
  Undo/Redo و… به‌صورت کنش، نه اینکه تایپ شوند.
- 🎈 **میکروفون شناور** — قابل جابجایی، Always-On-Top، شفافیت قابل‌تنظیم، قابل مخفی‌شدن.
- 🔔 **System Tray** — منوی سریع: شروع/توقف ضبط، تنظیمات، خروج.
- 📝 **نوت‌پد داخلی** — تب‌دار، ذخیره‌ی خودکار، Dark/Light، جستجو و جایگزینی، RTL،
  Word Wrap، Session Restore.
- 📌 **Sticky Notes** — یادداشت‌های سریع پین‌شده روی دسکتاپ با Markdown و ذخیره‌ی خودکار.

## 🗺️ فراتر از v1 (نقشه‌ی راه)

ویرایش هوشمند متن · تاریخچه‌ی کلیپ‌بورد · OCR · اسکرین‌شات و حاشیه‌نویسی · آرشیو ضبط
صدا · Voice Macro · Text Expansion · بازنویسی هوشمند (آفلاین‌محور، opt-in) ·
marketplace افزونه‌ها · MSIX + به‌روزرسانی خودکار. جزئیات:
[docs/roadmap.md](docs/roadmap.md).

## 🧩 پشته‌ی فنی

</div>

<div align="center">

`.NET 10` · `C# 14` · `WPF + WPF-UI (Fluent)` · `Whisper.net / Faster Whisper` · `NAudio + Silero VAD` · `SQLite` · `Serilog` · `xUnit`

</div>

<div dir="rtl">

دلایل کامل: [docs/decisions.md](docs/decisions.md) · طراحی سیستم:
[docs/architecture.md](docs/architecture.md).

## 🛠️ ساخت و اجرا

نیازمند **.NET 10 SDK**.

</div>

```bash
dotnet build
dotnet test
dotnet run --project Pva.App
```

<div dir="rtl">

انتشار پرتابل (آماده‌ی ZIP، self-contained و تک‌فایل):

</div>

```bash
dotnet publish Pva.App -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -o publish/portable
```

<div dir="rtl">

> فایل‌های مدل Whisper جداگانه توزیع می‌شوند (حجیم و git-ignore) و در پوشه‌ی
> `models/` کنار فایل اجرایی قرار می‌گیرند.

## 📚 مستندات

| سند | توضیح |
|-----|-------|
| [docs/architecture.md](docs/architecture.md) | طراحی سیستم، pipeline، ماژول‌ها، ریسک‌ها |
| [docs/decisions.md](docs/decisions.md) | ثبت تصمیم‌های معماری (ADR) |
| [docs/roadmap.md](docs/roadmap.md) | milestoneها و scope |
| [docs/session.md](docs/session.md) | لاگ جاری کار و قدم‌های بعدی |
| [CLAUDE.md](CLAUDE.md) | قوانین و قراردادهای مهندسی |

## 🤝 مشارکت

این پروژه متن‌باز است. Issue و Pull Request خوش‌آمدند. پیش از توسعه، `CLAUDE.md` و
`docs/architecture.md` را بخوانید. کامپوننت‌های شخص ثالث (Whisper، whisper.cpp،
CTranslate2، ONNX Runtime) با مجوزهای آزاد (MIT / Apache-2.0) استفاده می‌شوند.

## 📄 مجوز

منتشرشده تحت مجوز **MIT** — فایل [LICENSE](LICENSE).

</div>

<div align="center">
<br/>
ساخته‌شده برای فارسی‌زبان‌ها ❤️ — اگر مفید بود، یک ⭐ بده
</div>
