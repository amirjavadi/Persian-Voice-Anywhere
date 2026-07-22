# CLAUDE.md — Persian Voice Anywhere

راهنمای هر عامل هوش مصنوعی یا توسعه‌دهنده‌ای که در این مخزن کار می‌کند. اول این را
بخوان. این فایل، مرجع اصلی «چگونه اینجا می‌سازیم» است. آن را به‌روز نگه دار: وقتی
قانون، مسیر یا تصمیمی تغییر کرد، در همان تغییر این فایل را هم به‌روزرسانی کن.

---

## ۱. این محصول چیست

**Persian Voice Anywhere** یک نرم‌افزار دسکتاپ ویندوز، تجاری و در سطح Production است.
وعده‌ی اصلی: *کاربر در هر برنامه‌ای صحبت می‌کند و متن دقیق فارسی در محل مکان‌نما تایپ
می‌شود، کاملاً آفلاین و پرتابل.*

- صحبت → تایپ متن دقیق فارسی در **هر** برنامه‌ی فوکوس‌دار (Chrome، Word، VS Code،
  Telegram، Notepad و …) دقیقاً مثل یک کیبورد واقعی.
- **بدون Copy/Paste.** متن با `SendInput` ویندوز (به‌صورت Unicode) تزریق می‌شود.
- **کاملاً آفلاین.** هسته‌ی محصول هیچ وابستگی به شبکه ندارد. هر قابلیت شبکه‌ای
  (بررسی به‌روزرسانی، بازنویسی AI اختیاری) باید opt-in و کاملاً جدا باشد.
- **اولویت با پرتابل بودن.** از داخل ZIP اجرا می‌شود؛ بدون نصب، بدون نوشتن در
  Registry. تنظیمات کنار فایل اجرایی ذخیره می‌شوند.

این یک محصول واقعی برای فروش و انتشار است، نه نمونه‌ی اولیه یا تمرین آموزشی. کیفیت،
خوانایی، کارایی و قابلیت توسعه بر سرعت پیاده‌سازی مقدم‌اند.

## ۲. پشته‌ی فنی (قطعی‌شده — به docs/decisions.md مراجعه کن)

| حوزه               | انتخاب                                                        |
|--------------------|---------------------------------------------------------------|
| Runtime            | **.NET 10 (LTS)**، C# 14                                       |
| UI                 | **WPF** + **WPF-UI** (Fluent Design، ظاهر Windows 11)          |
| MVVM               | CommunityToolkit.Mvvm                                          |
| DI / composition   | Microsoft.Extensions.DependencyInjection + Hosting            |
| موتور STT          | هیبرید: **Whisper.net (whisper.cpp)** پیش‌فرض + **Faster Whisper** اختیاری، هر دو پشت `ISpeechToTextEngine` |
| ضبط صدا            | WASAPI با NAudio؛ VAD با Silero (ONNX Runtime)               |
| ویرایشگر (Notepad) | AvalonEdit                                                     |
| System Tray        | Hardcodet.NotifyIcon.Wpf                                       |
| ذخیره‌سازی         | SQLite (Microsoft.Data.Sqlite) + تنظیمات JSON کنار exe        |
| اطلاعات حساس       | DPAPI (محلی) / AES با رمز عبور (برای secretهای پرتابل)         |
| Logging            | Serilog (فایل چرخشی کنار exe، بدون PII)                        |
| تست                | xUnit (Assert خالص؛ در صورت نیاز Shouldly-MIT)؛ BenchmarkDotNet برای perf |
| بسته‌بندی          | ZIP پرتابل + EXE تک‌فایل (پایه)؛ MSIX + auto-update بعداً      |

بدون ثبت یک ADR در `docs/decisions.md` و یک خط دلیل، هیچ وابستگی جدیدی اضافه نکن.
کتابخانه‌های کمتر و درست‌انتخاب‌شده را ترجیح بده. از پیچیدگی غیرضروری، کد تکراری و
راه‌حل شکننده پرهیز کن.

## ۳. قوانین معماری

- **تفکیک تمیز:** لایه‌ی domain/orchestration مستقل از UI است. هیچ نوع WPF در
  `Pva.Core` یا ماژول‌های موتور/دامنه وجود ندارد.
- **MVVM در سرتاسر UI.** هیچ منطقی در code-behind فراتر از سیم‌کشی view نباشد.
- **همه‌ی درزها پشت اینترفیس:** `ISpeechToTextEngine`، `IAudioCapture`،
  `ITextInjector`، `IHotkeyService`، `IPersianTextProcessor`، `ICommandParser` و
  repositoryهای storage. همین چیزی است که موتورها/قابلیت‌ها را قابل‌تعویض و کد را
  قابل‌تست می‌کند.
- **مسیر داغ هرگز UI thread را بلاک نمی‌کند.** ضبط صدا و STT روی workerهای پس‌زمینه
  با صف محدود اجرا می‌شوند. UI thread فقط رسم می‌کند.
- **هزینه‌ی Idle نزدیک صفر.** هیچ حلقه‌ی polling نباشد. صدا و hookها event-driven.
  مدل Whisper هنگام اولین ضبط lazy-load و در صورت تنظیم، در Idle آزاد می‌شود.
- **آفلاین یک تضمین است، نه پیش‌فرض.** کد هسته هیچ فراخوانی شبکه‌ای ندارد.
- نقشه‌ی کامل ماژول‌ها و pipeline در `docs/architecture.md`.

چیدمان هدف ماژول‌ها (در Milestone 0 ساخته می‌شود):

```
Pva.App          میزبان WPF، composition root، tray، میکروفون شناور، تنظیمات، پنجره‌ها
Pva.Core         مدل‌های دامنه، اینترفیس‌ها، orchestration (pipeline دیکته)
Pva.Audio        ضبط WASAPI + قطعه‌بندی VAD
Pva.Stt          ISpeechToTextEngine, WhisperCppEngine, FasterWhisperEngine
Pva.PersianText  normalization، نیم‌فاصله (ZWNJ)، علائم نگارشی، اعداد، ترکیب fa/en
Pva.Commands     گرامر دستور صوتی → کنش‌های ویرایشگر
Pva.Injection    تزریق متن Unicode با SendInput
Pva.Hotkeys      کلید میانبر سراسری / low-level keyboard hook
Pva.Storage      repositoryهای SQLite، تنظیمات JSON، محافظت از secret
Pva.Notepad      قابلیت نوت‌پد تب‌دار داخلی (v1)
Pva.StickyNotes  قابلیت یادداشت چسبان (v1)
Pva.Plugins      SDK و host افزونه (اینترفیس‌ها در v1، marketplace بعداً)
Pva.Tests        xUnit: unit + integration + هارنس perf
```

## ۴. کیفیت فارسی، وجه تمایز محصول

خروجی خام Whisper، فارسیِ قابل‌قبول نیست. ماژول `Pva.PersianText` باید این‌ها را
مدیریت کند: نیم‌فاصله (ZWNJ)، علائم نگارشی، فاصله‌گذاری صحیح، اعداد فارسی، ترکیب
فارسی/انگلیسی (`Pull Request روی GitHub`)، اصطلاحات فنی، نام نرم‌افزارها و اسامی
خاص. این ماژول **خالص و قطعی (deterministic)** است و باید تست‌های golden-file
داشته باشد. این نمونه باید تمیز حفظ شود:

> امروز یک Pull Request روی GitHub زدم.

همچنین تشخیص را با یک `initial_prompt` فارسی + فنی و یک دیکشنری اصطلاحات محافظت‌شده
جهت‌دهی کن.

## ۵. روش کار ما (پروتکل توسعه — غیرقابل‌مذاکره)

مالک پروژه این روند را الزامی کرده است. دقیقاً رعایت کن:

۱. **قبل از کد، تحلیل کن.** نیازمندی‌ها، بهترین معماری، ریسک‌ها.
۲. **Milestoneهای کوچک.** کار را به milestoneهای کوچک و قابل‌تحویل بشکن
   (`docs/roadmap.md`). هر milestone برنامه‌ی توسعه‌ی خودش را دارد.
۳. **دروازه‌ی مرحله:** هر بار یک مرحله را توسعه بده. **پس از هر مرحله تست بنویس.**
   **بدون تأیید مالک، وارد milestone/مرحله‌ی بعد نشو.**
۴. **هر تغییر را با دلیلش توضیح بده.** هیچ ویرایش بی‌توضیحی نباشد.
۵. **بعد از هر تغییر مهم، مستندات و حافظه را به‌روزرسانی کن** (بخش ۶).
۶. کیفیت، خوانایی، کارایی و قابلیت توسعه را بر سرعت ترجیح بده.
۷. برای planning، review، QA، security و ship از **gstack** استفاده کن (بخش ۷).

## ۶. نظم Context و حافظه (تا هرگز به تاریخچه‌ی چت وابسته نشویم)

تمام context ضروری داخل خود مخزن است. بعد از هر تغییر مهم این‌ها را به‌روزرسانی کن:

- `docs/session.md` — لاگ جاری: چه شد، وضعیت فعلی، قدم‌های بعدی.
- `docs/decisions.md` — تصمیم‌های جدید/تغییریافته به‌صورت ADR.
- `docs/roadmap.md` — وضعیت milestoneها.
- `docs/architecture.md` — وقتی ساختار تغییر می‌کند.
- `CLAUDE.md` — وقتی قوانین/مسیرها/پشته تغییر می‌کند.
- پوشه‌ی حافظه‌ی ماندگار (پایین) برای واقعیت‌های ماندگارِ بین‌سشنی.

حافظه‌ی ماندگار در
`C:\Users\Amir\.claude\projects\d--Projects-Goyanegar\memory\` قرار دارد و
`MEMORY.md` فهرست آن است. فقط واقعیت‌های ماندگار و غیربدیهی را ذخیره کن؛ چیزی که
مخزن قبلاً ثبت کرده را تکرار نکن.

## ۷. مسیریابی مهارت‌ها (gstack)

وقتی درخواستی با یک skill می‌خواند، آن را با ابزار Skill فراخوانی کن. در شک، فراخوانی کن.

- ایده‌ی محصول / طوفان فکری → `/office-hours`
- استراتژی / scope → `/plan-ceo-review`
- معماری / قطعی‌کردن پلن → `/plan-eng-review`
- سیستم طراحی / هویت بصری → `/design-consultation` یا `/plan-design-review`
- خط‌لوله‌ی کامل بازبینی → `/autoplan`
- باگ / خطا / «چرا خراب است» → `/investigate`
- QA / تست رفتار → `/qa` یا `/qa-only`
- بازبینی کد / بررسی diff → `/review`
- پولیش بصری UI زنده → `/design-review`
- امنیت / آسیب‌پذیری → `/cso`
- Ship / deploy / PR → `/ship` یا `/land-and-deploy`
- نوشتن spec/issue آماده‌ی backlog → `/spec`
- ذخیره/بازیابی context کاری → `/context-save` / `/context-restore`
- بستن تمیز یک سشن → `/newsession`

## ۸. استانداردهای کدنویسی

- C# 14، nullable فعال، warnings-as-errors در Release، اعمال `.editorconfig`.
- Async تا انتها روی I/O و مسیر STT/صدا؛ `CancellationToken` روی هر عملیات طولانی.
  بدون `async void` مگر در event handlerها.
- بدون `.Result` / `.Wait()` مسدودکننده روی UI thread.
- Logging با Serilog به‌صورت structured؛ هرگز صدا یا متن کاربر را در سطح
  Information لاگ نکن. خطاها مدیریت، به کاربر شفاف اطلاع، و لاگ می‌شوند.
- هر درز عمومی، اینترفیس + unit test دارد. رفتار جدید با تست تحویل می‌شود.
- نام‌گذاری: namespaceهای `Pva.<Module>`، نوع‌ها PascalCase، فیلدهای خصوصی `_camelCase`.

## ۹. Build / run / test (در Milestone 0 تکمیل می‌شود)

```
dotnet build            # ساخت solution
dotnet test             # اجرای همه‌ی تست‌ها
dotnet run --project Pva.App
# انتشار پرتابل:
dotnet publish Pva.App -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -o publish/portable
```

داده‌ی زمان‌اجرا (settings.json، *.db، logs/) کنار exe ساخته می‌شود و git-ignore
است. مدل‌های Whisper در `models/` قرار می‌گیرند (git-ignore، جداگانه توزیع می‌شوند).
