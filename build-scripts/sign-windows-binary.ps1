# Sign Windows executables and installers
# Usage: ./sign-windows-binary.ps1 -FilePath "path/to/file.exe" -CertPath "path/to/cert.pfx" -CertPassword "password"

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath,
    
    [Parameter(Mandatory=$true)]
    [string]$CertPath,
    
    [Parameter(Mandatory=$true)]
    [string]$CertPassword,
    
    [string]$TimeStampServer = "http://timestamp.digicert.com",
    [string]$Description = "Dizimo - Church Financial Management System"
)

Write-Host "🔐 Signing Windows binary..." -ForegroundColor Cyan

# Validate inputs
if (-not (Test-Path $FilePath)) {
    Write-Host "❌ Error: File not found: $FilePath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $CertPath)) {
    Write-Host "❌ Error: Certificate not found: $CertPath" -ForegroundColor Red
    exit 1
}

# Check if SignTool is available
$signTool = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
if (-not (Test-Path $signTool)) {
    # Try alternate paths
    $signTool = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($null -eq $signTool) {
        Write-Host "⚠️  Warning: SignTool not found in standard location" -ForegroundColor Yellow
        Write-Host "   Attempting to use PATH..." -ForegroundColor Yellow
        $signTool = "signtool.exe"
    } else {
        $signTool = $signTool.Source
    }
}

Write-Host "📁 File to sign: $FilePath" -ForegroundColor Green
Write-Host "📄 Certificate: $CertPath" -ForegroundColor Green

# Sign the binary with timestamp
Write-Host "`n⏳ Signing binary (with timestamp)..." -ForegroundColor Yellow

try {
    $cmd = @(
        $signTool,
        "sign",
        "/f", $CertPath,
        "/p", $CertPassword,
        "/d", $Description,
        "/t", $TimeStampServer,
        "/tr", "http://timestamp.digicert.com",
        "/td", "sha256",
        "/fd", "sha256",
        $FilePath
    )
    
    & $signTool sign /f $CertPath /p $CertPassword /d $Description /t $TimeStampServer /tr $TimeStampServer /td sha256 /fd sha256 $FilePath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Signing with timestamp failed, retrying without timestamp..." -ForegroundColor Yellow
        & $signTool sign /f $CertPath /p $CertPassword /d $Description /fd sha256 $FilePath
    }
} catch {
    Write-Host "❌ Error signing file: $_" -ForegroundColor Red
    exit 1
}

# Verify signature
Write-Host "`n✅ Verifying signature..." -ForegroundColor Yellow
& $signTool verify /pa $FilePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✨ Binary signed successfully!" -ForegroundColor Green
    Write-Host "   File: $FilePath" -ForegroundColor Green
    Write-Host "`n📋 Signature Details:" -ForegroundColor Cyan
    & $signTool verify /pa /v $FilePath
} else {
    Write-Host "`n❌ Signature verification failed!" -ForegroundColor Red
    exit 1
}

