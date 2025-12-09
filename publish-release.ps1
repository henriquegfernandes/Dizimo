
# Manual publish script for Dizimo GitHub Release Beta
# Run this script to build, package, and create a release on GitHub

param(
    [string]$VersionTag = "v1.0.0-beta.2",
    [string]$ReleaseNotes = "Initial beta release"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting manual publish for Dizimo..." -ForegroundColor Cyan
Write-Host ""

# 1. Check Git repository
Write-Host "1. Checking for Git repository..." -ForegroundColor Yellow
if (-not (Test-Path ".git")) {
    Write-Host "Not a Git repository!" -ForegroundColor Red
    exit 1
}
Write-Host "Git repository found" -ForegroundColor Green

# 2. Build
Write-Host "2. Building project (Release)..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build completed successfully" -ForegroundColor Green

# 3. Publish
Write-Host "3. Publishing application..." -ForegroundColor Yellow
$publishDir = "publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

dotnet publish Dizimo/Dizimo.csproj -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Application published to: $publishDir" -ForegroundColor Green

# 4. Create ZIP
Write-Host "4. Creating compressed file..." -ForegroundColor Yellow
$zipName = "Dizimo-$VersionTag-windows.zip"
if (Test-Path $zipName) {
    Remove-Item $zipName
}

Compress-Archive -Path $publishDir -DestinationPath $zipName
Write-Host "File created: $zipName" -ForegroundColor Green

# 5. Create Git Tag
Write-Host "5. Creating Git tag: $VersionTag..." -ForegroundColor Yellow
$tagExists = git tag -l $VersionTag
if ($tagExists) {
    Write-Host "Tag $VersionTag already exists. Do you want to delete and recreate it? (y/n)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq "y") {
        git tag -d $VersionTag
        git push origin --delete $VersionTag
        Write-Host "Old tag deleted" -ForegroundColor Green
    } else {
        Write-Host "Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
}

git tag -a $VersionTag -m "Release: $VersionTag`n`n$ReleaseNotes"
git push origin $VersionTag
Write-Host "Tag created and pushed to GitHub" -ForegroundColor Green

# 6. Final instructions
Write-Host ""
Write-Host "PREPARATION COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "File ready: $zipName" -ForegroundColor Cyan
Write-Host "Git Tag: $VersionTag" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEP - Create Release on GitHub (manual):" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Go to: https://github.com/henriquegfernandes/Dizimo/releases/new" -ForegroundColor White
Write-Host ""
Write-Host "2. Fill in the fields:" -ForegroundColor White
Write-Host "   Release title: Dizimo $VersionTag" -ForegroundColor Cyan
Write-Host "   Tag: Select '$VersionTag'" -ForegroundColor Cyan
Write-Host "   Description:" -ForegroundColor Cyan
Write-Host "     $ReleaseNotes" -ForegroundColor Cyan
Write-Host "   Mark: This is a pre-release" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Click 'Choose a file' and select:" -ForegroundColor White
Write-Host "   $zipName" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Click 'Publish release'" -ForegroundColor White
Write-Host ""
Write-Host "Repository link:" -ForegroundColor Cyan
Write-Host "https://github.com/henriquegfernandes/Dizimo" -ForegroundColor White