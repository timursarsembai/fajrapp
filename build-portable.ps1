# Build FajrApp Portable Version
# This creates a standalone folder that can be run from anywhere

$ErrorActionPreference = "Stop"

Write-Host "Building FajrApp Portable..." -ForegroundColor Cyan

# Clean previous builds
if (Test-Path ".\publish\portable") {
    Remove-Item ".\publish\portable" -Recurse -Force
}

# Build Release
dotnet publish -c Release -o ".\publish\portable" --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create README for portable version
$readme = @"
FajrApp - Portable Version
==========================

REQUIREMENTS:
- .NET 8.0 Desktop Runtime
  Download: https://dotnet.microsoft.com/download/dotnet/8.0

HOW TO USE:
1. Extract this folder to any location
2. Run FajrApp.exe
3. The widget will appear on your taskbar
4. Right-click for options

SETTINGS:
- Settings are stored in: %LOCALAPPDATA%\FajrApp\
- To enable auto-start: Right-click widget > Auto start

LANGUAGES:
English, Spanish, Arabic, Russian, Indonesian, Kazakh
(Auto-detected from system, can be changed in Settings)

GitHub: https://github.com/timursarsembai/fajrapp
"@

$readme | Out-File ".\publish\portable\README.txt" -Encoding UTF8

Write-Host "Portable build completed!" -ForegroundColor Green
Write-Host "Output: .\publish\portable\" -ForegroundColor Yellow
