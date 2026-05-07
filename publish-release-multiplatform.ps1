# Multi-platform publish script for Dizimo GitHub Release
# Supports: Windows, Linux, and macOS
# Run this from Windows PowerShell

param(
    [string]$VersionTag = "v1.0.0",
    [string]$ReleaseNotes = "Release version $VersionTag"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Dizimo multi-platform release..." -ForegroundColor Cyan
Write-Host "Version: $VersionTag" -ForegroundColor Cyan
Write-Host "Release Notes: $ReleaseNotes" -ForegroundColor Cyan
Write-Host ""

# 1. Check Git repository
Write-Host "1. Checking for Git repository..." -ForegroundColor Yellow
if (-not (Test-Path ".git")) {
    Write-Host "❌ Not a Git repository!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Git repository found" -ForegroundColor Green
Write-Host ""

# 2. Build
Write-Host "2. Building project (Release)..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build completed successfully" -ForegroundColor Green
Write-Host ""

# 3. Define platforms
Write-Host "3. Publishing for multiple platforms..." -ForegroundColor Yellow
Write-Host ""

$platforms = @(
    @{
        RID = "win-x64"
        Name = "Windows (x64)"
    },
    @{
        RID = "linux-x64"
        Name = "Linux (x64)"
    },
    @{
        RID = "osx-x64"
        Name = "macOS (Intel x64)"
    },
    @{
        RID = "osx-arm64"
        Name = "macOS (Apple Silicon ARM64)"
    }
)

# Clean previous publish directory
if (Test-Path "publish") {
    Remove-Item "publish" -Recurse -Force
}
New-Item -ItemType Directory -Path "publish" -Force | Out-Null

$publishedArchives = @()

# Publish for each platform
foreach ($platform in $platforms) {
    Write-Host "   📦 Publishing for $($platform.Name) ($($platform.RID))..." -ForegroundColor Cyan
    
    $publishDir = "publish/Dizimo-$VersionTag-$($platform.RID)"
    
    dotnet publish Dizimo/Dizimo.csproj `
        -c Release `
        -o $publishDir `
        -r $platform.RID `
        --self-contained true
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   ❌ Publish failed for $($platform.RID)!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "   ✅ Published to: $publishDir" -ForegroundColor Green
    
    # Create ZIP archive
    $archiveName = "Dizimo-$VersionTag-$($platform.RID).zip"
    Write-Host "   📦 Creating archive: $archiveName" -ForegroundColor Cyan
    
    Push-Location publish
    Compress-Archive -Path "Dizimo-$VersionTag-$($platform.RID)" -DestinationPath "../$archiveName" -Force
    Pop-Location
    
    if (Test-Path $archiveName) {
        $size = (Get-Item $archiveName).Length / 1MB
        Write-Host "   ✅ Archive created: $archiveName ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
        $publishedArchives += $archiveName
    } else {
        Write-Host "   ❌ Failed to create archive!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
}

# 4. Create Git Tag
Write-Host "4. Creating Git tag..." -ForegroundColor Yellow

$tagExists = git tag -l $VersionTag
if ($tagExists) {
    Write-Host "   ⚠️  Tag $VersionTag already exists." -ForegroundColor Yellow
    $response = Read-Host "   Do you want to delete and recreate it? (y/n)"
    if ($response -eq "y") {
        git tag -d $VersionTag
        git push origin --delete $VersionTag 2>$null
        Write-Host "   ✅ Old tag deleted" -ForegroundColor Green
    } else {
        Write-Host "   ⏭️  Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
}

git tag -a $VersionTag -m "Release: $VersionTag`n`n$ReleaseNotes"
git push origin $VersionTag
Write-Host "✅ Tag created and pushed to GitHub" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "═════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ PREPARATION COMPLETE!" -ForegroundColor Green
Write-Host "═════════════════════════════════════════" -ForegroundColor Green
Write-Host ""

Write-Host "📦 Archives created:" -ForegroundColor Cyan
foreach ($archive in $publishedArchives) {
    $size = (Get-Item $archive).Length / 1MB
    Write-Host "   ✅ $archive ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
}
Write-Host ""

Write-Host "📝 Git Tag: $VersionTag" -ForegroundColor Cyan
Write-Host ""

Write-Host "🚀 NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Go to: https://github.com/henriquegfernandes/Dizimo/releases/new" -ForegroundColor White
Write-Host ""
Write-Host "2. Fill in:" -ForegroundColor White
Write-Host "   Release title: $VersionTag" -ForegroundColor Cyan
Write-Host "   Tag: Select '$VersionTag'" -ForegroundColor Cyan
Write-Host "   Description: [Paste your release notes]" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Upload archives:" -ForegroundColor White
foreach ($archive in $publishedArchives) {
    Write-Host "   → $archive" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "4. Mark 'This is a pre-release' if applicable" -ForegroundColor White
Write-Host "5. Click 'Publish release'" -ForegroundColor White
Write-Host ""
Write-Host "Repository: https://github.com/henriquegfernandes/Dizimo" -ForegroundColor Cyan
Write-Host ""

