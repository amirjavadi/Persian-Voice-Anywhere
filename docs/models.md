<div dir="rtl">

# مدل‌ها — Persian Voice Anywhere

فایل‌های مدل حجیم‌اند و در مخزن ذخیره نمی‌شوند (`models/` در `.gitignore`). آن‌ها
جداگانه توزیع و در پوشه‌ی `models/` کنار فایل اجرایی قرار می‌گیرند. این سند می‌گوید
چه فایل‌هایی لازم است و از کجا.

## VAD — Silero (لازم برای M1)

- فایل: `models/silero_vad.onnx`
- نسخه: Silero VAD v5 (ONNX)
- منبع: مخزن رسمی Silero VAD (`snakers4/silero-vad`)، پوشه‌ی مدل‌های ONNX.
- مصرف‌کننده: `SileroVoiceActivityDetector` (ورودی فریم ۵۱۲ نمونه‌ای در 16kHz، حالت
  بازگشتی [2,1,128]، sr = 16000).

اگر این فایل نباشد، ساخت `IVoiceActivityDetector` با پیام واضح خطا می‌دهد؛ منطق
قطعه‌بندی (`SpeechSegmenter`) و resampler مستقل از مدل‌اند و تست‌شان سبز است.

## STT — Whisper (لازم از M2)

- whisper.cpp (پیش‌فرض): مدل GGUF/GGML، مثل `models/ggml-base.bin` یا مدل fine-tune
  فارسی. اندازه‌ی مدل با دقت/سرعت trade-off دارد.
- Faster Whisper (اختیاری): مدل CTranslate2 در پوشه‌ی engine pack.

جزئیات انتخاب مدل و استراتژی دانلود در اولین اجرا، در Milestone M2 نهایی می‌شود.

## توزیع

برای انتشار، مدل پایه یا مکانیزم دانلود در اولین اجرا فراهم می‌شود (به `docs/roadmap.md`،
M2 و M10 مراجعه کنید). مجوز مدل‌ها پیش از انتشار در NOTICE ثبت می‌شود (ADR-0009).

</div>
