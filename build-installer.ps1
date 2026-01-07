# Build FajrApp Install Version using Inno Setup
# Creates a proper Windows installer (.exe)

$ErrorActionPreference = "Stop"

Write-Host "Building FajrApp Installer..." -ForegroundColor Cyan

# Clean previous builds
if (Test-Path ".\publish\install") {
    Remove-Item ".\publish\install" -Recurse -Force
}

New-Item -ItemType Directory -Path ".\publish\install" -Force | Out-Null

# Build Release
dotnet publish -c Release -o ".\publish\install\files" --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create Inno Setup script
$innoScript = @"
; FajrApp Installer Script for Inno Setup
; Download Inno Setup: https://jrsoftware.org/isinfo.php

#define MyAppName "FajrApp"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Timur Sarsembai"
#define MyAppURL "https://github.com/timursarsembai/fajrapp"
#define MyAppExeName "FajrApp.exe"

[Setup]
AppId={{8E5A7B3C-9D2F-4A1E-B6C8-3F7D9A2E1B4C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
OutputDir=.
OutputBaseFilename=FajrApp-Setup-{#MyAppVersion}
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start with Windows"; GroupDescription: "Additional options:"

[Files]
Source: "files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "FajrApp"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
"@

$innoScript | Out-File ".\publish\install\FajrApp-Installer.iss" -Encoding UTF8

Write-Host "" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installer files prepared!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "To create the installer:" -ForegroundColor Yellow
Write-Host "1. Download Inno Setup from: https://jrsoftware.org/isinfo.php" -ForegroundColor White
Write-Host "2. Open: .\publish\install\FajrApp-Installer.iss" -ForegroundColor White
Write-Host "3. Press Ctrl+F9 to compile" -ForegroundColor White
Write-Host ""
Write-Host "Or use command line (if Inno Setup is in PATH):" -ForegroundColor Yellow
Write-Host "iscc .\publish\install\FajrApp-Installer.iss" -ForegroundColor White
