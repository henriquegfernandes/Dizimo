<#
Best-effort script to install Visual Studio components required for .NET MAUI builds.
Run as Administrator on a Windows machine.

This script attempts to install Visual Studio 2022 Community and required workloads.
It is best-effort and may require manual steps for licensing/interactive components.
#>

Write-Host "Starting Visual Studio + MAUI workloads installation..."

function Ensure-Program {
    param($exe, $name)
    if (-not (Get-Command $exe -ErrorAction SilentlyContinue)) {
        Write-Host "$name not found on PATH"
    } else {
        Write-Host "$name already available"
    }
}

# Suggest interactive Visual Studio installer if not present
if (-not (Test-Path "$env:ProgramFiles(x86)\Microsoft Visual Studio\Installer\vs_installer.exe")) {
    Write-Host "Visual Studio Installer not found. Downloading Visual Studio 2022 Community installer (interactive)."
    $url = 'https://aka.ms/vs/17/release/vs_community.exe'
    $out = "$env:TEMP\vs_community_installer.exe"
    Invoke-WebRequest -Uri $url -OutFile $out
    Write-Host "Launching installer — you must select 'Mobile development with .NET' and install Android workloads." 
    Start-Process -FilePath $out -ArgumentList '/quiet' -Wait
} else { Write-Host "Visual Studio Installer present" }

# Attempt to install workloads with vswhere and vs_installers
$vswhere = "$env:ProgramFiles(x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswhere) {
    $inst = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    if ($inst) {
        Write-Host "Found VS at: $inst"
        # Try workloads via vs_install — best-effort
        $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
        if ($dotnet) {
            Write-Host "Installing .NET MAUI workload via dotnet workload install"
            dotnet workload install microsoft-net-maui -v minimal
        }
    }
} else { Write-Host "vswhere not found — ensure Visual Studio is installed and mobile workloads selected." }

Write-Host "Visual Studio/workloads installation script finished. Some steps may require manual interaction." 
