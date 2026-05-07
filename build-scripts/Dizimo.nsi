; Dizimo Installer Script (NSIS)
; This script creates a Windows installer executable for the Dizimo application

!include "MUI2.nsh"
!include "x64.nsh"

; Name and file
Name "Dizimo"
OutFile "Dizimo-Setup.exe"
InstallDir "$PROGRAMFILES\Dizimo"

; Request admin privileges
RequestExecutionLevel admin

; MUI Settings
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

; Installer sections
Section "Install"
    SetOutPath "$INSTDIR"
    
    ; Copy application files
    File /r "app\*.*"
    
    ; Create Start Menu shortcuts
    CreateDirectory "$SMPROGRAMS\Dizimo"
    CreateShortcut "$SMPROGRAMS\Dizimo\Dizimo.lnk" "$INSTDIR\Dizimo.exe"
    CreateShortcut "$SMPROGRAMS\Dizimo\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
    
    ; Create Desktop shortcut
    CreateShortcut "$DESKTOP\Dizimo.lnk" "$INSTDIR\Dizimo.exe"
    
    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    
    ; Register uninstall info
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Dizimo" "DisplayName" "Dizimo"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Dizimo" "UninstallString" "$INSTDIR\Uninstall.exe"
SectionEnd

; Uninstaller section
Section "Uninstall"
    RMDir /r "$INSTDIR"
    RMDir /r "$SMPROGRAMS\Dizimo"
    Delete "$DESKTOP\Dizimo.lnk"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Dizimo"
SectionEnd

