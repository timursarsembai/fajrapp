# Build FajrApp MSIX Package for Microsoft Store
# Requires: Visual Studio 2022 with Windows Application Packaging Project support

$ErrorActionPreference = "Stop"

Write-Host "Building FajrApp MSIX Package..." -ForegroundColor Cyan

# Clean previous builds
if (Test-Path ".\publish\msix") {
    Remove-Item ".\publish\msix" -Recurse -Force
}
New-Item -ItemType Directory -Path ".\publish\msix" -Force | Out-Null

# Build Release version
dotnet publish -c Release -r win-x64 --self-contained false -o ".\publish\msix\files"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps to create MSIX package:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option 1: Visual Studio (Recommended)" -ForegroundColor Cyan
Write-Host "1. Open FajrApp.sln in Visual Studio 2022" -ForegroundColor White
Write-Host "2. Add the FajrApp.Package project to solution" -ForegroundColor White
Write-Host "3. Right-click FajrApp.Package -> Publish -> Create App Packages" -ForegroundColor White
Write-Host "4. Select 'Microsoft Store under a new app name'" -ForegroundColor White
Write-Host "5. Follow the wizard" -ForegroundColor White
Write-Host ""
Write-Host "Option 2: MSIX Packaging Tool" -ForegroundColor Cyan
Write-Host "1. Install 'MSIX Packaging Tool' from Microsoft Store" -ForegroundColor White
Write-Host "2. Create package from existing installer (FajrApp-Setup-1.0.0.exe)" -ForegroundColor White
Write-Host ""
Write-Host "Required images in FajrApp.Package\Images:" -ForegroundColor Yellow
Write-Host "  - StoreLogo.png (50x50)" -ForegroundColor White
Write-Host "  - Square44x44Logo.png (44x44)" -ForegroundColor White
Write-Host "  - Square71x71Logo.png (71x71)" -ForegroundColor White
Write-Host "  - Square150x150Logo.png (150x150)" -ForegroundColor White
Write-Host "  - Square310x310Logo.png (310x310)" -ForegroundColor White
Write-Host "  - Wide310x150Logo.png (310x150)" -ForegroundColor White
