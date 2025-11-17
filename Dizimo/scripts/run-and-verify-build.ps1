<#
Runs dotnet workload install and builds the MAUI solution, collecting artifacts.
Run this on the self-hosted Windows runner after Visual Studio + workloads are installed.
It will attempt to produce artifacts in C:\actions-runner\_artifacts
#>

Write-Host "Starting workload install and build..."

try {
    dotnet workload install microsoft-net-maui -v minimal
} catch {
    Write-Host "dotnet workload install returned non-zero — ensure Visual Studio components are present." 
}

Write-Host "Building solution (Release)..."
dotnet build ..\..\Dizimo.sln -c Release --no-restore

Write-Host "Collecting artifacts..."
$out = 'C:\actions-runner\_artifacts'
if (-not (Test-Path $out)) { New-Item -ItemType Directory -Path $out -Force }

# Collect typical outputs (APK/MSIX) — heuristics
Get-ChildItem -Path ..\.. -Recurse -Include *.apk,*.msix,*.aab -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $out -Force
}

Write-Host "Build script finished. Check $out for artifacts." 
