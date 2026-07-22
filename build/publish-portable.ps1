# انتشار نسخه‌ی پرتابل Persian Voice Anywhere (ZIP، self-contained، تک‌فایل).
# اجرا: pwsh ./build/publish-portable.ps1
# خروجی: publish/portable/  و  publish/PersianVoiceAnywhere-portable.zip
#
# نیازمند .NET 10 SDK. مدل‌های Whisper/Silero جداگانه در پوشه‌ی models/ کنار exe قرار می‌گیرند.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'publish/portable'
$zipPath = Join-Path $root 'publish/PersianVoiceAnywhere-portable.zip'
$rid = 'win-x64'

Write-Host '==> پاک‌سازی خروجی قبلی' -ForegroundColor Cyan
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host '==> انتشار (Release، self-contained، single-file)' -ForegroundColor Cyan
dotnet publish (Join-Path $root 'src/Pva.App/Pva.App.csproj') `
    -c Release -r $rid --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $outDir

Write-Host '==> افزودن مستندات و پوشه‌ی models' -ForegroundColor Cyan
Copy-Item (Join-Path $root 'README.md') $outDir
Copy-Item (Join-Path $root 'LICENSE') $outDir
Copy-Item (Join-Path $root 'THIRD-PARTY-NOTICES.md') $outDir
New-Item -ItemType Directory -Force (Join-Path $outDir 'models') | Out-Null
@'
مدل‌ها را اینجا قرار دهید:
- silero_vad.onnx  (تشخیص فعالیت گفتاری)
- ggml-base.bin یا مدل فارسیِ whisper.cpp  (تشخیص گفتار)
راهنما: docs/models.md
'@ | Out-File -Encoding utf8 (Join-Path $outDir 'models/README.txt')

Write-Host '==> ساخت ZIP' -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $outDir '*') -DestinationPath $zipPath

Write-Host "انجام شد: $zipPath" -ForegroundColor Green
