# دانلود مدل‌های لازم برای Persian Voice Anywhere.
# اجرا:  pwsh ./build/fetch-models.ps1
#        pwsh ./build/fetch-models.ps1 -WhisperModel small -OutDir C:\PVA\models
#
# مدل‌ها بزرگ‌اند و در مخزن نیستند. این اسکریپت آن‌ها را در پوشه‌ی models/ می‌گذارد.
# اگر GitHub/HuggingFace فیلتر است، ابتدا پروکسی را تنظیم کنید:
#   $env:HTTPS_PROXY = 'http://127.0.0.1:PORT'

param(
    # small = ggml-small-q5_1 (~۱۹۰MB): نقطه‌ی تعادل دقت/سرعت — پیش‌فرض پیشنهادی.
    # turbo = large-v3-turbo-q5_0 (~۵۷۴MB): بهترین دقت؛ فقط با GPU (Vulkan) قابل‌استفاده است.
    [ValidateSet('tiny', 'base', 'small', 'medium', 'turbo')]
    [string]$WhisperModel = 'small',
    [string]$OutDir = ''
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutDir)) { $OutDir = Join-Path $root 'models' }
New-Item -ItemType Directory -Force $OutDir | Out-Null

function Get-Model {
    param([string]$Name, [string]$Url, [string]$Dest)

    if ((Test-Path $Dest) -and (Get-Item $Dest).Length -gt 0) {
        Write-Host "== $Name از قبل موجود است: $Dest" -ForegroundColor DarkGray
        return
    }

    Write-Host "== دانلود $Name ..." -ForegroundColor Cyan
    Write-Host "   $Url" -ForegroundColor DarkGray
    Invoke-WebRequest -Uri $Url -OutFile $Dest -UseBasicParsing
    $sizeMb = [Math]::Round((Get-Item $Dest).Length / 1MB, 1)
    Write-Host "   انجام شد ($sizeMb MB): $Dest" -ForegroundColor Green
}

# 1) Silero VAD (نسخه‌ی pin‌شده v5.1، ONNX)
# مهم: از تگِ v5.1 استفاده می‌کنیم، نه شاخه‌ی master. مدلِ روی master با ONNX Runtime
# فعلی ناسازگار است و روی گفتار همیشه احتمال ~۰ می‌دهد (VAD هیچ‌گاه گفتار را تشخیص
# نمی‌دهد و برنامه اصلاً چیزی ضبط نمی‌کند). نسخه‌ی v5.1 با فریم ۵۱۲ درست کار می‌کند.
Get-Model -Name 'Silero VAD (v5.1)' `
    -Url 'https://raw.githubusercontent.com/snakers4/silero-vad/v5.1/src/silero_vad/data/silero_vad.onnx' `
    -Dest (Join-Path $OutDir 'silero_vad.onnx')

# 2) Whisper (ggml برای whisper.cpp)
$whisperFile = switch ($WhisperModel) {
    'turbo' { 'ggml-large-v3-turbo-q5_0.bin' }
    'small' { 'ggml-small-q5_1.bin' }
    default { "ggml-$WhisperModel.bin" }
}
Get-Model -Name "Whisper $whisperFile" `
    -Url "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/$whisperFile" `
    -Dest (Join-Path $OutDir $whisperFile)

Write-Host ''
Write-Host "مدل‌ها در: $OutDir" -ForegroundColor Green
Write-Host 'برای اجرا:' -ForegroundColor Yellow
Write-Host '  • نسخه‌ی پرتابل: پوشه‌ی models/ را کنار Pva.App.exe بگذارید.' -ForegroundColor Yellow
Write-Host '  • dotnet run: در settings.json مسیرها را مطلق بدهید، مثلا:' -ForegroundColor Yellow
Write-Host "        \"VadModelPath\": \"$OutDir\silero_vad.onnx\"," -ForegroundColor Yellow
Write-Host "        \"WhisperModelPath\": \"$OutDir\ggml-$WhisperModel.bin\"" -ForegroundColor Yellow
if ($WhisperModel -ne 'base') {
    Write-Host "  توجه: مدل انتخابی '$WhisperModel' است؛ WhisperModelPath را به همین نام تنظیم کنید." -ForegroundColor Yellow
}
