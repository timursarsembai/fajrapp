# Build All Versions of FajrApp
# Creates both Portable and Installer versions

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FajrApp Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Build Portable
Write-Host "Step 1: Building Portable version..." -ForegroundColor Yellow
& ".\build-portable.ps1"

Write-Host ""

# Build Installer
Write-Host "Step 2: Preparing Installer version..." -ForegroundColor Yellow
& ".\build-installer.ps1"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All builds completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Outputs:" -ForegroundColor Yellow
Write-Host "  Portable: .\publish\portable\" -ForegroundColor White
Write-Host "  Installer: .\publish\install\" -ForegroundColor White
