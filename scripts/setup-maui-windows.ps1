param()

Write-Host "== Setup MAUI development environment (Windows) =="

$dotnets = & dotnet --list-sdks 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet not found. Please install .NET 10 SDK (10.0.100) from https://aka.ms/dotnet/download" -ForegroundColor Yellow
} else {
    if ($dotnets -match '10\.0\.100') {
        Write-Host "Found .NET SDK 10.0.100." -ForegroundColor Green
    } else {
        Write-Host "Warning: .NET 10.0.100 not found. Current installed SDKs:" -ForegroundColor Yellow
        $dotnets | ForEach-Object { Write-Host "  $_" }
        Write-Host "Please install .NET 10.0.100 and re-run this script: https://aka.ms/dotnet/download" -ForegroundColor Yellow
    }
}

Write-Host "\nInstalling MAUI workloads (best-effort). This may take a while."
try {
    dotnet workload install microsoft-net-maui -v minimal
    if ($LASTEXITCODE -eq 0) { Write-Host "Workloads installed (or already present)." -ForegroundColor Green }
    else { Write-Host "dotnet workload install returned non-zero. Check output above." -ForegroundColor Yellow }
} catch {
    Write-Host "Failed to run 'dotnet workload install'. Ensure dotnet 10 SDK is available and try again." -ForegroundColor Red
}

Write-Host "\nChecking Java (required for Android builds)..."
try {
    & java -version 2>&1 | ForEach-Object { Write-Host "  $_" }
    if ($LASTEXITCODE -ne 0) { throw "no-java" }
    Write-Host "Java found." -ForegroundColor Green
} catch {
    Write-Host "Java not found or not on PATH. Android builds require a JDK (e.g. Temurin/OpenJDK 17)." -ForegroundColor Yellow
    Write-Host "If you use Chocolatey you can install a JDK with: `choco install temurin -y`" -ForegroundColor Gray
}

Write-Host "\nNotes and next steps:" -ForegroundColor Cyan
Write-Host "- For full MAUI development on Windows, Visual Studio 2022/2023 with MAUI workloads is recommended." -ForegroundColor Gray
Write-Host "- If you only want to build non-MAUI projects (libraries, tools, tests), run dotnet build for the specific projects instead of the whole solution." -ForegroundColor Gray
Write-Host "  Example: `dotnet build Dizimo/tools/validate-standalone/ValidateStandalone.csproj -c Release`" -ForegroundColor Gray
Write-Host "- If CI should skip MAUI platform builds, configure workflows to only build non-MAUI projects or use a self-hosted runner with MAUI workloads installed." -ForegroundColor Gray

Write-Host "Setup script finished." -ForegroundColor Cyan
