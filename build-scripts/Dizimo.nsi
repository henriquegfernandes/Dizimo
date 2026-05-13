; Dizimo Installer Script (NSIS)
; This script creates a Windows installer executable for the Dizimo application
; with automatic icon registration

!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"

; Constants
!define APP_NAME "Dizimo"
!define APP_VERSION "1.1.2"
!define APP_PUBLISHER "Henrique Fernandes Tech"
!define APP_EXE "Dizimo.exe"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"

; Name and file
Name "${APP_NAME} v${APP_VERSION}"
OutFile "${APP_NAME}-${APP_VERSION}-Setup.exe"
InstallDir "$PROGRAMFILES\${APP_NAME}"

; Request admin privileges
RequestExecutionLevel admin

; MUI Settings
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"

; Installer sections
Section "Install"
    SetOutPath "$INSTDIR"
    
    ; Copy application files
    File /r "app\*.*"
    
    ; Copy and register icon
    SetOverwrite try
    File /oname=appicon.ico "Resources\AppIcon\appicon.ico" 
    
    ; Create Start Menu shortcuts with icon
    CreateDirectory "$SMPROGRAMS\${APP_NAME}"
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\appicon.ico" 0
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
    
    ; Create Desktop shortcut with icon
    CreateShortcut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\appicon.ico" 0
    
    ; Register icon in registry
    WriteRegStr HKCR "Applications\${APP_EXE}\DefaultIcon" "" "$INSTDIR\appicon.ico"
    
    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    
    ; Register uninstall info with icon
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName" "${APP_NAME}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\appicon.ico"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher" "${APP_PUBLISHER}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
    
    ; Refresh icon cache
    ${If} ${RunningX64}
        SetRegView 64
    ${EndIf}
SectionEnd

; Uninstaller section
Section "Uninstall"
    RMDir /r "$INSTDIR"
    RMDir /r "$SMPROGRAMS\${APP_NAME}"
    Delete "$DESKTOP\${APP_NAME}.lnk"
    DeleteRegKey HKLM "${UNINSTALL_KEY}"
    DeleteRegKey HKCR "Applications\${APP_EXE}"
SectionEnd

