@echo off
REM Force Windows GUI subsystem on executable
REM Usage: set-gui-subsystem.bat "path\to\Dizimo.exe"

setlocal enabledelayedexpansion

if "%~1"=="" (
    echo ❌ Usage: set-gui-subsystem.bat "path\to\executable.exe"
    exit /b 1
)

set "EXEPATH=%~1"

if not exist "%EXEPATH%" (
    echo ❌ Error: File not found: %EXEPATH%
    exit /b 1
)

echo 🔐 Setting Windows GUI subsystem for: %EXEPATH%

REM Try to find editbin in Windows SDK locations
set "EDITBIN="
for %%A in (
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\editbin.exe"
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\editbin.exe"
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x64\editbin.exe"
    "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Tools\MSVC\14.39.33519\bin\HostX64\x64\editbin.exe"
) do (
    if exist "%%A" (
        set "EDITBIN=%%A"
        goto found_editbin
    )
)

:found_editbin
if "!EDITBIN!"=="" (
    REM Fallback to editbin in PATH or use PowerShell for PE modification
    where editbin.exe >nul 2>&1
    if !errorlevel! equ 0 (
        set "EDITBIN=editbin.exe"
    ) else (
        echo ⚠️  editbin.exe not found, trying PowerShell approach...
        goto powershell_approach
    )
)

echo 📝 Using editbin: !EDITBIN!
"!EDITBIN!" /SUBSYSTEM:WINDOWS "%EXEPATH%"

if !errorlevel! equ 0 (
    echo ✅ Subsystem changed to WINDOWS (GUI) successfully
    exit /b 0
) else (
    echo ⚠️  editbin failed, trying PowerShell approach...
    goto powershell_approach
)

:powershell_approach
echo 🔐 Using PowerShell PE modification approach...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$exePath = '%EXEPATH%'; " ^
    "if (Test-Path $exePath) { " ^
    "  [byte[]]$bytes = [System.IO.File]::ReadAllBytes($exePath); " ^
    "  if ($bytes.Length -gt 60) { " ^
    "    Write-Host 'Current subsystem value at offset 60: ' $bytes[60]; " ^
    "    if ($bytes[60] -eq 2) { " ^
    "      $bytes[60] = 3; " ^
    "      [System.IO.File]::WriteAllBytes($exePath, $bytes); " ^
    "      Write-Host 'Console subsystem changed to GUI subsystem successfully'; " ^
    "      exit 0; " ^
    "    } else { " ^
    "      Write-Host 'Already running as GUI or already modified'; " ^
    "      exit 0; " ^
    "    } " ^
    "  } " ^
    "} else { " ^
    "  Write-Host 'ERROR: File not found: ' $exePath; " ^
    "  exit 1; " ^
    "}"

if !errorlevel! equ 0 (
    echo ✅ Subsystem configured successfully
    exit /b 0
) else (
    echo ❌ Failed to set GUI subsystem
    exit /b 1
)

